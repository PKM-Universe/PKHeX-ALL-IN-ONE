using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PKHeX.Core;

namespace PKHeX.WinForms.Plugins;

/// <summary>
/// Breeding Chain Optimizer - Calculate optimal breeding chains for perfect Pokemon
/// </summary>
public class BreedingOptimizer
{
    private readonly SaveFile SAV;

    // Egg group data (simplified - would need complete data in production)
    private static readonly Dictionary<int, int[]> EggGroups = new()
    {
        // Monster group (1)
        [1] = new[] { 1, 3, 6, 7, 8, 9, 29, 30, 31, 32, 33, 34, 104, 105, 108, 111, 112, 115, 131, 142, 143, 147, 148, 149, 152, 153, 154, 155, 156, 157, 158, 159, 160, 246, 247, 248, 252, 253, 254, 258, 259, 260 },
        // Water 1 group (2)
        [2] = new[] { 7, 8, 9, 54, 55, 60, 61, 62, 79, 80, 86, 87, 131, 134, 158, 159, 160, 183, 184, 186, 194, 195, 199, 222, 223, 224, 226, 258, 259, 260, 270, 271, 272, 278, 279, 283, 284, 320, 321, 339, 340 },
        // Bug group (3)
        [3] = new[] { 10, 11, 12, 13, 14, 15, 46, 47, 48, 49, 123, 127, 165, 166, 167, 168, 193, 204, 205, 207, 212, 213, 214, 265, 266, 267, 268, 269, 283, 284, 290, 291, 292, 313, 314, 328, 329, 330, 401, 402 },
        // Flying group (4)
        [4] = new[] { 16, 17, 18, 21, 22, 41, 42, 83, 84, 85, 142, 163, 164, 169, 176, 177, 178, 198, 225, 227, 276, 277, 278, 279, 333, 334, 396, 397, 398, 441 },
        // Field group (5)
        [5] = new[] { 19, 20, 25, 26, 27, 28, 37, 38, 50, 51, 52, 53, 56, 57, 58, 59, 77, 78, 83, 128, 133, 135, 136, 155, 156, 157, 161, 162, 179, 180, 181, 190, 203, 206, 209, 210, 215, 216, 217, 220, 221 },
        // Fairy group (6)
        [6] = new[] { 25, 26, 35, 36, 39, 40, 113, 173, 174, 175, 176, 183, 184, 187, 188, 189, 209, 210, 242, 280, 281, 282, 298, 300, 301, 303, 311, 312, 314, 315, 420, 421 },
        // Grass group (7)
        [7] = new[] { 1, 2, 3, 43, 44, 45, 46, 47, 69, 70, 71, 102, 103, 114, 152, 153, 154, 182, 187, 188, 189, 191, 192, 270, 271, 272, 273, 274, 275, 285, 286, 315, 331, 332, 357, 387, 388, 389 },
        // Human-Like group (8)
        [8] = new[] { 63, 64, 65, 66, 67, 68, 96, 97, 106, 107, 122, 124, 125, 126, 237, 239, 240, 241, 280, 281, 282, 296, 297, 302, 307, 308, 313, 327, 331, 332, 352, 390, 391, 392 },
        // Water 3 group (9)
        [9] = new[] { 72, 73, 90, 91, 98, 99, 120, 121, 138, 139, 140, 141, 222, 341, 342, 345, 346, 347, 348, 451, 452 },
        // Mineral group (10)
        [10] = new[] { 74, 75, 76, 81, 82, 95, 185, 208, 299, 337, 338, 343, 344, 361, 362, 374, 375, 376, 436, 437 },
        // Amorphous group (11)
        [11] = new[] { 88, 89, 92, 93, 94, 109, 110, 200, 218, 219, 280, 281, 282, 292, 316, 317, 353, 354, 355, 356, 358, 422, 423, 425, 426, 429, 442 },
        // Water 2 (Fish) group (12)
        [12] = new[] { 118, 119, 129, 130, 170, 171, 211, 223, 224, 318, 319, 320, 321, 339, 340, 369, 370 },
        // Ditto group (13) - breeds with everything
        [13] = new[] { 132 },
        // Dragon group (14)
        [14] = new[] { 23, 24, 130, 142, 147, 148, 149, 230, 252, 253, 254, 329, 330, 333, 334, 349, 350, 371, 372, 373, 443, 444, 445 },
        // Undiscovered group (15) - cannot breed
        [15] = new[] { 144, 145, 146, 150, 151, 172, 173, 174, 175, 201, 235, 236, 238, 239, 240, 243, 244, 245, 249, 250, 251 }
    };

    public BreedingOptimizer(SaveFile sav)
    {
        SAV = sav;
    }

