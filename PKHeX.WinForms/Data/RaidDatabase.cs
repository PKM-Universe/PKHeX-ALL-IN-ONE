using System;
using System.Collections.Generic;
using System.Linq;
using PKHeX.Core;

namespace PKHeX.WinForms.Data;

/// <summary>
/// Raid Database - Tera Raids, Max Raids, Dynamax Adventures
/// </summary>
public static class RaidDatabase
{
    public static List<RaidEncounter> GetAllRaids()
    {
        var raids = new List<RaidEncounter>();
        raids.AddRange(GetTeraRaids());
        raids.AddRange(GetMaxRaids());
        raids.AddRange(GetDynamaxAdventures());
        return raids;
    }

    public static List<RaidEncounter> GetTeraRaids()
    {
        return new List<RaidEncounter>
        {
            // 5-Star Tera Raids
            new() { Species = (ushort)Species.Charizard, RaidType = RaidType.Tera5Star, Stars = 5, TeraTypes = new[] { MoveType.Dragon }, Shiny = ShinyRate.Possible, Game = "SV" },
            new() { Species = (ushort)Species.Tyranitar, RaidType = RaidType.Tera5Star, Stars = 5, TeraTypes = new[] { MoveType.Dark, MoveType.Rock }, Shiny = ShinyRate.Possible, Game = "SV" },
            new() { Species = (ushort)Species.Garchomp, RaidType = RaidType.Tera5Star, Stars = 5, TeraTypes = new[] { MoveType.Dragon, MoveType.Ground }, Shiny = ShinyRate.Possible, Game = "SV" },
            new() { Species = (ushort)Species.Hydreigon, RaidType = RaidType.Tera5Star, Stars = 5, TeraTypes = new[] { MoveType.Dark, MoveType.Dragon }, Shiny = ShinyRate.Possible, Game = "SV" },
            new() { Species = (ushort)Species.Dragapult, RaidType = RaidType.Tera5Star, Stars = 5, TeraTypes = new[] { MoveType.Ghost, MoveType.Dragon }, Shiny = ShinyRate.Possible, Game = "SV" },
            new() { Species = (ushort)Species.Salamence, RaidType = RaidType.Tera5Star, Stars = 5, TeraTypes = new[] { MoveType.Dragon, MoveType.Flying }, Shiny = ShinyRate.Possible, Game = "SV" },

            // 6-Star Tera Raids
            new() { Species = (ushort)Species.Ditto, RaidType = RaidType.Tera6Star, Stars = 6, TeraTypes = new[] { MoveType.Normal }, Shiny = ShinyRate.Never, Game = "SV" },
            new() { Species = (ushort)Species.Gengar, RaidType = RaidType.Tera6Star, Stars = 6, TeraTypes = new[] { MoveType.Ghost, MoveType.Poison }, Shiny = ShinyRate.Possible, Game = "SV" },
            new() { Species = (ushort)Species.Blissey, RaidType = RaidType.Tera6Star, Stars = 6, TeraTypes = new[] { MoveType.Normal, MoveType.Fairy }, Shiny = ShinyRate.Possible, Game = "SV" },

            // 7-Star Event Tera Raids
            new() { Species = (ushort)Species.Charizard, RaidType = RaidType.Tera7Star, Stars = 7, TeraTypes = new[] { MoveType.Dragon }, Shiny = ShinyRate.Never, Game = "SV", EventName = "Unrivaled Charizard", MightyMark = true },
            new() { Species = (ushort)Species.Cinderace, RaidType = RaidType.Tera7Star, Stars = 7, TeraTypes = new[] { MoveType.Fighting }, Shiny = ShinyRate.Never, Game = "SV", EventName = "Unrivaled Cinderace", MightyMark = true },
            new() { Species = (ushort)Species.Greninja, RaidType = RaidType.Tera7Star, Stars = 7, TeraTypes = new[] { MoveType.Poison }, Shiny = ShinyRate.Never, Game = "SV", EventName = "Unrivaled Greninja", MightyMark = true },
            new() { Species = (ushort)Species.Decidueye, RaidType = RaidType.Tera7Star, Stars = 7, TeraTypes = new[] { MoveType.Fighting }, Shiny = ShinyRate.Never, Game = "SV", EventName = "Unrivaled Decidueye", MightyMark = true },
            new() { Species = (ushort)Species.Samurott, RaidType = RaidType.Tera7Star, Stars = 7, TeraTypes = new[] { MoveType.Bug }, Shiny = ShinyRate.Never, Game = "SV", EventName = "Unrivaled Samurott", MightyMark = true },
            new() { Species = (ushort)Species.Typhlosion, RaidType = RaidType.Tera7Star, Stars = 7, TeraTypes = new[] { MoveType.Ghost }, Shiny = ShinyRate.Never, Game = "SV", EventName = "Unrivaled Typhlosion", MightyMark = true },
            new() { Species = (ushort)Species.Mewtwo, RaidType = RaidType.Tera7Star, Stars = 7, TeraTypes = new[] { MoveType.Psychic }, Shiny = ShinyRate.Never, Game = "SV", EventName = "Unrivaled Mewtwo", MightyMark = true },
            new() { Species = (ushort)Species.Pikachu, RaidType = RaidType.Tera7Star, Stars = 7, TeraTypes = new[] { MoveType.Water }, Shiny = ShinyRate.Never, Game = "SV", EventName = "Unrivaled Pikachu", MightyMark = true },
            new() { Species = (ushort)Species.Eevee, RaidType = RaidType.Tera7Star, Stars = 7, TeraTypes = new[] { MoveType.Normal }, Shiny = ShinyRate.Never, Game = "SV", EventName = "Unrivaled Eevee", MightyMark = true },
            new() { Species = (ushort)Species.Sceptile, RaidType = RaidType.Tera7Star, Stars = 7, TeraTypes = new[] { MoveType.Dragon }, Shiny = ShinyRate.Never, Game = "SV", EventName = "Unrivaled Sceptile", MightyMark = true },
            new() { Species = (ushort)Species.Blaziken, RaidType = RaidType.Tera7Star, Stars = 7, TeraTypes = new[] { MoveType.Fire }, Shiny = ShinyRate.Never, Game = "SV", EventName = "Unrivaled Blaziken", MightyMark = true },
            new() { Species = (ushort)Species.Swampert, RaidType = RaidType.Tera7Star, Stars = 7, TeraTypes = new[] { MoveType.Poison }, Shiny = ShinyRate.Never, Game = "SV", EventName = "Unrivaled Swampert", MightyMark = true }
        };
    }

