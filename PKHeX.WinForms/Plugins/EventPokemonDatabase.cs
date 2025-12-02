using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PKHeX.Core;

namespace PKHeX.WinForms.Plugins;

/// <summary>
/// Event Pokemon Database - Browse and generate event Pokemon with auto-legalization
/// </summary>
public class EventPokemonDatabase
{
    private readonly SaveFile SAV;

    // Event Pokemon database (sample - would be expanded with full data)
    private static readonly List<EventTemplate> Events = new()
    {
        // Gen 9 Events
        new() { Species = 25, Form = 0, EventName = "Flying Pikachu (Scarlet/Violet)", OT = "GF", TID = 02056, IsShiny = false, Level = 25, Ball = Ball.Cherish, Moves = new ushort[] { 19, 84, 85, 97 }, Generation = 9, Region = "International", StartDate = new DateTime(2023, 2, 27), EndDate = new DateTime(2024, 2, 27) },
        new() { Species = 151, Form = 0, EventName = "Pokemon HOME Mew", OT = "HOME", TID = 21001, IsShiny = false, Level = 1, Ball = Ball.Poke, Moves = new ushort[] { 1 }, Generation = 9, Region = "International", RequiresItem = true },
        new() { Species = 1025, Form = 0, EventName = "Pecharunt (DLC)", OT = "Player", TID = 0, IsShiny = false, Level = 88, Ball = Ball.Poke, Generation = 9, Region = "International" },

        // Gen 8 Events
        new() { Species = 25, Form = 8, EventName = "Gigantamax Pikachu", OT = "Player", TID = 0, IsShiny = false, Level = 10, Ball = Ball.Poke, Moves = new ushort[] { 84, 85, 344, 583 }, Generation = 8, Region = "International", RequiresItem = false },
        new() { Species = 133, Form = 0, EventName = "Gigantamax Eevee", OT = "Player", TID = 0, IsShiny = false, Level = 10, Ball = Ball.Poke, Generation = 8, Region = "International" },
        new() { Species = 893, Form = 0, EventName = "Zarude (Jungle Movie)", OT = "Jungle", TID = 201125, IsShiny = false, Level = 60, Ball = Ball.Cherish, Moves = new ushort[] { 200, 389, 512, 693 }, Generation = 8, Region = "International" },
        new() { Species = 893, Form = 1, EventName = "Dada Zarude", OT = "Jungle", TID = 211006, IsShiny = false, Level = 70, Ball = Ball.Cherish, Generation = 8, Region = "International" },
        new() { Species = 898, Form = 0, EventName = "Calyrex (Crown Tundra)", OT = "Player", TID = 0, IsShiny = false, Level = 80, Ball = Ball.Poke, Generation = 8, Region = "International" },
        new() { Species = 494, Form = 0, EventName = "Victini (Pokemon HOME)", OT = "Victory", TID = 22000, IsShiny = false, Level = 15, Ball = Ball.Cherish, Generation = 8, Region = "International" },

        // Gen 7 Events
        new() { Species = 807, Form = 0, EventName = "Zeraora (Fula City)", OT = "Fula City", TID = 100118, IsShiny = false, Level = 50, Ball = Ball.Cherish, Moves = new ushort[] { 85, 394, 409, 435 }, Generation = 7, Region = "International" },
        new() { Species = 807, Form = 0, EventName = "Shiny Zeraora (HOME)", OT = "HOME", TID = 200630, IsShiny = true, Level = 100, Ball = Ball.Cherish, Generation = 8, Region = "International" },
        new() { Species = 802, Form = 0, EventName = "Marshadow (Mt. Tensei)", OT = "MT. Tensei", TID = 100917, IsShiny = false, Level = 50, Ball = Ball.Cherish, Generation = 7, Region = "International" },
        new() { Species = 801, Form = 0, EventName = "Magearna (QR Code)", OT = "Player", TID = 0, IsShiny = false, Level = 50, Ball = Ball.Cherish, Generation = 7, Region = "International" },
        new() { Species = 801, Form = 1, EventName = "Magearna Original Color", OT = "Player", TID = 0, IsShiny = false, Level = 50, Ball = Ball.Cherish, Generation = 7, Region = "International" },

        // Gen 6 Events
        new() { Species = 720, Form = 0, EventName = "Hoopa (McDonald's)", OT = "Mac", TID = 11275, IsShiny = false, Level = 50, Ball = Ball.Cherish, Generation = 6, Region = "North America" },
        new() { Species = 719, Form = 0, EventName = "Diancie (Movie)", OT = "Hope", TID = 07245, IsShiny = false, Level = 50, Ball = Ball.Cherish, Generation = 6, Region = "International" },
        new() { Species = 721, Form = 0, EventName = "Volcanion (Helen)", OT = "Helen", TID = 10016, IsShiny = false, Level = 70, Ball = Ball.Cherish, Generation = 6, Region = "International" },

        // Classic Events
        new() { Species = 151, Form = 0, EventName = "Mew (20th Anniversary)", OT = "GF", TID = 02016, IsShiny = false, Level = 100, Ball = Ball.Cherish, Generation = 6, Region = "International", StartDate = new DateTime(2016, 2, 1), EndDate = new DateTime(2016, 2, 24) },
        new() { Species = 251, Form = 0, EventName = "Celebi (20th Anniversary)", OT = "GF", TID = 03016, IsShiny = false, Level = 100, Ball = Ball.Cherish, Generation = 6, Region = "International" },
        new() { Species = 385, Form = 0, EventName = "Jirachi (20th Anniversary)", OT = "GF", TID = 04016, IsShiny = false, Level = 100, Ball = Ball.Cherish, Generation = 6, Region = "International" },
        new() { Species = 386, Form = 0, EventName = "Deoxys (20th Anniversary)", OT = "GF", TID = 05016, IsShiny = false, Level = 100, Ball = Ball.Cherish, Generation = 6, Region = "International" },
        new() { Species = 490, Form = 0, EventName = "Manaphy (20th Anniversary)", OT = "GF", TID = 06016, IsShiny = false, Level = 100, Ball = Ball.Cherish, Generation = 6, Region = "International" },
        new() { Species = 491, Form = 0, EventName = "Darkrai (20th Anniversary)", OT = "GF", TID = 05016, IsShiny = false, Level = 100, Ball = Ball.Cherish, Generation = 6, Region = "International" },
        new() { Species = 492, Form = 0, EventName = "Shaymin (20th Anniversary)", OT = "GF", TID = 07016, IsShiny = false, Level = 100, Ball = Ball.Cherish, Generation = 6, Region = "International" },
        new() { Species = 493, Form = 0, EventName = "Arceus (20th Anniversary)", OT = "GF", TID = 08016, IsShiny = false, Level = 100, Ball = Ball.Cherish, Generation = 6, Region = "International" },
        new() { Species = 647, Form = 0, EventName = "Keldeo (20th Anniversary)", OT = "GF", TID = 10016, IsShiny = false, Level = 100, Ball = Ball.Cherish, Generation = 6, Region = "International" },
        new() { Species = 648, Form = 0, EventName = "Meloetta (20th Anniversary)", OT = "GF", TID = 12016, IsShiny = false, Level = 100, Ball = Ball.Cherish, Generation = 6, Region = "International" },
        new() { Species = 649, Form = 0, EventName = "Genesect (20th Anniversary)", OT = "GF", TID = 11016, IsShiny = false, Level = 100, Ball = Ball.Cherish, Generation = 6, Region = "International" },

        // Shiny Events
        new() { Species = 384, Form = 0, EventName = "Shiny Rayquaza (Galileo)", OT = "Galileo", TID = 08055, IsShiny = true, Level = 70, Ball = Ball.Cherish, Generation = 6, Region = "International" },
        new() { Species = 718, Form = 3, EventName = "Shiny Zygarde (2018)", OT = "2018 Legends", TID = 060218, IsShiny = true, Level = 100, Ball = Ball.Cherish, Generation = 7, Region = "International" },
        new() { Species = 791, Form = 0, EventName = "Shiny Solgaleo (Eclipse)", OT = "Eclipse", TID = 100419, IsShiny = true, Level = 60, Ball = Ball.Cherish, Generation = 7, Region = "International" },
        new() { Species = 792, Form = 0, EventName = "Shiny Lunala (Eclipse)", OT = "Eclipse", TID = 100419, IsShiny = true, Level = 60, Ball = Ball.Cherish, Generation = 7, Region = "International" },
        new() { Species = 800, Form = 0, EventName = "Shiny Necrozma (Secret Club)", OT = "Secret", TID = 000132, IsShiny = true, Level = 75, Ball = Ball.Cherish, Generation = 7, Region = "Japan" },
    };

