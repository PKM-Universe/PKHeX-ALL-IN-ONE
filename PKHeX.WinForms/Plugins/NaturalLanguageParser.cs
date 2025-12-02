using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PKHeX.Core;

namespace PKHeX.WinForms.Plugins;

/// <summary>
/// Natural Language Pokemon Parser - AI-powered Pokemon creation from natural language descriptions
/// Examples: "Create a competitive Garchomp for VGC", "Make me a shiny Charizard with good IVs"
/// </summary>
public class NaturalLanguageParser
{
    private readonly SaveFile SAV;

    // Common competitive terms
    private static readonly Dictionary<string, string[]> CompetitiveTerms = new()
    {
        ["sweeper"] = new[] { "fast", "offensive", "attacker" },
        ["wall"] = new[] { "defensive", "tank", "bulky" },
        ["support"] = new[] { "utility", "setter", "pivot" },
        ["physical"] = new[] { "physical attacker", "physical sweeper" },
        ["special"] = new[] { "special attacker", "special sweeper" },
        ["mixed"] = new[] { "mixed attacker", "versatile" }
    };

    // Format keywords
    private static readonly Dictionary<string, string> FormatKeywords = new()
    {
        ["vgc"] = "VGC",
        ["doubles"] = "Doubles",
        ["singles"] = "Singles",
        ["ou"] = "OU",
        ["uu"] = "UU",
        ["uber"] = "Ubers",
        ["ag"] = "AG",
        ["monotype"] = "Monotype"
    };

    // Nature mappings based on keywords
    private static readonly Dictionary<string, Nature> NatureKeywords = new()
    {
        ["adamant"] = Nature.Adamant,
        ["jolly"] = Nature.Jolly,
        ["modest"] = Nature.Modest,
        ["timid"] = Nature.Timid,
        ["bold"] = Nature.Bold,
        ["impish"] = Nature.Impish,
        ["calm"] = Nature.Calm,
        ["careful"] = Nature.Careful,
        ["brave"] = Nature.Brave,
        ["quiet"] = Nature.Quiet,
        ["relaxed"] = Nature.Relaxed,
        ["sassy"] = Nature.Sassy,
        ["fast"] = Nature.Jolly,
        ["speedy"] = Nature.Jolly,
        ["quick"] = Nature.Jolly,
        ["strong"] = Nature.Adamant,
        ["powerful"] = Nature.Adamant,
        ["bulky"] = Nature.Bold,
        ["tanky"] = Nature.Impish,
        ["defensive"] = Nature.Bold
    };

    // Common held items
    private static readonly Dictionary<string, int> ItemKeywords = new()
    {
        ["scarf"] = 287,      // Choice Scarf
        ["choice scarf"] = 287,
        ["band"] = 220,       // Choice Band
        ["choice band"] = 220,
        ["specs"] = 297,      // Choice Specs
        ["choice specs"] = 297,
        ["leftovers"] = 234,
        ["life orb"] = 270,
        ["assault vest"] = 640,
        ["focus sash"] = 275,
        ["rocky helmet"] = 540,
        ["heavy duty boots"] = 1120,
        ["boots"] = 1120,
        ["eviolite"] = 538,
        ["black sludge"] = 281
    };

    public NaturalLanguageParser(SaveFile sav)
    {
        SAV = sav;
    }

    /// <summary>
    /// Parse natural language input and generate a Pokemon
    /// </summary>
    public NaturalLanguageResult Parse(string input)
    {
        var result = new NaturalLanguageResult();
        input = input.ToLowerInvariant();

        // Extract species name
        var species = ExtractSpecies(input);
        if (species == 0)
        {
            result.Success = false;
            result.ErrorMessage = "Could not identify Pokemon species. Please include a valid Pokemon name.";
            return result;
        }

        result.Species = species;
        result.SpeciesName = GameInfo.Strings.Species[species];

        // Check for shiny
        result.IsShiny = input.Contains("shiny") || input.Contains("star") || input.Contains("square");
        result.IsSquareShiny = input.Contains("square shiny") || input.Contains("ultra shiny");

        // Extract form
        result.Form = ExtractForm(input, species);

        // Extract nature
        result.Nature = ExtractNature(input, species);

        // Extract ability preference
        result.AbilityPreference = ExtractAbility(input, species);

        // Extract held item
        result.HeldItem = ExtractItem(input);

        // Extract level
        result.Level = ExtractLevel(input);

        // Extract IVs based on keywords
        result.IVs = ExtractIVs(input);

        // Extract EVs based on role/keywords
        result.EVs = ExtractEVs(input, species);

        // Extract moves
        result.Moves = ExtractMoves(input, species);

        // Extract ball preference
        result.Ball = ExtractBall(input);

        // Detect format/role
        result.DetectedFormat = ExtractFormat(input);
        result.DetectedRole = ExtractRole(input);

        // Generate Showdown text
        result.ShowdownText = GenerateShowdownText(result);

        result.Success = true;
        return result;
    }

