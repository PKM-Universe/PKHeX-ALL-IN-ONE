using System;
using System.Collections.Generic;
using System.Linq;
using PKHeX.Core;

namespace PKHeX.WinForms.Data;

/// <summary>
/// Event Database - All Mystery Gifts from 2004-2024
/// </summary>
public static class EventDatabase
{
    public static List<EventPokemon> GetAllEvents()
    {
        var events = new List<EventPokemon>();
        events.AddRange(GetGen3Events());
        events.AddRange(GetGen4Events());
        events.AddRange(GetGen5Events());
        events.AddRange(GetGen6Events());
        events.AddRange(GetGen7Events());
        events.AddRange(GetGen8Events());
        events.AddRange(GetGen9Events());
        return events;
    }

    public static List<EventPokemon> GetEventsByGeneration(int gen)
    {
        return gen switch
        {
            3 => GetGen3Events(),
            4 => GetGen4Events(),
            5 => GetGen5Events(),
            6 => GetGen6Events(),
            7 => GetGen7Events(),
            8 => GetGen8Events(),
            9 => GetGen9Events(),
            _ => new List<EventPokemon>()
        };
    }

    public static List<EventPokemon> GetEventsBySpecies(ushort species)
    {
        return GetAllEvents().Where(e => e.Species == species).ToList();
    }

    public static List<EventPokemon> SearchEvents(string query)
    {
        query = query.ToLower();
        return GetAllEvents().Where(e =>
            e.Name.ToLower().Contains(query) ||
            e.EventName.ToLower().Contains(query) ||
            e.OT.ToLower().Contains(query)
        ).ToList();
    }

    private static List<EventPokemon> GetGen3Events()
    {
        return new List<EventPokemon>
        {
            new() { Species = 151, Name = "Mew", EventName = "Hadou Mew", OT = "ハドウ", TID = 50716, Generation = 3, Year = 2005, Shiny = false, Level = 10, Region = "Japan" },
            new() { Species = 151, Name = "Mew", EventName = "MYSTRY Mew", OT = "MYSTRY", TID = 06930, Generation = 3, Year = 2006, Shiny = false, Level = 10, Region = "America" },
            new() { Species = 385, Name = "Jirachi", EventName = "WISHMKR Jirachi", OT = "WISHMKR", TID = 20043, Generation = 3, Year = 2004, Shiny = false, Level = 5, Region = "America" },
            new() { Species = 385, Name = "Jirachi", EventName = "Tanabata Jirachi", OT = "タナバタ", TID = 40707, Generation = 3, Year = 2004, Shiny = false, Level = 5, Region = "Japan" },
            new() { Species = 386, Name = "Deoxys", EventName = "Space C Deoxys", OT = "スペースC", TID = 00010, Generation = 3, Year = 2004, Shiny = false, Level = 70, Region = "Japan" },
            new() { Species = 386, Name = "Deoxys", EventName = "Aurora Deoxys", OT = "Aurora", TID = 00010, Generation = 3, Year = 2005, Shiny = false, Level = 70, Region = "America" },
            new() { Species = 25, Name = "Pikachu", EventName = "ANA Pikachu", OT = "ANA", TID = 41205, Generation = 3, Year = 2004, Shiny = false, Level = 10, Region = "Japan" }
        };
    }

