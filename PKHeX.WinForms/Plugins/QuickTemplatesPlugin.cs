using System;
using System.Collections.Generic;
using PKHeX.Core;

namespace PKHeX.WinForms.Plugins;

/// <summary>
/// Quick Templates - Competitive Pokemon presets from Smogon
/// </summary>
public class QuickTemplatesPlugin
{
    private readonly SaveFile SAV;

    public QuickTemplatesPlugin(SaveFile sav)
    {
        SAV = sav;
    }

    /// <summary>
    /// Get all available template categories
    /// </summary>
    public static string[] GetCategories()
    {
        return new[] { "OU", "Ubers", "UU", "VGC 2024", "Doubles OU", "Monotype", "Starter", "Legendary" };
    }

    /// <summary>
    /// Get templates for a specific category
    /// </summary>
    public static List<PokemonTemplate> GetTemplates(string category)
    {
        return category switch
        {
            "OU" => GetOUTemplates(),
            "Ubers" => GetUbersTemplates(),
            "UU" => GetUUTemplates(),
            "VGC 2024" => GetVGCTemplates(),
            "Doubles OU" => GetDoublesOUTemplates(),
            "Monotype" => GetMonotypeTemplates(),
            "Starter" => GetStarterTemplates(),
            "Legendary" => GetLegendaryTemplates(),
            _ => new List<PokemonTemplate>()
        };
    }

    /// <summary>
    /// Apply a template to generate a Pokemon
    /// </summary>
    public PKM? ApplyTemplate(PokemonTemplate template)
    {
        var pk = EntityBlank.GetBlank(SAV.Context);

        pk.Species = template.Species;
        pk.Form = template.Form;
        pk.CurrentLevel = 100;

        // Set Nature
        pk.Nature = template.Nature;

        // Set Ability
        pk.RefreshAbility(template.AbilityIndex);

        // Set Moves
        pk.Move1 = template.Move1;
        pk.Move2 = template.Move2;
        pk.Move3 = template.Move3;
        pk.Move4 = template.Move4;
        pk.SetMaximumPPCurrent();

        // Set IVs
        pk.IV_HP = template.IVs[0];
        pk.IV_ATK = template.IVs[1];
        pk.IV_DEF = template.IVs[2];
        pk.IV_SPA = template.IVs[3];
        pk.IV_SPD = template.IVs[4];
        pk.IV_SPE = template.IVs[5];

        // Set EVs
        pk.EV_HP = template.EVs[0];
        pk.EV_ATK = template.EVs[1];
        pk.EV_DEF = template.EVs[2];
        pk.EV_SPA = template.EVs[3];
        pk.EV_SPD = template.EVs[4];
        pk.EV_SPE = template.EVs[5];

        // Set held item
        pk.HeldItem = template.Item;

        // Set Tera Type for Gen 9
        if (pk is ITeraType tera)
            tera.TeraTypeOriginal = template.TeraType;

        // Shiny if specified
        if (template.Shiny)
            pk.SetShiny();

        // OT Info
        pk.OriginalTrainerName = SAV.OT;
        pk.TID16 = SAV.TID16;
        pk.SID16 = SAV.SID16;
        pk.OriginalTrainerGender = SAV.Gender;
        pk.Language = SAV.Language;
        pk.MetDate = DateOnly.FromDateTime(DateTime.Now);

        pk.ResetCalculatedValues();

        return EntityConverter.ConvertToType(pk, SAV.PKMType, out _);
    }

