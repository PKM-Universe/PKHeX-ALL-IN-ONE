/**
 * Pokemon Team Builder
 * Full 6-Pokemon team creation with synergy analysis, type coverage, and role assignment
 */

class PokemonTeamBuilder {
    constructor() {
        // Type chart for coverage analysis
        this.typeChart = {
            normal: { weaknesses: ['fighting'], resistances: [], immunities: ['ghost'] },
            fire: { weaknesses: ['water', 'ground', 'rock'], resistances: ['fire', 'grass', 'ice', 'bug', 'steel', 'fairy'], immunities: [] },
            water: { weaknesses: ['electric', 'grass'], resistances: ['fire', 'water', 'ice', 'steel'], immunities: [] },
            electric: { weaknesses: ['ground'], resistances: ['electric', 'flying', 'steel'], immunities: [] },
            grass: { weaknesses: ['fire', 'ice', 'poison', 'flying', 'bug'], resistances: ['water', 'electric', 'grass', 'ground'], immunities: [] },
            ice: { weaknesses: ['fire', 'fighting', 'rock', 'steel'], resistances: ['ice'], immunities: [] },
            fighting: { weaknesses: ['flying', 'psychic', 'fairy'], resistances: ['bug', 'rock', 'dark'], immunities: [] },
            poison: { weaknesses: ['ground', 'psychic'], resistances: ['grass', 'fighting', 'poison', 'bug', 'fairy'], immunities: [] },
            ground: { weaknesses: ['water', 'grass', 'ice'], resistances: ['poison', 'rock'], immunities: ['electric'] },
            flying: { weaknesses: ['electric', 'ice', 'rock'], resistances: ['grass', 'fighting', 'bug'], immunities: ['ground'] },
            psychic: { weaknesses: ['bug', 'ghost', 'dark'], resistances: ['fighting', 'psychic'], immunities: [] },
            bug: { weaknesses: ['fire', 'flying', 'rock'], resistances: ['grass', 'fighting', 'ground'], immunities: [] },
            rock: { weaknesses: ['water', 'grass', 'fighting', 'ground', 'steel'], resistances: ['normal', 'fire', 'poison', 'flying'], immunities: [] },
            ghost: { weaknesses: ['ghost', 'dark'], resistances: ['poison', 'bug'], immunities: ['normal', 'fighting'] },
            dragon: { weaknesses: ['ice', 'dragon', 'fairy'], resistances: ['fire', 'water', 'electric', 'grass'], immunities: [] },
            dark: { weaknesses: ['fighting', 'bug', 'fairy'], resistances: ['ghost', 'dark'], immunities: ['psychic'] },
            steel: { weaknesses: ['fire', 'fighting', 'ground'], resistances: ['normal', 'grass', 'ice', 'flying', 'psychic', 'bug', 'rock', 'dragon', 'steel', 'fairy'], immunities: ['poison'] },
            fairy: { weaknesses: ['poison', 'steel'], resistances: ['fighting', 'bug', 'dark'], immunities: ['dragon'] }
        };

        // Role definitions
        this.roles = {
            'physical-sweeper': {
                description: 'Fast physical attacker',
                statPriorities: ['atk', 'spe'],
                minStats: { atk: 100, spe: 90 }
            },
            'special-sweeper': {
                description: 'Fast special attacker',
                statPriorities: ['spa', 'spe'],
                minStats: { spa: 100, spe: 90 }
            },
            'physical-wall': {
                description: 'High physical defense',
                statPriorities: ['def', 'hp'],
                minStats: { def: 100, hp: 80 }
            },
            'special-wall': {
                description: 'High special defense',
                statPriorities: ['spd', 'hp'],
                minStats: { spd: 100, hp: 80 }
            },
            'mixed-wall': {
                description: 'Balanced defenses',
                statPriorities: ['hp', 'def', 'spd'],
                minStats: { hp: 90, def: 80, spd: 80 }
            },
            'pivot': {
                description: 'U-turn/Volt Switch user',
                statPriorities: ['spe', 'hp'],
                moves: ['u-turn', 'volt-switch', 'flip-turn', 'teleport']
            },
            'revenge-killer': {
                description: 'Priority move user',
                statPriorities: ['atk', 'spe'],
                moves: ['extreme-speed', 'bullet-punch', 'ice-shard', 'aqua-jet', 'sucker-punch', 'mach-punch']
            },
            'hazard-setter': {
                description: 'Entry hazard user',
                statPriorities: ['hp', 'def'],
                moves: ['stealth-rock', 'spikes', 'toxic-spikes', 'sticky-web']
            },
            'hazard-remover': {
                description: 'Defog/Rapid Spin user',
                statPriorities: ['hp', 'spe'],
                moves: ['defog', 'rapid-spin', 'court-change', 'mortal-spin']
            },
            'support': {
                description: 'Status/healing support',
                statPriorities: ['hp', 'spd'],
                moves: ['wish', 'heal-bell', 'aromatherapy', 'healing-wish', 'lunar-dance']
            },
            'setup-sweeper': {
                description: 'Boosting sweeper',
                statPriorities: ['atk', 'spa', 'spe'],
                moves: ['swords-dance', 'dragon-dance', 'nasty-plot', 'quiver-dance', 'calm-mind', 'bulk-up', 'shell-smash']
            },
            'wallbreaker': {
                description: 'High power to break walls',
                statPriorities: ['atk', 'spa'],
                minStats: { atk: 120 } // OR spa: 120
            },
            'weather-setter': {
                description: 'Summons weather',
                abilities: ['drought', 'drizzle', 'sand-stream', 'snow-warning'],
                moves: ['sunny-day', 'rain-dance', 'sandstorm', 'hail', 'snowscape']
            },
            'terrain-setter': {
                description: 'Summons terrain',
                abilities: ['electric-surge', 'grassy-surge', 'misty-surge', 'psychic-surge'],
                moves: ['electric-terrain', 'grassy-terrain', 'misty-terrain', 'psychic-terrain']
            }
        };

        // Common Pokemon by role (sample data)
        this.pokemonByRole = {
            'physical-sweeper': ['garchomp', 'dragapult', 'weavile', 'excadrill', 'landorus-therian', 'kartana', 'urshifu'],
            'special-sweeper': ['gengar', 'alakazam', 'hydreigon', 'volcarona', 'tapu-lele', 'spectrier', 'iron-moth'],
            'physical-wall': ['skarmory', 'hippowdon', 'corviknight', 'ferrothorn', 'great-tusk', 'dondozo'],
            'special-wall': ['blissey', 'chansey', 'gastrodon', 'toxapex', 'clodsire', 'ting-lu'],
            'pivot': ['landorus-therian', 'rotom-wash', 'tornadus-therian', 'slowbro', 'corviknight'],
            'hazard-setter': ['ferrothorn', 'garchomp', 'clefable', 'glimmora', 'great-tusk'],
            'hazard-remover': ['corviknight', 'excadrill', 'mandibuzz', 'great-tusk', 'iron-treads'],
            'support': ['clefable', 'blissey', 'toxapex', 'corviknight', 'dondozo'],
            'setup-sweeper': ['dragonite', 'volcarona', 'gyarados', 'kingambit', 'iron-valiant', 'roaring-moon'],
            'wallbreaker': ['urshifu', 'kartana', 'tapu-lele', 'iron-valiant', 'chi-yu']
        };

        // Competitive items
        this.competitiveItems = {
            'choice-band': { effect: '+50% Attack, locked into one move', roles: ['physical-sweeper', 'wallbreaker', 'revenge-killer'] },
            'choice-specs': { effect: '+50% Sp.Atk, locked into one move', roles: ['special-sweeper', 'wallbreaker'] },
            'choice-scarf': { effect: '+50% Speed, locked into one move', roles: ['revenge-killer', 'pivot'] },
            'life-orb': { effect: '+30% damage, 10% recoil', roles: ['physical-sweeper', 'special-sweeper', 'wallbreaker'] },
            'leftovers': { effect: '1/16 HP recovery each turn', roles: ['physical-wall', 'special-wall', 'support', 'pivot'] },
            'heavy-duty-boots': { effect: 'Immune to entry hazards', roles: ['pivot', 'hazard-remover'] },
            'assault-vest': { effect: '+50% Sp.Def, no status moves', roles: ['mixed-wall', 'pivot'] },
            'focus-sash': { effect: 'Survive one KO at 1 HP', roles: ['hazard-setter', 'setup-sweeper'] },
            'rocky-helmet': { effect: 'Contact moves deal 1/6 damage to attacker', roles: ['physical-wall'] },
            'eviolite': { effect: '+50% Def/SpD if not fully evolved', roles: ['physical-wall', 'special-wall'] },
            'black-sludge': { effect: '1/16 HP recovery for Poison types', roles: ['physical-wall', 'special-wall'] },
            'air-balloon': { effect: 'Immunity to Ground until hit', roles: ['setup-sweeper', 'special-sweeper'] },
            'weakness-policy': { effect: '+2 Atk/SpA when hit super-effectively', roles: ['setup-sweeper'] },
            'booster-energy': { effect: 'Activates Protosynthesis/Quark Drive', roles: ['physical-sweeper', 'special-sweeper'] }
        };
    }

