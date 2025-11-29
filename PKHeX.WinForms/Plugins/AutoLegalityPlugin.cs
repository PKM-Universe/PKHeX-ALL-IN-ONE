using System;
using System.Collections.Generic;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms.Plugins;

/// <summary>
/// Auto-Legality Mod - One-click legality fixing for Pokemon
/// </summary>
public class AutoLegalityPlugin
{
    private readonly SaveFile SAV;

    public AutoLegalityPlugin(SaveFile sav)
    {
        SAV = sav;
    }

    /// <summary>
    /// Automatically fix common legality issues on a Pokemon
    /// </summary>
    public LegalityFixResult FixLegality(PKM pk)
    {
        var result = new LegalityFixResult { Pokemon = pk };
        var la = new LegalityAnalysis(pk);

        if (la.Valid)
        {
            result.WasAlreadyLegal = true;
            return result;
        }

        var fixes = new List<string>();

        // Fix Met Location
        if (!IsMetLocationValid(pk))
        {
            FixMetLocation(pk);
            fixes.Add("Met Location");
        }

        // Fix Met Level
        if (!IsMetLevelValid(pk))
        {
            FixMetLevel(pk);
            fixes.Add("Met Level");
        }

        // Fix OT
        if (string.IsNullOrEmpty(pk.OriginalTrainerName))
        {
            pk.OriginalTrainerName = SAV.OT;
            pk.OriginalTrainerGender = SAV.Gender;
            fixes.Add("Original Trainer");
        }

        // Fix TID/SID
        if (pk.TID16 == 0)
        {
            pk.TID16 = SAV.TID16;
            pk.SID16 = SAV.SID16;
            fixes.Add("Trainer ID");
        }

        // Fix Ball
        if (!IsBallValid(pk))
        {
            FixBall(pk);
            fixes.Add("Ball");
        }

        // Fix Ability
        if (!IsAbilityValid(pk))
        {
            FixAbility(pk);
            fixes.Add("Ability");
        }

        // Fix Moves
        FixMoves(pk, fixes);

        // Fix IVs for Legendaries
        if (IsLegendary(pk.Species) && pk.IV_HP < 31)
        {
            SetPerfectIVs(pk, 3);
            fixes.Add("IVs (Legendary minimum)");
        }

        // Fix Ribbons
        FixRibbons(pk, fixes);

        // Fix Memory
        FixMemory(pk, fixes);

        // Recalculate stats
        pk.ResetCalculatedValues();

        // Re-check legality
        var newLa = new LegalityAnalysis(pk);
        result.IsNowLegal = newLa.Valid;
        result.FixesApplied = fixes;
        result.RemainingIssues = GetRemainingIssues(newLa);

        return result;
    }

    private bool IsMetLocationValid(PKM pk)
    {
        // Basic check - location should not be 0 for most Pokemon
        return pk.MetLocation != 0 || pk.Species == (int)Species.Mew;
    }

    private void FixMetLocation(PKM pk)
    {
        // Set to a valid met location based on game version
        pk.MetLocation = pk.Context switch
        {
            EntityContext.Gen9 => 6, // Mesagoza
            EntityContext.Gen8b => 3, // Twinleaf Town
            EntityContext.Gen8a => 6, // Jubilife Village
            EntityContext.Gen8 => 6, // Postwick
            EntityContext.Gen7 => 6, // Route 1
            _ => 30001 // Poké Transfer
        };
    }

    private bool IsMetLevelValid(PKM pk)
    {
        return pk.MetLevel > 0 && pk.MetLevel <= pk.CurrentLevel;
    }

    private void FixMetLevel(PKM pk)
    {
        pk.MetLevel = Math.Min((byte)1, pk.CurrentLevel);
    }

    private bool IsBallValid(PKM pk)
    {
        return BallVerifier.VerifyBall(new LegalityAnalysis(pk)).Valid;
    }

    private void FixBall(PKM pk)
    {
        // Set to Poke Ball as it's almost always legal
        pk.Ball = (byte)Ball.Poke;
    }

    private bool IsAbilityValid(PKM pk)
    {
        var pi = pk.PersonalInfo;
        return pk.AbilityNumber switch
        {
            1 => pi.Ability1 == pk.Ability,
            2 => pi.Ability2 == pk.Ability,
            4 => pi.AbilityH == pk.Ability,
            _ => false
        };
    }

    private void FixAbility(PKM pk)
    {
        var pi = pk.PersonalInfo;
        pk.Ability = pi.Ability1;
        pk.AbilityNumber = 1;
    }

    private void FixMoves(PKM pk, List<string> fixes)
    {
        var validMoves = GetValidMoves(pk);
        bool fixed = false;

        for (int i = 0; i < 4; i++)
        {
            var move = i switch
            {
                0 => pk.Move1,
                1 => pk.Move2,
                2 => pk.Move3,
                _ => pk.Move4
            };

            if (move != 0 && !validMoves.Contains(move))
            {
                switch (i)
                {
                    case 0: pk.Move1 = 0; break;
                    case 1: pk.Move2 = 0; break;
                    case 2: pk.Move3 = 0; break;
                    case 3: pk.Move4 = 0; break;
                }
                fixed = true;
            }
        }

        // Ensure at least one move
        if (pk.Move1 == 0 && validMoves.Count > 0)
        {
            pk.Move1 = validMoves[0];
            fixed = true;
        }

        if (fixed)
            fixes.Add("Moves");

        pk.FixMoves();
    }

