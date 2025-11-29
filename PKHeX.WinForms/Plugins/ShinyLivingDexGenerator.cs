using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms.Plugins;

/// <summary>
/// Shiny Living Dex Generator - Generate complete shiny living dex
/// Similar to HexByte3's PKHeX functionality
/// </summary>
public class ShinyLivingDexGenerator
{
    private readonly SaveFile SAV;

    public ShinyLivingDexGenerator(SaveFile sav)
    {
        SAV = sav;
    }

    /// <summary>
    /// Generation options for the Shiny Living Dex
    /// </summary>
    public class GeneratorOptions
    {
        public bool ShinyOnly { get; set; } = true;
        public ShinyType ShinyType { get; set; } = ShinyType.Star;
        public bool IncludeForms { get; set; } = false;
        public bool LegalOnly { get; set; } = true;
        public int StartGeneration { get; set; } = 1;
        public int EndGeneration { get; set; } = 9;
        public int StartBox { get; set; } = 0;
        public bool SetLevel100 { get; set; } = true;
        public bool MaxIVs { get; set; } = true;
        public bool SetOT { get; set; } = true;
        public string CustomOT { get; set; } = "";
        public int CustomTID { get; set; } = -1;
        public int CustomSID { get; set; } = -1;
    }

    public enum ShinyType
    {
        Star,
        Square,
        Random
    }

    // Generation ranges
    private static readonly Dictionary<int, (ushort Start, ushort End)> GenerationRanges = new()
    {
        [1] = (1, 151),
        [2] = (152, 251),
        [3] = (252, 386),
        [4] = (387, 493),
        [5] = (494, 649),
        [6] = (650, 721),
        [7] = (722, 809),
        [8] = (810, 905),
        [9] = (906, 1025)
    };

    /// <summary>
    /// Generate a complete Shiny Living Dex and fill boxes
    /// </summary>
    public GenerationResult GenerateShinyLivingDex(GeneratorOptions? options = null)
    {
        options ??= new GeneratorOptions();
        var result = new GenerationResult();

        // Get species range
        ushort startSpecies = 1;
        ushort endSpecies = (ushort)Math.Min((int)SAV.MaxSpeciesID, 1025);

        if (GenerationRanges.ContainsKey(options.StartGeneration))
            startSpecies = GenerationRanges[options.StartGeneration].Start;
        if (GenerationRanges.ContainsKey(options.EndGeneration))
            endSpecies = GenerationRanges[options.EndGeneration].End;

        endSpecies = (ushort)Math.Min((int)endSpecies, (int)SAV.MaxSpeciesID);

        // Get species list
        var speciesToGenerate = GetSpeciesInRange(startSpecies, endSpecies)
            .Where(s => SAV.Personal.IsSpeciesInGame(s))
            .ToList();

        result.TotalSpecies = speciesToGenerate.Count;

        int currentBox = options.StartBox;
        int currentSlot = 0;

        foreach (var species in speciesToGenerate)
        {
            if (currentBox >= SAV.BoxCount)
            {
                result.Errors.Add("Ran out of box space!");
                break;
            }

            var pokemon = GenerateShinyPokemon(species, 0, options);
            if (pokemon == null)
            {
                result.Failed++;
                result.Errors.Add($"Failed to generate #{species} {SpeciesName.GetSpeciesName(species, 2)}");
                continue;
            }

            // Place in box
            SAV.SetBoxSlotAtIndex(pokemon, currentBox, currentSlot);
            result.Generated++;

            currentSlot++;
            if (currentSlot >= SAV.BoxSlotCount)
            {
                currentSlot = 0;
                currentBox++;
            }

            // Generate forms if requested
            if (options.IncludeForms)
            {
                var pi = SAV.Personal.GetFormEntry(species, 0);
                for (byte form = 1; form < pi.FormCount; form++)
                {
                    if (currentBox >= SAV.BoxCount) break;

                    var formPokemon = GenerateShinyPokemon(species, form, options);
                    if (formPokemon == null) continue;

                    SAV.SetBoxSlotAtIndex(formPokemon, currentBox, currentSlot);
                    result.FormsGenerated++;

                    currentSlot++;
                    if (currentSlot >= SAV.BoxSlotCount)
                    {
                        currentSlot = 0;
                        currentBox++;
                    }
                }
            }
        }

        result.BoxesUsed = currentBox - options.StartBox + (currentSlot > 0 ? 1 : 0);
        return result;
    }