    /**
     * Create a new team
     */
    createTeam(name = 'New Team') {
        return {
            name,
            format: 'gen9ou',
            pokemon: [],
            analysis: null,
            createdAt: new Date().toISOString()
        };
    }

    /**
     * Add Pokemon to team
     */
    addPokemon(team, pokemon) {
        if (team.pokemon.length >= 6) {
            return { error: 'Team already has 6 Pokemon' };
        }

        // Validate Pokemon structure
        const validatedPokemon = {
            species: pokemon.species || pokemon.name,
            nickname: pokemon.nickname || '',
            item: pokemon.item || '',
            ability: pokemon.ability || '',
            nature: pokemon.nature || 'hardy',
            teraType: pokemon.teraType || null,
            evs: pokemon.evs || { hp: 0, atk: 0, def: 0, spa: 0, spd: 0, spe: 0 },
            ivs: pokemon.ivs || { hp: 31, atk: 31, def: 31, spa: 31, spd: 31, spe: 31 },
            moves: pokemon.moves || [],
            types: pokemon.types || [],
            role: pokemon.role || null
        };

        team.pokemon.push(validatedPokemon);
        return { success: true, teamSize: team.pokemon.length };
    }

    /**
     * Remove Pokemon from team
     */
    removePokemon(team, index) {
        if (index < 0 || index >= team.pokemon.length) {
            return { error: 'Invalid Pokemon index' };
        }

        const removed = team.pokemon.splice(index, 1)[0];
        return { success: true, removed: removed.species };
    }