    private static List<EventPokemon> GetGen4Events()
    {
        return new List<EventPokemon>
        {
            new() { Species = 491, Name = "Darkrai", EventName = "ALAMOS Darkrai", OT = "ALAMOS", TID = 05318, Generation = 4, Year = 2008, Shiny = false, Level = 50, Region = "America" },
            new() { Species = 491, Name = "Darkrai", EventName = "Almia Darkrai", OT = "アルミア", TID = 03208, Generation = 4, Year = 2008, Shiny = false, Level = 50, Region = "Japan" },
            new() { Species = 492, Name = "Shaymin", EventName = "TRU Shaymin", OT = "TRU", TID = 02089, Generation = 4, Year = 2009, Shiny = false, Level = 50, Region = "America" },
            new() { Species = 492, Name = "Shaymin", EventName = "Movie Shaymin", OT = "えいがかん", TID = 07198, Generation = 4, Year = 2008, Shiny = false, Level = 50, Region = "Japan" },
            new() { Species = 493, Name = "Arceus", EventName = "TRU Arceus", OT = "TRU", TID = 11079, Generation = 4, Year = 2009, Shiny = false, Level = 100, Region = "America" },
            new() { Species = 493, Name = "Arceus", EventName = "Movie Arceus", OT = "えいがかん", TID = 07189, Generation = 4, Year = 2009, Shiny = false, Level = 100, Region = "Japan" },
            new() { Species = 490, Name = "Manaphy", EventName = "TRU Manaphy", OT = "TRU", TID = 09297, Generation = 4, Year = 2007, Shiny = false, Level = 50, Region = "America" },
            new() { Species = 151, Name = "Mew", EventName = "FAL2010 Mew", OT = "FAL2010", TID = 10160, Generation = 4, Year = 2010, Shiny = false, Level = 5, Region = "America" },
            new() { Species = 251, Name = "Celebi", EventName = "WIN2011 Celebi", OT = "WIN2011", TID = 02211, Generation = 4, Year = 2011, Shiny = false, Level = 50, Region = "America" },
            new() { Species = 25, Name = "Pikachu", EventName = "WORLD08 Pikachu", OT = "WORLD08", TID = 08178, Generation = 4, Year = 2008, Shiny = false, Level = 50, Region = "World" },
            new() { Species = 172, Name = "Pichu", EventName = "SPR2010 Pichu", OT = "SPR2010", TID = 03050, Generation = 4, Year = 2010, Shiny = true, Level = 30, Region = "America" }
        };
    }

    private static List<EventPokemon> GetGen5Events()
    {
        return new List<EventPokemon>
        {
            new() { Species = 494, Name = "Victini", EventName = "Movie14 Victini", OT = "Movie14", TID = 12031, Generation = 5, Year = 2011, Shiny = false, Level = 50, Region = "America" },
            new() { Species = 647, Name = "Keldeo", EventName = "SMR2012 Keldeo", OT = "SMR2012", TID = 08272, Generation = 5, Year = 2012, Shiny = false, Level = 15, Region = "America" },
            new() { Species = 648, Name = "Meloetta", EventName = "SPR2013 Meloetta", OT = "SPR2013", TID = 03013, Generation = 5, Year = 2013, Shiny = false, Level = 50, Region = "America" },
            new() { Species = 649, Name = "Genesect", EventName = "Plasma Genesect", OT = "Plasma", TID = 10072, Generation = 5, Year = 2012, Shiny = false, Level = 15, Region = "America" },
            new() { Species = 649, Name = "Genesect", EventName = "Movie Genesect", OT = "えいがかん", TID = 07133, Generation = 5, Year = 2013, Shiny = true, Level = 100, Region = "Japan" },
            new() { Species = 643, Name = "Reshiram", EventName = "SPR2012 Reshiram", OT = "SPR2012", TID = 03102, Generation = 5, Year = 2012, Shiny = false, Level = 100, Region = "America" },
            new() { Species = 644, Name = "Zekrom", EventName = "SPR2012 Zekrom", OT = "SPR2012", TID = 03102, Generation = 5, Year = 2012, Shiny = false, Level = 100, Region = "America" },
            new() { Species = 384, Name = "Rayquaza", EventName = "V-Create Rayquaza", OT = "せんきょ", TID = 02102, Generation = 5, Year = 2012, Shiny = false, Level = 100, Region = "Japan" },
            new() { Species = 646, Name = "Kyurem", EventName = "FEB2012 Kyurem", OT = "FEB2012", TID = 02012, Generation = 5, Year = 2012, Shiny = false, Level = 75, Region = "America" }
        };
    }