    /// <summary>
    /// Generate a legal Pokemon from the parsed result
    /// </summary>
    public PKM? GeneratePokemon(NaturalLanguageResult result)
    {
        if (!result.Success || result.Species == 0)
            return null;

        var showdownText = result.ShowdownText ?? GenerateShowdownText(result);
        return ALMShowdownPlugin.ImportShowdownSetWithLegality(showdownText, SAV);
    }

    private ushort ExtractSpecies(string input)
    {
        var speciesList = GameInfo.Strings.Species;

        // Sort by length descending to match longer names first (e.g., "Mr. Mime" before "Mime")
        var sortedSpecies = speciesList
            .Select((name, index) => new { Name = name, Index = index })
            .Where(x => !string.IsNullOrEmpty(x.Name))
            .OrderByDescending(x => x.Name.Length)
            .ToList();

        foreach (var sp in sortedSpecies)
        {
            if (input.Contains(sp.Name.ToLowerInvariant()))
                return (ushort)sp.Index;
        }

        // Try common nicknames/aliases
        var aliases = GetSpeciesAliases();
        foreach (var alias in aliases)
        {
            if (input.Contains(alias.Key.ToLowerInvariant()))
                return alias.Value;
        }

        return 0;
    }

    private Dictionary<string, ushort> GetSpeciesAliases()
    {
        return new Dictionary<string, ushort>
        {
            ["chomp"] = 445,    // Garchomp
            ["lando"] = 645,    // Landorus
            ["incineroar"] = 727,
            ["incin"] = 727,
            ["rillaboom"] = 812,
            ["rilla"] = 812,
            ["zard"] = 6,       // Charizard
            ["gross"] = 376,    // Metagross
            ["ttar"] = 248,     // Tyranitar
            ["ferro"] = 598,    // Ferrothorn
            ["pex"] = 748,      // Toxapex
            ["rotom-w"] = 479,  // Rotom-Wash
            ["rotom-h"] = 479,  // Rotom-Heat
            ["weavile"] = 461,
            ["dragapult"] = 887,
            ["pult"] = 887
        };
    }

    private byte ExtractForm(string input, ushort species)
    {
        // Check for form-specific keywords
        if (input.Contains("gmax") || input.Contains("gigantamax"))
            return 0; // G-Max forms need special handling

        if (input.Contains("mega"))
            return 1; // Mega forms are usually form 1

        if (input.Contains("alolan") || input.Contains("alola"))
            return 1; // Alolan forms

        if (input.Contains("galarian") || input.Contains("galar"))
            return 1; // Galarian forms

        if (input.Contains("hisuian") || input.Contains("hisui"))
            return 1; // Hisuian forms

        if (input.Contains("paldean") || input.Contains("paldea"))
            return 1; // Paldean forms

        // Rotom forms
        if (species == 479)
        {
            if (input.Contains("wash")) return 1;
            if (input.Contains("heat")) return 2;
            if (input.Contains("frost") || input.Contains("fridge")) return 3;
            if (input.Contains("fan")) return 4;
            if (input.Contains("mow")) return 5;
        }

        return 0;
    }

    private Nature ExtractNature(string input, ushort species)
    {
        // Check for explicit nature
        foreach (var kvp in NatureKeywords)
        {
            if (input.Contains(kvp.Key))
                return kvp.Value;
        }

        // Infer from role keywords
        if (input.Contains("physical sweeper") || input.Contains("physical attacker"))
        {
            if (input.Contains("fast") || input.Contains("speed"))
                return Nature.Jolly;
            return Nature.Adamant;
        }

        if (input.Contains("special sweeper") || input.Contains("special attacker"))
        {
            if (input.Contains("fast") || input.Contains("speed"))
                return Nature.Timid;
            return Nature.Modest;
        }

        if (input.Contains("wall") || input.Contains("defensive") || input.Contains("tank"))
        {
            if (input.Contains("physical"))
                return Nature.Bold;
            if (input.Contains("special"))
                return Nature.Calm;
            return Nature.Bold;
        }

        // Default based on base stats
        return InferNatureFromBaseStats(species);
    }