    /**
     * Analyze team type coverage
     */
    analyzeTypeCoverage(team) {
        const allTypes = Object.keys(this.typeChart);
        const coverage = {
            offensive: {},
            defensive: {
                weaknesses: {},
                resistances: {},
                immunities: {}
            },
            uncoveredTypes: [],
            weaknessStack: []
        };

        // Initialize offensive coverage
        for (const type of allTypes) {
            coverage.offensive[type] = {
                superEffective: false,
                moves: []
            };
        }

        // Analyze each Pokemon
        for (const pokemon of team.pokemon) {
            // Offensive coverage from moves
            for (const move of pokemon.moves) {
                const moveType = this.getMoveType(move);
                if (moveType) {
                    for (const defType of allTypes) {
                        const effectiveness = this.getTypeEffectiveness(moveType, [defType]);
                        if (effectiveness > 1) {
                            coverage.offensive[defType].superEffective = true;
                            if (!coverage.offensive[defType].moves.includes(move)) {
                                coverage.offensive[defType].moves.push(move);
                            }
                        }
                    }
                }
            }

            // Defensive coverage from types
            const types = pokemon.types || [];
            for (const type of types) {
                const typeData = this.typeChart[type];
                if (typeData) {
                    // Track weaknesses
                    for (const weakness of typeData.weaknesses) {
                        coverage.defensive.weaknesses[weakness] = (coverage.defensive.weaknesses[weakness] || 0) + 1;
                    }
                    // Track resistances
                    for (const resistance of typeData.resistances) {
                        coverage.defensive.resistances[resistance] = (coverage.defensive.resistances[resistance] || 0) + 1;
                    }
                    // Track immunities
                    for (const immunity of typeData.immunities) {
                        coverage.defensive.immunities[immunity] = (coverage.defensive.immunities[immunity] || 0) + 1;
                    }
                }
            }
        }

        // Find uncovered types
        for (const type of allTypes) {
            if (!coverage.offensive[type].superEffective) {
                coverage.uncoveredTypes.push(type);
            }
        }

        // Find stacked weaknesses (3+ Pokemon weak to same type)
        for (const [type, count] of Object.entries(coverage.defensive.weaknesses)) {
            if (count >= 3) {
                coverage.weaknessStack.push({ type, count });
            }
        }

        return coverage;
    }