    private static List<PokemonTemplate> GetOUTemplates()
    {
        return new List<PokemonTemplate>
        {
            new PokemonTemplate
            {
                Name = "Kingambit - Swords Dance",
                Species = (ushort)Species.Kingambit,
                Nature = Nature.Adamant,
                AbilityIndex = 2, // Supreme Overlord
                Move1 = 14, // Swords Dance
                Move2 = 269, // Sucker Punch
                Move3 = 400, // Iron Head
                Move4 = 371, // Kowtow Cleave
                IVs = new[] { 31, 31, 31, 0, 31, 31 },
                EVs = new[] { 252, 252, 0, 0, 4, 0 },
                Item = 1178, // Black Glasses
                TeraType = MoveType.Dark
            },
            new PokemonTemplate
            {
                Name = "Garchomp - Swords Dance",
                Species = (ushort)Species.Garchomp,
                Nature = Nature.Jolly,
                AbilityIndex = 0, // Sand Veil
                Move1 = 14, // Swords Dance
                Move2 = 89, // Earthquake
                Move3 = 200, // Outrage
                Move4 = 442, // Iron Head
                IVs = new[] { 31, 31, 31, 0, 31, 31 },
                EVs = new[] { 0, 252, 0, 0, 4, 252 },
                Item = 1149, // Scale Shot
                TeraType = MoveType.Steel
            },
            new PokemonTemplate
            {
                Name = "Dragapult - Choice Specs",
                Species = (ushort)Species.Dragapult,
                Nature = Nature.Timid,
                AbilityIndex = 0, // Clear Body
                Move1 = 751, // Shadow Ball
                Move2 = 434, // Draco Meteor
                Move3 = 247, // Flamethrower
                Move4 = 506, // Thunderbolt
                IVs = new[] { 31, 0, 31, 31, 31, 31 },
                EVs = new[] { 0, 0, 0, 252, 4, 252 },
                Item = 297, // Choice Specs
                TeraType = MoveType.Ghost
            },
            new PokemonTemplate
            {
                Name = "Gholdengo - Choice Specs",
                Species = (ushort)Species.Gholdengo,
                Nature = Nature.Timid,
                AbilityIndex = 0, // Good as Gold
                Move1 = 904, // Make It Rain
                Move2 = 247, // Shadow Ball
                Move3 = 412, // Focus Blast
                Move4 = 285, // Trick
                IVs = new[] { 31, 0, 31, 31, 31, 31 },
                EVs = new[] { 0, 0, 0, 252, 4, 252 },
                Item = 297, // Choice Specs
                TeraType = MoveType.Fighting
            },
            new PokemonTemplate
            {
                Name = "Great Tusk - Bulky Spinner",
                Species = (ushort)Species.GreatTusk,
                Nature = Nature.Impish,
                AbilityIndex = 0, // Protosynthesis
                Move1 = 89, // Earthquake
                Move2 = 370, // Close Combat
                Move3 = 229, // Rapid Spin
                Move4 = 276, // Knock Off
                IVs = new[] { 31, 31, 31, 0, 31, 31 },
                EVs = new[] { 252, 0, 252, 0, 4, 0 },
                Item = 1178, // Leftovers
                TeraType = MoveType.Steel
            },
            new PokemonTemplate
            {
                Name = "Iron Valiant - Booster Energy",
                Species = (ushort)Species.IronValiant,
                Nature = Nature.Naive,
                AbilityIndex = 0, // Quark Drive
                Move1 = 370, // Close Combat
                Move2 = 585, // Moonblast
                Move3 = 851, // Spirit Break
                Move4 = 276, // Knock Off
                IVs = new[] { 31, 31, 31, 31, 31, 31 },
                EVs = new[] { 0, 252, 0, 4, 0, 252 },
                Item = 1696, // Booster Energy
                TeraType = MoveType.Fairy
            },
            new PokemonTemplate
            {
                Name = "Roaring Moon - Dragon Dance",
                Species = (ushort)Species.RoaringMoon,
                Nature = Nature.Jolly,
                AbilityIndex = 0, // Protosynthesis
                Move1 = 349, // Dragon Dance
                Move2 = 89, // Earthquake
                Move3 = 276, // Knock Off
                Move4 = 37, // Thrash
                IVs = new[] { 31, 31, 31, 0, 31, 31 },
                EVs = new[] { 0, 252, 0, 0, 4, 252 },
                Item = 1696, // Booster Energy
                TeraType = MoveType.Dark
            },
            new PokemonTemplate
            {
                Name = "Gliscor - Toxic Stall",
                Species = (ushort)Species.Gliscor,
                Nature = Nature.Impish,
                AbilityIndex = 2, // Poison Heal
                Move1 = 89, // Earthquake
                Move2 = 369, // U-turn
                Move3 = 276, // Knock Off
                Move4 = 182, // Protect
                IVs = new[] { 31, 31, 31, 0, 31, 31 },
                EVs = new[] { 252, 0, 252, 0, 4, 0 },
                Item = 1092, // Toxic Orb
                TeraType = MoveType.Water
            }
        };
    }