    /// <summary>
    /// Calculate the optimal breeding chain for a target Pokemon with specific IVs
    /// </summary>
    public BreedingPlan CalculateBreedingChain(ushort targetSpecies, int[] targetIVs, Nature targetNature)
    {
        var plan = new BreedingPlan
        {
            TargetSpecies = targetSpecies,
            TargetSpeciesName = SpeciesName.GetSpeciesName(targetSpecies, 2),
            TargetIVs = targetIVs,
            TargetNature = targetNature
        };

        // Find available breeding stock
        var availableParents = FindAvailableParents(targetSpecies);
        plan.AvailableParents = availableParents.Count;

        if (availableParents.Count == 0)
        {
            plan.Notes.Add("No compatible breeding parents found in boxes.");
            plan.Notes.Add("You'll need to catch or obtain a Ditto or same egg group Pokemon.");
            return plan;
        }

        // Find best parent combination
        var bestPair = FindBestBreedingPair(availableParents, targetIVs, targetNature);
        if (bestPair != null)
        {
            plan.Steps.Add(new BreedingStep
            {
                StepNumber = 1,
                Description = $"Breed {bestPair.Parent1Name} with {bestPair.Parent2Name}",
                Parent1 = bestPair.Parent1Name,
                Parent2 = bestPair.Parent2Name,
                ExpectedIVs = bestPair.ExpectedIVs,
                RequiredItem = bestPair.RequiredItem
            });
        }

        // Calculate expected attempts
        plan.EstimatedEggs = CalculateExpectedEggs(plan, targetIVs, targetNature);

        // Calculate success rate
        plan.SuccessRate = CalculateSuccessRate(targetIVs, targetNature, bestPair);

        return plan;
    }

    /// <summary>
    /// Find Pokemon that can breed with the target species
    /// </summary>
    public List<BreedingCandidate> FindAvailableParents(ushort targetSpecies)
    {
        var candidates = new List<BreedingCandidate>();
        var targetEggGroups = GetEggGroups(targetSpecies);

        for (int box = 0; box < SAV.BoxCount; box++)
        {
            foreach (var pk in SAV.GetBoxData(box).Where(p => p.Species > 0))
            {
                // Ditto can breed with anything (except Undiscovered)
                if (pk.Species == 132) // Ditto
                {
                    candidates.Add(CreateCandidate(pk, box, true));
                    continue;
                }

                // Check if same species or compatible egg group
                if (pk.Species == targetSpecies || SharesEggGroup(pk.Species, targetEggGroups))
                {
                    candidates.Add(CreateCandidate(pk, box, false));
                }
            }
        }

        return candidates.OrderByDescending(c => c.Score).ToList();
    }

    /// <summary>
    /// Find Pokemon with specific IV spreads for breeding
    /// </summary>
    public List<BreedingCandidate> FindIVDonors(int[] neededIVs)
    {
        var donors = new List<BreedingCandidate>();

        for (int box = 0; box < SAV.BoxCount; box++)
        {
            foreach (var pk in SAV.GetBoxData(box).Where(p => p.Species > 0))
            {
                int matchingIVs = 0;
                if (neededIVs[0] == 31 && pk.IV_HP == 31) matchingIVs++;
                if (neededIVs[1] == 31 && pk.IV_ATK == 31) matchingIVs++;
                if (neededIVs[2] == 31 && pk.IV_DEF == 31) matchingIVs++;
                if (neededIVs[3] == 31 && pk.IV_SPA == 31) matchingIVs++;
                if (neededIVs[4] == 31 && pk.IV_SPD == 31) matchingIVs++;
                if (neededIVs[5] == 31 && pk.IV_SPE == 31) matchingIVs++;

                if (matchingIVs >= 3) // At least 3 matching perfect IVs
                {
                    donors.Add(CreateCandidate(pk, box, pk.Species == 132));
                }
            }
        }

        return donors.OrderByDescending(d => d.PerfectIVCount).ToList();
    }

    /// <summary>
    /// Calculate odds for hatching with Masuda Method + Shiny Charm
    /// </summary>
    public ShinyBreedingOdds CalculateShinyOdds(bool hasMasudaMethod, bool hasShinyCharm)
    {
        var odds = new ShinyBreedingOdds();

        int baseOdds = 4096;
        int bonus = 0;

        if (hasMasudaMethod)
            bonus += 5;

        if (hasShinyCharm)
            bonus += 2;

        odds.BaseOdds = $"1/{baseOdds}";
        odds.FinalOdds = $"1/{baseOdds / (1 + bonus)}";
        odds.MasudaMethod = hasMasudaMethod;
        odds.ShinyCharm = hasShinyCharm;
        odds.ExpectedEggs = baseOdds / (1 + bonus);

        return odds;
    }