    /// <summary>
    /// Generate a single shiny Pokemon
    /// </summary>
    public PKM? GenerateShinyPokemon(ushort species, byte form, GeneratorOptions options)
    {
        try
        {
            var pk = SAV.BlankPKM;
            pk.Species = species;
            pk.Form = form;
            pk.Gender = pk.GetSaneGender();

            // Try to generate legally first
            if (options.LegalOnly)
            {
                var legal = TryGenerateLegal(species, form, options);
                if (legal != null)
                    return legal;
            }

            // Fallback to basic generation
            pk.CurrentLevel = options.SetLevel100 ? (byte)100 : (byte)50;

            // Set shiny
            if (options.ShinyOnly)
            {
                pk.SetShiny();
            }

            // Set IVs
            if (options.MaxIVs)
            {
                pk.IV_HP = 31;
                pk.IV_ATK = 31;
                pk.IV_DEF = 31;
                pk.IV_SPA = 31;
                pk.IV_SPD = 31;
                pk.IV_SPE = 31;
            }

            // Set OT
            if (options.SetOT)
            {
                pk.OriginalTrainerName = string.IsNullOrEmpty(options.CustomOT) ? SAV.OT : options.CustomOT;
                pk.TID16 = options.CustomTID >= 0 ? (ushort)options.CustomTID : SAV.TID16;
                pk.SID16 = options.CustomSID >= 0 ? (ushort)options.CustomSID : SAV.SID16;
                pk.OriginalTrainerGender = SAV.Gender;
                pk.Language = SAV.Language;
            }

            // Set basic data
            pk.Ball = (byte)Ball.Poke;
            pk.MetDate = DateOnly.FromDateTime(DateTime.Now);
            pk.MetLevel = (byte)Math.Min((int)pk.CurrentLevel, 100);
            pk.MetLocation = GetDefaultMetLocation(pk);

            // Set a valid move
            pk.Move1 = 33; // Tackle
            pk.HealPP();

            pk.RefreshChecksum();

            return EntityConverter.ConvertToType(pk, SAV.PKMType, out _);
        }
        catch
        {
            return null;
        }
    }

    private PKM? TryGenerateLegal(ushort species, byte form, GeneratorOptions options)
    {
        try
        {
            var template = SAV.BlankPKM;
            template.Species = species;
            template.Form = form;
            template.Gender = template.GetSaneGender();

            var moves = ArrayPool<ushort>.Shared.Rent(4);
            var memory = moves.AsMemory(0, 4);
            var span = memory.Span;
            template.GetMoves(span);

            var encounters = EncounterMovesetGenerator.GenerateEncounters(template, SAV, memory);
            var first = encounters.FirstOrDefault();

            span.Clear();
            ArrayPool<ushort>.Shared.Return(moves);

            if (first == null)
                return null;

            var pk = first.ConvertToPKM(SAV);
            if (pk == null)
                return null;

            // Convert to save format
            var result = EntityConverter.ConvertToType(pk, SAV.PKMType, out _);
            if (result == null)
                return null;

            // Apply shiny
            if (options.ShinyOnly)
            {
                result.SetShiny();
            }

            // Set level
            if (options.SetLevel100)
                result.CurrentLevel = 100;

            // Set IVs
            if (options.MaxIVs)
            {
                result.IV_HP = 31;
                result.IV_ATK = 31;
                result.IV_DEF = 31;
                result.IV_SPA = 31;
                result.IV_SPD = 31;
                result.IV_SPE = 31;
            }

            // Set OT if requested
            if (options.SetOT)
            {
                result.OriginalTrainerName = string.IsNullOrEmpty(options.CustomOT) ? SAV.OT : options.CustomOT;
                if (options.CustomTID >= 0) result.TID16 = (ushort)options.CustomTID;
                if (options.CustomSID >= 0) result.SID16 = (ushort)options.CustomSID;
            }

            result.Heal();
            result.RefreshChecksum();

            return result;
        }
        catch
        {
            return null;
        }
    }

    private ushort GetDefaultMetLocation(PKM pk)
    {
        return pk.Context switch
        {
            EntityContext.Gen9 => 6,    // Mesagoza
            EntityContext.Gen8b => 3,   // Twinleaf Town
            EntityContext.Gen8a => 6,   // Jubilife Village
            EntityContext.Gen8 => 6,    // Postwick
            EntityContext.Gen7 => 6,    // Route 1
            EntityContext.Gen6 => 6,    // Route 1
            EntityContext.Gen5 => 6,    // Route 1
            EntityContext.Gen4 => 6,    // Route 201
            EntityContext.Gen3 => 16,   // Route 101
            _ => 30001                  // Poke Transfer
        };
    }

    private IEnumerable<ushort> GetSpeciesInRange(ushort start, ushort end)
    {
        for (ushort i = start; i <= end; i++)
            yield return i;
    }