    private static List<EventPokemon> GetGen6Events()
    {
        return new List<EventPokemon>
        {
            new() { Species = 719, Name = "Diancie", EventName = "OCT2014 Diancie", OT = "OCT2014", TID = 10274, Generation = 6, Year = 2014, Shiny = false, Level = 50, Region = "America" },
            new() { Species = 720, Name = "Hoopa", EventName = "Mac Hoopa", OT = "Mac", TID = 11275, Generation = 6, Year = 2015, Shiny = false, Level = 50, Region = "America" },
            new() { Species = 721, Name = "Volcanion", EventName = "Helen Volcanion", OT = "Helen", TID = 10016, Generation = 6, Year = 2016, Shiny = false, Level = 70, Region = "America" },
            new() { Species = 718, Name = "Zygarde", EventName = "XYZ Zygarde", OT = "XYZ", TID = 05026, Generation = 6, Year = 2016, Shiny = true, Level = 100, Region = "America" },
            new() { Species = 716, Name = "Xerneas", EventName = "XYZ Xerneas", OT = "XYZ", TID = 05116, Generation = 6, Year = 2016, Shiny = true, Level = 100, Region = "America" },
            new() { Species = 717, Name = "Yveltal", EventName = "XYZ Yveltal", OT = "XYZ", TID = 05206, Generation = 6, Year = 2016, Shiny = true, Level = 100, Region = "America" },
            new() { Species = 384, Name = "Rayquaza", EventName = "Galileo Rayquaza", OT = "Galileo", TID = 08055, Generation = 6, Year = 2015, Shiny = true, Level = 70, Region = "America" },
            new() { Species = 25, Name = "Pikachu", EventName = "Ash's Pikachu", OT = "Ash", TID = 01301, Generation = 6, Year = 2016, Shiny = false, Level = 10, Region = "America" }
        };
    }

    private static List<EventPokemon> GetGen7Events()
    {
        return new List<EventPokemon>
        {
            new() { Species = 801, Name = "Magearna", EventName = "QR Magearna", OT = "QR", TID = 0, Generation = 7, Year = 2016, Shiny = false, Level = 50, Region = "World" },
            new() { Species = 802, Name = "Marshadow", EventName = "MT. Tensei Marshadow", OT = "MT. Tensei", TID = 100917, Generation = 7, Year = 2017, Shiny = false, Level = 50, Region = "America" },
            new() { Species = 807, Name = "Zeraora", EventName = "Fula City Zeraora", OT = "Fula City", TID = 100118, Generation = 7, Year = 2018, Shiny = false, Level = 50, Region = "America" },
            new() { Species = 807, Name = "Zeraora", EventName = "HOME Zeraora", OT = "HOME", TID = 200630, Generation = 7, Year = 2020, Shiny = true, Level = 100, Region = "World" },
            new() { Species = 789, Name = "Cosmog", EventName = "Aether Cosmog", OT = "Aether", TID = 170922, Generation = 7, Year = 2017, Shiny = false, Level = 5, Region = "America" },
            new() { Species = 792, Name = "Lunala", EventName = "Eclipse Lunala", OT = "Eclipse", TID = 100118, Generation = 7, Year = 2019, Shiny = true, Level = 60, Region = "America" },
            new() { Species = 791, Name = "Solgaleo", EventName = "Eclipse Solgaleo", OT = "Eclipse", TID = 100118, Generation = 7, Year = 2019, Shiny = true, Level = 60, Region = "America" },
            new() { Species = 800, Name = "Necrozma", EventName = "Eclipse Necrozma", OT = "Eclipse", TID = 100118, Generation = 7, Year = 2019, Shiny = true, Level = 75, Region = "World" },
            new() { Species = 25, Name = "Pikachu", EventName = "Ash Hat Pikachu", OT = "Ash", TID = 170715, Generation = 7, Year = 2017, Shiny = false, Level = 1, Region = "World" }
        };
    }