    private static List<PokemonTemplate> GetUbersTemplates()
    {
        return new List<PokemonTemplate>
        {
            new PokemonTemplate
            {
                Name = "Koraidon - Offensive",
                Species = (ushort)Species.Koraidon,
                Nature = Nature.Jolly,
                AbilityIndex = 0, // Orichalcum Pulse
                Move1 = 370, // Close Combat
                Move2 = 808, // Collision Course
                Move3 = 200, // Outrage
                Move4 = 421, // Drain Punch
                IVs = new[] { 31, 31, 31, 0, 31, 31 },
                EVs = new[] { 0, 252, 0, 0, 4, 252 },
                Item = 1690, // Choice Scarf
                TeraType = MoveType.Fighting
            },
            new PokemonTemplate
            {
                Name = "Miraidon - Choice Specs",
                Species = (ushort)Species.Miraidon,
                Nature = Nature.Timid,
                AbilityIndex = 0, // Hadron Engine
                Move1 = 809, // Electro Drift
                Move2 = 434, // Draco Meteor
                Move3 = 126, // Overheat
                Move4 = 528, // Volt Switch
                IVs = new[] { 31, 0, 31, 31, 31, 31 },
                EVs = new[] { 0, 0, 0, 252, 4, 252 },
                Item = 297, // Choice Specs
                TeraType = MoveType.Electric
            },
            new PokemonTemplate
            {
                Name = "Calyrex-Shadow - Choice Specs",
                Species = (ushort)Species.Calyrex,
                Form = 1, // Shadow Rider
                Nature = Nature.Timid,
                AbilityIndex = 0, // As One
                Move1 = 796, // Astral Barrage
                Move2 = 94, // Psychic
                Move3 = 412, // Focus Blast
                Move4 = 285, // Trick
                IVs = new[] { 31, 0, 31, 31, 31, 31 },
                EVs = new[] { 0, 0, 0, 252, 4, 252 },
                Item = 297, // Choice Specs
                TeraType = MoveType.Ghost
            },
            new PokemonTemplate
            {
                Name = "Zacian-Crowned - Swords Dance",
                Species = (ushort)Species.Zacian,
                Form = 1, // Crowned
                Nature = Nature.Jolly,
                AbilityIndex = 0, // Intrepid Sword
                Move1 = 14, // Swords Dance
                Move2 = 796, // Behemoth Blade
                Move3 = 370, // Close Combat
                Move4 = 269, // Wild Charge
                IVs = new[] { 31, 31, 31, 0, 31, 31 },
                EVs = new[] { 0, 252, 0, 0, 4, 252 },
                Item = 1103, // Rusted Sword
                TeraType = MoveType.Steel
            }
        };
    }

    private static List<PokemonTemplate> GetUUTemplates()
    {
        return new List<PokemonTemplate>
        {
            new PokemonTemplate
            {
                Name = "Hydreigon - Choice Specs",
                Species = (ushort)Species.Hydreigon,
                Nature = Nature.Timid,
                AbilityIndex = 0, // Levitate
                Move1 = 434, // Draco Meteor
                Move2 = 399, // Dark Pulse
                Move3 = 53, // Flamethrower
                Move4 = 247, // Flash Cannon
                IVs = new[] { 31, 0, 31, 31, 31, 31 },
                EVs = new[] { 0, 0, 0, 252, 4, 252 },
                Item = 297, // Choice Specs
                TeraType = MoveType.Steel
            },
            new PokemonTemplate
            {
                Name = "Lokix - First Impression",
                Species = (ushort)Species.Lokix,
                Nature = Nature.Adamant,
                AbilityIndex = 0, // Swarm
                Move1 = 660, // First Impression
                Move2 = 369, // U-turn
                Move3 = 276, // Knock Off
                Move4 = 370, // Close Combat
                IVs = new[] { 31, 31, 31, 0, 31, 31 },
                EVs = new[] { 0, 252, 0, 0, 4, 252 },
                Item = 275, // Choice Band
                TeraType = MoveType.Dark
            }
        };
    }

