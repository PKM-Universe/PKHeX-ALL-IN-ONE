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

        // Preserve shiny status before making changes
        bool wasShiny = pk.IsShiny;

        var la = new LegalityAnalysis(pk);

        if (la.Valid)
        {
            result.WasAlreadyLegal = true;
            return result;
        }

        var fixes = new List<string>();

        // Fix PID if needed (for shiny compatibility)
        if (wasShiny && HasPIDIssue(la))
        {
            FixShinyPID(pk);
            fixes.Add("PID (Shiny)");
        }

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

        // Fix Zygarde form (enforce Complete form rules for Z-A)
        if (pk.Species == (int)Species.Zygarde)
        {
            FixZygardeForm(pk, fixes);
        }

        // Fix Ribbons
        FixRibbons(pk, fixes);

        // Fix Memory
        FixMemory(pk, fixes);

        // Recalculate checksum
        pk.RefreshChecksum();

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
        var result = BallVerifier.VerifyBall(new LegalityAnalysis(pk));
        return result == BallVerificationResult.ValidEncounter || result == BallVerificationResult.ValidInheritedSpecies;
    }

    private void FixBall(PKM pk)
    {
        // Set to Poke Ball as it's almost always legal
        pk.Ball = (byte)Ball.Poke;
    }

    private bool IsAbilityValid(PKM pk)
    {
        var pi = pk.PersonalInfo;
        var ability = pk.Ability;
        // Check if the ability matches any of the valid abilities for this species
        for (int i = 0; i < pi.AbilityCount; i++)
        {
            if (pi.GetAbilityAtIndex(i) == ability)
                return true;
        }
        return false;
    }

    private void FixAbility(PKM pk)
    {
        var pi = pk.PersonalInfo;
        pk.Ability = (ushort)pi.GetAbilityAtIndex(0);
        pk.AbilityNumber = 1;
    }

    private void FixMoves(PKM pk, List<string> fixes)
    {
        var validMoves = GetValidMoves(pk);
        bool wasFixed = false;

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
                wasFixed = true;
            }
        }

        // Ensure at least one move
        if (pk.Move1 == 0 && validMoves.Count > 0)
        {
            pk.Move1 = validMoves[0];
            wasFixed = true;
        }

        if (wasFixed)
            fixes.Add("Moves");

        pk.FixMoves();
    }

    private List<ushort> GetValidMoves(PKM pk)
    {
        // Get the current moves as a simple valid list
        // For a full implementation, would use LegalMoveSource
        var validMoves = new List<ushort>();
        if (pk.Move1 != 0) validMoves.Add(pk.Move1);
        if (pk.Move2 != 0) validMoves.Add(pk.Move2);
        if (pk.Move3 != 0) validMoves.Add(pk.Move3);
        if (pk.Move4 != 0) validMoves.Add(pk.Move4);

        // If no moves, add Tackle as a default (almost universally learnable)
        if (validMoves.Count == 0)
            validMoves.Add(33); // Tackle

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

    /// <summary>
    /// Fix Zygarde form and ability for proper legality
    /// Forms: 0 = 50%, 1 = 10%, 2 = 10% (Power Construct), 3 = 50% (Power Construct), 4 = Complete
    /// </summary>
    private void FixZygardeForm(PKM pk, List<string> fixes)
    {
        // Zygarde Complete (form 4) cannot exist outside of battle
        // It transforms from 10% or 50% with Power Construct ability
        if (pk.Form == 4)
        {
            // Complete form is battle-only, revert to 50% with Power Construct
            pk.Form = 3; // 50% Power Construct form
            pk.Ability = (ushort)PKHeX.Core.Ability.PowerConstruct;
            pk.AbilityNumber = 4; // Hidden ability slot
            fixes.Add("Zygarde Form (Complete → 50% Power Construct)");
        }
        // For Z-A (Gen9a), enforce Power Construct forms
        else if (pk.Context == EntityContext.Gen9 && pk is PA9)
        {
            // Z-A Zygarde should have Power Construct
            if (pk.Ability != (ushort)PKHeX.Core.Ability.PowerConstruct)
            {
                // Set to Power Construct form based on current form
                if (pk.Form == 0) // 50%
                    pk.Form = 3; // 50% with Power Construct
                else if (pk.Form == 1) // 10%
                    pk.Form = 2; // 10% with Power Construct

                pk.Ability = (ushort)PKHeX.Core.Ability.PowerConstruct;
                pk.AbilityNumber = 4;
                fixes.Add("Zygarde Ability (Power Construct for Z-A)");
            }
        }
        // Validate ability matches form for other games
        else
        {
            bool hasPowerConstruct = pk.Ability == (ushort)PKHeX.Core.Ability.PowerConstruct;
            bool isPowerConstructForm = pk.Form == 2 || pk.Form == 3;

            if (hasPowerConstruct && !isPowerConstructForm)
            {
                // Has Power Construct but wrong form - fix form
                pk.Form = pk.Form == 1 ? (byte)2 : (byte)3;
                fixes.Add("Zygarde Form (matched to Power Construct)");
            }
            else if (!hasPowerConstruct && isPowerConstructForm)
            {
                // Power Construct form but wrong ability - fix ability
                pk.Ability = (ushort)PKHeX.Core.Ability.PowerConstruct;
                pk.AbilityNumber = 4;
                fixes.Add("Zygarde Ability (Power Construct for form)");
            }
        }
    }

    private bool HasPIDIssue(LegalityAnalysis la)
    {
        foreach (var check in la.Results)
        {
            if (!check.Valid && check.Identifier.ToString().Contains("PID"))
                return true;
        }
        return false;
    }

    private void FixShinyPID(PKM pk)
    {
        // Check if this is a BDSP roaming encounter (Mesprit/Cresselia)
        if (pk is PB8 pb8 && IsBDSPRoamingSpecies(pk.Species))
        {
            FixBDSPRoamingShiny(pb8);
            return;
        }

        // For other Pokemon, use proper shiny PID calculation
        FixGenericShinyPID(pk);
    }

    private bool IsBDSPRoamingSpecies(ushort species)
    {
        // Mesprit (481) and Cresselia (488) are roaming in BDSP
        return species == (int)Species.Mesprit || species == (int)Species.Cresselia;
    }

    private void FixBDSPRoamingShiny(PB8 pk)
    {
        // For BDSP roaming Pokemon, we need to find a valid EC seed that produces shiny PID
        // The EC is used as the RNG seed, and PID/IVs/Height/Weight all derive from it

        // Save current values we want to preserve
        var nature = pk.Nature;
        var statNature = pk.StatNature;
        var moves = new ushort[] { pk.Move1, pk.Move2, pk.Move3, pk.Move4 };
        var evs = new int[] { pk.EV_HP, pk.EV_ATK, pk.EV_DEF, pk.EV_SPA, pk.EV_SPD, pk.EV_SPE };
        var heldItem = pk.HeldItem;
        var nickname = pk.IsNicknamed ? pk.Nickname : null;

        // Create criteria that requests shiny
        var criteria = new EncounterCriteria
        {
            Shiny = Shiny.AlwaysStar
        };

        // Use the Roaming8bRNG to find a valid shiny seed
        // This searches up to 70,000 seeds to find one that:
        // 1. Produces a shiny PID for this trainer's TID/SID
        // 2. Has valid IVs, height, weight derived from the same seed
        Roaming8bRNG.ApplyDetails(pk, criteria, Shiny.AlwaysStar, 3);

        // Restore nature (can be set by Synchronize in-game)
        pk.Nature = nature;
        pk.StatNature = statNature;

        // Restore moves
        pk.Move1 = moves[0];
        pk.Move2 = moves[1];
        pk.Move3 = moves[2];
        pk.Move4 = moves[3];
        pk.SetMaximumPPCurrent(moves);

        // Restore EVs
        pk.EV_HP = evs[0];
        pk.EV_ATK = evs[1];
        pk.EV_DEF = evs[2];
        pk.EV_SPA = evs[3];
        pk.EV_SPD = evs[4];
        pk.EV_SPE = evs[5];

        // Restore held item
        pk.HeldItem = heldItem;

        // Restore nickname if it had one
        if (nickname != null)
            pk.SetNickname(nickname);

        pk.RefreshChecksum();
    }

    private void FixGenericShinyPID(PKM pk)
    {
        // Use ShinyUtil.GetShinyPID for proper shiny PID calculation
        // This modifies the upper half of the PID to make it shiny
        var pid = pk.PID;
        var newPid = ShinyUtil.GetShinyPID(pk.TID16, pk.SID16, pid, 1); // 1 = star shiny
        pk.PID = newPid;

        // For Gen 8+, may also need to adjust EC
        if (pk.Format >= 8 && !pk.IsShiny)
        {
            // If still not shiny after PID fix, regenerate EC
            pk.EncryptionConstant = Util.Rand32();
            pk.PID = ShinyUtil.GetShinyPID(pk.TID16, pk.SID16, pk.PID, 1);
        }

        // Final check - if legality still fails, try encounter-based regeneration
        var la = new LegalityAnalysis(pk);
        if (!la.Valid && HasPIDIssue(la))
        {
            // Try to regenerate from encounter
            TryRegenerateFromEncounter(pk);
        }
    }

    private void TryRegenerateFromEncounter(PKM pk)
    {
        // Find a matching encounter
        var la = new LegalityAnalysis(pk);
        var encounter = la.Info.EncounterOriginal;

        if (encounter == null)
            return;

        // Create shiny criteria
        var criteria = new EncounterCriteria
        {
            Shiny = Shiny.AlwaysStar
        };

        // Try to generate from the encounter
        var generated = encounter.ConvertToPKM(SAV, criteria);
        if (generated == null)
            return;

        // Copy the shiny PID/EC to our Pokemon
        pk.PID = generated.PID;
        pk.EncryptionConstant = generated.EncryptionConstant;

        // Copy IVs if they changed (some encounters have fixed IV structure)
        if (generated is IScaledSize gs && pk is IScaledSize ps)
        {
            ps.HeightScalar = gs.HeightScalar;
            ps.WeightScalar = gs.WeightScalar;
        }
    }

    private List<string> GetRemainingIssues(LegalityAnalysis la)
    {
        var issues = new List<string>();
        foreach (var check in la.Results)
        {
            if (!check.Valid)
                issues.Add($"{check.Identifier}: {check.Result}");
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