    private Nature InferNatureFromBaseStats(ushort species)
    {
        var pk = SAV.BlankPKM;
        pk.Species = species;
        var pi = pk.PersonalInfo;

        if (pi.ATK > pi.SPA)
        {
            return pi.SPE >= 90 ? Nature.Jolly : Nature.Adamant;
        }
        else if (pi.SPA > pi.ATK)
        {
            return pi.SPE >= 90 ? Nature.Timid : Nature.Modest;
        }

        return Nature.Adamant; // Default
    }

    private int ExtractAbility(string input, ushort species)
    {
        var pk = SAV.BlankPKM;
        pk.Species = species;
        var pi = pk.PersonalInfo;

        // Check for "hidden ability" or "HA"
        if (input.Contains("hidden ability") || input.Contains(" ha ") || input.Contains(" ha,") || input.EndsWith(" ha"))
        {
            return pi.GetAbilityAtIndex(pi.AbilityCount - 1);
        }

        // Check for specific ability names
        var abilities = GameInfo.Strings.Ability;
        for (int i = 0; i < abilities.Count; i++)
        {
            if (!string.IsNullOrEmpty(abilities[i]) && input.Contains(abilities[i].ToLowerInvariant()))
                return i;
        }

        // Default to first ability
        return pi.GetAbilityAtIndex(0);
    }

    private int ExtractItem(string input)
    {
        foreach (var kvp in ItemKeywords)
        {
            if (input.Contains(kvp.Key))
                return kvp.Value;
        }

        // Check game item list
        var items = GameInfo.Strings.Item;
        for (int i = 0; i < items.Count; i++)
        {
            if (!string.IsNullOrEmpty(items[i]) && items[i].Length > 3 && input.Contains(items[i].ToLowerInvariant()))
                return i;
        }

        return 0; // No item
    }

    private int ExtractLevel(string input)
    {
        // Check for "level X" or "lv X" or "lvl X"
        var levelMatch = Regex.Match(input, @"(?:level|lv|lvl)[\s]*(\d+)", RegexOptions.IgnoreCase);
        if (levelMatch.Success && int.TryParse(levelMatch.Groups[1].Value, out int level))
        {
            return Math.Clamp(level, 1, 100);
        }

        // Check for "L50" style
        levelMatch = Regex.Match(input, @"l(\d+)", RegexOptions.IgnoreCase);
        if (levelMatch.Success && int.TryParse(levelMatch.Groups[1].Value, out level))
        {
            return Math.Clamp(level, 1, 100);
        }

        // VGC default is 50
        if (input.Contains("vgc") || input.Contains("doubles"))
            return 50;

        return 100; // Default to 100
    }

    private int[] ExtractIVs(string input)
    {
        var ivs = new int[] { 31, 31, 31, 31, 31, 31 }; // Default perfect IVs

        // Check for "0 speed" or "minimum speed" for Trick Room
        if (input.Contains("trick room") || input.Contains("0 speed") || input.Contains("zero speed") || input.Contains("min speed"))
        {
            ivs[5] = 0; // 0 Speed IV
        }

        // Check for "0 attack" (for special attackers to reduce confusion damage)
        if (input.Contains("0 attack") || input.Contains("zero attack") || input.Contains("0 atk"))
        {
            ivs[1] = 0; // 0 Atk IV
        }

        // Check for specific IV values
        var ivMatch = Regex.Match(input, @"(\d+)\s*(?:iv|ivs)?[\s]*(?:in\s+)?(\w+)", RegexOptions.IgnoreCase);
        if (ivMatch.Success)
        {
            var value = int.Parse(ivMatch.Groups[1].Value);
            var stat = ivMatch.Groups[2].Value.ToLower();

            int index = stat switch
            {
                "hp" => 0,
                "atk" or "attack" => 1,
                "def" or "defense" => 2,
                "spa" or "spatk" or "special attack" => 3,
                "spd" or "spdef" or "special defense" => 4,
                "spe" or "speed" => 5,
                _ => -1
            };

            if (index >= 0)
                ivs[index] = Math.Clamp(value, 0, 31);
        }

        return ivs;
    }

