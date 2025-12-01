using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms.Plugins;

/// <summary>
/// Shiny Living Dex Generator - Generate complete shiny living dex
/// Similar to HexByte3's PKHeX functionality
/// </summary>
public class ShinyLivingDexGenerator
{
    private readonly SaveFile SAV;

    public ShinyLivingDexGenerator(SaveFile sav)
    {
        SAV = sav;
    }

    /// <summary>
    /// Generation options for the Shiny Living Dex
    /// </summary>
    public class GeneratorOptions
    {
        public bool ShinyOnly { get; set; } = true;
        public ShinyType ShinyType { get; set; } = ShinyType.Star;
        public bool IncludeForms { get; set; } = false;
        public bool LegalOnly { get; set; } = true;
        public int StartGeneration { get; set; } = 1;
        public int EndGeneration { get; set; } = 9;
        public int StartBox { get; set; } = 0;
        public bool SetLevel100 { get; set; } = true;
        public bool MaxIVs { get; set; } = true;
        public bool SetOT { get; set; } = true;
        public string CustomOT { get; set; } = "";
        public int CustomTID { get; set; } = -1;
        public int CustomSID { get; set; } = -1;
    }

    public enum ShinyType
    {
        Star,
        Square,
        Random
    }

    // Generation ranges
    private static readonly Dictionary<int, (ushort Start, ushort End)> GenerationRanges = new()
    {
        [1] = (1, 151),
        [2] = (152, 251),
        [3] = (252, 386),
        [4] = (387, 493),
        [5] = (494, 649),
        [6] = (650, 721),
        [7] = (722, 809),
        [8] = (810, 905),
        [9] = (906, 1025)
    };

    /// <summary>
    /// Generate a complete Shiny Living Dex and fill boxes
    /// </summary>
    public GenerationResult GenerateShinyLivingDex(GeneratorOptions? options = null)
    {
        options ??= new GeneratorOptions();
        var result = new GenerationResult();

        // Get species range
        ushort startSpecies = 1;
        ushort endSpecies = (ushort)Math.Min((int)SAV.MaxSpeciesID, 1025);

        if (GenerationRanges.ContainsKey(options.StartGeneration))
            startSpecies = GenerationRanges[options.StartGeneration].Start;
        if (GenerationRanges.ContainsKey(options.EndGeneration))
            endSpecies = GenerationRanges[options.EndGeneration].End;

        endSpecies = (ushort)Math.Min((int)endSpecies, (int)SAV.MaxSpeciesID);

        // Get species list
        var speciesToGenerate = GetSpeciesInRange(startSpecies, endSpecies)
            .Where(s => SAV.Personal.IsSpeciesInGame(s))
            .ToList();

        result.TotalSpecies = speciesToGenerate.Count;

        int currentBox = options.StartBox;
        int currentSlot = 0;

        foreach (var species in speciesToGenerate)
        {
            if (currentBox >= SAV.BoxCount)
            {
                result.Errors.Add("Ran out of box space!");
                break;
            }

            var pokemon = GenerateShinyPokemon(species, 0, options);
            if (pokemon == null)
            {
                result.Failed++;
                result.Errors.Add($"Failed to generate #{species} {SpeciesName.GetSpeciesName(species, 2)}");
                continue;
            }

            // Place in box
            SAV.SetBoxSlotAtIndex(pokemon, currentBox, currentSlot);
            result.Generated++;

            currentSlot++;
            if (currentSlot >= SAV.BoxSlotCount)
            {
                currentSlot = 0;
                currentBox++;
            }

            // Generate forms if requested
            if (options.IncludeForms)
            {
                var pi = SAV.Personal.GetFormEntry(species, 0);
                for (byte form = 1; form < pi.FormCount; form++)
                {
                    if (currentBox >= SAV.BoxCount) break;

                    var formPokemon = GenerateShinyPokemon(species, form, options);
                    if (formPokemon == null) continue;

                    SAV.SetBoxSlotAtIndex(formPokemon, currentBox, currentSlot);
                    result.FormsGenerated++;

                    currentSlot++;
                    if (currentSlot >= SAV.BoxSlotCount)
                    {
                        currentSlot = 0;
                        currentBox++;
                    }
                }
            }
        }

        result.BoxesUsed = currentBox - options.StartBox + (currentSlot > 0 ? 1 : 0);
        return result;
    }