    /**
     * Get type effectiveness
     */
    getTypeEffectiveness(attackType, defenderTypes) {
        let multiplier = 1;
        for (const defType of defenderTypes) {
            const typeData = this.typeChart[defType];
            if (typeData) {
                if (typeData.weaknesses.includes(attackType)) multiplier *= 2;
                if (typeData.resistances.includes(attackType)) multiplier *= 0.5;
                if (typeData.immunities.includes(attackType)) multiplier *= 0;
            }
        }
        return multiplier;
    }

    /**
     * Get move type (simplified)
     */
    getMoveType(moveName) {
        const moveTypes = {
            'earthquake': 'ground', 'close-combat': 'fighting', 'flamethrower': 'fire',
            'thunderbolt': 'electric', 'ice-beam': 'ice', 'psychic': 'psychic',
            'shadow-ball': 'ghost', 'dark-pulse': 'dark', 'dragon-claw': 'dragon',
            'iron-head': 'steel', 'moonblast': 'fairy', 'sludge-bomb': 'poison',
            'stone-edge': 'rock', 'u-turn': 'bug', 'brave-bird': 'flying',
            'hydro-pump': 'water', 'energy-ball': 'grass', 'body-slam': 'normal',
            'knock-off': 'dark', 'draco-meteor': 'dragon', 'flash-cannon': 'steel',
            'fire-blast': 'fire', 'thunder': 'electric', 'blizzard': 'ice',
            'surf': 'water', 'leaf-storm': 'grass', 'focus-blast': 'fighting',
            'stealth-rock': 'rock', 'spikes': 'ground', 'toxic-spikes': 'poison',
            'defog': 'flying', 'rapid-spin': 'normal', 'volt-switch': 'electric',
            'flip-turn': 'water', 'swords-dance': 'normal', 'dragon-dance': 'dragon',
            'nasty-plot': 'dark', 'calm-mind': 'psychic', 'wish': 'normal',
            'protect': 'normal', 'substitute': 'normal', 'roost': 'flying'
        };
        return moveTypes[moveName?.toLowerCase()] || null;
    }

