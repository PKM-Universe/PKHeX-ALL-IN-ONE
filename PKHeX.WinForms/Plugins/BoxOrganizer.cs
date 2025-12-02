using System;
using System.Collections.Generic;
using System.Linq;
using PKHeX.Core;

namespace PKHeX.WinForms.Plugins;

/// <summary>
/// Intelligent Box Organizer - Automatically sort and organize boxes
/// </summary>
public class BoxOrganizer
{
    private readonly SaveFile SAV;

    public enum SortMode
    {
        NationalDex,
        Type,
        Level,
        Shiny,
        Legendary,
        Generation,
        AlphabeticalSpecies,
        AlphabeticalNickname,
        OriginalTrainer,
        IVTotal,
        EVTotal,
        Competitive,
        FormVariants,
        GameOrigin,
        Ball,
        HeldItem
    }

    public BoxOrganizer(SaveFile sav)
    {
        SAV = sav;
    }

    /// <summary>
    /// Sort all boxes by specified mode
    /// </summary>
    public BoxOrganizeResult SortAllBoxes(SortMode mode, bool ascending = true)
    {
        var result = new BoxOrganizeResult();
        var allPokemon = new List<PKM>();

        // Collect all Pokemon from all boxes
        for (int box = 0; box < SAV.BoxCount; box++)
        {
            var boxData = SAV.GetBoxData(box);
            allPokemon.AddRange(boxData.Where(p => p.Species > 0));
        }

        result.TotalPokemon = allPokemon.Count;
        result.OriginalOrder = allPokemon.Select(p => p.Species).ToList();

        // Sort based on mode
        var sorted = mode switch
        {
            SortMode.NationalDex => SortByNationalDex(allPokemon, ascending),
            SortMode.Type => SortByType(allPokemon, ascending),
            SortMode.Level => SortByLevel(allPokemon, ascending),
            SortMode.Shiny => SortByShiny(allPokemon),
            SortMode.Legendary => SortByLegendary(allPokemon),
            SortMode.Generation => SortByGeneration(allPokemon, ascending),
            SortMode.AlphabeticalSpecies => SortBySpeciesName(allPokemon, ascending),
            SortMode.AlphabeticalNickname => SortByNickname(allPokemon, ascending),
            SortMode.OriginalTrainer => SortByOT(allPokemon, ascending),
            SortMode.IVTotal => SortByIVTotal(allPokemon, ascending),
            SortMode.EVTotal => SortByEVTotal(allPokemon, ascending),
            SortMode.Competitive => SortByCompetitive(allPokemon),
            SortMode.FormVariants => SortByFormVariants(allPokemon),
            SortMode.GameOrigin => SortByGameOrigin(allPokemon, ascending),
            SortMode.Ball => SortByBall(allPokemon, ascending),
            SortMode.HeldItem => SortByHeldItem(allPokemon, ascending),
            _ => allPokemon
        };

        // Place sorted Pokemon back into boxes
        int index = 0;
        for (int box = 0; box < SAV.BoxCount && index < sorted.Count; box++)
        {
            var boxData = new PKM[SAV.BoxSlotCount];
            for (int slot = 0; slot < SAV.BoxSlotCount && index < sorted.Count; slot++)
            {
                boxData[slot] = sorted[index++];
            }
            SAV.SetBoxData(boxData, box);
        }

        result.SortedOrder = sorted.Select(p => p.Species).ToList();
        result.Success = true;
        return result;
    }

    /// <summary>
    /// Sort a single box
    /// </summary>
    public BoxOrganizeResult SortSingleBox(int boxIndex, SortMode mode, bool ascending = true)
    {
        var result = new BoxOrganizeResult();
        var boxData = SAV.GetBoxData(boxIndex).Where(p => p.Species > 0).ToList();

        result.TotalPokemon = boxData.Count;

        var sorted = mode switch
        {
            SortMode.NationalDex => SortByNationalDex(boxData, ascending),
            SortMode.Type => SortByType(boxData, ascending),
            SortMode.Level => SortByLevel(boxData, ascending),
            _ => boxData
        };

        // Create new box array
        var newBoxData = new PKM[SAV.BoxSlotCount];
        for (int i = 0; i < SAV.BoxSlotCount; i++)
        {
            newBoxData[i] = i < sorted.Count ? sorted[i] : SAV.BlankPKM;
        }

        SAV.SetBoxData(newBoxData, boxIndex);
        result.Success = true;
        return result;
    }