    /// <summary>
    /// Generate a single Pokemon for the living dex
    /// </summary>
    public PKM? GenerateShinyPokemon(ushort species, byte form, GeneratorOptions options)
    {
        try
        {
            // Try encounter-based generation first
            var pk = TryGenerateFromEncounter(species, form, options);
            if (pk != null)
                return pk;

            // Fallback to basic generation
            return GenerateBasicPokemon(species, form, options);
        }
        catch
        {
            return null;
        }
    }

    private PKM? TryGenerateFromEncounter(ushort species, byte form, GeneratorOptions options)
    {
        try
        {
            var template = SAV.BlankPKM;
            template.Species = species;
            template.Form = form;
            template.Gender = template.GetSaneGender();

            var moves = ArrayPool<ushort>.Shared.Rent(4);
            var memory = moves.AsMemory(0, 4);
            var span = memory.Span;
            template.GetMoves(span);

            var encounters = EncounterMovesetGenerator.GenerateEncounters(template, SAV, memory);
            IEncounterable? selectedEncounter = null;

            // Try to find an encounter - prefer ones that can be shiny if shiny is requested
            foreach (var enc in encounters)
            {
                // Skip shiny-locked encounters if we want shiny
                if (options.ShinyOnly && enc is IShinyPotential sp && sp.Shiny == Shiny.Never)
                    continue;
                selectedEncounter = enc;
                break;
            }

            span.Clear();
            ArrayPool<ushort>.Shared.Return(moves);

            if (selectedEncounter == null)
                return null;

            // Create criteria for the encounter generation
            var criteria = new EncounterCriteria
            {
                Shiny = options.ShinyOnly ? Shiny.Always : Shiny.Never,
                IV_HP = options.MaxIVs ? (sbyte)31 : (sbyte)-1,
                IV_ATK = options.MaxIVs ? (sbyte)31 : (sbyte)-1,
                IV_DEF = options.MaxIVs ? (sbyte)31 : (sbyte)-1,
                IV_SPA = options.MaxIVs ? (sbyte)31 : (sbyte)-1,
                IV_SPD = options.MaxIVs ? (sbyte)31 : (sbyte)-1,
                IV_SPE = options.MaxIVs ? (sbyte)31 : (sbyte)-1,
            };

            // Generate with criteria if the encounter supports it
            PKM pk;
            if (selectedEncounter is IEncounterConvertible conv)
            {
                pk = conv.ConvertToPKM(SAV, criteria);
            }
            else
            {
                pk = selectedEncounter.ConvertToPKM(SAV);
            }

            if (pk == null)
                return null;

            // Set level if requested
            if (options.SetLevel100)
            {
                pk.CurrentLevel = 100;
            }

            // If shiny wasn't set by criteria (encounter may not support it), try to set it
            if (options.ShinyOnly && !pk.IsShiny)
            {
                pk.SetShiny();
            }

            // If max IVs weren't set by criteria, set them manually
            if (options.MaxIVs)
            {
                pk.IV_HP = 31;
                pk.IV_ATK = 31;
                pk.IV_DEF = 31;
                pk.IV_SPA = 31;
                pk.IV_SPD = 31;
                pk.IV_SPE = 31;
            }

            // Set move flags (TM records, move mastery, etc.)
            SetMoveFlags(pk);

            pk.Heal();
            pk.RefreshChecksum();

            // Convert to save format
            return EntityConverter.ConvertToType(pk, SAV.PKMType, out _);
        }
        catch
        {
            return null;
        }
    }

