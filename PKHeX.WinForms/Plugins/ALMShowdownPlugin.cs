using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms.Plugins;

/// <summary>
/// ALM (Auto Legality Mod) enhanced Showdown import - Creates legal Pokemon from Showdown sets
/// </summary>
public static class ALMShowdownPlugin
{
    /// <summary>
    /// Import a Showdown set and automatically make it legal
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
    /// Generate a legal Pokemon from a battle template set
    /// </summary>
    public static PKM? GenerateLegalPokemon(IBattleTemplate set, SaveFile sav)
    {
        var pk = sav.BlankPKM;
        pk.Species = set.Species;
        pk.Form = set.Form;

        // Apply the template
        ApplyTemplate(pk, set, sav);

        // Try to make it legal
        var legalizer = new AutoLegalityPlugin(sav);
        var result = legalizer.FixLegality(pk);

        // If still not legal, try alternative approaches
        if (!result.IsNowLegal && !result.WasAlreadyLegal)
        {
            pk = TryAlternativeLegalization(set, sav);
        }

        return pk;
    }

    private static void ApplyTemplate(PKM pk, IBattleTemplate set, SaveFile sav)
    {
        // Species and Form
        pk.Species = set.Species;
        pk.Form = set.Form;

        // Nickname
        if (!string.IsNullOrEmpty(set.Nickname))
        {
            pk.SetNickname(set.Nickname);
        }
        else
        {
            pk.SetDefaultNickname();
        }

        // Level
        pk.CurrentLevel = (byte)Math.Clamp((int)set.Level, 1, 100);

        // Gender
        if (set.Gender.HasValue)
        {
            pk.Gender = set.Gender.Value;
        }
        else
        {
            var pi = pk.PersonalInfo;
            pk.Gender = (byte)(pi.Genderless ? 2 : (pi.OnlyFemale ? 1 : 0));
        }

        // Nature
        if (set.Nature != Nature.Random)
            pk.Nature = set.Nature;
        else
            pk.Nature = Nature.Adamant;

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
        pk.IV_HP = ivs[0];
        pk.IV_ATK = ivs[1];
        pk.IV_DEF = ivs[2];
        pk.IV_SPA = ivs[3];
        pk.IV_SPD = ivs[4];
        pk.IV_SPE = ivs[5];

        // EVs
        var evs = set.EVs;
        pk.EV_HP = evs[0];
        pk.EV_ATK = evs[1];
        pk.EV_DEF = evs[2];
        pk.EV_SPA = evs[3];
        pk.EV_SPD = evs[4];
        pk.EV_SPE = evs[5];

        // Held Item
        if (set.HeldItem > 0)
            pk.HeldItem = set.HeldItem;

        // Shiny - set with valid PID/EC for the generation
        if (set.Shiny)
        {
            MakeShinyValid(pk);
        }

        // Tera Type for Gen 9
        if (pk is ITeraType tera && set is ITeraType setTera && pk is PK9 pk9)
        {
            pk9.TeraTypeOriginal = setTera.TeraType;
        }

        // Set trainer info
        pk.OriginalTrainerName = sav.OT;
        pk.OriginalTrainerGender = sav.Gender;
        pk.TID16 = sav.TID16;
        pk.SID16 = sav.SID16;
        pk.Language = sav.Language;

        // Set valid met data
        SetMetData(pk, sav);

        pk.RefreshChecksum();
    }

    private static void SetMetData(PKM pk, SaveFile sav)
    {
        pk.MetDate = DateOnly.FromDateTime(DateTime.Now);

        // Set appropriate met location based on context
        pk.MetLocation = pk.Context switch
        {
            EntityContext.Gen9 => 6, // Mesagoza
            EntityContext.Gen8b => 3, // Twinleaf Town
            EntityContext.Gen8a => 6, // Jubilife Village
            EntityContext.Gen8 => 6, // Postwick
            EntityContext.Gen7 => 6, // Route 1
            EntityContext.Gen6 => 6, // Route 1
            _ => 30001 // Poke Transfer
        };

        pk.MetLevel = 1;
        if (pk.CurrentLevel < pk.MetLevel)
            pk.MetLevel = pk.CurrentLevel;

        // Ball
        pk.Ball = (byte)Ball.Poke;

        // Version
        pk.Version = sav.Version;
    }

    private static PKM? TryAlternativeLegalization(IBattleTemplate set, SaveFile sav)
    {
        // Try with encounter-based generation
        var pk = sav.BlankPKM;
        pk.Species = set.Species;
        pk.Form = set.Form;

        // Find a legal encounter
        var encounters = EncounterMovesetGenerator.GenerateEncounters(pk, set.Moves, sav.Version);
        var encounter = encounters.FirstOrDefault();

        if (encounter == null)
            return pk;

        // Generate from encounter using basic criteria
        var criteria = EncounterCriteria.Unrestricted;

        var generated = encounter.ConvertToPKM(sav, criteria);
        if (generated == null)
            return pk;

        // Apply moves and other properties
        var moves = set.Moves;
        generated.Move1 = moves.Length > 0 ? moves[0] : (ushort)0;
        generated.Move2 = moves.Length > 1 ? moves[1] : (ushort)0;
        generated.Move3 = moves.Length > 2 ? moves[2] : (ushort)0;
        generated.Move4 = moves.Length > 3 ? moves[3] : (ushort)0;
        generated.SetMaximumPPCurrent(moves);

        // Apply EVs
        var evs = set.EVs;
        generated.EV_HP = evs[0];
        generated.EV_ATK = evs[1];
        generated.EV_DEF = evs[2];
        generated.EV_SPA = evs[3];
        generated.EV_SPD = evs[4];
        generated.EV_SPE = evs[5];

        // Held item
        if (set.HeldItem > 0)
            generated.HeldItem = set.HeldItem;

        // Nickname
        if (!string.IsNullOrEmpty(set.Nickname))
            generated.SetNickname(set.Nickname);

        // Apply shiny status
        if (set.Shiny)
            generated.SetShiny();

        // Apply nature
        if (set.Nature != Nature.Random)
        {
            generated.Nature = set.Nature;
            generated.StatNature = set.Nature;
        }

        generated.RefreshChecksum();
        return generated;
    }

    /// <summary>
    /// Make a Pokemon shiny with a valid PID/EC for its generation
    /// Uses SetShinySID for better compatibility
    /// </summary>
    private static void MakeShinyValid(PKM pk)
    {
        // Use SetShinySID which adjusts SID to make the current PID shiny
        // This is more compatible than regenerating PID
        pk.SetShinySID(Shiny.AlwaysStar);

        // For Gen 8+, also ensure the shiny flag is properly set
        if (pk.Format >= 8)
        {
            // EC-based shiny for modern games - adjust EC if needed
            if (!pk.IsShiny)
            {
                pk.SetShiny();
            }
        }
    }

    private static AbilityPermission GetAbilityNumber(int ability, IPersonalInfo pi)
    {
        if (ability < 0)
            return AbilityPermission.Any12;

        for (int i = 0; i < pi.AbilityCount; i++)
        {
            if (pi.GetAbilityAtIndex(i) == ability)
            {
                return i switch
                {
                    0 => AbilityPermission.OnlyFirst,
                    1 => AbilityPermission.OnlySecond,
                    2 => AbilityPermission.OnlyHidden,
                    _ => AbilityPermission.Any12
                };
            }
        }

        return AbilityPermission.Any12;
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