    /**
     * Analyze team synergy
     */
    analyzeTeamSynergy(team) {
        const synergy = {
            score: 0,
            maxScore: 100,
            roleBalance: { filled: [], missing: [] },
            speedTiers: { fast: [], medium: [], slow: [] },
            offensiveBalance: { physical: 0, special: 0, mixed: 0 },
            hazardControl: { setters: [], removers: [] },
            pivoting: [],
            weatherSupport: null,
            terrainSupport: null,
            suggestions: []
        };

        const essentialRoles = ['hazard-setter', 'pivot', 'physical-sweeper', 'special-sweeper', 'physical-wall'];
        const filledRoles = new Set();

        for (const pokemon of team.pokemon) {
            // Role tracking
            if (pokemon.role) {
                filledRoles.add(pokemon.role);
            }

            // Speed tier analysis
            const baseSpeed = this.getBaseSpeed(pokemon.species);
            if (baseSpeed >= 100) {
                synergy.speedTiers.fast.push(pokemon.species);
            } else if (baseSpeed >= 70) {
                synergy.speedTiers.medium.push(pokemon.species);
            } else {
                synergy.speedTiers.slow.push(pokemon.species);
            }

            // Offensive balance
            const attackType = this.determineOffensiveType(pokemon);
            synergy.offensiveBalance[attackType]++;

            // Hazard control
            const hazardSetMoves = ['stealth-rock', 'spikes', 'toxic-spikes', 'sticky-web'];
            const hazardRemoveMoves = ['defog', 'rapid-spin', 'court-change', 'mortal-spin'];

            for (const move of pokemon.moves) {
                if (hazardSetMoves.includes(move.toLowerCase())) {
                    synergy.hazardControl.setters.push({ pokemon: pokemon.species, move });
                }
                if (hazardRemoveMoves.includes(move.toLowerCase())) {
                    synergy.hazardControl.removers.push({ pokemon: pokemon.species, move });
                }
            }

            // Pivot moves
            const pivotMoves = ['u-turn', 'volt-switch', 'flip-turn', 'teleport', 'parting-shot'];
            for (const move of pokemon.moves) {
                if (pivotMoves.includes(move.toLowerCase())) {
                    synergy.pivoting.push({ pokemon: pokemon.species, move });
                }
            }
        }

        // Role balance
        synergy.roleBalance.filled = [...filledRoles];
        synergy.roleBalance.missing = essentialRoles.filter(r => !filledRoles.has(r));

        // Generate suggestions
        if (synergy.hazardControl.setters.length === 0) {
            synergy.suggestions.push('Consider adding a Stealth Rock setter');
        }
        if (synergy.hazardControl.removers.length === 0) {
            synergy.suggestions.push('Consider adding Defog or Rapid Spin for hazard removal');
        }
        if (synergy.pivoting.length === 0) {
            synergy.suggestions.push('Consider adding a Pokemon with U-turn/Volt Switch for momentum');
        }
        if (synergy.speedTiers.fast.length === 0) {
            synergy.suggestions.push('Team lacks fast Pokemon - consider adding a speed threat');
        }
        if (synergy.offensiveBalance.physical === 0) {
            synergy.suggestions.push('No physical attackers - special walls will be problematic');
        }
        if (synergy.offensiveBalance.special === 0) {
            synergy.suggestions.push('No special attackers - physical walls will be problematic');
        }

        // Calculate score
        let score = 50; // Base score
        if (synergy.hazardControl.setters.length > 0) score += 10;
        if (synergy.hazardControl.removers.length > 0) score += 10;
        if (synergy.pivoting.length > 0) score += 10;
        if (synergy.speedTiers.fast.length >= 2) score += 10;
        if (synergy.offensiveBalance.physical > 0 && synergy.offensiveBalance.special > 0) score += 10;
        score -= synergy.roleBalance.missing.length * 5;

        synergy.score = Math.max(0, Math.min(100, score));

        return synergy;
    }

    /**
     * Get base speed (simplified)
     */
    getBaseSpeed(species) {
        const speedData = {
            'dragapult': 142, 'weavile': 125, 'gengar': 110, 'garchomp': 102,
            'landorus-therian': 91, 'excadrill': 88, 'ferrothorn': 20,
            'toxapex': 35, 'corviknight': 67, 'clefable': 60, 'blissey': 55,
            'dragonite': 80, 'volcarona': 100, 'hydreigon': 98, 'alakazam': 120,
            'gyarados': 81, 'kingambit': 50, 'great-tusk': 87, 'iron-valiant': 116
        };
        return speedData[species?.toLowerCase()] || 80;
    }

    /**
     * Determine offensive type
     */
    determineOffensiveType(pokemon) {
        const physicalMoves = ['earthquake', 'close-combat', 'stone-edge', 'knock-off', 'u-turn', 'iron-head'];
        const specialMoves = ['flamethrower', 'thunderbolt', 'ice-beam', 'psychic', 'shadow-ball', 'moonblast'];

        let physical = 0, special = 0;
        for (const move of pokemon.moves) {
            if (physicalMoves.includes(move.toLowerCase())) physical++;
            if (specialMoves.includes(move.toLowerCase())) special++;
        }

        if (physical > special) return 'physical';
        if (special > physical) return 'special';
        return 'mixed';
    }