    private PKM? GenerateBasicPokemon(ushort species, byte form, GeneratorOptions options)
    {
        try
        {
            var pk = SAV.BlankPKM;
            pk.Species = species;
            pk.Form = form;
            pk.Gender = pk.GetSaneGender();
            pk.Nature = (Nature)Util.Rand.Next(25);
            pk.Ability = pk.PersonalInfo.GetAbilityAtIndex(0);

            // Set trainer info
            pk.OriginalTrainerName = string.IsNullOrEmpty(options.CustomOT) ? SAV.OT : options.CustomOT;
            pk.TID16 = options.CustomTID >= 0 ? (ushort)options.CustomTID : SAV.TID16;
            pk.SID16 = options.CustomSID >= 0 ? (ushort)options.CustomSID : SAV.SID16;
            pk.OriginalTrainerGender = SAV.Gender;
            pk.Language = SAV.Language;

            // Set level
            pk.CurrentLevel = options.SetLevel100 ? (byte)100 : (byte)50;
            pk.MetLevel = 1;

            // Set shiny using PKHeX's built-in method
            if (options.ShinyOnly)
            {
                pk.SetShiny();
            }
            else
            {
                pk.PID = Util.Rand32();
            }

            // Set IVs
            if (options.MaxIVs)
            {
                pk.IV_HP = 31;
                pk.IV_ATK = 31;
                pk.IV_DEF = 31;
                pk.IV_SPA = 31;
                pk.IV_SPD = 31;
                pk.IV_SPE = 31;
            }
            else
            {
                pk.SetRandomIVs();
            }

            // Set met data
            pk.Ball = (byte)Ball.Poke;
            pk.MetDate = DateOnly.FromDateTime(DateTime.Now);
            pk.MetLocation = GetDefaultMetLocation(pk);

            // Set valid moves
            pk.SetMoveset();
            pk.HealPP();

            // Set proper encryption constant
            pk.EncryptionConstant = Util.Rand32();

            // Set move flags (TM records, move mastery, etc.)
            SetMoveFlags(pk);

            pk.RefreshChecksum();

            return EntityConverter.ConvertToType(pk, SAV.PKMType, out _);
        }
        catch
        {
            return null;
        }
    }

    private ushort GetDefaultMetLocation(PKM pk)
    {
        return pk.Context switch
        {
            EntityContext.Gen9 => 6,    // Mesagoza
            EntityContext.Gen8b => 3,   // Twinleaf Town
            EntityContext.Gen8a => 6,   // Jubilife Village
            EntityContext.Gen8 => 6,    // Postwick
            EntityContext.Gen7 => 6,    // Route 1
            EntityContext.Gen6 => 6,    // Route 1
            EntityContext.Gen5 => 6,    // Route 1
            EntityContext.Gen4 => 6,    // Route 201
            EntityContext.Gen3 => 16,   // Route 101
            _ => 30001                  // Poke Transfer
        };
    }

    /// <summary>
    /// Sets required move flags for legality (TM records, move mastery, etc.)
    /// </summary>
    private void SetMoveFlags(PKM pk)
    {
        // Run legality analysis first to get proper context
        var la = new LegalityAnalysis(pk);

        // Handle TM/TR records for Gen 8+ (SWSH, BDSP, SV)
        // Uses extension methods from TechnicalRecordApplicator with LegalityAnalysis
        if (pk is ITechRecord tr)
        {
            // Set record flags using legality-aware method (sets flags based on current moves legally)
            tr.SetRecordFlags(pk, TechnicalRecordApplicatorOption.LegalCurrent, la);
        }

        // Handle Plus Records for Legends Z-A (PA9)
        // Uses extension methods from PlusRecordApplicator
        if (pk is IPlusRecord plus && pk is PA9 pa9)
        {
            // Get the permit from the personal info (PersonalInfo9ZA implements IPermitPlus)
            var permit = (IPermitPlus)pa9.PersonalInfo;
            // Set plus flags using legality-aware method with TM support
            plus.SetPlusFlags(permit, PlusRecordApplicatorOption.LegalCurrentTM, la);
        }

        // Handle move mastery for Legends Arceus
        // Uses extension methods from MoveShopRecordApplicator
        if (pk is IMoveShop8Mastery mastery)
        {
            // Set move shop flags for current moves
            mastery.SetMoveShopFlags(pk);
        }

        // Handle memory/affection for Gen 6/7 Pokemon
        if (pk is IAffection aff)
        {
            aff.OriginalTrainerAffection = 0;
        }

        // Handle dynamax level for Gen 8 (SWSH)
        if (pk is IDynamaxLevel dmax)
        {
            dmax.DynamaxLevel = 10;
        }

        // Handle Tera Type for Gen 9 (SV)
        if (pk is ITeraType tera)
        {
            // Set tera type to match first type
            tera.TeraTypeOriginal = (MoveType)pk.PersonalInfo.Type1;
        }

        // Handle Gigantamax factor (SWSH)
        if (pk is IGigantamax gmaxW)
        {
            // Only set if species can Gigantamax
            gmaxW.CanGigantamax = CanHaveGigantamax(pk.Species, pk.Form);
        }

        // Set contest stats for older gens
        if (pk is IContestStats cs)
        {
            cs.ContestCool = 0;
            cs.ContestBeauty = 0;
            cs.ContestCute = 0;
            cs.ContestSmart = 0;
            cs.ContestTough = 0;
            cs.ContestSheen = 0;
        }

        // Set height/weight scale for Gen 8+
        if (pk is IScaledSize3 size3)
        {
            size3.Scale = 128; // Default/average scale
        }
        else if (pk is IScaledSize size)
        {
            size.HeightScalar = 128;
            size.WeightScalar = 128;
        }

        // Handle mark for Gen 8+ (wild marks)
        if (pk is IRibbonSetMark8 mark8)
        {
            // Clear any marks that might cause issues
            mark8.RibbonMarkLunchtime = false;
            mark8.RibbonMarkSleepyTime = false;
            mark8.RibbonMarkDusk = false;
            mark8.RibbonMarkDawn = false;
            mark8.RibbonMarkCloudy = false;
            mark8.RibbonMarkRainy = false;
            mark8.RibbonMarkStormy = false;
            mark8.RibbonMarkSnowy = false;
            mark8.RibbonMarkBlizzard = false;
            mark8.RibbonMarkDry = false;
            mark8.RibbonMarkSandstorm = false;
        }
    }

