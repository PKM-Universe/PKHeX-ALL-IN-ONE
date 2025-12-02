using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PKHeX.Core;

namespace PKHeX.WinForms.Plugins;

/// <summary>
/// Shiny Hunting Probability Calculator - Calculate odds and track progress
/// </summary>
public class ShinyHuntingCalculator
{
    private readonly SaveFile SAV;

    // Base shiny odds (1 in X)
    private const int BaseOdds = 4096;
    private const int Gen2To5Odds = 8192;

    // Method bonuses
    private const int ShinyCharmBonus = 2;
    private const int MasudaMethodBonus = 5;
    private const int ChainFishingMaxBonus = 40;
    private const int SOSChainMax = 15;
    private const int RadarChainMax = 40;
    private const int OutbreakBonus = 25;

    public enum HuntingMethod
    {
        RandomEncounter,
        MasudaMethod,
        ChainFishing,
        PokeRadar,
        DexNavChaining,
        SOSChaining,
        WormholeUltra,
        MaxRaidDen,
        MassOutbreak,
        MassiveOutbreak,
        Sandwich,
        FullOdds
    }

    public ShinyHuntingCalculator(SaveFile sav)
    {
        SAV = sav;
    }

    /// <summary>
    /// Calculate shiny odds for a given method
    /// </summary>
    public ShinyOddsResult CalculateOdds(HuntingMethod method, bool hasShinyCharm = false, int chainCount = 0)
    {
        var result = new ShinyOddsResult
        {
            Method = method,
            HasShinyCharm = hasShinyCharm,
            ChainCount = chainCount
        };

        int baseRate = GetBaseRate();
        int bonus = 0;

        // Shiny Charm bonus
        if (hasShinyCharm)
            bonus += ShinyCharmBonus;

        // Method-specific bonuses
        switch (method)
        {
            case HuntingMethod.MasudaMethod:
                bonus += MasudaMethodBonus;
                result.Notes.Add("Breeding with different language Pokemon");
                break;

            case HuntingMethod.ChainFishing:
                bonus += Math.Min(chainCount, ChainFishingMaxBonus);
                result.Notes.Add($"Chain of {chainCount} increases odds");
                break;

            case HuntingMethod.PokeRadar:
                bonus += CalculateRadarBonus(chainCount);
                result.Notes.Add($"Radar chain of {chainCount}");
                break;

            case HuntingMethod.SOSChaining:
                bonus += CalculateSOSBonus(chainCount);
                result.Notes.Add($"SOS chain of {chainCount}");
                break;

            case HuntingMethod.WormholeUltra:
                // Ultra Wormhole has distance-based odds
                result.Odds = 1.0 / CalculateWormholeOdds(chainCount);
                result.OneInX = (int)(1 / result.Odds);
                result.Notes.Add("Distance traveled affects odds");
                return result;

            case HuntingMethod.MassOutbreak:
                bonus += OutbreakBonus;
                result.Notes.Add("Mass Outbreak active");
                break;

            case HuntingMethod.MassiveOutbreak:
                bonus += OutbreakBonus + 10;
                result.Notes.Add("Massive Mass Outbreak active");
                break;

            case HuntingMethod.Sandwich:
                bonus += 3; // Sparkling Power Lv.3
                result.Notes.Add("Sparkling Power sandwich active");
                break;

            case HuntingMethod.FullOdds:
                // No bonus
                result.Notes.Add("Full odds - no bonuses");
                break;
        }

        // Calculate final odds
        result.TotalBonus = bonus;
        result.Odds = (1.0 + bonus) / baseRate;
        result.OneInX = (int)(baseRate / (1.0 + bonus));

        // Calculate expected encounters
        result.ExpectedEncounters = (int)(baseRate / (1.0 + bonus));

        // Calculate probability after X encounters
        result.After100Encounters = CalculateProbabilityAfterN(100, result.Odds);
        result.After500Encounters = CalculateProbabilityAfterN(500, result.Odds);
        result.After1000Encounters = CalculateProbabilityAfterN(1000, result.Odds);

        return result;
    }