    private static List<PokemonTemplate> GetVGCTemplates()
    {
        return new List<PokemonTemplate>
        {
            new PokemonTemplate
            {
                Name = "Flutter Mane - Choice Specs",
                Species = (ushort)Species.FlutterMane,
                Nature = Nature.Timid,
                AbilityIndex = 0, // Protosynthesis
                Move1 = 585, // Moonblast
                Move2 = 247, // Shadow Ball
                Move3 = 94, // Psychic
                Move4 = 182, // Protect
                IVs = new[] { 31, 0, 31, 31, 31, 31 },
                EVs = new[] { 4, 0, 0, 252, 0, 252 },
                Item = 297, // Choice Specs
                TeraType = MoveType.Fairy
            },
            new PokemonTemplate
            {
                Name = "Rillaboom - Grassy Surge",
                Species = (ushort)Species.Rillaboom,
                Nature = Nature.Adamant,
                AbilityIndex = 2, // Grassy Surge
                Move1 = 803, // Grassy Glide
                Move2 = 452, // Wood Hammer
                Move3 = 359, // Hammer Arm
                Move4 = 182, // Protect
                IVs = new[] { 31, 31, 31, 0, 31, 31 },
                EVs = new[] { 252, 252, 0, 0, 4, 0 },
                Item = 1178, // Assault Vest
                TeraType = MoveType.Fire
            },
            new PokemonTemplate
            {
                Name = "Urshifu-Rapid - Choice Band",
                Species = (ushort)Species.Urshifu,
                Form = 1, // Rapid Strike
                Nature = Nature.Jolly,
                AbilityIndex = 0, // Unseen Fist
                Move1 = 818, // Surging Strikes
                Move2 = 370, // Close Combat
                Move3 = 710, // Aqua Jet
                Move4 = 369, // U-turn
                IVs = new[] { 31, 31, 31, 0, 31, 31 },
                EVs = new[] { 0, 252, 0, 0, 4, 252 },
                Item = 275, // Choice Band
                TeraType = MoveType.Water
            }
        };
    }

    private static List<PokemonTemplate> GetDoublesOUTemplates()
    {
        return new List<PokemonTemplate>
        {
            new PokemonTemplate
            {
                Name = "Incineroar - Intimidate Support",
                Species = (ushort)Species.Incineroar,
                Nature = Nature.Careful,
                AbilityIndex = 2, // Intimidate
                Move1 = 269, // Fake Out
                Move2 = 389, // Flare Blitz
                Move3 = 276, // Knock Off
                Move4 = 360, // Parting Shot
                IVs = new[] { 31, 31, 31, 0, 31, 31 },
                EVs = new[] { 252, 0, 0, 0, 252, 4 },
                Item = 1290, // Sitrus Berry
                TeraType = MoveType.Ghost
            }
        };
    }

    private static List<PokemonTemplate> GetMonotypeTemplates()
    {
        return new List<PokemonTemplate>
        {
            new PokemonTemplate
            {
                Name = "Tyranitar - Sand Stream",
                Species = (ushort)Species.Tyranitar,
                Nature = Nature.Careful,
                AbilityIndex = 0, // Sand Stream
                Move1 = 444, // Stone Edge
                Move2 = 242, // Crunch
                Move3 = 89, // Earthquake
                Move4 = 157, // Rock Slide
                IVs = new[] { 31, 31, 31, 0, 31, 31 },
                EVs = new[] { 252, 0, 0, 0, 252, 4 },
                Item = 1290, // Leftovers
                TeraType = MoveType.Ghost
            }
        };
    }