    public EventPokemonDatabase(SaveFile sav)
    {
        SAV = sav;
    }

    /// <summary>
    /// Search for events by species
    /// </summary>
    public List<EventTemplate> SearchBySpecies(ushort species)
    {
        return Events.Where(e => e.Species == species).ToList();
    }

    /// <summary>
    /// Search for events by name
    /// </summary>
    public List<EventTemplate> SearchByName(string searchTerm)
    {
        return Events.Where(e =>
            e.EventName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            e.OT.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Get all events for a generation
    /// </summary>
    public List<EventTemplate> GetByGeneration(int generation)
    {
        return Events.Where(e => e.Generation == generation).OrderBy(e => e.Species).ToList();
    }

    /// <summary>
    /// Get all shiny events
    /// </summary>
    public List<EventTemplate> GetShinyEvents()
    {
        return Events.Where(e => e.IsShiny).ToList();
    }

    /// <summary>
    /// Get currently active events (within date range)
    /// </summary>
    public List<EventTemplate> GetActiveEvents()
    {
        var now = DateTime.Now;
        return Events.Where(e =>
            (!e.StartDate.HasValue || e.StartDate <= now) &&
            (!e.EndDate.HasValue || e.EndDate >= now))
            .ToList();
    }

    /// <summary>
    /// Get all mythical Pokemon events
    /// </summary>
    public List<EventTemplate> GetMythicalEvents()
    {
        var mythicals = new HashSet<ushort> { 151, 251, 385, 386, 489, 490, 491, 492, 493, 494, 647, 648, 649, 719, 720, 721, 801, 802, 807, 808, 809, 893, 1025 };
        return Events.Where(e => mythicals.Contains(e.Species)).ToList();
    }

    /// <summary>
    /// Generate a legal event Pokemon
    /// </summary>
    public PKM? GenerateEventPokemon(EventTemplate template)
    {
        // Create Showdown set from template
        var showdownText = GenerateShowdownText(template);

        // Try to generate using ALM
        var pk = ALMShowdownPlugin.ImportShowdownSetWithLegality(showdownText, SAV);

        if (pk != null)
        {
            // Apply event-specific data
            ApplyEventData(pk, template);
        }

        return pk;
    }

    /// <summary>
    /// Generate all mythicals for a generation
    /// </summary>
    public List<PKM> GenerateAllMythicals()
    {
        var results = new List<PKM>();
        var mythicalEvents = GetMythicalEvents();

        foreach (var template in mythicalEvents)
        {
            var pk = GenerateEventPokemon(template);
            if (pk != null)
                results.Add(pk);
        }

        return results;
    }

    /// <summary>
    /// Check what events are missing from the save
    /// </summary>
    public List<EventTemplate> GetMissingEvents()
    {
        var owned = new HashSet<ushort>();

        // Check boxes
        for (int box = 0; box < SAV.BoxCount; box++)
        {
            foreach (var pk in SAV.GetBoxData(box).Where(p => p.Species > 0))
            {
                owned.Add(pk.Species);
            }
        }

        // Check party
        foreach (var pk in SAV.PartyData.Where(p => p.Species > 0))
        {
            owned.Add(pk.Species);
        }

        return Events.Where(e => !owned.Contains(e.Species)).ToList();
    }

    private string GenerateShowdownText(EventTemplate template)
    {
        var sb = new StringBuilder();
        var speciesName = SpeciesName.GetSpeciesName(template.Species, 2);

        sb.AppendLine(speciesName);

        if (template.IsShiny)
            sb.AppendLine("Shiny: Yes");

        sb.AppendLine($"Level: {template.Level}");

        if (template.Moves != null && template.Moves.Length > 0)
        {
            foreach (var moveId in template.Moves.Where(m => m > 0))
            {
                var moveName = GameInfo.Strings.Move[moveId];
                sb.AppendLine($"- {moveName}");
            }
        }

        return sb.ToString();
    }

    private void ApplyEventData(PKM pk, EventTemplate template)
    {
        // Set OT and TID
        if (!string.IsNullOrEmpty(template.OT) && template.OT != "Player")
        {
            pk.OriginalTrainerName = template.OT;
            pk.TID16 = (ushort)template.TID;
        }

        // Set ball
        pk.Ball = (byte)template.Ball;

        // Set fateful encounter flag
        pk.FatefulEncounter = true;

        // Set shiny
        if (template.IsShiny && !pk.IsShiny)
            pk.SetShiny();
        else if (!template.IsShiny && pk.IsShiny)
            pk.SetUnshiny();

        // Set form
        pk.Form = template.Form;

        // Set level
        pk.CurrentLevel = (byte)template.Level;

        // Set moves if specified
        if (template.Moves != null && template.Moves.Length > 0)
        {
            pk.Move1 = template.Moves.Length > 0 ? template.Moves[0] : pk.Move1;
            pk.Move2 = template.Moves.Length > 1 ? template.Moves[1] : pk.Move2;
            pk.Move3 = template.Moves.Length > 2 ? template.Moves[2] : pk.Move3;
            pk.Move4 = template.Moves.Length > 3 ? template.Moves[3] : pk.Move4;
            pk.SetMaximumPPCurrent(pk.Moves);
        }

        pk.RefreshChecksum();
    }

    /// <summary>
    /// Generate a report of all events
    /// </summary>
    public string GenerateReport()
    {
        var sb = new StringBuilder();

        sb.AppendLine("╔══════════════════════════════════════════════════════════════╗");
        sb.AppendLine("║                   EVENT POKEMON DATABASE                      ║");
        sb.AppendLine("╠══════════════════════════════════════════════════════════════╣");
        sb.AppendLine($"║ Total Events: {Events.Count,-45}║");
        sb.AppendLine($"║ Shiny Events: {GetShinyEvents().Count,-45}║");
        sb.AppendLine($"║ Mythical Events: {GetMythicalEvents().Count,-42}║");
        sb.AppendLine("╠══════════════════════════════════════════════════════════════╣");

        // Group by generation
        for (int gen = 9; gen >= 1; gen--)
        {
            var genEvents = GetByGeneration(gen);
            if (genEvents.Count > 0)
            {
                sb.AppendLine($"║ Generation {gen}: {genEvents.Count} events{new string(' ', 40 - genEvents.Count.ToString().Length)}║");
            }
        }

        sb.AppendLine("╚══════════════════════════════════════════════════════════════╝");

        return sb.ToString();
    }
}

/// <summary>
/// Template for an event Pokemon
/// </summary>
public class EventTemplate
{
    public ushort Species { get; set; }
    public byte Form { get; set; }
    public string EventName { get; set; } = "";
    public string OT { get; set; } = "";
    public int TID { get; set; }
    public bool IsShiny { get; set; }
    public int Level { get; set; }
    public Ball Ball { get; set; }
    public ushort[]? Moves { get; set; }
    public int Generation { get; set; }
    public string Region { get; set; } = "International";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool RequiresItem { get; set; }

    public string GetSpeciesDisplayName() => PKHeX.Core.SpeciesName.GetSpeciesName(Species, 2);
    public bool IsActive => (!StartDate.HasValue || StartDate <= DateTime.Now) &&
                           (!EndDate.HasValue || EndDate >= DateTime.Now);
}