    /// <summary>
    /// Calculate probability of finding at least one shiny after N encounters
    /// </summary>
    public double CalculateProbabilityAfterN(int encounters, double oddsPer)
    {
        // P(at least one) = 1 - P(none) = 1 - (1 - p)^n
        return 1.0 - Math.Pow(1.0 - oddsPer, encounters);
    }

    /// <summary>
    /// Calculate how many encounters for X% chance
    /// </summary>
    public int EncountersForProbability(double targetProbability, double oddsPerEncounter)
    {
        // n = log(1 - target) / log(1 - p)
        return (int)Math.Ceiling(Math.Log(1 - targetProbability) / Math.Log(1 - oddsPerEncounter));
    }

    /// <summary>
    /// Analyze current save for shiny hunting progress
    /// </summary>
    public ShinyCollectionStats AnalyzeCollection()
    {
        var stats = new ShinyCollectionStats();

        for (int box = 0; box < SAV.BoxCount; box++)
        {
            foreach (var pk in SAV.GetBoxData(box).Where(p => p.Species > 0))
            {
                stats.TotalPokemon++;

                if (pk.IsShiny)
                {
                    stats.TotalShinies++;

                    var entry = new ShinyEntry
                    {
                        Species = pk.Species,
                        SpeciesName = SpeciesName.GetSpeciesName(pk.Species, 2),
                        IsSquare = pk.ShinyXor == 0,
                        OT = pk.OriginalTrainerName,
                        MetLocation = pk.MetLocation
                    };

                    stats.Shinies.Add(entry);

                    if (entry.IsSquare)
                        stats.SquareShinies++;
                    else
                        stats.StarShinies++;
                }
            }
        }

        // Check party too
        foreach (var pk in SAV.PartyData.Where(p => p.Species > 0))
        {
            stats.TotalPokemon++;
            if (pk.IsShiny)
            {
                stats.TotalShinies++;
                stats.Shinies.Add(new ShinyEntry
                {
                    Species = pk.Species,
                    SpeciesName = SpeciesName.GetSpeciesName(pk.Species, 2),
                    IsSquare = pk.ShinyXor == 0,
                    OT = pk.OriginalTrainerName,
                    MetLocation = pk.MetLocation
                });
            }
        }

        stats.ShinyRate = stats.TotalPokemon > 0
            ? (stats.TotalShinies * 100.0) / stats.TotalPokemon
            : 0;

        return stats;
    }