    private static List<PokemonTemplate> GetStarterTemplates()
    {
        return new List<PokemonTemplate>
        {
            new PokemonTemplate
            {
                Name = "Charizard - Dragon Dance",
                Species = (ushort)Species.Charizard,
                Nature = Nature.Jolly,
                AbilityIndex = 0, // Blaze
                Move1 = 349, // Dragon Dance
                Move2 = 394, // Flare Blitz
                Move3 = 89, // Earthquake
                Move4 = 200, // Outrage
                IVs = new[] { 31, 31, 31, 0, 31, 31 },
                EVs = new[] { 0, 252, 0, 0, 4, 252 },
                Item = 1149, // Heavy-Duty Boots
                TeraType = MoveType.Dragon,
                Shiny = true
            },
            new PokemonTemplate
            {
                Name = "Greninja - Protean",
                Species = (ushort)Species.Greninja,
                Nature = Nature.Timid,
                AbilityIndex = 2, // Protean
                Move1 = 57, // Surf
                Move2 = 399, // Dark Pulse
                Move3 = 58, // Ice Beam
                Move4 = 269, // Spikes
                IVs = new[] { 31, 0, 31, 31, 31, 31 },
                EVs = new[] { 0, 0, 0, 252, 4, 252 },
                Item = 1178, // Focus Sash
                TeraType = MoveType.Poison
            },
            new PokemonTemplate
            {
                Name = "Meowscarada - Choice Band",
                Species = (ushort)Species.Meowscarada,
                Nature = Nature.Jolly,
                AbilityIndex = 0, // Overgrow
                Move1 = 870, // Flower Trick
                Move2 = 276, // Knock Off
                Move3 = 279, // Triple Axel
                Move4 = 369, // U-turn
                IVs = new[] { 31, 31, 31, 0, 31, 31 },
                EVs = new[] { 0, 252, 0, 0, 4, 252 },
                Item = 275, // Choice Band
                TeraType = MoveType.Dark
            }
        };
    }

    private static List<PokemonTemplate> GetLegendaryTemplates()
    {
        return new List<PokemonTemplate>
        {
            new PokemonTemplate
            {
                Name = "Mewtwo - Nasty Plot",
                Species = (ushort)Species.Mewtwo,
                Nature = Nature.Timid,
                AbilityIndex = 0, // Pressure
                Move1 = 417, // Nasty Plot
                Move2 = 94, // Psychic
                Move3 = 396, // Aura Sphere
                Move4 = 58, // Ice Beam
                IVs = new[] { 31, 0, 31, 31, 31, 31 },
                EVs = new[] { 0, 0, 0, 252, 4, 252 },
                Item = 1178, // Life Orb
                TeraType = MoveType.Psychic,
                Shiny = true
            },
            new PokemonTemplate
            {
                Name = "Rayquaza - Dragon Dance",
                Species = (ushort)Species.Rayquaza,
                Nature = Nature.Jolly,
                AbilityIndex = 0, // Air Lock
                Move1 = 349, // Dragon Dance
                Move2 = 200, // Outrage
                Move3 = 97, // Extreme Speed
                Move4 = 89, // Earthquake
                IVs = new[] { 31, 31, 31, 0, 31, 31 },
                EVs = new[] { 0, 252, 0, 0, 4, 252 },
                Item = 1178, // Life Orb
                TeraType = MoveType.Flying,
                Shiny = true
            },
            new PokemonTemplate
            {
                Name = "Arceus - Extreme Killer",
                Species = (ushort)Species.Arceus,
                Nature = Nature.Jolly,
                AbilityIndex = 0, // Multitype
                Move1 = 14, // Swords Dance
                Move2 = 97, // Extreme Speed
                Move3 = 89, // Earthquake
                Move4 = 247, // Shadow Claw
                IVs = new[] { 31, 31, 31, 0, 31, 31 },
                EVs = new[] { 0, 252, 0, 0, 4, 252 },
                Item = 1178, // Silk Scarf
                TeraType = MoveType.Normal,
                Shiny = true
            }
        };
    }
}

public class PokemonTemplate
{
    public string Name { get; set; } = "";
    public ushort Species { get; set; }
    public byte Form { get; set; }
    public Nature Nature { get; set; }
    public int AbilityIndex { get; set; }
    public ushort Move1 { get; set; }
    public ushort Move2 { get; set; }
    public ushort Move3 { get; set; }
    public ushort Move4 { get; set; }
    public int[] IVs { get; set; } = new[] { 31, 31, 31, 31, 31, 31 };
    public int[] EVs { get; set; } = new[] { 0, 0, 0, 0, 0, 0 };
    public int Item { get; set; }
    public MoveType TeraType { get; set; } = MoveType.Normal;
    public bool Shiny { get; set; }
}
