using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;
using PKHeX.Drawing.PokeSprite;

namespace PKHeX.WinForms;

public class EventPokemonChecker : Form
{
    private readonly PKM _pokemon;
    private readonly Panel PNL_Result;
    private readonly PictureBox PB_Pokemon;
    private readonly Label L_Species;
    private readonly Label L_Result;
    private readonly Label L_MatchDetails;
    private readonly ListView LV_Matches;
    private readonly Panel PNL_Details;

    // Known event signatures
    private static readonly List<EventSignature> KnownEvents = new()
    {
        // Gen 9 Events
        new("Flying Pikachu SV", 25, "POKEMON", new[] { 25 }, 2023, true, new[] { "Fly" }),
        new("Mew (Get Mew)", 151, "Get Mew", new[] { 151 }, 2023, false, null),
        new("Ogerpon (Teal Mask)", 1017, null, null, 2023, false, null),

        // Gen 8 Events
        new("Zarude (Jungle)", 893, "Jungle", new[] { 893 }, 2020, false, new[] { "Close Combat", "Power Whip" }),
        new("Dada Zarude", 893, "Jungle", new[] { 893 }, 2021, false, new[] { "Jungle Healing" }),
        new("Shiny Zeraora (HOME)", 807, "HOME", new[] { 807 }, 2020, true, new[] { "Plasma Fists" }),
        new("Shiny Eternatus", 890, "Galar", new[] { 890 }, 2022, true, new[] { "Eternabeam" }),
        new("Victini (Movie)", 494, "Pokemon", new[] { 494 }, 2020, false, new[] { "V-Create" }),

        // Gen 7 Events
        new("Marshadow", 802, "MT. Tensei", new[] { 802 }, 2017, false, new[] { "Spectral Thief" }),
        new("Magearna (QR)", 801, "Pokemon", new[] { 801 }, 2016, false, new[] { "Fleur Cannon" }),
        new("Shiny Poipole", 803, "Ultra", new[] { 803 }, 2018, true, null),
        new("Shiny Necrozma", 800, "Hikari", new[] { 800 }, 2019, true, null),
        new("Shiny Zygarde (2018)", 718, "2018 Legends", new[] { 718 }, 2018, true, new[] { "Thousand Arrows", "Extreme Speed" }),
        new("Ash-Greninja", 658, "Ash", new[] { 658 }, 2016, false, new[] { "Water Shuriken" }),
        new("Cap Pikachu", 25, "Ash", new[] { 25 }, 2017, false, null),

        // Gen 6 Events
        new("Hoopa", 720, "Manesh", new[] { 720 }, 2015, false, new[] { "Hyperspace Hole" }),
        new("Diancie", 719, "Pokemon", new[] { 719 }, 2014, false, new[] { "Diamond Storm" }),
        new("Volcanion", 721, "Nebel", new[] { 721 }, 2016, false, new[] { "Steam Eruption" }),
        new("Shiny Rayquaza (Galileo)", 384, "Galileo", new[] { 384 }, 2015, true, new[] { "Dragon Ascent" }),

        // Gen 5 Events
        new("Genesect (Plasma)", 649, "Plasma", new[] { 649 }, 2013, false, new[] { "Techno Blast" }),
        new("Meloetta", 648, "SPR2013", new[] { 648 }, 2012, false, new[] { "Relic Song" }),
        new("Keldeo", 647, "SMR2012", new[] { 647 }, 2012, false, new[] { "Secret Sword" }),
        new("V-Create Victini", 494, "Movie14", new[] { 494 }, 2011, false, new[] { "V-Create", "Fusion Flare", "Fusion Bolt" }),

        // Gen 4 Events
        new("Shaymin (TRU)", 492, "TRU", new[] { 492 }, 2008, false, new[] { "Seed Flare" }),
        new("Darkrai (ALAMOS)", 491, "ALAMOS", new[] { 491 }, 2008, false, new[] { "Dark Void" }),
        new("Arceus (TRU)", 493, "TRU", new[] { 493 }, 2009, false, new[] { "Judgment" }),
        new("Manaphy (TRU)", 490, "TRU", new[] { 490 }, 2007, false, new[] { "Heart Swap" }),

        // Gen 3 Events
        new("WISHMKR Jirachi", 385, "WISHMKR", new[] { 385 }, 2003, false, new[] { "Wish" }),
        new("SPACE C Deoxys", 386, "SPACE C", new[] { 386 }, 2004, false, new[] { "Psycho Boost" }),
        new("MYSTRY Mew", 151, "MYSTRY", new[] { 151 }, 2005, false, null),
    };