    /// <summary>
    /// Get recommended hunting method for a species
    /// </summary>
    public HuntingRecommendation GetRecommendation(ushort species)
    {
        var rec = new HuntingRecommendation
        {
            Species = species,
            SpeciesName = SpeciesName.GetSpeciesName(species, 2)
        };

        // Check if breedable (not legendary/mythical)
        if (!IsLegendaryOrMythical(species))
        {
            rec.RecommendedMethods.Add(new MethodRecommendation
            {
                Method = HuntingMethod.MasudaMethod,
                Reason = "Best odds for breedable Pokemon with Shiny Charm",
                EstimatedOdds = "1/512"
            });
        }

        // Check game context for method availability
        switch (SAV.Context)
        {
            case EntityContext.Gen9:
                rec.RecommendedMethods.Add(new MethodRecommendation
                {
                    Method = HuntingMethod.MassOutbreak,
                    Reason = "High spawn rates with boosted odds",
                    EstimatedOdds = "1/128 (with charm)"
                });
                rec.RecommendedMethods.Add(new MethodRecommendation
                {
                    Method = HuntingMethod.Sandwich,
                    Reason = "Sparkling Power boosts shiny rate",
                    EstimatedOdds = "1/1024 (Lv.3 + charm)"
                });
                break;

            case EntityContext.Gen8a: // Legends Arceus
                rec.RecommendedMethods.Add(new MethodRecommendation
                {
                    Method = HuntingMethod.MassiveOutbreak,
                    Reason = "Best odds in any Pokemon game",
                    EstimatedOdds = "1/128"
                });
                break;

            case EntityContext.Gen8:
                rec.RecommendedMethods.Add(new MethodRecommendation
                {
                    Method = HuntingMethod.MaxRaidDen,
                    Reason = "RNG manipulation possible",
                    EstimatedOdds = "Varies"
                });
                break;

            case EntityContext.Gen7:
                rec.RecommendedMethods.Add(new MethodRecommendation
                {
                    Method = HuntingMethod.SOSChaining,
                    Reason = "Chain to 31+ for best odds",
                    EstimatedOdds = "1/273 (chain 31+)"
                });
                break;

            case EntityContext.Gen6:
                rec.RecommendedMethods.Add(new MethodRecommendation
                {
                    Method = HuntingMethod.DexNavChaining,
                    Reason = "Chain fishing or DexNav",
                    EstimatedOdds = "1/512 (chain 40+)"
                });
                rec.RecommendedMethods.Add(new MethodRecommendation
                {
                    Method = HuntingMethod.ChainFishing,
                    Reason = "Easy chain for water Pokemon",
                    EstimatedOdds = "1/100 (chain 40+)"
                });
                break;
        }

        // Always add random encounter as fallback
        rec.RecommendedMethods.Add(new MethodRecommendation
        {
            Method = HuntingMethod.RandomEncounter,
            Reason = "Standard hunting method",
            EstimatedOdds = "1/4096 (1/1365 with charm)"
        });

        return rec;
    }

    private int GetBaseRate()
    {
        // Gen 2-5 had 1/8192, Gen 6+ has 1/4096
        return SAV.Context switch
        {
            EntityContext.Gen1 or EntityContext.Gen2 or EntityContext.Gen3 or
            EntityContext.Gen4 or EntityContext.Gen5 => Gen2To5Odds,
            _ => BaseOdds
        };
    }

    private int CalculateRadarBonus(int chain)
    {
        // PokeRadar chain bonuses
        return chain switch
        {
            < 10 => chain,
            < 20 => 10 + (chain - 10) * 2,
            < 30 => 30 + (chain - 20) * 3,
            < 40 => 60 + (chain - 30) * 4,
            >= 40 => RadarChainMax
        };
    }

    private int CalculateSOSBonus(int chain)
    {
        // SOS chain bonuses (USUM)
        return chain switch
        {
            < 11 => 0,
            < 21 => 4,
            < 31 => 8,
            >= 31 => SOSChainMax
        };
    }

    private double CalculateWormholeOdds(int distance)
    {
        // Ultra Wormhole odds based on light years traveled
        return distance switch
        {
            < 1000 => 1.0,
            < 2000 => 2.0,
            < 3000 => 3.0,
            < 4000 => 5.0,
            >= 4000 => 15.0
        } / 100.0;
    }

    private bool IsLegendaryOrMythical(ushort species)
    {
        var legendaries = new HashSet<ushort>
        {
            144, 145, 146, 150, 151, // Gen 1
            243, 244, 245, 249, 250, 251, // Gen 2
            377, 378, 379, 380, 381, 382, 383, 384, 385, 386, // Gen 3
            480, 481, 482, 483, 484, 485, 486, 487, 488, 489, 490, 491, 492, 493, // Gen 4
            494, 638, 639, 640, 641, 642, 643, 644, 645, 646, 647, 648, 649, // Gen 5
            716, 717, 718, 719, 720, 721, // Gen 6
            785, 786, 787, 788, 789, 790, 791, 792, 800, 801, 802, 807, 808, 809, // Gen 7
            888, 889, 890, 891, 892, 893, 894, 895, 896, 897, 898, // Gen 8
            905, 1001, 1002, 1003, 1004, 1007, 1008, 1009, 1010, 1014, 1015, 1016, 1017, 1024, 1025 // Gen 9
        };
        return legendaries.Contains(species);
    }