    public static List<RaidEncounter> GetMaxRaids()
    {
        return new List<RaidEncounter>
        {
            // Gigantamax Raids
            new() { Species = (ushort)Species.Charizard, RaidType = RaidType.MaxRaidGmax, Stars = 5, Game = "SWSH", Form = 1, Shiny = ShinyRate.Possible },
            new() { Species = (ushort)Species.Venusaur, RaidType = RaidType.MaxRaidGmax, Stars = 5, Game = "SWSH", Form = 1, Shiny = ShinyRate.Possible },
            new() { Species = (ushort)Species.Blastoise, RaidType = RaidType.MaxRaidGmax, Stars = 5, Game = "SWSH", Form = 1, Shiny = ShinyRate.Possible },
            new() { Species = (ushort)Species.Pikachu, RaidType = RaidType.MaxRaidGmax, Stars = 5, Game = "SWSH", Form = 1, Shiny = ShinyRate.Possible },
            new() { Species = (ushort)Species.Eevee, RaidType = RaidType.MaxRaidGmax, Stars = 5, Game = "SWSH", Form = 1, Shiny = ShinyRate.Possible },
            new() { Species = (ushort)Species.Snorlax, RaidType = RaidType.MaxRaidGmax, Stars = 5, Game = "SWSH", Form = 1, Shiny = ShinyRate.Possible },
            new() { Species = (ushort)Species.Gengar, RaidType = RaidType.MaxRaidGmax, Stars = 5, Game = "SWSH", Form = 1, Shiny = ShinyRate.Possible },
            new() { Species = (ushort)Species.Machamp, RaidType = RaidType.MaxRaidGmax, Stars = 5, Game = "SWSH", Form = 1, Shiny = ShinyRate.Possible },
            new() { Species = (ushort)Species.Lapras, RaidType = RaidType.MaxRaidGmax, Stars = 5, Game = "SWSH", Form = 1, Shiny = ShinyRate.Possible },
            new() { Species = (ushort)Species.Corviknight, RaidType = RaidType.MaxRaidGmax, Stars = 5, Game = "SWSH", Form = 1, Shiny = ShinyRate.Possible },
            new() { Species = (ushort)Species.Orbeetle, RaidType = RaidType.MaxRaidGmax, Stars = 5, Game = "SWSH", Form = 1, Shiny = ShinyRate.Possible },
            new() { Species = (ushort)Species.Drednaw, RaidType = RaidType.MaxRaidGmax, Stars = 5, Game = "SWSH", Form = 1, Shiny = ShinyRate.Possible },
            new() { Species = (ushort)Species.Coalossal, RaidType = RaidType.MaxRaidGmax, Stars = 5, Game = "SWSH", Form = 1, Shiny = ShinyRate.Possible },
            new() { Species = (ushort)Species.Flapple, RaidType = RaidType.MaxRaidGmax, Stars = 5, Game = "SWSH", Form = 1, Shiny = ShinyRate.Possible },
            new() { Species = (ushort)Species.Appletun, RaidType = RaidType.MaxRaidGmax, Stars = 5, Game = "SWSH", Form = 1, Shiny = ShinyRate.Possible },
            new() { Species = (ushort)Species.Sandaconda, RaidType = RaidType.MaxRaidGmax, Stars = 5, Game = "SWSH", Form = 1, Shiny = ShinyRate.Possible },
            new() { Species = (ushort)Species.Toxtricity, RaidType = RaidType.MaxRaidGmax, Stars = 5, Game = "SWSH", Form = 1, Shiny = ShinyRate.Possible },
            new() { Species = (ushort)Species.Centiskorch, RaidType = RaidType.MaxRaidGmax, Stars = 5, Game = "SWSH", Form = 1, Shiny = ShinyRate.Possible },
            new() { Species = (ushort)Species.Hatterene, RaidType = RaidType.MaxRaidGmax, Stars = 5, Game = "SWSH", Form = 1, Shiny = ShinyRate.Possible },
            new() { Species = (ushort)Species.Grimmsnarl, RaidType = RaidType.MaxRaidGmax, Stars = 5, Game = "SWSH", Form = 1, Shiny = ShinyRate.Possible },
            new() { Species = (ushort)Species.Alcremie, RaidType = RaidType.MaxRaidGmax, Stars = 5, Game = "SWSH", Form = 1, Shiny = ShinyRate.Possible },
            new() { Species = (ushort)Species.Copperajah, RaidType = RaidType.MaxRaidGmax, Stars = 5, Game = "SWSH", Form = 1, Shiny = ShinyRate.Possible },
            new() { Species = (ushort)Species.Duraludon, RaidType = RaidType.MaxRaidGmax, Stars = 5, Game = "SWSH", Form = 1, Shiny = ShinyRate.Possible },
            new() { Species = (ushort)Species.Urshifu, RaidType = RaidType.MaxRaidGmax, Stars = 5, Game = "SWSH", Form = 1, Shiny = ShinyRate.Never }
        };
    }