    private List<ushort> GetValidMoves(PKM pk)
    {
        var validMoves = new List<ushort>();
        var moves = MoveList.GetValidMoves(pk, pk.Format);
        foreach (var move in moves)
            validMoves.Add(move);
        return validMoves;
    }

    private void SetPerfectIVs(PKM pk, int count)
    {
        var ivs = new int[] { pk.IV_HP, pk.IV_ATK, pk.IV_DEF, pk.IV_SPA, pk.IV_SPD, pk.IV_SPE };
        var indices = new List<int> { 0, 1, 2, 3, 4, 5 };

        int set = 0;
        foreach (var idx in indices)
        {
            if (set >= count) break;
            if (ivs[idx] < 31)
            {
                ivs[idx] = 31;
                set++;
            }
        }

        pk.IV_HP = ivs[0];
        pk.IV_ATK = ivs[1];
        pk.IV_DEF = ivs[2];
        pk.IV_SPA = ivs[3];
        pk.IV_SPD = ivs[4];
        pk.IV_SPE = ivs[5];
    }

    private void FixRibbons(PKM pk, List<string> fixes)
    {
        // Remove invalid ribbons based on generation
        if (pk is IRibbonSetCommon7 r7 && pk.Generation < 7)
        {
            if (r7.RibbonChampionAlola) { r7.RibbonChampionAlola = false; fixes.Add("Ribbons"); }
        }
    }

    private void FixMemory(PKM pk, List<string> fixes)
    {
        if (pk is IMemoryOT m && m.OriginalTrainerMemory == 0)
        {
            m.OriginalTrainerMemory = 1; // Generic memory
            m.OriginalTrainerMemoryIntensity = 1;
            m.OriginalTrainerMemoryFeeling = 0;
            fixes.Add("Memory");
        }
    }

    private bool IsLegendary(ushort species)
    {
        return species switch
        {
            (int)Species.Mewtwo or (int)Species.Lugia or (int)Species.HoOh or
            (int)Species.Rayquaza or (int)Species.Dialga or (int)Species.Palkia or
            (int)Species.Giratina or (int)Species.Zekrom or (int)Species.Reshiram or
            (int)Species.Kyurem or (int)Species.Xerneas or (int)Species.Yveltal or
            (int)Species.Zygarde or (int)Species.Solgaleo or (int)Species.Lunala or
            (int)Species.Necrozma or (int)Species.Zacian or (int)Species.Zamazenta or
            (int)Species.Eternatus or (int)Species.Calyrex or (int)Species.Koraidon or
            (int)Species.Miraidon => true,
            _ => false
        };
    }

    private List<string> GetRemainingIssues(LegalityAnalysis la)
    {
        var issues = new List<string>();
        foreach (var check in la.Results)
        {
            if (!check.Valid)
                issues.Add($"{check.Identifier}: {check.Comment}");
        }
        return issues;
    }

    /// <summary>
    /// Batch fix all Pokemon in box
    /// </summary>
    public BatchFixResult FixBox(int box)
    {
        var result = new BatchFixResult();
        var pokemon = SAV.GetBoxData(box);

        foreach (var pk in pokemon)
        {
            if (pk.Species == 0) continue;

            result.TotalProcessed++;
            var fixResult = FixLegality(pk);

            if (fixResult.WasAlreadyLegal)
                result.AlreadyLegal++;
            else if (fixResult.IsNowLegal)
                result.Fixed++;
            else
                result.CouldNotFix++;
        }

        SAV.SetBoxData(pokemon, box);
        return result;
    }

    /// <summary>
    /// Fix all Pokemon in all boxes
    /// </summary>
    public BatchFixResult FixAllBoxes()
    {
        var result = new BatchFixResult();

        for (int box = 0; box < SAV.BoxCount; box++)
        {
            var boxResult = FixBox(box);
            result.TotalProcessed += boxResult.TotalProcessed;
            result.AlreadyLegal += boxResult.AlreadyLegal;
            result.Fixed += boxResult.Fixed;
            result.CouldNotFix += boxResult.CouldNotFix;
        }

        return result;
    }
}

public class LegalityFixResult
{
    public PKM Pokemon { get; set; } = null!;
    public bool WasAlreadyLegal { get; set; }
    public bool IsNowLegal { get; set; }
    public List<string> FixesApplied { get; set; } = new();
    public List<string> RemainingIssues { get; set; } = new();
}

public class BatchFixResult
{
    public int TotalProcessed { get; set; }
    public int AlreadyLegal { get; set; }
    public int Fixed { get; set; }
    public int CouldNotFix { get; set; }
}