    /// <summary>
    /// Check if species can have Gigantamax form
    /// </summary>
    private static bool CanHaveGigantamax(ushort species, byte form)
    {
        // List of species that can Gigantamax
        return species switch
        {
            (ushort)Species.Venusaur => true,
            (ushort)Species.Charizard => true,
            (ushort)Species.Blastoise => true,
            (ushort)Species.Butterfree => true,
            (ushort)Species.Pikachu => true,
            (ushort)Species.Meowth when form == 0 => true, // Only Kantonian
            (ushort)Species.Machamp => true,
            (ushort)Species.Gengar => true,
            (ushort)Species.Kingler => true,
            (ushort)Species.Lapras => true,
            (ushort)Species.Eevee => true,
            (ushort)Species.Snorlax => true,
            (ushort)Species.Garbodor => true,
            (ushort)Species.Melmetal => true,
            (ushort)Species.Rillaboom => true,
            (ushort)Species.Cinderace => true,
            (ushort)Species.Inteleon => true,
            (ushort)Species.Corviknight => true,
            (ushort)Species.Orbeetle => true,
            (ushort)Species.Drednaw => true,
            (ushort)Species.Coalossal => true,
            (ushort)Species.Flapple => true,
            (ushort)Species.Appletun => true,
            (ushort)Species.Sandaconda => true,
            (ushort)Species.Toxtricity => true,
            (ushort)Species.Centiskorch => true,
            (ushort)Species.Hatterene => true,
            (ushort)Species.Grimmsnarl => true,
            (ushort)Species.Alcremie => true,
            (ushort)Species.Copperajah => true,
            (ushort)Species.Duraludon => true,
            (ushort)Species.Urshifu => true,
            _ => false
        };
    }

    private IEnumerable<ushort> GetSpeciesInRange(ushort start, ushort end)
    {
        for (ushort i = start; i <= end; i++)
            yield return i;
    }