    /// <summary>
    /// Create a Living Dex layout
    /// </summary>
    public BoxOrganizeResult CreateLivingDex(bool includeAlternateFormes = false)
    {
        var result = new BoxOrganizeResult();
        var allPokemon = new List<PKM>();

        // Collect all Pokemon
        for (int box = 0; box < SAV.BoxCount; box++)
        {
            allPokemon.AddRange(SAV.GetBoxData(box).Where(p => p.Species > 0));
        }

        // Group by species (and optionally form)
        var grouped = includeAlternateFormes
            ? allPokemon.GroupBy(p => (p.Species, p.Form)).ToDictionary(g => g.Key, g => g.First())
            : allPokemon.GroupBy(p => p.Species).ToDictionary(g => (g.Key, (byte)0), g => g.First());

        // Create ordered list
        var ordered = new List<PKM>();
        for (ushort species = 1; species <= SAV.MaxSpeciesID; species++)
        {
            if (grouped.TryGetValue((species, 0), out var pk))
            {
                ordered.Add(pk);
            }
            else
            {
                // Add blank placeholder
                var blank = SAV.BlankPKM;
                blank.Species = species;
                ordered.Add(blank);
            }
        }

        result.TotalPokemon = ordered.Count;
        result.LivingDexProgress = grouped.Count;
        result.LivingDexTotal = SAV.MaxSpeciesID;
        result.Success = true;

        return result;
    }

    /// <summary>
    /// Auto-organize into themed boxes
    /// </summary>
    public BoxOrganizeResult OrganizeByTheme()
    {
        var result = new BoxOrganizeResult();
        var boxAssignments = new Dictionary<string, List<PKM>>
        {
            ["Shinies"] = new(),
            ["Legendaries"] = new(),
            ["Starters"] = new(),
            ["Competitive"] = new(),
            ["Eggs"] = new(),
            ["Events"] = new(),
            ["Trade Fodder"] = new(),
            ["Regular"] = new()
        };

        // Collect and categorize
        for (int box = 0; box < SAV.BoxCount; box++)
        {
            foreach (var pk in SAV.GetBoxData(box).Where(p => p.Species > 0))
            {
                var category = CategorizePokemon(pk);
                boxAssignments[category].Add(pk);
            }
        }

        result.BoxCategories = boxAssignments.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Count
        );