    private int[] ExtractEVs(string input, ushort species)
    {
        var evs = new int[6];

        // Check for explicit EV spreads like "252 Atk / 252 Spe / 4 HP"
        var evPattern = @"(\d+)\s*(?:ev|evs)?[\s]*(?:in\s+)?(hp|atk|attack|def|defense|spa|spatk|spd|spdef|spe|speed)";
        var matches = Regex.Matches(input, evPattern, RegexOptions.IgnoreCase);

        if (matches.Count > 0)
        {
            foreach (Match match in matches)
            {
                var value = int.Parse(match.Groups[1].Value);
                var stat = match.Groups[2].Value.ToLower();

                int index = stat switch
                {
                    "hp" => 0,
                    "atk" or "attack" => 1,
                    "def" or "defense" => 2,
                    "spa" or "spatk" => 3,
                    "spd" or "spdef" => 4,
                    "spe" or "speed" => 5,
                    _ => -1
                };

                if (index >= 0)
                    evs[index] = Math.Clamp(value, 0, 252);
            }
        }
        else
        {
            // Infer EVs from role
            evs = InferEVsFromRole(input, species);
        }

        return evs;
    }

    private int[] InferEVsFromRole(string input, ushort species)
    {
        var pk = SAV.BlankPKM;
        pk.Species = species;
        var pi = pk.PersonalInfo;

        // Physical sweeper
        if (input.Contains("sweeper") && (input.Contains("physical") || pi.ATK > pi.SPA))
        {
            return new[] { 4, 252, 0, 0, 0, 252 }; // 4 HP / 252 Atk / 252 Spe
        }

        // Special sweeper
        if (input.Contains("sweeper") && (input.Contains("special") || pi.SPA > pi.ATK))
        {
            return new[] { 4, 0, 0, 252, 0, 252 }; // 4 HP / 252 SpA / 252 Spe
        }

        // Wall/Tank
        if (input.Contains("wall") || input.Contains("tank") || input.Contains("defensive"))
        {
            if (input.Contains("physical"))
                return new[] { 252, 0, 252, 0, 4, 0 }; // 252 HP / 252 Def / 4 SpD
            if (input.Contains("special"))
                return new[] { 252, 0, 4, 0, 252, 0 }; // 252 HP / 4 Def / 252 SpD
            return new[] { 252, 0, 128, 0, 128, 0 }; // Mixed bulk
        }

        // VGC/Doubles tends to run bulk
        if (input.Contains("vgc") || input.Contains("doubles"))
        {
            if (pi.ATK > pi.SPA)
                return new[] { 4, 252, 0, 0, 0, 252 };
            return new[] { 4, 0, 0, 252, 0, 252 };
        }

        // Default based on stats
        if (pi.ATK > pi.SPA)
            return new[] { 4, 252, 0, 0, 0, 252 };
        return new[] { 4, 0, 0, 252, 0, 252 };
    }

    private ushort[] ExtractMoves(string input, ushort species)
    {
        var moves = new List<ushort>();
        var moveList = GameInfo.Strings.Move;

        // Check for specific move names in input
        foreach (var move in moveList.Select((name, index) => new { Name = name, Index = index }))
        {
            if (!string.IsNullOrEmpty(move.Name) && move.Name.Length > 3)
            {
                if (input.Contains(move.Name.ToLowerInvariant()))
                    moves.Add((ushort)move.Index);
            }
        }

        // If no moves found, will rely on ALM to provide moves
        return moves.Take(4).ToArray();
    }

    private int ExtractBall(string input)
    {
        var ballKeywords = new Dictionary<string, int>
        {
            ["master ball"] = 1,
            ["ultra ball"] = 2,
            ["great ball"] = 3,
            ["poke ball"] = 4,
            ["safari ball"] = 5,
            ["net ball"] = 6,
            ["dive ball"] = 7,
            ["nest ball"] = 8,
            ["repeat ball"] = 9,
            ["timer ball"] = 10,
            ["luxury ball"] = 11,
            ["premier ball"] = 12,
            ["dusk ball"] = 13,
            ["heal ball"] = 14,
            ["quick ball"] = 15,
            ["cherish ball"] = 16,
            ["fast ball"] = 17,
            ["level ball"] = 18,
            ["lure ball"] = 19,
            ["heavy ball"] = 20,
            ["love ball"] = 21,
            ["friend ball"] = 22,
            ["moon ball"] = 23,
            ["sport ball"] = 24,
            ["dream ball"] = 25,
            ["beast ball"] = 26
        };

        foreach (var kvp in ballKeywords)
        {
            if (input.Contains(kvp.Key))
                return kvp.Value;
        }

        return 4; // Default Poke Ball
    }

