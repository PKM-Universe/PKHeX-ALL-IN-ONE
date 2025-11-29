using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PKHeX.Core;

namespace PKHeX.WinForms.Plugins;

/// <summary>
/// Showdown Exporter - Export Pokemon to Pokemon Showdown format
/// Supports single Pokemon, full boxes, party, and entire save files
/// </summary>
public class ShowdownExporter
{
    private readonly SaveFile SAV;

    public ShowdownExporter(SaveFile sav)
    {
        SAV = sav;
    }

    /// <summary>
    /// Export a single Pokemon to Showdown format
    /// </summary>
    public string ExportPokemon(PKM pk)
    {
        if (pk.Species == 0) return "";
        var set = new ShowdownSet(pk);
        return set.Text;
    }

    /// <summary>
    /// Export party to Showdown format
    /// </summary>
    public string ExportParty()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Party Pokemon ===");
        sb.AppendLine();

        foreach (var pk in SAV.PartyData)
        {
            if (pk.Species == 0) continue;
            sb.AppendLine(ExportPokemon(pk));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Export a box to Showdown format
    /// </summary>
    public string ExportBox(int box, string boxName = "")
    {
        var sb = new StringBuilder();
        var name = string.IsNullOrEmpty(boxName) ? $"Box {box + 1}" : boxName;
        sb.AppendLine($"=== {name} ===");
        sb.AppendLine();

        var pokemon = SAV.GetBoxData(box);
        foreach (var pk in pokemon)
        {
            if (pk.Species == 0) continue;
            sb.AppendLine(ExportPokemon(pk));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Export multiple boxes to Showdown format
    /// </summary>
    public string ExportBoxes(int startBox, int endBox)
    {
        var sb = new StringBuilder();

        for (int i = startBox; i <= endBox && i < SAV.BoxCount; i++)
        {
            var boxContent = ExportBox(i);
            if (!string.IsNullOrWhiteSpace(boxContent))
            {
                sb.AppendLine(boxContent);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Export all boxes to Showdown format
    /// </summary>
    public string ExportAllBoxes()
    {
        return ExportBoxes(0, SAV.BoxCount - 1);
    }

    /// <summary>
    /// Export to file
    /// </summary>
    public void ExportToFile(string filePath, string content)
    {
        File.WriteAllText(filePath, content);
    }

    /// <summary>
    /// Export party to file
    /// </summary>
    public void ExportPartyToFile(string filePath)
    {
        ExportToFile(filePath, ExportParty());
    }

    /// <summary>
    /// Export box to file
    /// </summary>
    public void ExportBoxToFile(int box, string filePath)
    {
        ExportToFile(filePath, ExportBox(box));
    }

    /// <summary>
    /// Generate a shareable team link (simulated - would need Showdown API)
    /// </summary>
    public string GenerateShareableTeam(IList<PKM> team)
    {
        var sb = new StringBuilder();

        foreach (var pk in team.Take(6))
        {
            if (pk.Species == 0) continue;
            sb.AppendLine(ExportPokemon(pk));
            sb.AppendLine();
        }

        // In a real implementation, this would upload to Showdown and return a pokepast.es link
        return sb.ToString();
    }

    /// <summary>
    /// Export with custom formatting options
    /// </summary>
    public string ExportWithOptions(PKM pk, ShowdownExportOptions options)
    {
        if (pk.Species == 0) return "";

        var sb = new StringBuilder();

        // Species line
        var speciesName = SpeciesName.GetSpeciesName(pk.Species, 2);
        var formName = GetFormName(pk);

        if (!string.IsNullOrEmpty(options.Nickname) && options.Nickname != speciesName)
            sb.Append($"{options.Nickname} ({speciesName}");
        else
            sb.Append(speciesName);

        if (!string.IsNullOrEmpty(formName))
            sb.Append($"-{formName}");

        if (options.Gender.HasValue)
        {
            if (options.Gender == 0) sb.Append(" (M)");
            else if (options.Gender == 1) sb.Append(" (F)");
        }

        if (options.IncludeItem && pk.HeldItem > 0)
        {
            var itemName = GetItemName(pk.HeldItem);
            sb.Append($" @ {itemName}");
        }

        sb.AppendLine();

        // Ability
        if (options.IncludeAbility)
        {
            var abilityName = GetAbilityName(pk.Ability);
            sb.AppendLine($"Ability: {abilityName}");
        }

        // Level
        if (options.IncludeLevel && pk.CurrentLevel != 100)
        {
            sb.AppendLine($"Level: {pk.CurrentLevel}");
        }

        // Shiny
        if (options.IncludeShiny && pk.IsShiny)
        {
            sb.AppendLine("Shiny: Yes");
        }

        // Tera Type (Gen 9)
        if (options.IncludeTeraType && pk is PK9 pk9)
        {
            sb.AppendLine($"Tera Type: {GetTeraTypeName(pk9.TeraTypeOriginal)}");
        }

        // EVs
        if (options.IncludeEVs)
        {
            var evs = new List<string>();
            if (pk.EV_HP > 0) evs.Add($"{pk.EV_HP} HP");
            if (pk.EV_ATK > 0) evs.Add($"{pk.EV_ATK} Atk");
            if (pk.EV_DEF > 0) evs.Add($"{pk.EV_DEF} Def");
            if (pk.EV_SPA > 0) evs.Add($"{pk.EV_SPA} SpA");
            if (pk.EV_SPD > 0) evs.Add($"{pk.EV_SPD} SpD");
            if (pk.EV_SPE > 0) evs.Add($"{pk.EV_SPE} Spe");

            if (evs.Count > 0)
                sb.AppendLine($"EVs: {string.Join(" / ", evs)}");
        }

        // Nature
        if (options.IncludeNature)
        {
            var natureName = GetNatureName(pk.Nature);
            sb.AppendLine($"{natureName} Nature");
        }

        // IVs
        if (options.IncludeIVs)
        {
            var ivs = new List<string>();
            if (pk.IV_HP < 31) ivs.Add($"{pk.IV_HP} HP");
            if (pk.IV_ATK < 31) ivs.Add($"{pk.IV_ATK} Atk");
            if (pk.IV_DEF < 31) ivs.Add($"{pk.IV_DEF} Def");
            if (pk.IV_SPA < 31) ivs.Add($"{pk.IV_SPA} SpA");
            if (pk.IV_SPD < 31) ivs.Add($"{pk.IV_SPD} SpD");
            if (pk.IV_SPE < 31) ivs.Add($"{pk.IV_SPE} Spe");

            if (ivs.Count > 0)
                sb.AppendLine($"IVs: {string.Join(" / ", ivs)}");
        }

        // Moves
        if (options.IncludeMoves)
        {
            if (pk.Move1 > 0) sb.AppendLine($"- {GetMoveName(pk.Move1)}");
            if (pk.Move2 > 0) sb.AppendLine($"- {GetMoveName(pk.Move2)}");
            if (pk.Move3 > 0) sb.AppendLine($"- {GetMoveName(pk.Move3)}");
            if (pk.Move4 > 0) sb.AppendLine($"- {GetMoveName(pk.Move4)}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Batch export selected Pokemon
    /// </summary>
    public string ExportSelected(IEnumerable<PKM> pokemon, ShowdownExportOptions? options = null)
    {
        var sb = new StringBuilder();

        foreach (var pk in pokemon)
        {
            if (pk.Species == 0) continue;

            if (options != null)
                sb.AppendLine(ExportWithOptions(pk, options));
            else
                sb.AppendLine(ExportPokemon(pk));

            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Export as PokePaste format (for sharing online)
    /// </summary>
    public string ExportAsPokePaste(IList<PKM> team)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== [Gen 9] Untitled ===");
        sb.AppendLine();

        foreach (var pk in team.Take(6))
        {
            if (pk.Species == 0) continue;
            sb.AppendLine(ExportPokemon(pk));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Export team summary for competitive use
    /// </summary>
    public string ExportTeamSummary(IList<PKM> team)
    {
        var sb = new StringBuilder();
        sb.AppendLine("╔══════════════════════════════════════════════════════════════╗");
        sb.AppendLine("║                     TEAM EXPORT SUMMARY                      ║");
        sb.AppendLine("╠══════════════════════════════════════════════════════════════╣");

        foreach (var pk in team.Take(6))
        {
            if (pk.Species == 0) continue;

            var name = SpeciesName.GetSpeciesName(pk.Species, 2);
            var item = pk.HeldItem > 0 ? GetItemName(pk.HeldItem) : "None";
            var ability = GetAbilityName(pk.Ability);

            sb.AppendLine($"║ {name,-20} @ {item,-15}");
            sb.AppendLine($"║   Ability: {ability,-15} Nature: {GetNatureName(pk.Nature)}");
            sb.AppendLine($"║   EVs: {pk.EV_HP}/{pk.EV_ATK}/{pk.EV_DEF}/{pk.EV_SPA}/{pk.EV_SPD}/{pk.EV_SPE}");
            sb.AppendLine($"║   Moves: {GetMoveName(pk.Move1)}, {GetMoveName(pk.Move2)}, {GetMoveName(pk.Move3)}, {GetMoveName(pk.Move4)}");
            sb.AppendLine("╠──────────────────────────────────────────────────────────────╣");
        }

        sb.AppendLine("╚══════════════════════════════════════════════════════════════╝");
        return sb.ToString();
    }

    // Helper methods
    private string GetFormName(PKM pk)
    {
        if (pk.Form == 0) return "";

        // Handle common form names
        return pk.Species switch
        {
            201 => ((char)('A' + pk.Form)).ToString(), // Unown
            386 => pk.Form switch { 1 => "Attack", 2 => "Defense", 3 => "Speed", _ => "" }, // Deoxys
            413 => pk.Form switch { 1 => "Sandy", 2 => "Trash", _ => "Plant" }, // Wormadam
            479 => pk.Form switch { 1 => "Heat", 2 => "Wash", 3 => "Frost", 4 => "Fan", 5 => "Mow", _ => "" }, // Rotom
            487 => pk.Form == 1 ? "Origin" : "", // Giratina
            492 => pk.Form == 1 ? "Sky" : "", // Shaymin
            641 or 642 or 645 => pk.Form == 1 ? "Therian" : "Incarnate", // Genies
            646 => pk.Form switch { 1 => "White", 2 => "Black", _ => "" }, // Kyurem
            718 => pk.Form switch { 1 => "10%", 2 => "10%", 3 => "Complete", _ => "" }, // Zygarde
            720 => pk.Form == 1 ? "Unbound" : "Confined", // Hoopa
            745 => pk.Form switch { 1 => "Midnight", 2 => "Dusk", _ => "Midday" }, // Lycanroc
            800 => pk.Form switch { 1 => "Dusk-Mane", 2 => "Dawn-Wings", 3 => "Ultra", _ => "" }, // Necrozma
            849 => pk.Form == 1 ? "Low-Key" : "Amped", // Toxtricity
            892 => pk.Form == 1 ? "Rapid-Strike" : "Single-Strike", // Urshifu
            898 => pk.Form switch { 1 => "Ice", 2 => "Shadow", _ => "" }, // Calyrex
            1017 => pk.Form switch { 1 => "Wellspring", 2 => "Hearthflame", 3 => "Cornerstone", _ => "Teal" }, // Ogerpon
            _ => pk.Form > 0 ? pk.Form.ToString() : ""
        };
    }

    private string GetItemName(int item)
    {
        // Common competitive items
        return item switch
        {
            1 => "Master Ball",
            4 => "Poke Ball",
            156 => "Leftovers",
            184 => "Choice Band",
            185 => "Exp. Share",
            188 => "Assault Vest",
            197 => "Choice Specs",
            220 => "Black Sludge",
            229 => "Focus Sash",
            235 => "Air Balloon",
            242 => "Eviolite",
            247 => "Rocky Helmet",
            249 => "Choice Scarf",
            275 => "Life Orb",
            276 => "Power Herb",
            289 => "Flame Orb",
            290 => "Toxic Orb",
            636 => "Weakness Policy",
            639 => "Expert Belt",
            1178 => "Loaded Dice",
            1879 => "Booster Energy",
            1881 => "Clear Amulet",
            1882 => "Mirror Herb",
            1883 => "Punching Glove",
            1884 => "Covert Cloak",
            _ => $"Item_{item}"
        };
    }

    private string GetAbilityName(int ability)
    {
        // Return ability name - would use AbilityNames in real implementation
        return Ability.GetAbilityName(ability, 2);
    }

    private string GetMoveName(int move)
    {
        if (move == 0) return "None";
        return MoveInfo.GetMoveName(move, 2);
    }

    private string GetNatureName(Nature nature)
    {
        return nature.ToString();
    }

    private string GetTeraTypeName(MoveType type)
    {
        return type.ToString();
    }
}

public class ShowdownExportOptions
{
    public string? Nickname { get; set; }
    public byte? Gender { get; set; }
    public bool IncludeItem { get; set; } = true;
    public bool IncludeAbility { get; set; } = true;
    public bool IncludeLevel { get; set; } = true;
    public bool IncludeShiny { get; set; } = true;
    public bool IncludeTeraType { get; set; } = true;
    public bool IncludeEVs { get; set; } = true;
    public bool IncludeIVs { get; set; } = true;
    public bool IncludeNature { get; set; } = true;
    public bool IncludeMoves { get; set; } = true;

    public static ShowdownExportOptions Default => new();

    public static ShowdownExportOptions Minimal => new()
    {
        IncludeItem = true,
        IncludeAbility = true,
        IncludeLevel = false,
        IncludeShiny = false,
        IncludeTeraType = false,
        IncludeEVs = false,
        IncludeIVs = false,
        IncludeNature = true,
        IncludeMoves = true
    };

    public static ShowdownExportOptions Competitive => new()
    {
        IncludeItem = true,
        IncludeAbility = true,
        IncludeLevel = false,
        IncludeShiny = false,
        IncludeTeraType = true,
        IncludeEVs = true,
        IncludeIVs = true,
        IncludeNature = true,
        IncludeMoves = true
    };
}

// Helper class for ability names (simplified)
public static class Ability
{
    private static readonly string[] AbilityNames = new[]
    {
        "None", "Stench", "Drizzle", "Speed Boost", "Battle Armor", "Sturdy", "Damp", "Limber",
        "Sand Veil", "Static", "Volt Absorb", "Water Absorb", "Oblivious", "Cloud Nine", "Compound Eyes",
        "Insomnia", "Color Change", "Immunity", "Flash Fire", "Shield Dust", "Own Tempo", "Suction Cups",
        "Intimidate", "Shadow Tag", "Rough Skin", "Wonder Guard", "Levitate", "Effect Spore", "Synchronize",
        "Clear Body", "Natural Cure", "Lightning Rod", "Serene Grace", "Swift Swim", "Chlorophyll",
        "Illuminate", "Trace", "Huge Power", "Poison Point", "Inner Focus", "Magma Armor", "Water Veil",
        "Magnet Pull", "Soundproof", "Rain Dish", "Sand Stream", "Pressure", "Thick Fat", "Early Bird",
        "Flame Body", "Run Away", "Keen Eye", "Hyper Cutter", "Pickup", "Truant", "Hustle", "Cute Charm",
        "Plus", "Minus", "Forecast", "Sticky Hold", "Shed Skin", "Guts", "Marvel Scale", "Liquid Ooze",
        "Overgrow", "Blaze", "Torrent", "Swarm", "Rock Head", "Drought", "Arena Trap", "Vital Spirit",
        "White Smoke", "Pure Power", "Shell Armor", "Air Lock", "Tangled Feet", "Motor Drive", "Rivalry",
        "Steadfast", "Snow Cloak", "Gluttony", "Anger Point", "Unburden", "Heatproof", "Simple",
        "Dry Skin", "Download", "Iron Fist", "Poison Heal", "Adaptability", "Skill Link", "Hydration",
        "Solar Power", "Quick Feet", "Normalize", "Sniper", "Magic Guard", "No Guard", "Stall",
        "Technician", "Leaf Guard", "Klutz", "Mold Breaker", "Super Luck", "Aftermath", "Anticipation",
        "Forewarn", "Unaware", "Tinted Lens", "Filter", "Slow Start", "Scrappy", "Storm Drain",
        "Ice Body", "Solid Rock", "Snow Warning", "Honey Gather", "Frisk", "Reckless", "Multitype",
        "Flower Gift", "Bad Dreams", "Pickpocket", "Sheer Force", "Contrary", "Unnerve", "Defiant",
        "Defeatist", "Cursed Body", "Healer", "Friend Guard", "Weak Armor", "Heavy Metal", "Light Metal",
        "Multiscale", "Toxic Boost", "Flare Boost", "Harvest", "Telepathy", "Moody", "Overcoat",
        "Poison Touch", "Regenerator", "Big Pecks", "Sand Rush", "Wonder Skin", "Analytic", "Illusion",
        "Imposter", "Infiltrator", "Mummy", "Moxie", "Justified", "Rattled", "Magic Bounce", "Sap Sipper",
        "Prankster", "Sand Force", "Iron Barbs", "Zen Mode", "Victory Star", "Turboblaze", "Teravolt",
        "Aroma Veil", "Flower Veil", "Cheek Pouch", "Protean", "Fur Coat", "Magician", "Bulletproof",
        "Competitive", "Strong Jaw", "Refrigerate", "Sweet Veil", "Stance Change", "Gale Wings",
        "Mega Launcher", "Grass Pelt", "Symbiosis", "Tough Claws", "Pixilate", "Gooey", "Aerilate",
        "Parental Bond", "Dark Aura", "Fairy Aura", "Aura Break", "Primordial Sea", "Desolate Land",
        "Delta Stream", "Stamina", "Wimp Out", "Emergency Exit", "Water Compaction", "Merciless",
        "Shields Down", "Stakeout", "Water Bubble", "Steelworker", "Berserk", "Slush Rush",
        "Long Reach", "Liquid Voice", "Triage", "Galvanize", "Surge Surfer", "Schooling", "Disguise",
        "Battle Bond", "Power Construct", "Corrosion", "Comatose", "Queenly Majesty", "Innards Out",
        "Dancer", "Battery", "Fluffy", "Dazzling", "Soul-Heart", "Tangling Hair", "Receiver",
        "Power of Alchemy", "Beast Boost", "RKS System", "Electric Surge", "Psychic Surge",
        "Misty Surge", "Grassy Surge", "Full Metal Body", "Shadow Shield", "Prism Armor",
        "Neuroforce", "Intrepid Sword", "Dauntless Shield", "Libero", "Ball Fetch", "Cotton Down",
        "Propeller Tail", "Mirror Armor", "Gulp Missile", "Stalwart", "Steam Engine", "Punk Rock",
        "Sand Spit", "Ice Scales", "Ripen", "Ice Face", "Power Spot", "Mimicry", "Screen Cleaner",
        "Steely Spirit", "Perish Body", "Wandering Spirit", "Gorilla Tactics", "Neutralizing Gas",
        "Pastel Veil", "Hunger Switch", "Quick Draw", "Unseen Fist", "Curious Medicine",
        "Transistor", "Dragon's Maw", "Chilling Neigh", "Grim Neigh", "As One (Glastrier)",
        "As One (Spectrier)", "Lingering Aroma", "Seed Sower", "Thermal Exchange", "Anger Shell",
        "Purifying Salt", "Well-Baked Body", "Wind Rider", "Guard Dog", "Rocky Payload",
        "Wind Power", "Zero to Hero", "Commander", "Electromorphosis", "Protosynthesis",
        "Quark Drive", "Good as Gold", "Vessel of Ruin", "Sword of Ruin", "Tablets of Ruin",
        "Beads of Ruin", "Orichalcum Pulse", "Hadron Engine", "Opportunist", "Cud Chew",
        "Sharpness", "Supreme Overlord", "Costar", "Toxic Debris", "Armor Tail", "Earth Eater",
        "Mycelium Might", "Minds Eye", "Supersweet Syrup", "Hospitality", "Toxic Chain",
        "Embody Aspect", "Tera Shift", "Tera Shell", "Teraform Zero", "Poison Puppeteer"
    };

    public static string GetAbilityName(int ability, int language)
    {
        if (ability >= 0 && ability < AbilityNames.Length)
            return AbilityNames[ability];
        return $"Ability_{ability}";
    }
}

// Helper class for move info
public static class MoveInfo
{
    public static string GetMoveName(int move, int language)
    {
        // In real implementation would use PKHeX.Core move name tables
        return $"Move_{move}";
    }

    public static byte GetType(int move, EntityContext context)
    {
        // Simplified - would use actual move data
        return 0;
    }
}