    private BreedingCandidate CreateCandidate(PKM pk, int box, bool isDitto)
    {
        int[] ivs = { pk.IV_HP, pk.IV_ATK, pk.IV_DEF, pk.IV_SPA, pk.IV_SPD, pk.IV_SPE };
        int perfectCount = ivs.Count(iv => iv == 31);

        return new BreedingCandidate
        {
            Species = pk.Species,
            SpeciesName = SpeciesName.GetSpeciesName(pk.Species, 2),
            Gender = pk.Gender,
            Nature = pk.Nature,
            IVs = ivs,
            PerfectIVCount = perfectCount,
            IsDitto = isDitto,
            Box = box,
            Language = pk.Language,
            Score = CalculateCandidateScore(pk, perfectCount, isDitto)
        };
    }

    private int CalculateCandidateScore(PKM pk, int perfectIVs, bool isDitto)
    {
        int score = perfectIVs * 20;

        // Ditto bonus
        if (isDitto) score += 50;

        // Foreign language bonus (for Masuda Method)
        if (pk.Language != SAV.Language) score += 30;

        // Has Destiny Knot potential
        if (perfectIVs >= 5) score += 25;

        return score;
    }

    private int[] GetEggGroups(ushort species)
    {
        var groups = new List<int>();

        foreach (var kvp in EggGroups)
        {
            if (kvp.Value.Contains(species))
                groups.Add(kvp.Key);
        }

        return groups.ToArray();
    }

    private bool SharesEggGroup(ushort species, int[] targetGroups)
    {
        var speciesGroups = GetEggGroups(species);
        return speciesGroups.Any(g => targetGroups.Contains(g) && g != 15); // 15 = Undiscovered
    }

    private BreedingPair? FindBestBreedingPair(List<BreedingCandidate> candidates, int[] targetIVs, Nature targetNature)
    {
        if (candidates.Count < 2)
            return null;

        BreedingPair? bestPair = null;
        int bestScore = 0;

        // Find Ditto if available
        var ditto = candidates.FirstOrDefault(c => c.IsDitto);

        foreach (var parent1 in candidates)
        {
            foreach (var parent2 in candidates)
            {
                if (parent1 == parent2) continue;

                // Need opposite genders or one Ditto
                if (!parent1.IsDitto && !parent2.IsDitto && parent1.Gender == parent2.Gender)
                    continue;

                var pair = CreateBreedingPair(parent1, parent2, targetIVs, targetNature);
                if (pair.Score > bestScore)
                {
                    bestScore = pair.Score;
                    bestPair = pair;
                }
            }
        }

        return bestPair;
    }

    private BreedingPair CreateBreedingPair(BreedingCandidate parent1, BreedingCandidate parent2, int[] targetIVs, Nature targetNature)
    {
        var pair = new BreedingPair
        {
            Parent1Name = parent1.SpeciesName,
            Parent2Name = parent2.SpeciesName,
            ExpectedIVs = new int[6]
        };

        // Calculate expected IVs (with Destiny Knot, 5 IVs are inherited)
        int totalPerfect = 0;
        for (int i = 0; i < 6; i++)
        {
            if (parent1.IVs[i] == 31 || parent2.IVs[i] == 31)
            {
                pair.ExpectedIVs[i] = 31;
                totalPerfect++;
            }
            else
            {
                pair.ExpectedIVs[i] = Math.Max(parent1.IVs[i], parent2.IVs[i]);
            }
        }

        // Determine required items
        if (parent1.PerfectIVCount >= 4 || parent2.PerfectIVCount >= 4)
            pair.RequiredItem = "Destiny Knot";

        if (parent1.Nature == targetNature || parent2.Nature == targetNature)
        {
            if (!string.IsNullOrEmpty(pair.RequiredItem))
                pair.RequiredItem += " + Everstone";
            else
                pair.RequiredItem = "Everstone";
        }

        pair.Score = totalPerfect * 10;
        if (parent1.Language != parent2.Language)
            pair.Score += 20; // Masuda Method potential

        return pair;
    }

    private int CalculateExpectedEggs(BreedingPlan plan, int[] targetIVs, Nature targetNature)
    {
        // Base calculation for 6IV Pokemon
        int perfectIVsNeeded = targetIVs.Count(iv => iv == 31);

        // With Destiny Knot (5 IVs inherited)
        // Chance of 6th IV being 31 = 1/32
        // Chance of nature (with Everstone) = 100%
        // Without Everstone = 1/25

        int baseEggs = perfectIVsNeeded switch
        {
            6 => 32,   // 1/32 for the 6th IV
            5 => 6,    // Very likely
            4 => 3,
            _ => 2
        };

        return baseEggs;
    }