    /**
     * Full team analysis
     */
    analyzeTeam(team) {
        if (team.pokemon.length === 0) {
            return { error: 'Team is empty' };
        }

        const typeCoverage = this.analyzeTypeCoverage(team);
        const synergy = this.analyzeTeamSynergy(team);

        // Threat analysis
        const threats = this.identifyThreats(team);

        return {
            teamName: team.name,
            pokemonCount: team.pokemon.length,
            typeCoverage,
            synergy,
            threats,
            overallRating: this.calculateOverallRating(typeCoverage, synergy),
            summary: this.generateTeamSummary(team, typeCoverage, synergy)
        };
    }

    /**
     * Identify common threats
     */
    identifyThreats(team) {
        const commonThreats = [
            { pokemon: 'garchomp', types: ['dragon', 'ground'], threats: ['swords-dance', 'earthquake', 'scale-shot'] },
            { pokemon: 'dragapult', types: ['dragon', 'ghost'], threats: ['dragon-darts', 'shadow-ball', 'u-turn'] },
            { pokemon: 'iron-valiant', types: ['fairy', 'fighting'], threats: ['moonblast', 'close-combat', 'swords-dance'] },
            { pokemon: 'great-tusk', types: ['ground', 'fighting'], threats: ['earthquake', 'close-combat', 'rapid-spin'] },
            { pokemon: 'gholdengo', types: ['steel', 'ghost'], threats: ['make-it-rain', 'shadow-ball', 'nasty-plot'] },
            { pokemon: 'kingambit', types: ['dark', 'steel'], threats: ['kowtow-cleave', 'sucker-punch', 'swords-dance'] },
            { pokemon: 'roaring-moon', types: ['dragon', 'dark'], threats: ['dragon-dance', 'acrobatics', 'crunch'] }
        ];

        const threats = [];

        for (const threat of commonThreats) {
            let canHandle = false;
            let checks = [];

            for (const pokemon of team.pokemon) {
                // Check if team member can handle threat
                const effectiveness = Math.max(
                    ...threat.types.map(t => {
                        const coverage = this.analyzeTypeCoverage({ pokemon: [pokemon] });
                        return coverage.offensive[t]?.superEffective ? 2 : 1;
                    })
                );

                if (effectiveness > 1) {
                    canHandle = true;
                    checks.push(pokemon.species);
                }
            }

            threats.push({
                pokemon: threat.pokemon,
                canHandle,
                checks,
                severity: canHandle ? 'low' : 'high'
            });
        }

        return threats;
    }

    /**
     * Calculate overall rating
     */
    calculateOverallRating(typeCoverage, synergy) {
        let rating = synergy.score;

        // Penalize for uncovered types
        rating -= typeCoverage.uncoveredTypes.length * 3;

        // Penalize for stacked weaknesses
        rating -= typeCoverage.weaknessStack.length * 5;

        // Bonus for good coverage
        const coveredTypes = 18 - typeCoverage.uncoveredTypes.length;
        if (coveredTypes >= 15) rating += 10;

        return {
            score: Math.max(0, Math.min(100, Math.round(rating))),
            grade: rating >= 80 ? 'A' : rating >= 60 ? 'B' : rating >= 40 ? 'C' : rating >= 20 ? 'D' : 'F',
            description: rating >= 80 ? 'Excellent team composition' :
                         rating >= 60 ? 'Good team with minor gaps' :
                         rating >= 40 ? 'Decent team, needs improvement' :
                         rating >= 20 ? 'Weak team composition' : 'Major issues need addressing'
        };
    }

