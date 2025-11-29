using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PKHeX.Core;

namespace PKHeX.WinForms.Plugins;

/// <summary>
/// Team Analyzer - Comprehensive team analysis for competitive and casual play
/// Analyzes type coverage, weaknesses, resistances, and provides team suggestions
/// </summary>
public class TeamAnalyzer
{
    private readonly SaveFile SAV;

    // Type effectiveness chart
    private static readonly Dictionary<PokemonType, Dictionary<PokemonType, double>> TypeChart = InitTypeChart();

    public TeamAnalyzer(SaveFile sav)
    {
        SAV = sav;
    }

    /// <summary>
    /// Analyze the current party
    /// </summary>
    public TeamAnalysis AnalyzeParty()
    {
        var pokemon = SAV.PartyData.Where(p => p.Species > 0).ToList();
        return AnalyzeTeam(pokemon);
    }

    /// <summary>
    /// Analyze a specific box
    /// </summary>
    public TeamAnalysis AnalyzeBox(int box)
    {
        var pokemon = SAV.GetBoxData(box).Where(p => p.Species > 0).Take(6).ToList();
        return AnalyzeTeam(pokemon);
    }

    /// <summary>
    /// Full team analysis
    /// </summary>
    public TeamAnalysis AnalyzeTeam(IList<PKM> team)
    {
        var analysis = new TeamAnalysis();

        if (team.Count == 0)
        {
            analysis.Summary = "No Pokemon in team to analyze.";
            return analysis;
        }

        // Collect team types
        var teamTypes = new List<(PokemonType Type1, PokemonType Type2)>();
        foreach (var pk in team)
        {
            var types = GetPokemonTypes(pk);
            teamTypes.Add(types);
            analysis.TeamMembers.Add(new TeamMember
            {
                Species = pk.Species,
                Name = SpeciesName.GetSpeciesName(pk.Species, 2),
                Type1 = types.Type1,
                Type2 = types.Type2,
                Level = pk.CurrentLevel,
                Moves = new[] { pk.Move1, pk.Move2, pk.Move3, pk.Move4 }.Where(m => m > 0).ToArray()
            });
        }

        // Calculate defensive coverage (weaknesses/resistances)
        analysis.TypeWeaknesses = CalculateTeamWeaknesses(teamTypes);
        analysis.TypeResistances = CalculateTeamResistances(teamTypes);
        analysis.TypeImmunities = CalculateTeamImmunities(teamTypes);

        // Calculate offensive coverage
        analysis.OffensiveCoverage = CalculateOffensiveCoverage(team);

        // Identify critical weaknesses (types that hit 3+ team members super effectively)
        analysis.CriticalWeaknesses = analysis.TypeWeaknesses
            .Where(kv => kv.Value >= 3)
            .Select(kv => kv.Key)
            .ToList();

        // Identify gaps in coverage
        analysis.CoverageGaps = CalculateCoverageGaps(team);

        // Calculate stats spread
        analysis.TeamStats = CalculateTeamStats(team);

        // Role analysis
        analysis.RoleDistribution = AnalyzeRoles(team);

        // Speed tiers
        analysis.SpeedTiers = CalculateSpeedTiers(team);

        // Generate summary and suggestions
        analysis.Summary = GenerateSummary(analysis);
        analysis.Suggestions = GenerateSuggestions(analysis);

        return analysis;
    }

    private (PokemonType Type1, PokemonType Type2) GetPokemonTypes(PKM pk)
    {
        var pi = pk.PersonalInfo;
        return ((PokemonType)pi.Type1, (PokemonType)pi.Type2);
    }

