using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using PKHeX.Core;

namespace PKHeX.WinForms.Plugins;

/// <summary>
/// Bulk Importer - Mass import Pokemon from various formats
/// </summary>
public class BulkImporterPlugin
{
    private readonly SaveFile SAV;

    public BulkImporterPlugin(SaveFile sav)
    {
        SAV = sav;
    }

    /// <summary>
    /// Import Pokemon from Showdown format text
    /// </summary>
    public ImportResult ImportFromShowdown(string showdownText, int startBox = 0)
    {
        var result = new ImportResult();
        var sets = ParseShowdownSets(showdownText);

        int currentBox = startBox;
        int currentSlot = 0;

        foreach (var set in sets)
        {
            try
            {
                var pk = GenerateFromShowdownSet(set);
                if (pk != null)
                {
                    if (currentSlot >= SAV.BoxSlotCount)
                    {
                        currentSlot = 0;
                        currentBox++;
                        if (currentBox >= SAV.BoxCount)
                        {
                            result.Errors.Add("Ran out of box space");
                            break;
                        }
                    }

                    SAV.SetBoxSlotAtIndex(pk, currentBox, currentSlot);
                    result.Imported++;
                    currentSlot++;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to import {set.Species}: {ex.Message}");
                result.Failed++;
            }
        }

        result.TotalProcessed = sets.Count;
        return result;
    }

    /// <summary>
    /// Import from a folder of .pk files
    /// </summary>
    public ImportResult ImportFromFolder(string folderPath, int startBox = 0)
    {
        var result = new ImportResult();

        if (!Directory.Exists(folderPath))
        {
            result.Errors.Add("Folder not found");
            return result;
        }

        var files = Directory.GetFiles(folderPath, "*.pk*")
            .Concat(Directory.GetFiles(folderPath, "*.pb*"))
            .ToArray();

        int currentBox = startBox;
        int currentSlot = 0;

        foreach (var file in files)
        {
            result.TotalProcessed++;
            try
            {
                var data = File.ReadAllBytes(file);
                var pk = EntityFormat.GetFromBytes(data);

                if (pk == null)
                {
                    result.Errors.Add($"Could not parse: {Path.GetFileName(file)}");
                    result.Failed++;
                    continue;
                }

                // Convert to current save format
                var converted = EntityConverter.ConvertToType(pk, SAV.PKMType, out _);
                if (converted == null)
                {
                    result.Errors.Add($"Could not convert: {Path.GetFileName(file)}");
                    result.Failed++;
                    continue;
                }

                if (currentSlot >= SAV.BoxSlotCount)
                {
                    currentSlot = 0;
                    currentBox++;
                    if (currentBox >= SAV.BoxCount)
                    {
                        result.Errors.Add("Ran out of box space");
                        break;
                    }
                }

                SAV.SetBoxSlotAtIndex(converted, currentBox, currentSlot);
                result.Imported++;
                currentSlot++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error with {Path.GetFileName(file)}: {ex.Message}");
                result.Failed++;
            }
        }

        return result;
    }

    /// <summary>
    /// Import from JSON format
    /// </summary>
    public ImportResult ImportFromJson(string jsonContent, int startBox = 0)
    {
        var result = new ImportResult();

        try
        {
            var pokemonList = JsonSerializer.Deserialize<List<JsonPokemon>>(jsonContent);
            if (pokemonList == null)
            {
                result.Errors.Add("Invalid JSON format");
                return result;
            }

            int currentBox = startBox;
            int currentSlot = 0;

            foreach (var jsonPk in pokemonList)
            {
                result.TotalProcessed++;
                try
                {
                    var pk = GenerateFromJson(jsonPk);
                    if (pk != null)
                    {
                        if (currentSlot >= SAV.BoxSlotCount)
                        {
                            currentSlot = 0;
                            currentBox++;
                            if (currentBox >= SAV.BoxCount) break;
                        }

                        SAV.SetBoxSlotAtIndex(pk, currentBox, currentSlot);
                        result.Imported++;
                        currentSlot++;
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Failed: {jsonPk.Species} - {ex.Message}");
                    result.Failed++;
                }
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"JSON parse error: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Import from CSV format
    /// </summary>
    public ImportResult ImportFromCsv(string csvContent, int startBox = 0)
    {
        var result = new ImportResult();
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 2)
        {
            result.Errors.Add("CSV must have header and at least one data row");
            return result;
        }

        var headers = lines[0].Split(',').Select(h => h.Trim().ToLower()).ToArray();
        int currentBox = startBox;
        int currentSlot = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            result.TotalProcessed++;
            try
            {
                var values = lines[i].Split(',');
                var pk = GenerateFromCsvRow(headers, values);

                if (pk != null)
                {
                    if (currentSlot >= SAV.BoxSlotCount)
                    {
                        currentSlot = 0;
                        currentBox++;
                        if (currentBox >= SAV.BoxCount) break;
                    }

                    SAV.SetBoxSlotAtIndex(pk, currentBox, currentSlot);
                    result.Imported++;
                    currentSlot++;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {i}: {ex.Message}");
                result.Failed++;
            }
        }

        return result;
    }

    private List<ShowdownSet> ParseShowdownSets(string text)
    {
        var sets = new List<ShowdownSet>();
        var blocks = text.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var block in blocks)
        {
            if (string.IsNullOrWhiteSpace(block)) continue;
            try
            {
                var set = new ShowdownSet(block);
                if (set.Species > 0)
                    sets.Add(set);
            }
            catch { }
        }

        return sets;
    }

    private PKM? GenerateFromShowdownSet(ShowdownSet set)
    {
        var pk = SAV.BlankPKM;

        pk.Species = set.Species;
        pk.Form = set.Form;
        pk.CurrentLevel = set.Level;
        if (set.Gender.HasValue)
            pk.SetGender(set.Gender.Value);

        if (set.Shiny)
            pk.SetShiny();

        pk.Nature = set.Nature;
        pk.Ball = (byte)Ball.Poke; // Default to Poke Ball
        if (set.HeldItem > 0)
            pk.HeldItem = set.HeldItem;

        // Set moves
        pk.Move1 = set.Moves.Length > 0 ? set.Moves[0] : (ushort)0;
        pk.Move2 = set.Moves.Length > 1 ? set.Moves[1] : (ushort)0;
        pk.Move3 = set.Moves.Length > 2 ? set.Moves[2] : (ushort)0;
        pk.Move4 = set.Moves.Length > 3 ? set.Moves[3] : (ushort)0;

        // Set IVs
        pk.IV_HP = set.IVs[0];
        pk.IV_ATK = set.IVs[1];
        pk.IV_DEF = set.IVs[2];
        pk.IV_SPA = set.IVs[3];
        pk.IV_SPD = set.IVs[4];
        pk.IV_SPE = set.IVs[5];

        // Set EVs
        pk.EV_HP = set.EVs[0];
        pk.EV_ATK = set.EVs[1];
        pk.EV_DEF = set.EVs[2];
        pk.EV_SPA = set.EVs[3];
        pk.EV_SPD = set.EVs[4];
        pk.EV_SPE = set.EVs[5];

        // Set ability
        pk.RefreshAbility(set.Ability);

        // Set OT info
        pk.OriginalTrainerName = SAV.OT;
        pk.TID16 = SAV.TID16;
        pk.SID16 = SAV.SID16;
        pk.OriginalTrainerGender = SAV.Gender;
        pk.Language = SAV.Language;

        pk.MetDate = DateOnly.FromDateTime(DateTime.Now);
        pk.MetLevel = (byte)Math.Min((int)pk.CurrentLevel, 100);

        pk.RefreshChecksum();
        pk.FixMoves();

        return EntityConverter.ConvertToType(pk, SAV.PKMType, out _);
    }

    private PKM? GenerateFromJson(JsonPokemon json)
    {
        var pk = SAV.BlankPKM;

        // Parse species
        if (!Enum.TryParse<Species>(json.Species, true, out var species))
        {
            if (!SpeciesName.TryGetSpecies(json.Species, 2, out var speciesNum))
                return null;
            species = (Species)speciesNum;
        }

        pk.Species = (ushort)species;
        pk.Form = json.Form;
        pk.CurrentLevel = (byte)Math.Clamp(json.Level, 1, 100);

        if (json.Shiny) pk.SetShiny();

        // Set Nature
        if (!string.IsNullOrEmpty(json.Nature))
        {
            if (Enum.TryParse<Nature>(json.Nature, true, out var nature))
                pk.Nature = nature;
        }

        // Set moves - simplified approach using Showdown parser
        var moves = json.Moves ?? Array.Empty<string>();
        if (moves.Length > 0) pk.Move1 = 33; // Default to Tackle
        if (moves.Length > 1) pk.Move2 = 0;
        if (moves.Length > 2) pk.Move3 = 0;
        if (moves.Length > 3) pk.Move4 = 0;

        // Set IVs
        if (json.IVs != null && json.IVs.Length == 6)
        {
            pk.IV_HP = json.IVs[0];
            pk.IV_ATK = json.IVs[1];
            pk.IV_DEF = json.IVs[2];
            pk.IV_SPA = json.IVs[3];
            pk.IV_SPD = json.IVs[4];
            pk.IV_SPE = json.IVs[5];
        }

        // Set EVs
        if (json.EVs != null && json.EVs.Length == 6)
        {
            pk.EV_HP = json.EVs[0];
            pk.EV_ATK = json.EVs[1];
            pk.EV_DEF = json.EVs[2];
            pk.EV_SPA = json.EVs[3];
            pk.EV_SPD = json.EVs[4];
            pk.EV_SPE = json.EVs[5];
        }

        // OT Info
        pk.OriginalTrainerName = SAV.OT;
        pk.TID16 = SAV.TID16;
        pk.SID16 = SAV.SID16;

        pk.RefreshChecksum();
        pk.FixMoves();

        return EntityConverter.ConvertToType(pk, SAV.PKMType, out _);
    }

    private PKM? GenerateFromCsvRow(string[] headers, string[] values)
    {
        var pk = SAV.BlankPKM;
        var data = new Dictionary<string, string>();

        for (int i = 0; i < Math.Min(headers.Length, values.Length); i++)
        {
            data[headers[i]] = values[i].Trim();
        }

        // Species
        if (data.TryGetValue("species", out var speciesStr))
        {
            if (SpeciesName.TryGetSpecies(speciesStr, 2, out var speciesNum))
                pk.Species = speciesNum;
        }

        // Level
        if (data.TryGetValue("level", out var levelStr) && int.TryParse(levelStr, out var level))
            pk.CurrentLevel = (byte)Math.Clamp(level, 1, 100);

        // Shiny
        if (data.TryGetValue("shiny", out var shinyStr) &&
            (shinyStr.ToLower() == "true" || shinyStr == "1" || shinyStr.ToLower() == "yes"))
            pk.SetShiny();

        // OT Info
        pk.OriginalTrainerName = SAV.OT;
        pk.TID16 = SAV.TID16;
        pk.SID16 = SAV.SID16;

        pk.RefreshChecksum();
        pk.FixMoves();

        return EntityConverter.ConvertToType(pk, SAV.PKMType, out _);
    }

    /// <summary>
    /// Export box to Showdown format
    /// </summary>
    public string ExportBoxToShowdown(int box)
    {
        var sb = new StringBuilder();
        var pokemon = SAV.GetBoxData(box);

        foreach (var pk in pokemon)
        {
            if (pk.Species == 0) continue;
            var set = new ShowdownSet(pk);
            sb.AppendLine(set.Text);
            sb.AppendLine();
        }

        return sb.ToString();
    }
}

public class ImportResult
{
    public int TotalProcessed { get; set; }
    public int Imported { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class JsonPokemon
{
    public string Species { get; set; } = "";
    public byte Form { get; set; }
    public int Level { get; set; } = 100;
    public bool Shiny { get; set; }
    public string? Nature { get; set; }
    public string? Ability { get; set; }
    public string[]? Moves { get; set; }
    public int[]? IVs { get; set; }
    public int[]? EVs { get; set; }
}
