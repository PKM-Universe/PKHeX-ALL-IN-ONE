using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PKHeX.Core;
using PKHeX.Core.AutoMod;

namespace PKHeX.WinForms.Plugins;

/// <summary>
/// ALM (Auto Legality Mod) enhanced Showdown import - Creates legal Pokemon from Showdown sets
/// Uses the full PKHeX.Core.AutoMod legalization engine
/// </summary>
public static class ALMShowdownPlugin
{
    /// <summary>
    /// Import a Showdown set and automatically make it legal using the full ALM engine
    /// </summary>
    public static PKM? ImportShowdownSetWithLegality(string showdownText, SaveFile sav)
    {
        var sets = BattleTemplateTeams.TryGetSets(showdownText);
        var set = sets.FirstOrDefault();
        if (set == null || set.Species == 0)
            return null;

        return GenerateLegalPokemon(set, sav);
    }

    /// <summary>
    /// Import multiple Showdown sets and generate legal Pokemon
    /// </summary>
    public static List<PKM> ImportTeamWithLegality(string showdownText, SaveFile sav)
    {
        var results = new List<PKM>();
        var sets = BattleTemplateTeams.TryGetSets(showdownText);

        foreach (var set in sets)
        {
            if (set.Species == 0) continue;
            var pk = GenerateLegalPokemon(set, sav);
            if (pk != null)
                results.Add(pk);
        }

        return results;
    }

    /// <summary>
    /// Generate a legal Pokemon from a battle template set using the full ALM engine
    /// </summary>
    public static PKM? GenerateLegalPokemon(IBattleTemplate set, SaveFile sav)
    {
        try
        {
            // Use the proper ALM GetLegalFromSet extension method
            var result = sav.GetLegalFromSet(set);
            var pk = result.Created;

            // Check if legalization was successful
            if (result.Status == LegalizationResult.Regenerated ||
                result.Status == LegalizationResult.Timeout ||
                result.Status == LegalizationResult.Failed)
            {
                // Try alternative approach with ShowdownSet
                var showdownSet = set as ShowdownSet;
                if (showdownSet != null)
                {
                    var regen = new RegenTemplate(showdownSet, sav.Generation);
                    var retryResult = sav.GetLegalFromSet(regen);
                    pk = retryResult.Created;
                }
            }

            // Final check
            if (pk == null || pk.Species == 0)
                return FallbackGeneration(set, sav);

            return pk;
        }
        catch (Exception)
        {
            // Fallback to basic generation if ALM fails
            return FallbackGeneration(set, sav);
        }
    }

    /// <summary>
    /// Fallback generation when ALM engine fails
    /// </summary>
    private static PKM? FallbackGeneration(IBattleTemplate set, SaveFile sav)
    {
        var pk = sav.BlankPKM;
        pk.Species = set.Species;
        pk.Form = set.Form;

        // Apply basic template
        ApplyBasicTemplate(pk, set, sav);

        // Try encounter-based generation
        var encounters = EncounterMovesetGenerator.GenerateEncounters(pk, set.Moves, sav.Version);
        var encounter = encounters.FirstOrDefault();

        if (encounter != null)
        {
            var criteria = EncounterCriteria.Unrestricted;
            var generated = encounter.ConvertToPKM(sav, criteria);
            if (generated != null)
            {
                ApplySetDetails(generated, set, sav);
                return generated;
            }
        }

        return pk;
    }