    /// <summary>
    /// Generate a hunting odds report
    /// </summary>
    public string GenerateReport(ShinyOddsResult result)
    {
        var sb = new StringBuilder();

        sb.AppendLine("╔══════════════════════════════════════════════════════════════╗");
        sb.AppendLine("║                  SHINY HUNTING CALCULATOR                     ║");
        sb.AppendLine("╠══════════════════════════════════════════════════════════════╣");
        sb.AppendLine($"║ Method: {result.Method,-51}║");
        sb.AppendLine($"║ Shiny Charm: {(result.HasShinyCharm ? "Yes" : "No"),-47}║");
        sb.AppendLine($"║ Chain Count: {result.ChainCount,-47}║");
        sb.AppendLine("╠══════════════════════════════════════════════════════════════╣");
        sb.AppendLine($"║ Final Odds: 1/{result.OneInX,-48}║");
        sb.AppendLine($"║ Total Bonus: +{result.TotalBonus,-46}║");
        sb.AppendLine("╠══════════════════════════════════════════════════════════════╣");
        sb.AppendLine("║ PROBABILITY AFTER N ENCOUNTERS:                              ║");
        sb.AppendLine($"║   100 encounters: {$"{result.After100Encounters * 100:F1}%",-42}║");
        sb.AppendLine($"║   500 encounters: {$"{result.After500Encounters * 100:F1}%",-42}║");
        sb.AppendLine($"║  1000 encounters: {$"{result.After1000Encounters * 100:F1}%",-41}║");
        sb.AppendLine("╠══════════════════════════════════════════════════════════════╣");

        if (result.Notes.Count > 0)
        {
            sb.AppendLine("║ NOTES:                                                       ║");
            foreach (var note in result.Notes)
            {
                sb.AppendLine($"║ - {note,-57}║");
            }
        }

        sb.AppendLine("╚══════════════════════════════════════════════════════════════╝");

        return sb.ToString();
    }
}

/// <summary>
/// Shiny odds calculation result
/// </summary>
public class ShinyOddsResult
{
    public ShinyHuntingCalculator.HuntingMethod Method { get; set; }
    public bool HasShinyCharm { get; set; }
    public int ChainCount { get; set; }
    public double Odds { get; set; }
    public int OneInX { get; set; }
    public int TotalBonus { get; set; }
    public int ExpectedEncounters { get; set; }
    public double After100Encounters { get; set; }
    public double After500Encounters { get; set; }
    public double After1000Encounters { get; set; }
    public List<string> Notes { get; set; } = new();
}

/// <summary>
/// Shiny collection statistics
/// </summary>
public class ShinyCollectionStats
{
    public int TotalPokemon { get; set; }
    public int TotalShinies { get; set; }
    public int StarShinies { get; set; }
    public int SquareShinies { get; set; }
    public double ShinyRate { get; set; }
    public List<ShinyEntry> Shinies { get; set; } = new();
}

/// <summary>
/// A shiny Pokemon entry
/// </summary>
public class ShinyEntry
{
    public ushort Species { get; set; }
    public string SpeciesName { get; set; } = "";
    public bool IsSquare { get; set; }
    public string? OT { get; set; }
    public int MetLocation { get; set; }
}

/// <summary>
/// Hunting method recommendation
/// </summary>
public class HuntingRecommendation
{
    public ushort Species { get; set; }
    public string SpeciesName { get; set; } = "";
    public List<MethodRecommendation> RecommendedMethods { get; set; } = new();
}

/// <summary>
/// A recommended hunting method
/// </summary>
public class MethodRecommendation
{
    public ShinyHuntingCalculator.HuntingMethod Method { get; set; }
    public string Reason { get; set; } = "";
    public string EstimatedOdds { get; set; } = "";
}