    /// <summary>
    /// Generate only missing shiny Pokemon (fills gaps in existing collection)
    /// </summary>
    public GenerationResult FillMissingShiny(GeneratorOptions? options = null)
    {
        options ??= new GeneratorOptions();
        var result = new GenerationResult();

        // Scan existing Pokemon
        var existingShiny = new HashSet<ushort>();
        for (int box = 0; box < SAV.BoxCount; box++)
        {
            var pokemon = SAV.GetBoxData(box);
            foreach (var pk in pokemon)
            {
                if (pk.Species > 0 && pk.IsShiny)
                    existingShiny.Add(pk.Species);
            }
        }

        // Get species range
        ushort startSpecies = 1;
        ushort endSpecies = (ushort)Math.Min((int)SAV.MaxSpeciesID, 1025);

        if (GenerationRanges.ContainsKey(options.StartGeneration))
            startSpecies = GenerationRanges[options.StartGeneration].Start;
        if (GenerationRanges.ContainsKey(options.EndGeneration))
            endSpecies = GenerationRanges[options.EndGeneration].End;

        var missingSpecies = GetSpeciesInRange(startSpecies, endSpecies)
            .Where(s => SAV.Personal.IsSpeciesInGame(s) && !existingShiny.Contains(s))
            .ToList();

        result.TotalSpecies = missingSpecies.Count;

        // Find first empty slot
        int currentBox = options.StartBox;
        int currentSlot = 0;

        // Find empty slots
        while (currentBox < SAV.BoxCount)
        {
            var boxData = SAV.GetBoxData(currentBox);
            if (boxData[currentSlot].Species == 0)
                break;

            currentSlot++;
            if (currentSlot >= SAV.BoxSlotCount)
            {
                currentSlot = 0;
                currentBox++;
            }
        }

        foreach (var species in missingSpecies)
        {
            // Find next empty slot
            while (currentBox < SAV.BoxCount)
            {
                var boxData = SAV.GetBoxData(currentBox);
                if (boxData[currentSlot].Species == 0)
                    break;

                currentSlot++;
                if (currentSlot >= SAV.BoxSlotCount)
                {
                    currentSlot = 0;
                    currentBox++;
                }
            }

            if (currentBox >= SAV.BoxCount)
            {
                result.Errors.Add("Ran out of box space!");
                break;
            }

            var pokemon = GenerateShinyPokemon(species, 0, options);
            if (pokemon == null)
            {
                result.Failed++;
                continue;
            }

            SAV.SetBoxSlotAtIndex(pokemon, currentBox, currentSlot);
            result.Generated++;

            currentSlot++;
            if (currentSlot >= SAV.BoxSlotCount)
            {
                currentSlot = 0;
                currentBox++;
            }
        }

        return result;
    }

    /// <summary>
    /// Generate a specific generation's shiny Pokemon
    /// </summary>
    public GenerationResult GenerateGeneration(int generation, int startBox, GeneratorOptions? options = null)
    {
        options ??= new GeneratorOptions();
        options.StartGeneration = generation;
        options.EndGeneration = generation;
        options.StartBox = startBox;
        return GenerateShinyLivingDex(options);
    }

    /// <summary>
    /// Get count of how many boxes are needed for full shiny living dex
    /// </summary>
    public int CalculateBoxesNeeded(GeneratorOptions? options = null)
    {
        options ??= new GeneratorOptions();

        ushort startSpecies = 1;
        ushort endSpecies = (ushort)Math.Min((int)SAV.MaxSpeciesID, 1025);

        if (GenerationRanges.ContainsKey(options.StartGeneration))
            startSpecies = GenerationRanges[options.StartGeneration].Start;
        if (GenerationRanges.ContainsKey(options.EndGeneration))
            endSpecies = GenerationRanges[options.EndGeneration].End;

        int count = GetSpeciesInRange(startSpecies, endSpecies)
            .Count(s => SAV.Personal.IsSpeciesInGame(s));

        if (options.IncludeForms)
        {
            foreach (var species in GetSpeciesInRange(startSpecies, endSpecies))
            {
                if (!SAV.Personal.IsSpeciesInGame(species)) continue;
                var pi = SAV.Personal.GetFormEntry(species, 0);
                count += pi.FormCount - 1;
            }
        }

        return (int)Math.Ceiling(count / (double)SAV.BoxSlotCount);
    }

    /// <summary>
    /// Clear all Pokemon from specified boxes (for fresh generation)
    /// </summary>
    public void ClearBoxes(int startBox, int endBox)
    {
        for (int box = startBox; box <= endBox && box < SAV.BoxCount; box++)
        {
            var blank = SAV.BlankPKM;
            for (int slot = 0; slot < SAV.BoxSlotCount; slot++)
            {
                SAV.SetBoxSlotAtIndex(blank, box, slot);
            }
        }
    }
}

public class GenerationResult
{
    public int TotalSpecies { get; set; }
    public int Generated { get; set; }
    public int FormsGenerated { get; set; }
    public int Failed { get; set; }
    public int BoxesUsed { get; set; }
    public List<string> Errors { get; set; } = new();

    public string GetSummary()
    {
        var summary = $"Shiny Living Dex Generation Complete!\n\n" +
                     $"Pokemon Generated: {Generated}/{TotalSpecies}\n" +
                     $"Forms Generated: {FormsGenerated}\n" +
                     $"Failed: {Failed}\n" +
                     $"Boxes Used: {BoxesUsed}";

        if (Errors.Count > 0)
        {
            summary += $"\n\nErrors ({Errors.Count}):\n";
            summary += string.Join("\n", Errors.Take(10));
            if (Errors.Count > 10)
                summary += $"\n... and {Errors.Count - 10} more";
        }

        return summary;
    }
}