    /// <summary>
    /// Generate only missing shiny Pokemon (fills gaps in existing collection)
    /// </summary>
    public GenerationResult FillMissingShiny(GeneratorOptions? options = null)
    {
        options ??= new GeneratorOptions();
        var result = new GenerationResult();

        // Scan existing Pokemon
        var existingShiny = new HashSet<ushort>();
        for (int box = 0; box < SAV.BoxCount; box++)
        {
            var pokemon = SAV.GetBoxData(box);
            foreach (var pk in pokemon)
            {
                if (pk.Species > 0 && pk.IsShiny)
                    existingShiny.Add(pk.Species);
            }
        }

        // Get species range
        ushort startSpecies = 1;
        ushort endSpecies = (ushort)Math.Min((int)SAV.MaxSpeciesID, 1025);

        if (GenerationRanges.ContainsKey(options.StartGeneration))
            startSpecies = GenerationRanges[options.StartGeneration].Start;
        if (GenerationRanges.ContainsKey(options.EndGeneration))
            endSpecies = GenerationRanges[options.EndGeneration].End;

        var missingSpecies = GetSpeciesInRange(startSpecies, endSpecies)
            .Where(s => SAV.Personal.IsSpeciesInGame(s) && !existingShiny.Contains(s))
            .ToList();

        result.TotalSpecies = missingSpecies.Count;

        // Find first empty slot
        int currentBox = options.StartBox;
        int currentSlot = 0;

        // Find empty slots
        while (currentBox < SAV.BoxCount)
        {
            var boxData = SAV.GetBoxData(currentBox);
            if (boxData[currentSlot].Species == 0)
                break;

            currentSlot++;
            if (currentSlot >= SAV.BoxSlotCount)
            {
                currentSlot = 0;
                currentBox++;
            }
        }

        foreach (var species in missingSpecies)
        {
            // Find next empty slot
            while (currentBox < SAV.BoxCount)
            {
                var boxData = SAV.GetBoxData(currentBox);
                if (boxData[currentSlot].Species == 0)
                    break;

                currentSlot++;
                if (currentSlot >= SAV.BoxSlotCount)
                {
                    currentSlot = 0;
                    currentBox++;
                }
            }

            if (currentBox >= SAV.BoxCount)
            {
                result.Errors.Add("Ran out of box space!");
                break;
            }

            var pokemon = GenerateShinyPokemon(species, 0, options);
            if (pokemon == null)
            {
                result.Failed++;
                continue;
            }

            SAV.SetBoxSlotAtIndex(pokemon, currentBox, currentSlot);
            result.Generated++;

            currentSlot++;
            if (currentSlot >= SAV.BoxSlotCount)
            {
                currentSlot = 0;
                currentBox++;
            }
        }

        return result;
    }

    /// <summary>
    /// Generate a specific generation's shiny Pokemon
    /// </summary>
    public GenerationResult GenerateGeneration(int generation, int startBox, GeneratorOptions? options = null)
    {
        options ??= new GeneratorOptions();
        options.StartGeneration = generation;
        options.EndGeneration = generation;
        options.StartBox = startBox;
        return GenerateShinyLivingDex(options);
    }

    /// <summary>
    /// Get count of how many boxes are needed for full shiny living dex
    /// </summary>
    public int CalculateBoxesNeeded(GeneratorOptions? options = null)
    {
        options ??= new GeneratorOptions();

        ushort startSpecies = 1;
        ushort endSpecies = (ushort)Math.Min((int)SAV.MaxSpeciesID, 1025);

        if (GenerationRanges.ContainsKey(options.StartGeneration))
            startSpecies = GenerationRanges[options.StartGeneration].Start;
        if (GenerationRanges.ContainsKey(options.EndGeneration))
            endSpecies = GenerationRanges[options.EndGeneration].End;

        int count = GetSpeciesInRange(startSpecies, endSpecies)
            .Count(s => SAV.Personal.IsSpeciesInGame(s));

        if (options.IncludeForms)
        {
            foreach (var species in GetSpeciesInRange(startSpecies, endSpecies))
            {
                if (!SAV.Personal.IsSpeciesInGame(species)) continue;
                var pi = SAV.Personal.GetFormEntry(species, 0);
                count += pi.FormCount - 1;
            }
        }

        return (int)Math.Ceiling(count / (double)SAV.BoxSlotCount);
    }

    /// <summary>
    /// Clear all Pokemon from specified boxes (for fresh generation)
    /// </summary>
    public void ClearBoxes(int startBox, int endBox)
    {
        for (int box = startBox; box <= endBox && box < SAV.BoxCount; box++)
        {
            var blank = SAV.BlankPKM;
            for (int slot = 0; slot < SAV.BoxSlotCount; slot++)
            {
                SAV.SetBoxSlotAtIndex(blank, box, slot);
            }
        }
    }
}

public class GenerationResult
{
    public int TotalSpecies { get; set; }
    public int Generated { get; set; }
    public int FormsGenerated { get; set; }
    public int Failed { get; set; }
    public int BoxesUsed { get; set; }
    public List<string> Errors { get; set; } = new();

    public string GetSummary()
    {
        var summary = $"Shiny Living Dex Generation Complete!\n\n" +
                     $"Pokemon Generated: {Generated}/{TotalSpecies}\n" +
                     $"Forms Generated: {FormsGenerated}\n" +
                     $"Failed: {Failed}\n" +
                     $"Boxes Used: {BoxesUsed}";

        if (Errors.Count > 0)
        {
            summary += $"\n\nErrors ({Errors.Count}):\n";
            summary += string.Join("\n", Errors.Take(10));
            if (Errors.Count > 10)
                summary += $"\n... and {Errors.Count - 10} more";
        }

        return summary;
    }
}