    public static List<RaidEncounter> GetDynamaxAdventures()
    {
        return new List<RaidEncounter>
        {
            new() { Species = (ushort)Species.Articuno, RaidType = RaidType.DynamaxAdventure, Stars = 5, Game = "SWSH", Shiny = ShinyRate.Enhanced },
            new() { Species = (ushort)Species.Zapdos, RaidType = RaidType.DynamaxAdventure, Stars = 5, Game = "SWSH", Shiny = ShinyRate.Enhanced },
            new() { Species = (ushort)Species.Moltres, RaidType = RaidType.DynamaxAdventure, Stars = 5, Game = "SWSH", Shiny = ShinyRate.Enhanced },
            new() { Species = (ushort)Species.Mewtwo, RaidType = RaidType.DynamaxAdventure, Stars = 5, Game = "SWSH", Shiny = ShinyRate.Enhanced },
            new() { Species = (ushort)Species.Raikou, RaidType = RaidType.DynamaxAdventure, Stars = 5, Game = "SWSH", Shiny = ShinyRate.Enhanced },
            new() { Species = (ushort)Species.Entei, RaidType = RaidType.DynamaxAdventure, Stars = 5, Game = "SWSH", Shiny = ShinyRate.Enhanced },
            new() { Species = (ushort)Species.Suicune, RaidType = RaidType.DynamaxAdventure, Stars = 5, Game = "SWSH", Shiny = ShinyRate.Enhanced },
            new() { Species = (ushort)Species.Lugia, RaidType = RaidType.DynamaxAdventure, Stars = 5, Game = "SWSH", Shiny = ShinyRate.Enhanced },
            new() { Species = (ushort)Species.HoOh, RaidType = RaidType.DynamaxAdventure, Stars = 5, Game = "SWSH", Shiny = ShinyRate.Enhanced },
            new() { Species = (ushort)Species.Kyogre, RaidType = RaidType.DynamaxAdventure, Stars = 5, Game = "SWSH", Shiny = ShinyRate.Enhanced },
            new() { Species = (ushort)Species.Groudon, RaidType = RaidType.DynamaxAdventure, Stars = 5, Game = "SWSH", Shiny = ShinyRate.Enhanced },
            new() { Species = (ushort)Species.Rayquaza, RaidType = RaidType.DynamaxAdventure, Stars = 5, Game = "SWSH", Shiny = ShinyRate.Enhanced },
            new() { Species = (ushort)Species.Dialga, RaidType = RaidType.DynamaxAdventure, Stars = 5, Game = "SWSH", Shiny = ShinyRate.Enhanced },
            new() { Species = (ushort)Species.Palkia, RaidType = RaidType.DynamaxAdventure, Stars = 5, Game = "SWSH", Shiny = ShinyRate.Enhanced },
            new() { Species = (ushort)Species.Giratina, RaidType = RaidType.DynamaxAdventure, Stars = 5, Game = "SWSH", Shiny = ShinyRate.Enhanced },
            new() { Species = (ushort)Species.Zekrom, RaidType = RaidType.DynamaxAdventure, Stars = 5, Game = "SWSH", Shiny = ShinyRate.Enhanced },
            new() { Species = (ushort)Species.Reshiram, RaidType = RaidType.DynamaxAdventure, Stars = 5, Game = "SWSH", Shiny = ShinyRate.Enhanced },
            new() { Species = (ushort)Species.Kyurem, RaidType = RaidType.DynamaxAdventure, Stars = 5, Game = "SWSH", Shiny = ShinyRate.Enhanced },
            new() { Species = (ushort)Species.Xerneas, RaidType = RaidType.DynamaxAdventure, Stars = 5, Game = "SWSH", Shiny = ShinyRate.Enhanced },
            new() { Species = (ushort)Species.Yveltal, RaidType = RaidType.DynamaxAdventure, Stars = 5, Game = "SWSH", Shiny = ShinyRate.Enhanced },
            new() { Species = (ushort)Species.Zygarde, RaidType = RaidType.DynamaxAdventure, Stars = 5, Game = "SWSH", Shiny = ShinyRate.Enhanced },
            new() { Species = (ushort)Species.Solgaleo, RaidType = RaidType.DynamaxAdventure, Stars = 5, Game = "SWSH", Shiny = ShinyRate.Enhanced },
            new() { Species = (ushort)Species.Lunala, RaidType = RaidType.DynamaxAdventure, Stars = 5, Game = "SWSH", Shiny = ShinyRate.Enhanced },
            new() { Species = (ushort)Species.Necrozma, RaidType = RaidType.DynamaxAdventure, Stars = 5, Game = "SWSH", Shiny = ShinyRate.Enhanced }
        };
    }

    public static List<RaidEncounter> GetRaidsBySpecies(ushort species)
    {
        return GetAllRaids().Where(r => r.Species == species).ToList();
    }

    public static List<RaidEncounter> GetRaidsByType(RaidType type)
    {
        return GetAllRaids().Where(r => r.RaidType == type).ToList();
    }
}

public class RaidEncounter
{
    public ushort Species { get; set; }
    public RaidType RaidType { get; set; }
    public int Stars { get; set; }
    public byte Form { get; set; }
    public MoveType[]? TeraTypes { get; set; }
    public ShinyRate Shiny { get; set; }
    public string Game { get; set; } = "";
    public string? EventName { get; set; }
    public bool MightyMark { get; set; }

    public string DisplayName => EventName ?? $"{Stars}-Star {SpeciesName.GetSpeciesName(Species, 2)}";
}

public enum RaidType
{
    Tera5Star,
    Tera6Star,
    Tera7Star,
    MaxRaid,
    MaxRaidGmax,
    DynamaxAdventure
}

public enum ShinyRate
{
    Never,
    Possible,
    Enhanced,
    Guaranteed
}