    private static void ApplyBasicTemplate(PKM pk, IBattleTemplate set, SaveFile sav)
    {
        pk.Species = set.Species;
        pk.Form = set.Form;
        pk.CurrentLevel = (byte)Math.Clamp((int)set.Level, 1, 100);

        // Gender
        if (set.Gender.HasValue)
            pk.Gender = set.Gender.Value;
        else
        {
            var pi = pk.PersonalInfo;
            pk.Gender = (byte)(pi.Genderless ? 2 : (pi.OnlyFemale ? 1 : 0));
        }

        // Nature
        pk.Nature = set.Nature != Nature.Random ? set.Nature : Nature.Adamant;
        pk.StatNature = pk.Nature;

        // Ability
        if (set.Ability >= 0)
        {
            var abilities = pk.PersonalInfo;
            for (int i = 0; i < abilities.AbilityCount; i++)
            {
                if (abilities.GetAbilityAtIndex(i) == set.Ability)
                {
                    pk.AbilityNumber = 1 << i;
                    pk.Ability = (ushort)set.Ability;
                    break;
                }
            }
        }

        // Moves
        var moves = set.Moves;
        pk.Move1 = moves.Length > 0 ? moves[0] : (ushort)0;
        pk.Move2 = moves.Length > 1 ? moves[1] : (ushort)0;
        pk.Move3 = moves.Length > 2 ? moves[2] : (ushort)0;
        pk.Move4 = moves.Length > 3 ? moves[3] : (ushort)0;
        pk.SetMaximumPPCurrent(moves);
        pk.FixMoves();

        // IVs
        var ivs = set.IVs;
        pk.IV_HP = ivs[0]; pk.IV_ATK = ivs[1]; pk.IV_DEF = ivs[2];
        pk.IV_SPA = ivs[3]; pk.IV_SPD = ivs[4]; pk.IV_SPE = ivs[5];

        // EVs
        var evs = set.EVs;
        pk.EV_HP = evs[0]; pk.EV_ATK = evs[1]; pk.EV_DEF = evs[2];
        pk.EV_SPA = evs[3]; pk.EV_SPD = evs[4]; pk.EV_SPE = evs[5];

        // Item
        if (set.HeldItem > 0)
            pk.HeldItem = set.HeldItem;

        // Trainer info
        pk.OriginalTrainerName = sav.OT;
        pk.OriginalTrainerGender = sav.Gender;
        pk.TID16 = sav.TID16;
        pk.SID16 = sav.SID16;
        pk.Language = sav.Language;

        // Met data
        pk.MetDate = DateOnly.FromDateTime(DateTime.Now);
        pk.MetLocation = pk.Context switch
        {
            EntityContext.Gen9 => 6,
            EntityContext.Gen8b => 3,
            EntityContext.Gen8a => 6,
            EntityContext.Gen8 => 6,
            EntityContext.Gen7 => 6,
            _ => 30001
        };
        pk.MetLevel = 1;
        pk.Ball = (byte)Ball.Poke;
        pk.Version = sav.Version;

        // Shiny
        if (set.Shiny)
            pk.SetShinySID(Shiny.AlwaysStar);

        // Nickname
        if (!string.IsNullOrEmpty(set.Nickname))
            pk.SetNickname(set.Nickname);
        else
            pk.SetDefaultNickname();

        pk.RefreshChecksum();
    }

    private static void ApplySetDetails(PKM pk, IBattleTemplate set, SaveFile sav)
    {
        // Moves
        var moves = set.Moves;
        pk.Move1 = moves.Length > 0 ? moves[0] : pk.Move1;
        pk.Move2 = moves.Length > 1 ? moves[1] : pk.Move2;
        pk.Move3 = moves.Length > 2 ? moves[2] : pk.Move3;
        pk.Move4 = moves.Length > 3 ? moves[3] : pk.Move4;
        pk.SetMaximumPPCurrent(moves);

        // EVs
        var evs = set.EVs;
        pk.EV_HP = evs[0]; pk.EV_ATK = evs[1]; pk.EV_DEF = evs[2];
        pk.EV_SPA = evs[3]; pk.EV_SPD = evs[4]; pk.EV_SPE = evs[5];

        // Item
        if (set.HeldItem > 0)
            pk.HeldItem = set.HeldItem;

        // Nature
        if (set.Nature != Nature.Random)
        {
            pk.Nature = set.Nature;
            pk.StatNature = set.Nature;
        }

        // Nickname
        if (!string.IsNullOrEmpty(set.Nickname))
            pk.SetNickname(set.Nickname);

        // Shiny
        if (set.Shiny)
            pk.SetShiny();

        pk.RefreshChecksum();
    }

    /// <summary>
    /// Legalize an existing Pokemon using the full ALM engine
    /// </summary>
    public static PKM? LegalizeExisting(PKM pk, SaveFile sav)
    {
        try
        {
            var la = new LegalityAnalysis(pk);
            if (la.Valid)
                return pk; // Already legal

            // Try to legalize using ALM
            var result = sav.Legalize(pk);
            return result;
        }
        catch
        {
            return pk; // Return original if legalization fails
        }
    }

    /// <summary>
    /// Generate a Pokemon from Smogon sets
    /// </summary>
    public static PKM? GenerateFromSmogon(PKM template, SaveFile sav)
    {
        try
        {
            // Use SmogonSetGenerator from PKHeX.Core.AutoMod namespace to fetch sets from Smogon
            var generator = new PKHeX.Core.AutoMod.SmogonSetGenerator(template);
            if (!generator.Valid || generator.Sets.Count == 0)
                return null;

            // Use the first set
            var set = generator.Sets.First();
            return GenerateLegalPokemon(set, sav);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Check if a showdown set is valid
    /// </summary>
    public static bool ValidateShowdownSet(string text, out string errors)
    {
        var sb = new StringBuilder();
        var sets = BattleTemplateTeams.TryGetSets(text);

        if (!sets.Any())
        {
            errors = "No valid Showdown sets found in clipboard.";
            return false;
        }

        foreach (var set in sets)
        {
            if (set.Species == 0)
            {
                sb.AppendLine("Invalid species in set.");
                continue;
            }

            var invalid = set.InvalidLines;
            if (invalid.Count > 0)
            {
                foreach (var line in invalid)
                {
                    sb.AppendLine($"Invalid line: {line}");
                }
            }
        }

        errors = sb.ToString();
        return string.IsNullOrEmpty(errors);
    }
}