    private Dictionary<PokemonType, int> CalculateTeamWeaknesses(List<(PokemonType Type1, PokemonType Type2)> teamTypes)
    {
        var weaknesses = new Dictionary<PokemonType, int>();

        foreach (PokemonType attackType in Enum.GetValues(typeof(PokemonType)))
        {
            if (attackType == PokemonType.None) continue;

            int weakCount = 0;
            foreach (var (type1, type2) in teamTypes)
            {
                double effectiveness = GetTypeEffectiveness(attackType, type1, type2);
                if (effectiveness > 1) weakCount++;
            }
            if (weakCount > 0)
                weaknesses[attackType] = weakCount;
        }

        return weaknesses.OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    private Dictionary<PokemonType, int> CalculateTeamResistances(List<(PokemonType Type1, PokemonType Type2)> teamTypes)
    {
        var resistances = new Dictionary<PokemonType, int>();

        foreach (PokemonType attackType in Enum.GetValues(typeof(PokemonType)))
        {
            if (attackType == PokemonType.None) continue;

            int resistCount = 0;
            foreach (var (type1, type2) in teamTypes)
            {
                double effectiveness = GetTypeEffectiveness(attackType, type1, type2);
                if (effectiveness < 1 && effectiveness > 0) resistCount++;
            }
            if (resistCount > 0)
                resistances[attackType] = resistCount;
        }

        return resistances.OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    private Dictionary<PokemonType, int> CalculateTeamImmunities(List<(PokemonType Type1, PokemonType Type2)> teamTypes)
    {
        var immunities = new Dictionary<PokemonType, int>();

        foreach (PokemonType attackType in Enum.GetValues(typeof(PokemonType)))
        {
            if (attackType == PokemonType.None) continue;

            int immuneCount = 0;
            foreach (var (type1, type2) in teamTypes)
            {
                double effectiveness = GetTypeEffectiveness(attackType, type1, type2);
                if (effectiveness == 0) immuneCount++;
            }
            if (immuneCount > 0)
                immunities[attackType] = immuneCount;
        }

        return immunities;
    }

    private Dictionary<PokemonType, int> CalculateOffensiveCoverage(IList<PKM> team)
    {
        var coverage = new Dictionary<PokemonType, int>();

        foreach (var pk in team)
        {
            var moves = new[] { pk.Move1, pk.Move2, pk.Move3, pk.Move4 };
            foreach (var moveId in moves)
            {
                if (moveId == 0) continue;
                var moveData = MoveInfo.GetType(moveId, pk.Context);
                if (moveData != 0)
                {
                    var moveType = (PokemonType)moveData;
                    if (!coverage.ContainsKey(moveType))
                        coverage[moveType] = 0;
                    coverage[moveType]++;
                }
            }
        }

        return coverage.OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    private List<PokemonType> CalculateCoverageGaps(IList<PKM> team)
    {
        var coveredTypes = new HashSet<PokemonType>();

        // Check what types the team can hit super effectively
        foreach (var pk in team)
        {
            var moves = new[] { pk.Move1, pk.Move2, pk.Move3, pk.Move4 };
            foreach (var moveId in moves)
            {
                if (moveId == 0) continue;
                var moveType = (PokemonType)MoveInfo.GetType(moveId, pk.Context);

                // Check what this move type hits super effectively
                foreach (PokemonType defType in Enum.GetValues(typeof(PokemonType)))
                {
                    if (defType == PokemonType.None) continue;
                    if (TypeChart.TryGetValue(moveType, out var matchups) &&
                        matchups.TryGetValue(defType, out var eff) && eff > 1)
                    {
                        coveredTypes.Add(defType);
                    }
                }
            }
        }

        // Find types not covered
        var gaps = new List<PokemonType>();
        foreach (PokemonType type in Enum.GetValues(typeof(PokemonType)))
        {
            if (type != PokemonType.None && !coveredTypes.Contains(type))
                gaps.Add(type);
        }

        return gaps;
    }

    private TeamStats CalculateTeamStats(IList<PKM> team)
    {
        var stats = new TeamStats();

        foreach (var pk in team)
        {
            stats.TotalHP += pk.Stat_HPCurrent;
            stats.TotalAtk += pk.Stat_ATK;
            stats.TotalDef += pk.Stat_DEF;
            stats.TotalSpA += pk.Stat_SPA;
            stats.TotalSpD += pk.Stat_SPD;
            stats.TotalSpe += pk.Stat_SPE;
        }

        int count = team.Count;
        if (count > 0)
        {
            stats.AvgHP = stats.TotalHP / count;
            stats.AvgAtk = stats.TotalAtk / count;
            stats.AvgDef = stats.TotalDef / count;
            stats.AvgSpA = stats.TotalSpA / count;
            stats.AvgSpD = stats.TotalSpD / count;
            stats.AvgSpe = stats.TotalSpe / count;
        }

        // Determine if team is physical, special, or balanced
        if (stats.TotalAtk > stats.TotalSpA * 1.3)
            stats.OffensiveBias = "Physical";
        else if (stats.TotalSpA > stats.TotalAtk * 1.3)
            stats.OffensiveBias = "Special";
        else
            stats.OffensiveBias = "Mixed";

        // Defensive assessment
        if (stats.AvgDef > 100 && stats.AvgSpD > 100)
            stats.DefensiveRating = "Bulky";
        else if (stats.AvgDef > 80 || stats.AvgSpD > 80)
            stats.DefensiveRating = "Moderate";
        else
            stats.DefensiveRating = "Frail";

        return stats;
    }

    private Dictionary<string, int> AnalyzeRoles(IList<PKM> team)
    {
        var roles = new Dictionary<string, int>
        {
            ["Physical Attacker"] = 0,
            ["Special Attacker"] = 0,
            ["Physical Wall"] = 0,
            ["Special Wall"] = 0,
            ["Speed Control"] = 0,
            ["Support"] = 0
        };

        foreach (var pk in team)
        {
            // Determine role based on stats
            if (pk.Stat_ATK > pk.Stat_SPA && pk.Stat_ATK > 100)
                roles["Physical Attacker"]++;
            else if (pk.Stat_SPA > pk.Stat_ATK && pk.Stat_SPA > 100)
                roles["Special Attacker"]++;

            if (pk.Stat_DEF > 100)
                roles["Physical Wall"]++;
            if (pk.Stat_SPD > 100)
                roles["Special Wall"]++;
            if (pk.Stat_SPE > 100)
                roles["Speed Control"]++;

            // Check for support moves
            var moves = new[] { pk.Move1, pk.Move2, pk.Move3, pk.Move4 };
            var supportMoves = new[] { 73, 104, 115, 156, 164, 182, 219, 240, 244, 356 }; // Status moves
            if (moves.Any(m => supportMoves.Contains(m)))
                roles["Support"]++;
        }

        return roles.Where(r => r.Value > 0).ToDictionary(r => r.Key, r => r.Value);
    }

    private List<SpeedTier> CalculateSpeedTiers(IList<PKM> team)
    {
        return team.Select(pk => new SpeedTier
        {
            Name = SpeciesName.GetSpeciesName(pk.Species, 2),
            Speed = pk.Stat_SPE,
            WithScarf = (int)(pk.Stat_SPE * 1.5),
            WithTailwind = pk.Stat_SPE * 2,
            AfterParalysis = pk.Stat_SPE / 2
        }).OrderByDescending(s => s.Speed).ToList();
    }

    private string GenerateSummary(TeamAnalysis analysis)
    {
        var sb = new StringBuilder();

        sb.AppendLine("╔══════════════════════════════════════════════════════════════╗");
        sb.AppendLine("║                    TEAM ANALYSIS REPORT                      ║");
        sb.AppendLine("╠══════════════════════════════════════════════════════════════╣");

        // Team composition
        sb.AppendLine("║ TEAM MEMBERS:");
        foreach (var member in analysis.TeamMembers)
        {
            sb.AppendLine($"║   {member.Name,-15} [{member.Type1}{(member.Type2 != PokemonType.None ? "/" + member.Type2 : "")}]");
        }

        sb.AppendLine("╠══════════════════════════════════════════════════════════════╣");

        // Critical weaknesses
        if (analysis.CriticalWeaknesses.Any())
        {
            sb.AppendLine("║ ⚠️ CRITICAL WEAKNESSES (3+ Pokemon weak):");
            foreach (var type in analysis.CriticalWeaknesses)
            {
                sb.AppendLine($"║   {type} - {analysis.TypeWeaknesses[type]} Pokemon weak");
            }
        }
        else
        {
            sb.AppendLine("║ ✓ No critical team weaknesses");
        }

        sb.AppendLine("╠══════════════════════════════════════════════════════════════╣");

        // Type weaknesses summary
        sb.AppendLine("║ TYPE COVERAGE SUMMARY:");
        sb.AppendLine($"║   Offensive Bias: {analysis.TeamStats.OffensiveBias}");
        sb.AppendLine($"║   Defensive Rating: {analysis.TeamStats.DefensiveRating}");

        if (analysis.CoverageGaps.Any())
        {
            sb.AppendLine($"║   Coverage Gaps: {string.Join(", ", analysis.CoverageGaps.Take(5))}");
        }
        else
        {
            sb.AppendLine("║   ✓ Full type coverage");
        }

        sb.AppendLine("╠══════════════════════════════════════════════════════════════╣");

        // Speed tiers
        sb.AppendLine("║ SPEED TIERS:");
        foreach (var tier in analysis.SpeedTiers.Take(6))
        {
            sb.AppendLine($"║   {tier.Name,-15}: {tier.Speed,3} (Scarf: {tier.WithScarf})");
        }

        sb.AppendLine("╚══════════════════════════════════════════════════════════════╝");

        return sb.ToString();
    }

    private List<string> GenerateSuggestions(TeamAnalysis analysis)
    {
        var suggestions = new List<string>();

        // Critical weakness suggestions
        foreach (var weakness in analysis.CriticalWeaknesses)
        {
            suggestions.Add($"Consider adding a {weakness}-resistant Pokemon to cover your {weakness} weakness.");
        }

        // Coverage gap suggestions
        foreach (var gap in analysis.CoverageGaps.Take(3))
        {
            suggestions.Add($"Your team lacks super-effective coverage against {gap}-types. Consider adding a {GetCounterType(gap)} move.");
        }

        // Role suggestions
        if (!analysis.RoleDistribution.ContainsKey("Physical Wall") && !analysis.RoleDistribution.ContainsKey("Special Wall"))
        {
            suggestions.Add("Your team lacks a dedicated defensive Pokemon. Consider adding a tank or wall.");
        }

        if (analysis.TeamStats.AvgSpe < 80)
        {
            suggestions.Add("Your team is quite slow. Consider adding a fast Pokemon or speed control options like Tailwind.");
        }

        // Balance suggestions
        if (analysis.TeamStats.OffensiveBias == "Physical")
        {
            suggestions.Add("Your team is heavily Physical-biased. Consider adding a Special Attacker for balance.");
        }
        else if (analysis.TeamStats.OffensiveBias == "Special")
        {
            suggestions.Add("Your team is heavily Special-biased. Consider adding a Physical Attacker for balance.");
        }

        return suggestions;
    }

    private string GetCounterType(PokemonType type)
    {
        return type switch
        {
            PokemonType.Normal => "Fighting",
            PokemonType.Fire => "Water/Ground/Rock",
            PokemonType.Water => "Electric/Grass",
            PokemonType.Electric => "Ground",
            PokemonType.Grass => "Fire/Ice/Flying",
            PokemonType.Ice => "Fire/Fighting/Rock",
            PokemonType.Fighting => "Flying/Psychic/Fairy",
            PokemonType.Poison => "Ground/Psychic",
            PokemonType.Ground => "Water/Grass/Ice",
            PokemonType.Flying => "Electric/Ice/Rock",
            PokemonType.Psychic => "Bug/Ghost/Dark",
            PokemonType.Bug => "Fire/Flying/Rock",
            PokemonType.Rock => "Water/Grass/Fighting",
            PokemonType.Ghost => "Ghost/Dark",
            PokemonType.Dragon => "Ice/Dragon/Fairy",
            PokemonType.Dark => "Fighting/Bug/Fairy",
            PokemonType.Steel => "Fire/Fighting/Ground",
            PokemonType.Fairy => "Poison/Steel",
            _ => "any"
        };
    }

    private double GetTypeEffectiveness(PokemonType attackType, PokemonType defType1, PokemonType defType2)
    {
        double effectiveness = 1.0;

        if (TypeChart.TryGetValue(attackType, out var matchups))
        {
            if (matchups.TryGetValue(defType1, out var eff1))
                effectiveness *= eff1;
            if (defType2 != PokemonType.None && defType2 != defType1 && matchups.TryGetValue(defType2, out var eff2))
                effectiveness *= eff2;
        }

        return effectiveness;
    }

    private static Dictionary<PokemonType, Dictionary<PokemonType, double>> InitTypeChart()
    {
        var chart = new Dictionary<PokemonType, Dictionary<PokemonType, double>>();

        // Initialize all types
        foreach (PokemonType type in Enum.GetValues(typeof(PokemonType)))
        {
            if (type != PokemonType.None)
                chart[type] = new Dictionary<PokemonType, double>();
        }

        // Normal
        chart[PokemonType.Normal][PokemonType.Rock] = 0.5;
        chart[PokemonType.Normal][PokemonType.Ghost] = 0;
        chart[PokemonType.Normal][PokemonType.Steel] = 0.5;

        // Fire
        chart[PokemonType.Fire][PokemonType.Fire] = 0.5;
        chart[PokemonType.Fire][PokemonType.Water] = 0.5;
        chart[PokemonType.Fire][PokemonType.Grass] = 2;
        chart[PokemonType.Fire][PokemonType.Ice] = 2;
        chart[PokemonType.Fire][PokemonType.Bug] = 2;
        chart[PokemonType.Fire][PokemonType.Rock] = 0.5;
        chart[PokemonType.Fire][PokemonType.Dragon] = 0.5;
        chart[PokemonType.Fire][PokemonType.Steel] = 2;

        // Water
        chart[PokemonType.Water][PokemonType.Fire] = 2;
        chart[PokemonType.Water][PokemonType.Water] = 0.5;
        chart[PokemonType.Water][PokemonType.Grass] = 0.5;
        chart[PokemonType.Water][PokemonType.Ground] = 2;
        chart[PokemonType.Water][PokemonType.Rock] = 2;
        chart[PokemonType.Water][PokemonType.Dragon] = 0.5;

        // Electric
        chart[PokemonType.Electric][PokemonType.Water] = 2;
        chart[PokemonType.Electric][PokemonType.Electric] = 0.5;
        chart[PokemonType.Electric][PokemonType.Grass] = 0.5;
        chart[PokemonType.Electric][PokemonType.Ground] = 0;
        chart[PokemonType.Electric][PokemonType.Flying] = 2;
        chart[PokemonType.Electric][PokemonType.Dragon] = 0.5;

        // Grass
        chart[PokemonType.Grass][PokemonType.Fire] = 0.5;
        chart[PokemonType.Grass][PokemonType.Water] = 2;
        chart[PokemonType.Grass][PokemonType.Grass] = 0.5;
        chart[PokemonType.Grass][PokemonType.Poison] = 0.5;
        chart[PokemonType.Grass][PokemonType.Ground] = 2;
        chart[PokemonType.Grass][PokemonType.Flying] = 0.5;
        chart[PokemonType.Grass][PokemonType.Bug] = 0.5;
        chart[PokemonType.Grass][PokemonType.Rock] = 2;
        chart[PokemonType.Grass][PokemonType.Dragon] = 0.5;
        chart[PokemonType.Grass][PokemonType.Steel] = 0.5;

        // Ice
        chart[PokemonType.Ice][PokemonType.Fire] = 0.5;
        chart[PokemonType.Ice][PokemonType.Water] = 0.5;
        chart[PokemonType.Ice][PokemonType.Grass] = 2;
        chart[PokemonType.Ice][PokemonType.Ice] = 0.5;
        chart[PokemonType.Ice][PokemonType.Ground] = 2;
        chart[PokemonType.Ice][PokemonType.Flying] = 2;
        chart[PokemonType.Ice][PokemonType.Dragon] = 2;
        chart[PokemonType.Ice][PokemonType.Steel] = 0.5;

        // Fighting
        chart[PokemonType.Fighting][PokemonType.Normal] = 2;
        chart[PokemonType.Fighting][PokemonType.Ice] = 2;
        chart[PokemonType.Fighting][PokemonType.Poison] = 0.5;
        chart[PokemonType.Fighting][PokemonType.Flying] = 0.5;
        chart[PokemonType.Fighting][PokemonType.Psychic] = 0.5;
        chart[PokemonType.Fighting][PokemonType.Bug] = 0.5;
        chart[PokemonType.Fighting][PokemonType.Rock] = 2;
        chart[PokemonType.Fighting][PokemonType.Ghost] = 0;
        chart[PokemonType.Fighting][PokemonType.Dark] = 2;
        chart[PokemonType.Fighting][PokemonType.Steel] = 2;
        chart[PokemonType.Fighting][PokemonType.Fairy] = 0.5;

        // Poison
        chart[PokemonType.Poison][PokemonType.Grass] = 2;
        chart[PokemonType.Poison][PokemonType.Poison] = 0.5;
        chart[PokemonType.Poison][PokemonType.Ground] = 0.5;
        chart[PokemonType.Poison][PokemonType.Rock] = 0.5;
        chart[PokemonType.Poison][PokemonType.Ghost] = 0.5;
        chart[PokemonType.Poison][PokemonType.Steel] = 0;
        chart[PokemonType.Poison][PokemonType.Fairy] = 2;

        // Ground
        chart[PokemonType.Ground][PokemonType.Fire] = 2;
        chart[PokemonType.Ground][PokemonType.Electric] = 2;
        chart[PokemonType.Ground][PokemonType.Grass] = 0.5;
        chart[PokemonType.Ground][PokemonType.Poison] = 2;
        chart[PokemonType.Ground][PokemonType.Flying] = 0;
        chart[PokemonType.Ground][PokemonType.Bug] = 0.5;
        chart[PokemonType.Ground][PokemonType.Rock] = 2;
        chart[PokemonType.Ground][PokemonType.Steel] = 2;

        // Flying
        chart[PokemonType.Flying][PokemonType.Electric] = 0.5;
        chart[PokemonType.Flying][PokemonType.Grass] = 2;
        chart[PokemonType.Flying][PokemonType.Fighting] = 2;
        chart[PokemonType.Flying][PokemonType.Bug] = 2;
        chart[PokemonType.Flying][PokemonType.Rock] = 0.5;
        chart[PokemonType.Flying][PokemonType.Steel] = 0.5;

        // Psychic
        chart[PokemonType.Psychic][PokemonType.Fighting] = 2;
        chart[PokemonType.Psychic][PokemonType.Poison] = 2;
        chart[PokemonType.Psychic][PokemonType.Psychic] = 0.5;
        chart[PokemonType.Psychic][PokemonType.Dark] = 0;
        chart[PokemonType.Psychic][PokemonType.Steel] = 0.5;

        // Bug
        chart[PokemonType.Bug][PokemonType.Fire] = 0.5;
        chart[PokemonType.Bug][PokemonType.Grass] = 2;
        chart[PokemonType.Bug][PokemonType.Fighting] = 0.5;
        chart[PokemonType.Bug][PokemonType.Poison] = 0.5;
        chart[PokemonType.Bug][PokemonType.Flying] = 0.5;
        chart[PokemonType.Bug][PokemonType.Psychic] = 2;
        chart[PokemonType.Bug][PokemonType.Ghost] = 0.5;
        chart[PokemonType.Bug][PokemonType.Dark] = 2;
        chart[PokemonType.Bug][PokemonType.Steel] = 0.5;
        chart[PokemonType.Bug][PokemonType.Fairy] = 0.5;

        // Rock
        chart[PokemonType.Rock][PokemonType.Fire] = 2;
        chart[PokemonType.Rock][PokemonType.Ice] = 2;
        chart[PokemonType.Rock][PokemonType.Fighting] = 0.5;
        chart[PokemonType.Rock][PokemonType.Ground] = 0.5;
        chart[PokemonType.Rock][PokemonType.Flying] = 2;
        chart[PokemonType.Rock][PokemonType.Bug] = 2;
        chart[PokemonType.Rock][PokemonType.Steel] = 0.5;

        // Ghost
        chart[PokemonType.Ghost][PokemonType.Normal] = 0;
        chart[PokemonType.Ghost][PokemonType.Psychic] = 2;
        chart[PokemonType.Ghost][PokemonType.Ghost] = 2;
        chart[PokemonType.Ghost][PokemonType.Dark] = 0.5;

        // Dragon
        chart[PokemonType.Dragon][PokemonType.Dragon] = 2;
        chart[PokemonType.Dragon][PokemonType.Steel] = 0.5;
        chart[PokemonType.Dragon][PokemonType.Fairy] = 0;

        // Dark
        chart[PokemonType.Dark][PokemonType.Fighting] = 0.5;
        chart[PokemonType.Dark][PokemonType.Psychic] = 2;
        chart[PokemonType.Dark][PokemonType.Ghost] = 2;
        chart[PokemonType.Dark][PokemonType.Dark] = 0.5;
        chart[PokemonType.Dark][PokemonType.Fairy] = 0.5;

        // Steel
        chart[PokemonType.Steel][PokemonType.Fire] = 0.5;
        chart[PokemonType.Steel][PokemonType.Water] = 0.5;
        chart[PokemonType.Steel][PokemonType.Electric] = 0.5;
        chart[PokemonType.Steel][PokemonType.Ice] = 2;
        chart[PokemonType.Steel][PokemonType.Rock] = 2;
        chart[PokemonType.Steel][PokemonType.Steel] = 0.5;
        chart[PokemonType.Steel][PokemonType.Fairy] = 2;

        // Fairy
        chart[PokemonType.Fairy][PokemonType.Fire] = 0.5;
        chart[PokemonType.Fairy][PokemonType.Fighting] = 2;
        chart[PokemonType.Fairy][PokemonType.Poison] = 0.5;
        chart[PokemonType.Fairy][PokemonType.Dragon] = 2;
        chart[PokemonType.Fairy][PokemonType.Dark] = 2;
        chart[PokemonType.Fairy][PokemonType.Steel] = 0.5;

        return chart;
    }
}

public enum PokemonType
{
    None = 0,
    Normal = 1,
    Fighting = 2,
    Flying = 3,
    Poison = 4,
    Ground = 5,
    Rock = 6,
    Bug = 7,
    Ghost = 8,
    Steel = 9,
    Fire = 10,
    Water = 11,
    Grass = 12,
    Electric = 13,
    Psychic = 14,
    Ice = 15,
    Dragon = 16,
    Dark = 17,
    Fairy = 18
}

public class TeamAnalysis
{
    public List<TeamMember> TeamMembers { get; set; } = new();
    public Dictionary<PokemonType, int> TypeWeaknesses { get; set; } = new();
    public Dictionary<PokemonType, int> TypeResistances { get; set; } = new();
    public Dictionary<PokemonType, int> TypeImmunities { get; set; } = new();
    public Dictionary<PokemonType, int> OffensiveCoverage { get; set; } = new();
    public List<PokemonType> CriticalWeaknesses { get; set; } = new();
    public List<PokemonType> CoverageGaps { get; set; } = new();
    public TeamStats TeamStats { get; set; } = new();
    public Dictionary<string, int> RoleDistribution { get; set; } = new();
    public List<SpeedTier> SpeedTiers { get; set; } = new();
    public string Summary { get; set; } = "";
    public List<string> Suggestions { get; set; } = new();
}

public class TeamMember
{
    public int Species { get; set; }
    public string Name { get; set; } = "";
    public PokemonType Type1 { get; set; }
    public PokemonType Type2 { get; set; }
    public int Level { get; set; }
    public ushort[] Moves { get; set; } = Array.Empty<ushort>();
}

public class TeamStats
{
    public int TotalHP { get; set; }
    public int TotalAtk { get; set; }
    public int TotalDef { get; set; }
    public int TotalSpA { get; set; }
    public int TotalSpD { get; set; }
    public int TotalSpe { get; set; }
    public int AvgHP { get; set; }
    public int AvgAtk { get; set; }
    public int AvgDef { get; set; }
    public int AvgSpA { get; set; }
    public int AvgSpD { get; set; }
    public int AvgSpe { get; set; }
    public string OffensiveBias { get; set; } = "";
    public string DefensiveRating { get; set; } = "";
}

public class SpeedTier
{
    public string Name { get; set; } = "";
    public int Speed { get; set; }
    public int WithScarf { get; set; }
    public int WithTailwind { get; set; }
    public int AfterParalysis { get; set; }
}