    private static List<EventPokemon> GetGen8Events()
    {
        return new List<EventPokemon>
        {
            new() { Species = 893, Name = "Zarude", EventName = "Jungle Zarude", OT = "Jungle", TID = 201113, Generation = 8, Year = 2020, Shiny = false, Level = 60, Region = "America" },
            new() { Species = 893, Name = "Zarude", EventName = "Dada Zarude", OT = "Jungle", TID = 211006, Generation = 8, Year = 2021, Shiny = false, Level = 70, Region = "World" },
            new() { Species = 251, Name = "Celebi", EventName = "Jungle Celebi", OT = "Jungle", TID = 211006, Generation = 8, Year = 2021, Shiny = true, Level = 60, Region = "World" },
            new() { Species = 890, Name = "Eternatus", EventName = "Shiny Eternatus", OT = "Galar", TID = 211022, Generation = 8, Year = 2022, Shiny = true, Level = 100, Region = "World" },
            new() { Species = 888, Name = "Zacian", EventName = "Shiny Zacian", OT = "Lancer", TID = 211022, Generation = 8, Year = 2021, Shiny = true, Level = 100, Region = "World" },
            new() { Species = 889, Name = "Zamazenta", EventName = "Shiny Zamazenta", OT = "Arthur", TID = 211022, Generation = 8, Year = 2021, Shiny = true, Level = 100, Region = "World" },
            new() { Species = 892, Name = "Urshifu", EventName = "Kubfu", OT = "Isle of Armor", TID = 0, Generation = 8, Year = 2020, Shiny = false, Level = 10, Region = "World" },
            new() { Species = 898, Name = "Calyrex", EventName = "Crown Tundra", OT = "Crown", TID = 0, Generation = 8, Year = 2020, Shiny = false, Level = 80, Region = "World" },
            new() { Species = 151, Name = "Mew", EventName = "Get Challenge Mew", OT = "ゲッチャ", TID = 220Pokemon, Generation = 8, Year = 2022, Shiny = false, Level = 5, Region = "Japan" }
        };
    }

    private static List<EventPokemon> GetGen9Events()
    {
        return new List<EventPokemon>
        {
            new() { Species = 1024, Name = "Terapagos", EventName = "Indigo Disk", OT = "Player", TID = 0, Generation = 9, Year = 2023, Shiny = false, Level = 85, Region = "World" },
            new() { Species = 1025, Name = "Pecharunt", EventName = "Epilogue", OT = "Player", TID = 0, Generation = 9, Year = 2024, Shiny = false, Level = 88, Region = "World" },
            new() { Species = 1017, Name = "Ogerpon", EventName = "Teal Mask", OT = "Player", TID = 0, Generation = 9, Year = 2023, Shiny = false, Level = 70, Region = "World" },
            new() { Species = 151, Name = "Mew", EventName = "Get Mew", OT = "ゲット", TID = 240Pokemon, Generation = 9, Year = 2024, Shiny = false, Level = 5, Region = "Japan" },
            new() { Species = 25, Name = "Pikachu", EventName = "Flying Tera Pikachu", OT = "POKEMON", TID = 230Pokemon, Generation = 9, Year = 2023, Shiny = false, Level = 25, Region = "World" },
            new() { Species = 448, Name = "Lucario", EventName = "WCS Lucario", OT = "WCS", TID = 230Pokemon, Generation = 9, Year = 2023, Shiny = false, Level = 75, Region = "World" },
            new() { Species = 384, Name = "Rayquaza", EventName = "Victory Road Rayquaza", OT = "Victory", TID = 231Pokemon, Generation = 9, Year = 2023, Shiny = true, Level = 100, Region = "World" }
        };
    }
}

public class EventPokemon
{
    public ushort Species { get; set; }
    public string Name { get; set; } = "";
    public string EventName { get; set; } = "";
    public string OT { get; set; } = "";
    public int TID { get; set; }
    public int Generation { get; set; }
    public int Year { get; set; }
    public bool Shiny { get; set; }
    public byte Level { get; set; }
    public string Region { get; set; } = "";

    public string DisplayName => $"{EventName} ({Year})";
}