    private double CalculateSuccessRate(int[] targetIVs, Nature targetNature, BreedingPair? pair)
    {
        if (pair == null) return 0;

        int matchingIVs = 0;
        for (int i = 0; i < 6; i++)
        {
            if (pair.ExpectedIVs[i] == 31 && targetIVs[i] == 31)
                matchingIVs++;
        }

        // Base success rate
        double rate = matchingIVs / 6.0 * 100;

        return Math.Round(rate, 1);
    }

    /// <summary>
    /// Generate breeding report
    /// </summary>
    public string GenerateReport(BreedingPlan plan)
    {
        var sb = new StringBuilder();

        sb.AppendLine("╔══════════════════════════════════════════════════════════════╗");
        sb.AppendLine("║                  BREEDING OPTIMIZATION PLAN                   ║");
        sb.AppendLine("╠══════════════════════════════════════════════════════════════╣");
        sb.AppendLine($"║ Target: {plan.TargetSpeciesName,-51}║");
        sb.AppendLine($"║ Nature: {plan.TargetNature,-51}║");
        sb.AppendLine($"║ IVs: {string.Join("/", plan.TargetIVs),-54}║");
        sb.AppendLine("╠══════════════════════════════════════════════════════════════╣");
        sb.AppendLine($"║ Available Parents: {plan.AvailableParents,-40}║");
        sb.AppendLine($"║ Estimated Eggs: ~{plan.EstimatedEggs,-42}║");
        sb.AppendLine($"║ Success Rate: {$"{plan.SuccessRate:F1}%",-45}║");
        sb.AppendLine("╠══════════════════════════════════════════════════════════════╣");

        if (plan.Steps.Count > 0)
        {
            sb.AppendLine("║ BREEDING STEPS:                                              ║");
            foreach (var step in plan.Steps)
            {
                sb.AppendLine($"║ {step.StepNumber}. {step.Description,-55}║");
                if (!string.IsNullOrEmpty(step.RequiredItem))
                    sb.AppendLine($"║    Item: {step.RequiredItem,-50}║");
            }
        }

        if (plan.Notes.Count > 0)
        {
            sb.AppendLine("╠══════════════════════════════════════════════════════════════╣");
            sb.AppendLine("║ NOTES:                                                       ║");
            foreach (var note in plan.Notes)
            {
                sb.AppendLine($"║ - {note,-57}║");
            }
        }

        sb.AppendLine("╚══════════════════════════════════════════════════════════════╝");

        return sb.ToString();
    }
}

/// <summary>
/// Breeding plan result
/// </summary>
public class BreedingPlan
{
    public ushort TargetSpecies { get; set; }
    public string TargetSpeciesName { get; set; } = "";
    public int[] TargetIVs { get; set; } = new int[6];
    public Nature TargetNature { get; set; }
    public int AvailableParents { get; set; }
    public int EstimatedEggs { get; set; }
    public double SuccessRate { get; set; }
    public List<BreedingStep> Steps { get; set; } = new();
    public List<string> Notes { get; set; } = new();
}

/// <summary>
/// A step in the breeding chain
/// </summary>
public class BreedingStep
{
    public int StepNumber { get; set; }
    public string Description { get; set; } = "";
    public string Parent1 { get; set; } = "";
    public string Parent2 { get; set; } = "";
    public int[] ExpectedIVs { get; set; } = new int[6];
    public string? RequiredItem { get; set; }
}

/// <summary>
/// A potential breeding parent
/// </summary>
public class BreedingCandidate
{
    public ushort Species { get; set; }
    public string SpeciesName { get; set; } = "";
    public byte Gender { get; set; }
    public Nature Nature { get; set; }
    public int[] IVs { get; set; } = new int[6];
    public int PerfectIVCount { get; set; }
    public bool IsDitto { get; set; }
    public int Box { get; set; }
    public int Language { get; set; }
    public int Score { get; set; }
}

/// <summary>
/// A breeding pair
/// </summary>
public class BreedingPair
{
    public string Parent1Name { get; set; } = "";
    public string Parent2Name { get; set; } = "";
    public int[] ExpectedIVs { get; set; } = new int[6];
    public string? RequiredItem { get; set; }
    public int Score { get; set; }
}

/// <summary>
/// Shiny breeding odds
/// </summary>
public class ShinyBreedingOdds
{
    public string BaseOdds { get; set; } = "";
    public string FinalOdds { get; set; } = "";
    public bool MasudaMethod { get; set; }
    public bool ShinyCharm { get; set; }
    public int ExpectedEggs { get; set; }
}