    public EventPokemonChecker(PKM pokemon)
    {
        _pokemon = pokemon;
        Text = "Event Pokemon Checker";
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);

        var lblTitle = new Label
        {
            Text = "Event Pokemon Verification",
            Location = new Point(20, 15),
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        };

        // Pokemon info panel
        var pnlPokemon = new Panel
        {
            Location = new Point(20, 55),
            Size = new Size(350, 150),
            BackColor = Color.FromArgb(35, 35, 55),
            BorderStyle = BorderStyle.FixedSingle
        };

        PB_Pokemon = new PictureBox
        {
            Location = new Point(20, 25),
            Size = new Size(100, 100),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(45, 45, 65)
        };

        try
        {
            var shiny = pokemon.IsShiny ? Shiny.AlwaysStar : Shiny.Never;
            PB_Pokemon.Image = SpriteUtil.GetSprite(pokemon.Species, pokemon.Form, pokemon.Gender,
                0, pokemon.SpriteItem, pokemon.IsEgg, shiny, pokemon.Context);
        }
        catch { }

        L_Species = new Label
        {
            Text = $"{GameInfo.Strings.specieslist[pokemon.Species]}\nOT: {pokemon.OriginalTrainerName}\nTID: {pokemon.DisplayTID}\nLevel: {pokemon.CurrentLevel}",
            Location = new Point(130, 30),
            Size = new Size(200, 80),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10F)
        };

        pnlPokemon.Controls.AddRange(new Control[] { PB_Pokemon, L_Species });

        // Result panel
        PNL_Result = new Panel
        {
            Location = new Point(390, 55),
            Size = new Size(390, 150),
            BackColor = Color.FromArgb(35, 35, 55),
            BorderStyle = BorderStyle.FixedSingle
        };

        var lblResultTitle = new Label
        {
            Text = "Verification Result",
            Location = new Point(10, 10),
            AutoSize = true,
            ForeColor = Color.Cyan,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };

        L_Result = new Label
        {
            Location = new Point(10, 40),
            Size = new Size(370, 30),
            Font = new Font("Segoe UI", 12F, FontStyle.Bold)
        };

        L_MatchDetails = new Label
        {
            Location = new Point(10, 75),
            Size = new Size(370, 60),
            ForeColor = Color.LightGray,
            Font = new Font("Segoe UI", 9F)
        };

        PNL_Result.Controls.AddRange(new Control[] { lblResultTitle, L_Result, L_MatchDetails });

        // Matches list
        var lblMatches = new Label
        {
            Text = "Potential Event Matches:",
            Location = new Point(20, 220),
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };

        LV_Matches = new ListView
        {
            Location = new Point(20, 250),
            Size = new Size(760, 200),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            GridLines = true
        };
        LV_Matches.Columns.Add("Event Name", 200);
        LV_Matches.Columns.Add("OT Match", 80);
        LV_Matches.Columns.Add("Year", 60);
        LV_Matches.Columns.Add("Shiny", 60);
        LV_Matches.Columns.Add("Moves Match", 100);
        LV_Matches.Columns.Add("Confidence", 100);

        // Details panel
        PNL_Details = new Panel
        {
            Location = new Point(20, 460),
            Size = new Size(760, 90),
            BackColor = Color.FromArgb(35, 35, 55),
            BorderStyle = BorderStyle.FixedSingle
        };

        var lblDetails = new Label
        {
            Text = "Analysis Details",
            Location = new Point(10, 10),
            AutoSize = true,
            ForeColor = Color.Cyan,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        var lblAnalysis = new Label
        {
            Location = new Point(10, 35),
            Size = new Size(740, 45),
            ForeColor = Color.LightGray,
            Font = new Font("Segoe UI", 9F)
        };

        PNL_Details.Controls.AddRange(new Control[] { lblDetails, lblAnalysis });

        Controls.AddRange(new Control[] { lblTitle, pnlPokemon, PNL_Result, lblMatches, LV_Matches, PNL_Details });

        AnalyzePokemon(lblAnalysis);
    }

    private void AnalyzePokemon(Label lblAnalysis)
    {
        var matches = FindEventMatches();
        var analysisNotes = new List<string>();

        // Check for event indicators
        bool hasEventMoves = HasEventExclusiveMoves();
        bool hasEventRibbon = HasEventRibbon();
        bool isMythical = IsMythical(_pokemon.Species);
        bool hasSpecialOT = IsKnownEventOT(_pokemon.OriginalTrainerName);

        if (hasEventRibbon) analysisNotes.Add("Has event ribbon");
        if (hasEventMoves) analysisNotes.Add("Has event-exclusive moves");
        if (isMythical) analysisNotes.Add("Mythical Pokemon (event-only)");
        if (hasSpecialOT) analysisNotes.Add("Known event OT detected");
        if (_pokemon.FatefulEncounter) analysisNotes.Add("Fateful encounter flag set");

        // Determine result
        if (matches.Count > 0)
        {
            var bestMatch = matches.OrderByDescending(m => m.confidence).First();

            if (bestMatch.confidence >= 90)
            {
                L_Result.Text = "HIGH CONFIDENCE MATCH";
                L_Result.ForeColor = Color.LightGreen;
                L_MatchDetails.Text = $"This Pokemon closely matches: {bestMatch.eventName}\nConfidence: {bestMatch.confidence}%";
            }
            else if (bestMatch.confidence >= 60)
            {
                L_Result.Text = "POSSIBLE EVENT POKEMON";
                L_Result.ForeColor = Color.Gold;
                L_MatchDetails.Text = $"Potential match: {bestMatch.eventName}\nConfidence: {bestMatch.confidence}%\nSome details may differ from original event.";
            }
            else
            {
                L_Result.Text = "LOW CONFIDENCE";
                L_Result.ForeColor = Color.Orange;
                L_MatchDetails.Text = $"Weak match to: {bestMatch.eventName}\nThis may be edited or not from this event.";
            }
        }
        else if (isMythical || hasEventRibbon)
        {
            L_Result.Text = "LIKELY EVENT POKEMON";
            L_Result.ForeColor = Color.Cyan;
            L_MatchDetails.Text = "No exact match found, but this Pokemon has event indicators.\nMay be from an unknown or regional event.";
        }
        else
        {
            L_Result.Text = "NOT AN EVENT POKEMON";
            L_Result.ForeColor = Color.Salmon;
            L_MatchDetails.Text = "This Pokemon does not appear to be from a known event distribution.";
        }

        // Populate matches list
        foreach (var match in matches.OrderByDescending(m => m.confidence))
        {
            var item = new ListViewItem(match.eventName);
            item.SubItems.Add(match.otMatch ? "Yes" : "No");
            item.SubItems.Add(match.year.ToString());
            item.SubItems.Add(match.shinyMatch ? "Yes" : "-");
            item.SubItems.Add(match.movesMatch ? "Yes" : "Partial");
            item.SubItems.Add($"{match.confidence}%");

            item.ForeColor = match.confidence >= 90 ? Color.LightGreen :
                            match.confidence >= 60 ? Color.Gold : Color.Orange;

            LV_Matches.Items.Add(item);
        }

        lblAnalysis.Text = analysisNotes.Count > 0 ?
            "Indicators: " + string.Join(" | ", analysisNotes) :
            "No special event indicators detected.";
    }

    private List<(string eventName, bool otMatch, int year, bool shinyMatch, bool movesMatch, int confidence)> FindEventMatches()
    {
        var matches = new List<(string, bool, int, bool, bool, int)>();

        foreach (var evt in KnownEvents.Where(e => e.Species == _pokemon.Species))
        {
            int confidence = 50; // Base confidence for species match
            bool otMatch = false;
            bool shinyMatch = false;
            bool movesMatch = false;

            // OT match
            if (!string.IsNullOrEmpty(evt.OT) && _pokemon.OriginalTrainerName.Contains(evt.OT))
            {
                otMatch = true;
                confidence += 25;
            }

            // Shiny match
            if (evt.IsShiny == _pokemon.IsShiny)
            {
                shinyMatch = true;
                if (evt.IsShiny) confidence += 15;
            }
            else if (evt.IsShiny && !_pokemon.IsShiny)
            {
                confidence -= 20; // Event should be shiny but isn't
            }

            // Moves match
            if (evt.SignatureMoves != null && evt.SignatureMoves.Length > 0)
            {
                var pokemonMoves = new[] { _pokemon.Move1, _pokemon.Move2, _pokemon.Move3, _pokemon.Move4 };
                var pokemonMoveNames = pokemonMoves.Where(m => m > 0).Select(m => GameInfo.Strings.movelist[m]).ToArray();

                int matchedMoves = evt.SignatureMoves.Count(sm => pokemonMoveNames.Any(pm => pm.Contains(sm)));
                if (matchedMoves == evt.SignatureMoves.Length)
                {
                    movesMatch = true;
                    confidence += 10;
                }
                else if (matchedMoves > 0)
                {
                    confidence += 5;
                }
            }

            if (confidence > 50)
                matches.Add((evt.Name, otMatch, evt.Year, shinyMatch, movesMatch, Math.Min(100, confidence)));
        }

        return matches;
    }

    private bool HasEventExclusiveMoves()
    {
        var eventMoves = new[] { "V-Create", "Celebrate", "Happy Hour", "Hold Hands", "Hyperspace Hole",
            "Diamond Storm", "Steam Eruption", "Thousand Arrows", "Spectral Thief", "Plasma Fists",
            "Seed Flare", "Dark Void", "Judgment", "Heart Swap", "Relic Song", "Secret Sword" };

        var pokemonMoves = new[] { _pokemon.Move1, _pokemon.Move2, _pokemon.Move3, _pokemon.Move4 };
        return pokemonMoves.Where(m => m > 0).Any(m => eventMoves.Contains(GameInfo.Strings.movelist[m]));
    }

    private bool HasEventRibbon()
    {
        // Check for fateful encounter which usually indicates an event Pokemon
        // This is the most reliable indicator across all generations
        return _pokemon.FatefulEncounter;
    }

    private bool IsMythical(int species)
    {
        var mythicals = new[] { 151, 251, 385, 386, 489, 490, 491, 492, 493, 494, 647, 648, 649,
            719, 720, 721, 801, 802, 807, 893 };
        return mythicals.Contains(species);
    }

    private bool IsKnownEventOT(string ot)
    {
        var eventOTs = new[] { "POKEMON", "Pokemon", "GF", "Ash", "WISHMKR", "ALAMOS", "TRU", "Plasma",
            "Movie", "Galileo", "Jungle", "HOME", "MT. Tensei", "Hikari", "2018 Legends" };
        return eventOTs.Any(e => ot.Contains(e));
    }

    private class EventSignature
    {
        public string Name { get; }
        public int Species { get; }
        public string? OT { get; }
        public int[]? IDs { get; }
        public int Year { get; }
        public bool IsShiny { get; }
        public string[]? SignatureMoves { get; }

        public EventSignature(string name, int species, string? ot, int[]? ids, int year, bool isShiny, string[]? moves)
        {
            Name = name;
            Species = species;
            OT = ot;
            IDs = ids;
            Year = year;
            IsShiny = isShiny;
            SignatureMoves = moves;
        }
    }
}