    /**
     * Generate team summary
     */
    generateTeamSummary(team, typeCoverage, synergy) {
        const lines = [];

        lines.push(`**Team: ${team.name}** (${team.pokemon.length}/6 Pokemon)`);
        lines.push('');

        // Pokemon list
        lines.push('**Pokemon:**');
        for (const pokemon of team.pokemon) {
            lines.push(`- ${pokemon.species} @ ${pokemon.item || 'No item'} (${pokemon.role || 'No role'})`);
        }
        lines.push('');

        // Type coverage
        if (typeCoverage.uncoveredTypes.length > 0) {
            lines.push(`**Uncovered Types:** ${typeCoverage.uncoveredTypes.join(', ')}`);
        } else {
            lines.push('**Type Coverage:** Complete!');
        }

        // Weakness stacking
        if (typeCoverage.weaknessStack.length > 0) {
            lines.push(`**Stacked Weaknesses:** ${typeCoverage.weaknessStack.map(w => `${w.type} (${w.count}x)`).join(', ')}`);
        }
        lines.push('');

        // Synergy suggestions
        if (synergy.suggestions.length > 0) {
            lines.push('**Suggestions:**');
            for (const suggestion of synergy.suggestions) {
                lines.push(`- ${suggestion}`);
            }
        }

        return lines.join('\n');
    }

    /**
     * Suggest Pokemon to fill gaps
     */
    suggestPokemon(team) {
        const analysis = this.analyzeTeam(team);
        const suggestions = [];

        // Based on uncovered types
        for (const type of analysis.typeCoverage.uncoveredTypes) {
            suggestions.push({
                reason: `Cover ${type} type`,
                pokemon: this.getPokemonWithCoverage(type)
            });
        }

        // Based on missing roles
        for (const role of analysis.synergy.roleBalance.missing) {
            suggestions.push({
                reason: `Fill ${role} role`,
                pokemon: this.pokemonByRole[role] || []
            });
        }

        return suggestions.slice(0, 5); // Top 5 suggestions
    }

    /**
     * Get Pokemon that cover a type
     */
    getPokemonWithCoverage(type) {
        const coverage = {
            'steel': ['garchomp', 'excadrill', 'great-tusk'],
            'fairy': ['iron-valiant', 'gholdengo', 'kingambit'],
            'dragon': ['iron-valiant', 'clefable', 'gardevoir'],
            'ground': ['weavile', 'rotom-wash', 'gyarados'],
            'fighting': ['dragapult', 'corviknight', 'dragonite'],
            'fire': ['great-tusk', 'garchomp', 'gastrodon'],
            'water': ['ferrothorn', 'rillaboom', 'rotom-wash'],
            'electric': ['great-tusk', 'garchomp', 'ting-lu'],
            'ice': ['iron-valiant', 'scizor', 'kingambit'],
            'psychic': ['kingambit', 'tyranitar', 'gholdengo']
        };
        return coverage[type] || [];
    }

    /**
     * Export team to Showdown format
     */
    exportToShowdown(team) {
        let output = '';

        for (const pokemon of team.pokemon) {
            // Species and item
            if (pokemon.nickname) {
                output += `${pokemon.nickname} (${pokemon.species})`;
            } else {
                output += pokemon.species;
            }
            if (pokemon.item) {
                output += ` @ ${pokemon.item}`;
            }
            output += '\n';

            // Ability
            if (pokemon.ability) {
                output += `Ability: ${pokemon.ability}\n`;
            }

            // Tera Type
            if (pokemon.teraType) {
                output += `Tera Type: ${pokemon.teraType}\n`;
            }

            // EVs
            const evs = [];
            for (const [stat, value] of Object.entries(pokemon.evs || {})) {
                if (value > 0) {
                    evs.push(`${value} ${stat.charAt(0).toUpperCase() + stat.slice(1)}`);
                }
            }
            if (evs.length > 0) {
                output += `EVs: ${evs.join(' / ')}\n`;
            }

            // Nature
            if (pokemon.nature && pokemon.nature !== 'hardy') {
                output += `${pokemon.nature.charAt(0).toUpperCase() + pokemon.nature.slice(1)} Nature\n`;
            }

            // IVs (only if not 31)
            const ivs = [];
            for (const [stat, value] of Object.entries(pokemon.ivs || {})) {
                if (value < 31) {
                    ivs.push(`${value} ${stat.charAt(0).toUpperCase() + stat.slice(1)}`);
                }
            }
            if (ivs.length > 0) {
                output += `IVs: ${ivs.join(' / ')}\n`;
            }

            // Moves
            for (const move of pokemon.moves || []) {
                output += `- ${move}\n`;
            }

            output += '\n';
        }

        return output.trim();
    }

