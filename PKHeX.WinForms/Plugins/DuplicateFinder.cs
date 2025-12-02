using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PKHeX.Core;

namespace PKHeX.WinForms.Plugins;

/// <summary>
/// Duplicate Pokemon Finder - Find and manage duplicate Pokemon across boxes
/// </summary>
public class DuplicateFinder
{
    private readonly SaveFile SAV;

    public enum DuplicateMode
    {
        ExactMatch,      // Same species, form, OT, TID, PID
        SameSpecies,     // Same species only
        SameSpeciesForm, // Same species and form
        SameOT,          // Same original trainer
        SameStats,       // Same IVs and EVs
        TradeReady       // Duplicates suitable for trading away
    }

    public DuplicateFinder(SaveFile sav)
    {
        SAV = sav;
    }

    /// <summary>
    /// Find all duplicates in the save file
    /// </summary>
    public DuplicateResult FindDuplicates(DuplicateMode mode = DuplicateMode.SameSpecies)
    {
        var result = new DuplicateResult();
        var allPokemon = new List<(PKM Pokemon, int Box, int Slot)>();

        // Collect all Pokemon with their locations
        for (int box = 0; box < SAV.BoxCount; box++)
        {
            var boxData = SAV.GetBoxData(box).ToList();
            for (int slot = 0; slot < boxData.Count; slot++)
            {
                var pk = boxData[slot];
                if (pk.Species > 0)
                {
                    allPokemon.Add((pk, box, slot));
                }
            }
        }

        result.TotalPokemon = allPokemon.Count;

        // Group by the selected mode
        var groups = mode switch
        {
            DuplicateMode.ExactMatch => GroupByExact(allPokemon),
            DuplicateMode.SameSpecies => GroupBySpecies(allPokemon),
            DuplicateMode.SameSpeciesForm => GroupBySpeciesForm(allPokemon),
            DuplicateMode.SameOT => GroupByOT(allPokemon),
            DuplicateMode.SameStats => GroupByStats(allPokemon),
            DuplicateMode.TradeReady => FindTradeReady(allPokemon),
            _ => GroupBySpecies(allPokemon)
        };

        // Find groups with more than one member (duplicates)
        foreach (var group in groups.Where(g => g.Value.Count > 1))
        {
            var duplicateGroup = new DuplicateGroup
            {
                Key = group.Key,
                Count = group.Value.Count,
                Pokemon = group.Value.Select(p => new DuplicateEntry
                {
                    Species = p.Pokemon.Species,
                    SpeciesName = SpeciesName.GetSpeciesName(p.Pokemon.Species, 2),
                    Nickname = p.Pokemon.Nickname,
                    Level = p.Pokemon.CurrentLevel,
                    Box = p.Box,
                    Slot = p.Slot,
                    IsShiny = p.Pokemon.IsShiny,
                    IVTotal = p.Pokemon.IV_HP + p.Pokemon.IV_ATK + p.Pokemon.IV_DEF + p.Pokemon.IV_SPA + p.Pokemon.IV_SPD + p.Pokemon.IV_SPE,
                    OT = p.Pokemon.OriginalTrainerName
                }).ToList()
            };

            result.DuplicateGroups.Add(duplicateGroup);
        }

        result.TotalDuplicates = result.DuplicateGroups.Sum(g => g.Count - 1); // Subtract 1 for the "original"
        result.UniqueSpecies = groups.Count;

        return result;
    }

    /// <summary>
    /// Get recommended Pokemon to keep and release
    /// </summary>
    public KeepReleaseRecommendation GetKeepReleaseRecommendation(DuplicateGroup group)
    {
        var recommendation = new KeepReleaseRecommendation();
        var sorted = group.Pokemon.OrderByDescending(p => ScorePokemon(p)).ToList();

        recommendation.KeepPokemon = sorted.First();
        recommendation.ReleasePokemon = sorted.Skip(1).ToList();
        recommendation.Reason = GenerateKeepReason(recommendation.KeepPokemon);

        return recommendation;
    }

    /// <summary>
    /// Auto-select best duplicates to release
    /// </summary>
    public List<DuplicateEntry> GetReleaseCandidates(int maxToRelease = 30)
    {
        var candidates = new List<DuplicateEntry>();
        var result = FindDuplicates(DuplicateMode.SameSpeciesForm);

        foreach (var group in result.DuplicateGroups)
        {
            var recommendation = GetKeepReleaseRecommendation(group);
            candidates.AddRange(recommendation.ReleasePokemon);

            if (candidates.Count >= maxToRelease)
                break;
        }

        return candidates.Take(maxToRelease).ToList();
    }

