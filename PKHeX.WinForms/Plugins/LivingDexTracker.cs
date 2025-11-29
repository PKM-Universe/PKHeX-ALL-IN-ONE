using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using PKHeX.Core;

namespace PKHeX.WinForms.Plugins;

/// <summary>
/// Living Dex Tracker - Track your complete Pokemon collection
/// Supports all 1025+ species including forms and shinies
/// </summary>
public class LivingDexTracker
{
    private readonly SaveFile SAV;
    private LivingDexProgress Progress;
    private static readonly string SavePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PKHeX", "LivingDexProgress.json");

    // Total Pokemon counts by generation
    public static readonly Dictionary<int, (int Start, int End, string Name)> Generations = new()
    {
        [1] = (1, 151, "Kanto"),
        [2] = (152, 251, "Johto"),
        [3] = (252, 386, "Hoenn"),
        [4] = (387, 493, "Sinnoh"),
        [5] = (494, 649, "Unova"),
        [6] = (650, 721, "Kalos"),
        [7] = (722, 809, "Alola"),
        [8] = (810, 905, "Galar/Hisui"),
        [9] = (906, 1025, "Paldea")
    };

    public LivingDexTracker(SaveFile sav)
    {
        SAV = sav;
        Progress = LoadProgress();
    }

    /// <summary>
    /// Scan all boxes and update progress
    /// </summary>
    public LivingDexStats ScanAndUpdate()
    {
        var stats = new LivingDexStats();
        var foundSpecies = new HashSet<int>();
        var foundShiny = new HashSet<int>();
        var foundForms = new HashSet<string>(); // "Species-Form"

        // Scan all boxes
        for (int box = 0; box < SAV.BoxCount; box++)
        {
            var pokemon = SAV.GetBoxData(box);
            foreach (var pk in pokemon)
            {
                if (pk.Species == 0) continue;

                foundSpecies.Add(pk.Species);

                if (pk.IsShiny)
                    foundShiny.Add(pk.Species);

                if (pk.Form > 0)
                    foundForms.Add($"{pk.Species}-{pk.Form}");
            }
        }

        // Also scan party
        foreach (var pk in SAV.PartyData)
        {
            if (pk.Species == 0) continue;
            foundSpecies.Add(pk.Species);
            if (pk.IsShiny) foundShiny.Add(pk.Species);
            if (pk.Form > 0) foundForms.Add($"{pk.Species}-{pk.Form}");
        }

        // Update progress
        foreach (var species in foundSpecies)
        {
            if (!Progress.OwnedSpecies.Contains(species))
                Progress.OwnedSpecies.Add(species);
        }

        foreach (var species in foundShiny)
        {
            if (!Progress.ShinySpecies.Contains(species))
                Progress.ShinySpecies.Add(species);
        }

        foreach (var form in foundForms)
        {
            if (!Progress.OwnedForms.Contains(form))
                Progress.OwnedForms.Add(form);
        }

        Progress.LastScanDate = DateTime.Now;
        SaveProgress();

        // Calculate stats
        stats.TotalSpecies = 1025;
        stats.OwnedSpecies = Progress.OwnedSpecies.Count;
        stats.ShinyOwned = Progress.ShinySpecies.Count;
        stats.FormsOwned = Progress.OwnedForms.Count;
        stats.CompletionPercent = (stats.OwnedSpecies * 100.0) / stats.TotalSpecies;
        stats.ShinyPercent = (stats.ShinyOwned * 100.0) / stats.TotalSpecies;

        // Generation breakdown
        foreach (var gen in Generations)
        {
            int owned = Progress.OwnedSpecies.Count(s => s >= gen.Value.Start && s <= gen.Value.End);
            int total = gen.Value.End - gen.Value.Start + 1;
            stats.GenerationProgress[gen.Key] = new GenProgress
            {
                Name = gen.Value.Name,
                Owned = owned,
                Total = total,
                Percent = (owned * 100.0) / total
            };
        }

        return stats;
    }

    /// <summary>
    /// Get list of missing Pokemon
    /// </summary>
    public List<MissingPokemon> GetMissing(int? generation = null)
    {
        var missing = new List<MissingPokemon>();
        int start = 1;
        int end = 1025;

        if (generation.HasValue && Generations.ContainsKey(generation.Value))
        {
            start = Generations[generation.Value].Start;
            end = Generations[generation.Value].End;
        }

        for (int species = start; species <= end; species++)
        {
            if (!Progress.OwnedSpecies.Contains(species))
            {
                var speciesName = SpeciesName.GetSpeciesName(species, 2);
                missing.Add(new MissingPokemon
                {
                    Species = species,
                    Name = speciesName,
                    Generation = GetGeneration(species),
                    HasShiny = false,
                    MissingForms = GetFormsForSpecies(species)
                });
            }
            else if (!Progress.ShinySpecies.Contains(species))
            {
                // Has regular but not shiny
                var speciesName = SpeciesName.GetSpeciesName(species, 2);
                missing.Add(new MissingPokemon
                {
                    Species = species,
                    Name = speciesName,
                    Generation = GetGeneration(species),
                    HasShiny = false,
                    MissingShiny = true,
                    MissingForms = new List<string>()
                });
            }
        }

        return missing;
    }