    private string ExtractFormat(string input)
    {
        foreach (var kvp in FormatKeywords)
        {
            if (input.Contains(kvp.Key))
                return kvp.Value;
        }
        return "OU"; // Default
    }

    private string ExtractRole(string input)
    {
        if (input.Contains("sweeper"))
            return "Sweeper";
        if (input.Contains("wall") || input.Contains("tank"))
            return "Wall";
        if (input.Contains("support") || input.Contains("setter"))
            return "Support";
        if (input.Contains("pivot"))
            return "Pivot";
        if (input.Contains("revenge killer"))
            return "Revenge Killer";
        if (input.Contains("breaker") || input.Contains("wallbreaker"))
            return "Wallbreaker";
        return "Attacker";
    }

    private string GenerateShowdownText(NaturalLanguageResult result)
    {
        var sb = new StringBuilder();
        var speciesName = result.SpeciesName;

        // Handle nicknames
        if (!string.IsNullOrEmpty(result.Nickname))
            sb.Append($"{result.Nickname} ({speciesName})");
        else
            sb.Append(speciesName);

        // Add item
        if (result.HeldItem > 0)
        {
            var itemName = GameInfo.Strings.Item[result.HeldItem];
            sb.Append($" @ {itemName}");
        }

        sb.AppendLine();

        // Ability
        if (result.AbilityPreference > 0)
        {
            var abilityName = GameInfo.Strings.Ability[result.AbilityPreference];
            sb.AppendLine($"Ability: {abilityName}");
        }

        // Level (if not 100)
        if (result.Level != 100)
            sb.AppendLine($"Level: {result.Level}");

        // Shiny
        if (result.IsShiny)
            sb.AppendLine("Shiny: Yes");

        // EVs
        if (result.EVs != null && result.EVs.Sum() > 0)
        {
            var evParts = new List<string>();
            var statNames = new[] { "HP", "Atk", "Def", "SpA", "SpD", "Spe" };
            for (int i = 0; i < 6; i++)
            {
                if (result.EVs[i] > 0)
                    evParts.Add($"{result.EVs[i]} {statNames[i]}");
            }
            if (evParts.Count > 0)
                sb.AppendLine($"EVs: {string.Join(" / ", evParts)}");
        }

        // Nature
        var natureName = GameInfo.Strings.Natures[(int)result.Nature];
        sb.AppendLine($"{natureName} Nature");

        // IVs (only if not all 31)
        if (result.IVs != null && result.IVs.Any(iv => iv != 31))
        {
            var ivParts = new List<string>();
            var statNames = new[] { "HP", "Atk", "Def", "SpA", "SpD", "Spe" };
            for (int i = 0; i < 6; i++)
            {
                if (result.IVs[i] != 31)
                    ivParts.Add($"{result.IVs[i]} {statNames[i]}");
            }
            if (ivParts.Count > 0)
                sb.AppendLine($"IVs: {string.Join(" / ", ivParts)}");
        }

        // Moves
        if (result.Moves != null && result.Moves.Length > 0)
        {
            foreach (var move in result.Moves)
            {
                if (move > 0)
                {
                    var moveName = GameInfo.Strings.Move[move];
                    sb.AppendLine($"- {moveName}");
                }
            }
        }

        return sb.ToString();
    }
}

/// <summary>
/// Result of natural language parsing
/// </summary>
public class NaturalLanguageResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    public ushort Species { get; set; }
    public string SpeciesName { get; set; } = "";
    public string? Nickname { get; set; }
    public byte Form { get; set; }
    public Nature Nature { get; set; }
    public int AbilityPreference { get; set; }
    public int HeldItem { get; set; }
    public int Level { get; set; } = 100;
    public int[] IVs { get; set; } = new[] { 31, 31, 31, 31, 31, 31 };
    public int[] EVs { get; set; } = new int[6];
    public ushort[] Moves { get; set; } = Array.Empty<ushort>();
    public int Ball { get; set; }

    public bool IsShiny { get; set; }
    public bool IsSquareShiny { get; set; }

    public string DetectedFormat { get; set; } = "";
    public string DetectedRole { get; set; } = "";

    public string? ShowdownText { get; set; }

    public string GetSummary()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Species: {SpeciesName}");
        sb.AppendLine($"Nature: {Nature}");
        sb.AppendLine($"Level: {Level}");
        sb.AppendLine($"Shiny: {(IsShiny ? "Yes" : "No")}");
        sb.AppendLine($"Format: {DetectedFormat}");
        sb.AppendLine($"Role: {DetectedRole}");
        return sb.ToString();
    }
}