        result.Success = true;
        return result;
    }

    private string CategorizePokemon(PKM pk)
    {
        if (pk.IsShiny)
            return "Shinies";

        if (IsLegendary(pk.Species))
            return "Legendaries";

        if (IsStarter(pk.Species))
            return "Starters";

        if (pk.IsEgg)
            return "Eggs";

        if (IsEventPokemon(pk))
            return "Events";

        if (IsCompetitiveReady(pk))
            return "Competitive";

        if (pk.CurrentLevel < 10)
            return "Trade Fodder";

        return "Regular";
    }

    private List<PKM> SortByNationalDex(List<PKM> pokemon, bool ascending)
    {
        var sorted = pokemon.OrderBy(p => p.Species).ThenBy(p => p.Form);
        return ascending ? sorted.ToList() : sorted.Reverse().ToList();
    }

    private List<PKM> SortByType(List<PKM> pokemon, bool ascending)
    {
        var sorted = pokemon.OrderBy(p => p.PersonalInfo.Type1).ThenBy(p => p.PersonalInfo.Type2).ThenBy(p => p.Species);
        return ascending ? sorted.ToList() : sorted.Reverse().ToList();
    }

    private List<PKM> SortByLevel(List<PKM> pokemon, bool ascending)
    {
        return ascending
            ? pokemon.OrderBy(p => p.CurrentLevel).ThenBy(p => p.Species).ToList()
            : pokemon.OrderByDescending(p => p.CurrentLevel).ThenBy(p => p.Species).ToList();
    }

    private List<PKM> SortByShiny(List<PKM> pokemon)
    {
        // Shinies first, then by species
        return pokemon.OrderByDescending(p => p.IsShiny).ThenBy(p => p.Species).ToList();
    }

    private List<PKM> SortByLegendary(List<PKM> pokemon)
    {
        return pokemon.OrderByDescending(p => IsLegendary(p.Species)).ThenBy(p => p.Species).ToList();
    }

    private List<PKM> SortByGeneration(List<PKM> pokemon, bool ascending)
    {
        var sorted = pokemon.OrderBy(p => GetGeneration(p.Species)).ThenBy(p => p.Species);
        return ascending ? sorted.ToList() : sorted.Reverse().ToList();
    }

    private List<PKM> SortBySpeciesName(List<PKM> pokemon, bool ascending)
    {
        var sorted = pokemon.OrderBy(p => SpeciesName.GetSpeciesName(p.Species, 2));
        return ascending ? sorted.ToList() : sorted.Reverse().ToList();
    }

    private List<PKM> SortByNickname(List<PKM> pokemon, bool ascending)
    {
        var sorted = pokemon.OrderBy(p => p.Nickname ?? SpeciesName.GetSpeciesName(p.Species, 2));
        return ascending ? sorted.ToList() : sorted.Reverse().ToList();
    }

    private List<PKM> SortByOT(List<PKM> pokemon, bool ascending)
    {
        var sorted = pokemon.OrderBy(p => p.OriginalTrainerName).ThenBy(p => p.Species);
        return ascending ? sorted.ToList() : sorted.Reverse().ToList();
    }

    private List<PKM> SortByIVTotal(List<PKM> pokemon, bool ascending)
    {
        Func<PKM, int> ivSum = p => p.IV_HP + p.IV_ATK + p.IV_DEF + p.IV_SPA + p.IV_SPD + p.IV_SPE;
        return ascending
            ? pokemon.OrderBy(ivSum).ThenBy(p => p.Species).ToList()
            : pokemon.OrderByDescending(ivSum).ThenBy(p => p.Species).ToList();
    }

    private List<PKM> SortByEVTotal(List<PKM> pokemon, bool ascending)
    {
        Func<PKM, int> evSum = p => p.EV_HP + p.EV_ATK + p.EV_DEF + p.EV_SPA + p.EV_SPD + p.EV_SPE;
        return ascending
            ? pokemon.OrderBy(evSum).ThenBy(p => p.Species).ToList()
            : pokemon.OrderByDescending(evSum).ThenBy(p => p.Species).ToList();
    }

    private List<PKM> SortByCompetitive(List<PKM> pokemon)
    {
        // Sort by competitive readiness score
        return pokemon.OrderByDescending(p => CalculateCompetitiveScore(p)).ThenBy(p => p.Species).ToList();
    }

    private List<PKM> SortByFormVariants(List<PKM> pokemon)
    {
        return pokemon.OrderBy(p => p.Species).ThenBy(p => p.Form).ToList();
    }

    private List<PKM> SortByGameOrigin(List<PKM> pokemon, bool ascending)
    {
        var sorted = pokemon.OrderBy(p => p.Version).ThenBy(p => p.Species);
        return ascending ? sorted.ToList() : sorted.Reverse().ToList();
    }

    private List<PKM> SortByBall(List<PKM> pokemon, bool ascending)
    {
        var sorted = pokemon.OrderBy(p => p.Ball).ThenBy(p => p.Species);
        return ascending ? sorted.ToList() : sorted.Reverse().ToList();
    }

    private List<PKM> SortByHeldItem(List<PKM> pokemon, bool ascending)
    {
        var sorted = pokemon.OrderByDescending(p => p.HeldItem > 0).ThenBy(p => p.HeldItem).ThenBy(p => p.Species);
        return ascending ? sorted.ToList() : sorted.Reverse().ToList();
    }

    private int CalculateCompetitiveScore(PKM pk)
    {
        int score = 0;

        // Perfect IVs
        if (pk.IV_HP == 31) score += 10;
        if (pk.IV_ATK == 31) score += 10;
        if (pk.IV_DEF == 31) score += 10;
        if (pk.IV_SPA == 31) score += 10;
        if (pk.IV_SPD == 31) score += 10;
        if (pk.IV_SPE == 31) score += 10;

        // EVs trained
        int evTotal = pk.EV_HP + pk.EV_ATK + pk.EV_DEF + pk.EV_SPA + pk.EV_SPD + pk.EV_SPE;
        if (evTotal == 510) score += 30;
        else if (evTotal > 400) score += 15;

        // Level 100 or 50
        if (pk.CurrentLevel == 100) score += 10;
        else if (pk.CurrentLevel == 50) score += 5;

        // Has competitive nature (not neutral)
        if (!IsNeutralNature(pk.Nature)) score += 10;

        // Has held item
        if (pk.HeldItem > 0) score += 5;

        return score;
    }

    private bool IsNeutralNature(Nature nature)
    {
        return nature == Nature.Hardy || nature == Nature.Docile ||
               nature == Nature.Serious || nature == Nature.Bashful || nature == Nature.Quirky;
    }

    private bool IsLegendary(ushort species)
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

    private bool IsStarter(ushort species)
    {
        var starters = new HashSet<ushort>
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, // Gen 1
            152, 153, 154, 155, 156, 157, 158, 159, 160, // Gen 2
            252, 253, 254, 255, 256, 257, 258, 259, 260, // Gen 3
            387, 388, 389, 390, 391, 392, 393, 394, 395, // Gen 4
            495, 496, 497, 498, 499, 500, 501, 502, 503, // Gen 5
            650, 651, 652, 653, 654, 655, 656, 657, 658, // Gen 6
            722, 723, 724, 725, 726, 727, 728, 729, 730, // Gen 7
            810, 811, 812, 813, 814, 815, 816, 817, 818, // Gen 8
            906, 907, 908, 909, 910, 911, 912, 913, 914 // Gen 9
        };
        return starters.Contains(species);
    }

    private bool IsEventPokemon(PKM pk)
    {
        // Check for event-only traits
        return pk.FatefulEncounter ||
               pk.Ball == (int)Ball.Cherish ||
               (pk.OriginalTrainerName?.Contains("EVENT") ?? false);
    }

    private bool IsCompetitiveReady(PKM pk)
    {
        int perfectIVs = 0;
        if (pk.IV_HP == 31) perfectIVs++;
        if (pk.IV_ATK == 31) perfectIVs++;
        if (pk.IV_DEF == 31) perfectIVs++;
        if (pk.IV_SPA == 31) perfectIVs++;
        if (pk.IV_SPD == 31) perfectIVs++;
        if (pk.IV_SPE == 31) perfectIVs++;

        int evTotal = pk.EV_HP + pk.EV_ATK + pk.EV_DEF + pk.EV_SPA + pk.EV_SPD + pk.EV_SPE;

        return perfectIVs >= 5 && evTotal >= 500 && (pk.CurrentLevel == 100 || pk.CurrentLevel == 50);
    }

    private int GetGeneration(ushort species)
    {
        return species switch
        {
            <= 151 => 1,
            <= 251 => 2,
            <= 386 => 3,
            <= 493 => 4,
            <= 649 => 5,
            <= 721 => 6,
            <= 809 => 7,
            <= 905 => 8,
            _ => 9
        };
    }
}

/// <summary>
/// Result of box organization
/// </summary>
public class BoxOrganizeResult
{
    public bool Success { get; set; }
    public int TotalPokemon { get; set; }
    public List<ushort> OriginalOrder { get; set; } = new();
    public List<ushort> SortedOrder { get; set; } = new();
    public Dictionary<string, int> BoxCategories { get; set; } = new();
    public int LivingDexProgress { get; set; }
    public int LivingDexTotal { get; set; }
}