    /// <summary>
    /// Get missing Pokemon for Shiny Living Dex
    /// </summary>
    public List<MissingPokemon> GetMissingShiny()
    {
        var missing = new List<MissingPokemon>();

        for (int species = 1; species <= 1025; species++)
        {
            if (!Progress.ShinySpecies.Contains(species))
            {
                var speciesName = SpeciesName.GetSpeciesName(species, 2);
                missing.Add(new MissingPokemon
                {
                    Species = species,
                    Name = speciesName,
                    Generation = GetGeneration(species),
                    HasShiny = false,
                    MissingShiny = true
                });
            }
        }

        return missing;
    }

    /// <summary>
    /// Export progress report
    /// </summary>
    public string ExportReport()
    {
        var stats = ScanAndUpdate();
        var sb = new StringBuilder();

        sb.AppendLine("╔══════════════════════════════════════════════════════════════╗");
        sb.AppendLine("║              LIVING DEX PROGRESS REPORT                      ║");
        sb.AppendLine("╠══════════════════════════════════════════════════════════════╣");
        sb.AppendLine($"║  Overall Progress: {stats.OwnedSpecies}/{stats.TotalSpecies} ({stats.CompletionPercent:F1}%)");
        sb.AppendLine($"║  Shiny Progress:   {stats.ShinyOwned}/{stats.TotalSpecies} ({stats.ShinyPercent:F1}%)");
        sb.AppendLine($"║  Forms Collected:  {stats.FormsOwned}");
        sb.AppendLine("╠══════════════════════════════════════════════════════════════╣");
        sb.AppendLine("║  GENERATION BREAKDOWN                                        ║");
        sb.AppendLine("╠══════════════════════════════════════════════════════════════╣");

        foreach (var gen in stats.GenerationProgress.OrderBy(g => g.Key))
        {
            var bar = GenerateProgressBar(gen.Value.Percent, 20);
            sb.AppendLine($"║  Gen {gen.Key} ({gen.Value.Name,-8}): {bar} {gen.Value.Owned,3}/{gen.Value.Total,3} ({gen.Value.Percent:F1}%)");
        }

        sb.AppendLine("╠══════════════════════════════════════════════════════════════╣");
        sb.AppendLine($"║  Last Scan: {Progress.LastScanDate:yyyy-MM-dd HH:mm}");
        sb.AppendLine("╚══════════════════════════════════════════════════════════════╝");

        // Missing Pokemon summary
        var missing = GetMissing();
        if (missing.Count > 0 && missing.Count <= 50)
        {
            sb.AppendLine();
            sb.AppendLine("MISSING POKEMON:");
            foreach (var m in missing.Take(50))
            {
                sb.AppendLine($"  #{m.Species:D4} {m.Name}");
            }
            if (missing.Count > 50)
                sb.AppendLine($"  ... and {missing.Count - 50} more");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Export to CSV for spreadsheet tracking
    /// </summary>
    public string ExportToCsv()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Species,Name,Generation,Owned,Shiny,Forms");

        for (int species = 1; species <= 1025; species++)
        {
            var name = SpeciesName.GetSpeciesName(species, 2);
            var gen = GetGeneration(species);
            var owned = Progress.OwnedSpecies.Contains(species) ? "Yes" : "No";
            var shiny = Progress.ShinySpecies.Contains(species) ? "Yes" : "No";
            var forms = Progress.OwnedForms.Count(f => f.StartsWith($"{species}-"));

            sb.AppendLine($"{species},{name},{gen},{owned},{shiny},{forms}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Mark a Pokemon as owned manually
    /// </summary>
    public void MarkOwned(int species, bool shiny = false)
    {
        if (species < 1 || species > 1025) return;

        if (!Progress.OwnedSpecies.Contains(species))
            Progress.OwnedSpecies.Add(species);

        if (shiny && !Progress.ShinySpecies.Contains(species))
            Progress.ShinySpecies.Add(species);

        SaveProgress();
    }

    /// <summary>
    /// Get suggested next Pokemon to catch
    /// </summary>
    public List<SuggestedPokemon> GetSuggestions(GameVersion game)
    {
        var suggestions = new List<SuggestedPokemon>();
        var missing = GetMissing();

        // Filter by game availability
        foreach (var m in missing.Take(20))
        {
            var suggestion = new SuggestedPokemon
            {
                Species = m.Species,
                Name = m.Name,
                Location = GetCatchLocation(m.Species, game),
                Method = GetCatchMethod(m.Species, game),
                Difficulty = EstimateDifficulty(m.Species, game)
            };
            suggestions.Add(suggestion);
        }

        return suggestions.OrderBy(s => s.Difficulty).ToList();
    }

    /// <summary>
    /// Reset all progress
    /// </summary>
    public void ResetProgress()
    {
        Progress = new LivingDexProgress();
        SaveProgress();
    }

    private int GetGeneration(int species)
    {
        foreach (var gen in Generations)
        {
            if (species >= gen.Value.Start && species <= gen.Value.End)
                return gen.Key;
        }
        return 0;
    }

    private List<string> GetFormsForSpecies(int species)
    {
        // Return notable alternate forms
        var forms = new List<string>();

        // Notable Pokemon with forms
        switch (species)
        {
            case 025: forms.AddRange(new[] { "Partner Cap", "Original Cap", "Hoenn Cap", "Sinnoh Cap", "Unova Cap", "Kalos Cap", "Alola Cap", "World Cap" }); break; // Pikachu
            case 201: for (int i = 0; i < 28; i++) forms.Add($"Form {(char)('A' + i)}"); break; // Unown
            case 351: forms.AddRange(new[] { "Sunny", "Rainy", "Snowy" }); break; // Castform
            case 386: forms.AddRange(new[] { "Attack", "Defense", "Speed" }); break; // Deoxys
            case 412: case 413: forms.AddRange(new[] { "Plant", "Sandy", "Trash" }); break; // Burmy/Wormadam
            case 421: forms.AddRange(new[] { "Overcast", "Sunshine" }); break; // Cherrim
            case 422: case 423: forms.AddRange(new[] { "West Sea", "East Sea" }); break; // Shellos/Gastrodon
            case 479: forms.AddRange(new[] { "Heat", "Wash", "Frost", "Fan", "Mow" }); break; // Rotom
            case 487: forms.AddRange(new[] { "Origin" }); break; // Giratina
            case 492: forms.AddRange(new[] { "Sky" }); break; // Shaymin
            case 550: forms.AddRange(new[] { "Blue-Striped", "White-Striped" }); break; // Basculin
            case 641: case 642: case 645: forms.AddRange(new[] { "Therian" }); break; // Forces of Nature
            case 646: forms.AddRange(new[] { "White", "Black" }); break; // Kyurem
            case 647: forms.AddRange(new[] { "Resolute" }); break; // Keldeo
            case 648: forms.AddRange(new[] { "Pirouette" }); break; // Meloetta
            case 658: forms.AddRange(new[] { "Battle Bond" }); break; // Greninja
            case 681: forms.AddRange(new[] { "Blade" }); break; // Aegislash
            case 710: case 711: forms.AddRange(new[] { "Small", "Large", "Super" }); break; // Pumpkaboo/Gourgeist
            case 718: forms.AddRange(new[] { "10%", "Complete" }); break; // Zygarde
            case 720: forms.AddRange(new[] { "Unbound" }); break; // Hoopa
            case 741: forms.AddRange(new[] { "Pom-Pom", "Pa'u", "Sensu" }); break; // Oricorio
            case 745: forms.AddRange(new[] { "Midnight", "Dusk" }); break; // Lycanroc
            case 746: forms.AddRange(new[] { "School" }); break; // Wishiwashi
            case 774: forms.AddRange(new[] { "Meteor" }); break; // Minior
            case 800: forms.AddRange(new[] { "Dusk Mane", "Dawn Wings", "Ultra" }); break; // Necrozma
            case 849: forms.AddRange(new[] { "Low Key" }); break; // Toxtricity
            case 875: forms.AddRange(new[] { "Noice Face" }); break; // Eiscue
            case 876: forms.AddRange(new[] { "Female" }); break; // Indeedee
            case 892: forms.AddRange(new[] { "Rapid Strike" }); break; // Urshifu
            case 898: forms.AddRange(new[] { "Ice Rider", "Shadow Rider" }); break; // Calyrex
            case 901: forms.AddRange(new[] { "Bloodmoon" }); break; // Ursaluna
            case 902: forms.AddRange(new[] { "Female" }); break; // Basculegion
            case 925: forms.AddRange(new[] { "Hero" }); break; // Maushold
            case 931: forms.AddRange(new[] { "Blue", "Yellow", "White" }); break; // Squawkabilly
            case 964: forms.AddRange(new[] { "Droopy", "Stretchy" }); break; // Palafin
            case 978: forms.AddRange(new[] { "Curly", "Droopy", "Stretchy" }); break; // Tatsugiri
            case 982: forms.AddRange(new[] { "Three-Segment" }); break; // Dudunsparce
            case 1007: forms.AddRange(new[] { "Cornerstone", "Wellspring", "Hearthflame" }); break; // Ogerpon
            case 1017: forms.AddRange(new[] { "Cornerstone", "Wellspring", "Hearthflame" }); break; // Ogerpon
            case 1024: forms.AddRange(new[] { "Terastal" }); break; // Terapagos
        }

        return forms;
    }

    private string GetCatchLocation(int species, GameVersion game)
    {
        // Basic location hints - would be expanded with full location data
        if (game == GameVersion.SV || game == GameVersion.SL || game == GameVersion.VL)
        {
            if (species >= 906) return "Paldea Region";
            if (species >= 810) return "Tera Raids / Transfer";
            return "Pokemon HOME Transfer";
        }
        return "Various locations";
    }

    private string GetCatchMethod(int species, GameVersion game)
    {
        // Basic method hints
        if (species > 905) return "Wild Encounter";
        if (species > 809 && species <= 905) return "Legends: Arceus / Transfer";
        return "Transfer from older games";
    }

    private int EstimateDifficulty(int species, GameVersion game)
    {
        // 1 = Easy, 5 = Very Hard
        // Legendaries/Mythicals are harder
        if (IsLegendary(species)) return 4;
        if (IsMythical(species)) return 5;
        return 2;
    }

    private bool IsLegendary(int species)
    {
        var legendaries = new[] { 144, 145, 146, 150, 243, 244, 245, 249, 250, 377, 378, 379, 380, 381, 382, 383, 384,
            480, 481, 482, 483, 484, 485, 486, 487, 488, 641, 642, 643, 644, 645, 646, 716, 717, 718, 785, 786, 787, 788,
            789, 790, 791, 792, 800, 888, 889, 890, 891, 892, 894, 895, 896, 897, 898, 905, 1001, 1002, 1003, 1004, 1007, 1008,
            1009, 1010, 1014, 1015, 1016, 1017, 1024 };
        return legendaries.Contains(species);
    }

    private bool IsMythical(int species)
    {
        var mythicals = new[] { 151, 251, 385, 386, 489, 490, 491, 492, 493, 494, 647, 648, 649, 719, 720, 721, 801, 802,
            807, 808, 809, 893, 1025 };
        return mythicals.Contains(species);
    }

    private string GenerateProgressBar(double percent, int width)
    {
        int filled = (int)(percent / 100.0 * width);
        int empty = width - filled;
        return "[" + new string('█', filled) + new string('░', empty) + "]";
    }

    private LivingDexProgress LoadProgress()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                var json = File.ReadAllText(SavePath);
                return JsonSerializer.Deserialize<LivingDexProgress>(json) ?? new LivingDexProgress();
            }
        }
        catch { }
        return new LivingDexProgress();
    }

    private void SaveProgress()
    {
        try
        {
            var dir = Path.GetDirectoryName(SavePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(Progress, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SavePath, json);
        }
        catch { }
    }
}

public class LivingDexProgress
{
    public List<int> OwnedSpecies { get; set; } = new();
    public List<int> ShinySpecies { get; set; } = new();
    public List<string> OwnedForms { get; set; } = new();
    public DateTime LastScanDate { get; set; }
}

public class LivingDexStats
{
    public int TotalSpecies { get; set; }
    public int OwnedSpecies { get; set; }
    public int ShinyOwned { get; set; }
    public int FormsOwned { get; set; }
    public double CompletionPercent { get; set; }
    public double ShinyPercent { get; set; }
    public Dictionary<int, GenProgress> GenerationProgress { get; set; } = new();
}

public class GenProgress
{
    public string Name { get; set; } = "";
    public int Owned { get; set; }
    public int Total { get; set; }
    public double Percent { get; set; }
}

public class MissingPokemon
{
    public int Species { get; set; }
    public string Name { get; set; } = "";
    public int Generation { get; set; }
    public bool HasShiny { get; set; }
    public bool MissingShiny { get; set; }
    public List<string> MissingForms { get; set; } = new();
}

public class SuggestedPokemon
{
    public int Species { get; set; }
    public string Name { get; set; } = "";
    public string Location { get; set; } = "";
    public string Method { get; set; } = "";
    public int Difficulty { get; set; }
}