    /**
     * Import team from Showdown format
     */
    importFromShowdown(showdownText) {
        const team = this.createTeam('Imported Team');
        const pokemonBlocks = showdownText.trim().split('\n\n');

        for (const block of pokemonBlocks) {
            const lines = block.trim().split('\n');
            if (lines.length === 0) continue;

            const pokemon = {
                species: '',
                nickname: '',
                item: '',
                ability: '',
                nature: 'hardy',
                teraType: null,
                evs: { hp: 0, atk: 0, def: 0, spa: 0, spd: 0, spe: 0 },
                ivs: { hp: 31, atk: 31, def: 31, spa: 31, spd: 31, spe: 31 },
                moves: [],
                types: [],
                role: null
            };

            // Parse first line (species, nickname, item)
            const firstLine = lines[0];
            const itemMatch = firstLine.match(/(.+?)\s*@\s*(.+)/);

            if (itemMatch) {
                const namepart = itemMatch[1].trim();
                pokemon.item = itemMatch[2].trim();

                const nicknameMatch = namepart.match(/(.+?)\s*\((.+)\)/);
                if (nicknameMatch) {
                    pokemon.nickname = nicknameMatch[1].trim();
                    pokemon.species = nicknameMatch[2].trim();
                } else {
                    pokemon.species = namepart;
                }
            } else {
                const nicknameMatch = firstLine.match(/(.+?)\s*\((.+)\)/);
                if (nicknameMatch) {
                    pokemon.nickname = nicknameMatch[1].trim();
                    pokemon.species = nicknameMatch[2].trim();
                } else {
                    pokemon.species = firstLine.trim();
                }
            }

            // Parse remaining lines
            for (let i = 1; i < lines.length; i++) {
                const line = lines[i].trim();

                if (line.startsWith('Ability:')) {
                    pokemon.ability = line.replace('Ability:', '').trim();
                } else if (line.startsWith('Tera Type:')) {
                    pokemon.teraType = line.replace('Tera Type:', '').trim();
                } else if (line.startsWith('EVs:')) {
                    const evParts = line.replace('EVs:', '').split('/');
                    for (const part of evParts) {
                        const match = part.trim().match(/(\d+)\s*(\w+)/);
                        if (match) {
                            const stat = match[2].toLowerCase();
                            const statMap = { 'hp': 'hp', 'atk': 'atk', 'def': 'def', 'spa': 'spa', 'spd': 'spd', 'spe': 'spe' };
                            if (statMap[stat]) {
                                pokemon.evs[statMap[stat]] = parseInt(match[1]);
                            }
                        }
                    }
                } else if (line.startsWith('IVs:')) {
                    const ivParts = line.replace('IVs:', '').split('/');
                    for (const part of ivParts) {
                        const match = part.trim().match(/(\d+)\s*(\w+)/);
                        if (match) {
                            const stat = match[2].toLowerCase();
                            const statMap = { 'hp': 'hp', 'atk': 'atk', 'def': 'def', 'spa': 'spa', 'spd': 'spd', 'spe': 'spe' };
                            if (statMap[stat]) {
                                pokemon.ivs[statMap[stat]] = parseInt(match[1]);
                            }
                        }
                    }
                } else if (line.endsWith('Nature')) {
                    pokemon.nature = line.replace('Nature', '').trim().toLowerCase();
                } else if (line.startsWith('-')) {
                    pokemon.moves.push(line.substring(1).trim());
                }
            }

            this.addPokemon(team, pokemon);
        }

        return team;
    }
}

module.exports = PokemonTeamBuilder;