    /// <summary>
    /// Find Pokemon suitable for Wonder Trade
    /// </summary>
    public List<DuplicateEntry> GetWonderTradeCandidates()
    {
        var candidates = new List<DuplicateEntry>();
        var allPokemon = new List<(PKM Pokemon, int Box, int Slot)>();

        for (int box = 0; box < SAV.BoxCount; box++)
        {
            var boxData = SAV.GetBoxData(box).ToList();
            for (int slot = 0; slot < boxData.Count; slot++)
            {
                var pk = boxData[slot];
                if (pk.Species > 0 && IsWonderTradeWorthy(pk))
                {
                    candidates.Add(new DuplicateEntry
                    {
                        Species = pk.Species,
                        SpeciesName = SpeciesName.GetSpeciesName(pk.Species, 2),
                        Nickname = pk.Nickname,
                        Level = pk.CurrentLevel,
                        Box = box,
                        Slot = slot,
                        IsShiny = pk.IsShiny,
                        IVTotal = pk.IV_HP + pk.IV_ATK + pk.IV_DEF + pk.IV_SPA + pk.IV_SPD + pk.IV_SPE,
                        OT = pk.OriginalTrainerName
                    });
                }
            }
        }

        return candidates;
    }

    private Dictionary<string, List<(PKM Pokemon, int Box, int Slot)>> GroupByExact(
        List<(PKM Pokemon, int Box, int Slot)> pokemon)
    {
        return pokemon.GroupBy(p => $"{p.Pokemon.Species}-{p.Pokemon.Form}-{p.Pokemon.OriginalTrainerName}-{p.Pokemon.TID16}-{p.Pokemon.EncryptionConstant}")
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    private Dictionary<string, List<(PKM Pokemon, int Box, int Slot)>> GroupBySpecies(
        List<(PKM Pokemon, int Box, int Slot)> pokemon)
    {
        return pokemon.GroupBy(p => SpeciesName.GetSpeciesName(p.Pokemon.Species, 2))
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    private Dictionary<string, List<(PKM Pokemon, int Box, int Slot)>> GroupBySpeciesForm(
        List<(PKM Pokemon, int Box, int Slot)> pokemon)
    {
        return pokemon.GroupBy(p => $"{SpeciesName.GetSpeciesName(p.Pokemon.Species, 2)}-{p.Pokemon.Form}")
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    private Dictionary<string, List<(PKM Pokemon, int Box, int Slot)>> GroupByOT(
        List<(PKM Pokemon, int Box, int Slot)> pokemon)
    {
        return pokemon.GroupBy(p => p.Pokemon.OriginalTrainerName ?? "Unknown")
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    private Dictionary<string, List<(PKM Pokemon, int Box, int Slot)>> GroupByStats(
        List<(PKM Pokemon, int Box, int Slot)> pokemon)
    {
        return pokemon.GroupBy(p =>
            $"{p.Pokemon.Species}-{p.Pokemon.IV_HP}/{p.Pokemon.IV_ATK}/{p.Pokemon.IV_DEF}/{p.Pokemon.IV_SPA}/{p.Pokemon.IV_SPD}/{p.Pokemon.IV_SPE}")
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    private Dictionary<string, List<(PKM Pokemon, int Box, int Slot)>> FindTradeReady(
        List<(PKM Pokemon, int Box, int Slot)> pokemon)
    {
        // Group by species, but only include Pokemon that are good for trading
        var tradeReady = pokemon.Where(p => IsTradeReady(p.Pokemon)).ToList();
        return tradeReady.GroupBy(p => SpeciesName.GetSpeciesName(p.Pokemon.Species, 2))
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    private bool IsTradeReady(PKM pk)
    {
        // Not shiny (people usually want to keep shinies)
        if (pk.IsShiny) return false;

        // Has good IVs (3+ perfect)
        int perfectIVs = 0;
        if (pk.IV_HP == 31) perfectIVs++;
        if (pk.IV_ATK == 31) perfectIVs++;
        if (pk.IV_DEF == 31) perfectIVs++;
        if (pk.IV_SPA == 31) perfectIVs++;
        if (pk.IV_SPD == 31) perfectIVs++;
        if (pk.IV_SPE == 31) perfectIVs++;

        return perfectIVs >= 3 || pk.CurrentLevel >= 30;
    }

    private bool IsWonderTradeWorthy(PKM pk)
    {
        // Starters are always good for Wonder Trade
        if (IsStarter(pk.Species)) return true;

        // Rare Pokemon
        if (IsRare(pk.Species)) return true;

        // Good IVs
        int perfectIVs = 0;
        if (pk.IV_HP == 31) perfectIVs++;
        if (pk.IV_ATK == 31) perfectIVs++;
        if (pk.IV_DEF == 31) perfectIVs++;
        if (pk.IV_SPA == 31) perfectIVs++;
        if (pk.IV_SPD == 31) perfectIVs++;
        if (pk.IV_SPE == 31) perfectIVs++;

        return perfectIVs >= 4;
    }

    private int ScorePokemon(DuplicateEntry entry)
    {
        int score = 0;

        // Shiny bonus
        if (entry.IsShiny) score += 1000;

        // IV bonus
        score += entry.IVTotal * 2;

        // Level bonus
        score += entry.Level;

        // Nicknamed (may be sentimental)
        if (!string.IsNullOrEmpty(entry.Nickname) &&
            entry.Nickname != SpeciesName.GetSpeciesName((ushort)entry.Species, 2))
        {
            score += 50;
        }

        return score;
    }

    private string GenerateKeepReason(DuplicateEntry entry)
    {
        var reasons = new List<string>();

        if (entry.IsShiny)
            reasons.Add("Shiny");

        if (entry.IVTotal >= 180) // 30 avg per stat
            reasons.Add("High IVs");

        if (entry.Level == 100)
            reasons.Add("Max Level");

        if (reasons.Count == 0)
            reasons.Add("Best overall stats");

        return string.Join(", ", reasons);
    }

    private bool IsStarter(ushort species)
    {
        var starters = new HashSet<ushort>
        {
            1, 4, 7, 152, 155, 158, 252, 255, 258, 387, 390, 393,
            495, 498, 501, 650, 653, 656, 722, 725, 728, 810, 813, 816,
            906, 909, 912
        };
        return starters.Contains(species);
    }

    private bool IsRare(ushort species)
    {
        // Pseudo-legendaries and rare Pokemon
        var rare = new HashSet<ushort>
        {
            147, 148, 149, // Dratini line
            246, 247, 248, // Larvitar line
            371, 372, 373, // Bagon line
            374, 375, 376, // Beldum line
            443, 444, 445, // Gible line
            633, 634, 635, // Deino line
            704, 705, 706, // Goomy line
            782, 783, 784, // Jangmo-o line
            885, 886, 887, // Dreepy line
            996, 997, 998, // Frigibax line
            132, // Ditto
            133, // Eevee
        };
        return rare.Contains(species);
    }

    /// <summary>
    /// Generate a summary report of duplicates
    /// </summary>
    public string GenerateReport(DuplicateResult result)
    {
        var sb = new StringBuilder();

        sb.AppendLine("╔══════════════════════════════════════════════════════════════╗");
        sb.AppendLine("║                  DUPLICATE POKEMON REPORT                     ║");
        sb.AppendLine("╠══════════════════════════════════════════════════════════════╣");
        sb.AppendLine($"║ Total Pokemon Scanned: {result.TotalPokemon,-36}║");
        sb.AppendLine($"║ Unique Species: {result.UniqueSpecies,-43}║");
        sb.AppendLine($"║ Total Duplicates Found: {result.TotalDuplicates,-35}║");
        sb.AppendLine("╠══════════════════════════════════════════════════════════════╣");

        if (result.DuplicateGroups.Count == 0)
        {
            sb.AppendLine("║ No duplicates found!                                         ║");
        }
        else
        {
            sb.AppendLine("║ DUPLICATE GROUPS:                                            ║");
            foreach (var group in result.DuplicateGroups.OrderByDescending(g => g.Count).Take(20))
            {
                sb.AppendLine($"║ {group.Key,-40} x{group.Count,-5}║");
            }

            if (result.DuplicateGroups.Count > 20)
            {
                sb.AppendLine($"║ ... and {result.DuplicateGroups.Count - 20} more groups                              ║");
            }
        }

        sb.AppendLine("╚══════════════════════════════════════════════════════════════╝");

        return sb.ToString();
    }
}

/// <summary>
/// Result of duplicate search
/// </summary>
public class DuplicateResult
{
    public int TotalPokemon { get; set; }
    public int TotalDuplicates { get; set; }
    public int UniqueSpecies { get; set; }
    public List<DuplicateGroup> DuplicateGroups { get; set; } = new();
}

/// <summary>
/// A group of duplicate Pokemon
/// </summary>
public class DuplicateGroup
{
    public string Key { get; set; } = "";
    public int Count { get; set; }
    public List<DuplicateEntry> Pokemon { get; set; } = new();
}

/// <summary>
/// Information about a single duplicate
/// </summary>
public class DuplicateEntry
{
    public int Species { get; set; }
    public string SpeciesName { get; set; } = "";
    public string? Nickname { get; set; }
    public int Level { get; set; }
    public int Box { get; set; }
    public int Slot { get; set; }
    public bool IsShiny { get; set; }
    public int IVTotal { get; set; }
    public string? OT { get; set; }

    public string Location => $"Box {Box + 1}, Slot {Slot + 1}";
}

/// <summary>
/// Recommendation for which Pokemon to keep/release
/// </summary>
public class KeepReleaseRecommendation
{
    public DuplicateEntry KeepPokemon { get; set; } = null!;
    public List<DuplicateEntry> ReleasePokemon { get; set; } = new();
    public string Reason { get; set; } = "";
}
