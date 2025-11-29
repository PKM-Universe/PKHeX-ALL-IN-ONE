/**
 * Pokemon Auto-Legality Fixer
 * Automatically fixes illegal Pokemon to make them legal
 */

const fs = require('fs');
const path = require('path');

class PokemonAutoLegality {
    constructor() {
        this.dataPath = path.join(__dirname, '..', 'Json');

        // Valid natures
        this.natures = [
            'hardy', 'lonely', 'brave', 'adamant', 'naughty',
            'bold', 'docile', 'relaxed', 'impish', 'lax',
            'timid', 'hasty', 'serious', 'jolly', 'naive',
            'modest', 'mild', 'quiet', 'bashful', 'rash',
            'calm', 'gentle', 'sassy', 'careful', 'quirky'
        ];

        // Valid balls by generation
        this.validBalls = {
            gen1: ['poke-ball', 'great-ball', 'ultra-ball', 'master-ball'],
            gen2: ['poke-ball', 'great-ball', 'ultra-ball', 'master-ball', 'safari-ball', 'sport-ball', 'level-ball', 'lure-ball', 'moon-ball', 'friend-ball', 'love-ball', 'heavy-ball', 'fast-ball'],
            gen3: ['poke-ball', 'great-ball', 'ultra-ball', 'master-ball', 'safari-ball', 'net-ball', 'dive-ball', 'nest-ball', 'repeat-ball', 'timer-ball', 'luxury-ball', 'premier-ball'],
            gen4: ['poke-ball', 'great-ball', 'ultra-ball', 'master-ball', 'safari-ball', 'net-ball', 'dive-ball', 'nest-ball', 'repeat-ball', 'timer-ball', 'luxury-ball', 'premier-ball', 'dusk-ball', 'heal-ball', 'quick-ball', 'cherish-ball', 'park-ball'],
            gen5: ['poke-ball', 'great-ball', 'ultra-ball', 'master-ball', 'net-ball', 'dive-ball', 'nest-ball', 'repeat-ball', 'timer-ball', 'luxury-ball', 'premier-ball', 'dusk-ball', 'heal-ball', 'quick-ball', 'dream-ball'],
            gen7: ['poke-ball', 'great-ball', 'ultra-ball', 'master-ball', 'net-ball', 'dive-ball', 'nest-ball', 'repeat-ball', 'timer-ball', 'luxury-ball', 'premier-ball', 'dusk-ball', 'heal-ball', 'quick-ball', 'beast-ball'],
            gen8: ['poke-ball', 'great-ball', 'ultra-ball', 'master-ball', 'net-ball', 'dive-ball', 'nest-ball', 'repeat-ball', 'timer-ball', 'luxury-ball', 'premier-ball', 'dusk-ball', 'heal-ball', 'quick-ball', 'dream-ball', 'beast-ball', 'safari-ball', 'sport-ball', 'level-ball', 'lure-ball', 'moon-ball', 'friend-ball', 'love-ball', 'heavy-ball', 'fast-ball'],
            gen9: ['poke-ball', 'great-ball', 'ultra-ball', 'master-ball', 'net-ball', 'dive-ball', 'nest-ball', 'repeat-ball', 'timer-ball', 'luxury-ball', 'premier-ball', 'dusk-ball', 'heal-ball', 'quick-ball', 'dream-ball', 'beast-ball', 'safari-ball', 'sport-ball', 'level-ball', 'lure-ball', 'moon-ball', 'friend-ball', 'love-ball', 'heavy-ball', 'fast-ball']
        };

        // Gender ratios
        this.genderRatios = {
            'male-only': ['nidorino', 'nidoking', 'hitmonlee', 'hitmonchan', 'hitmontop', 'tauros', 'tyrogue', 'throh', 'sawk', 'rufflet', 'braviary', 'tornadus', 'thundurus', 'landorus'],
            'female-only': ['nidorina', 'nidoqueen', 'chansey', 'kangaskhan', 'jynx', 'miltank', 'blissey', 'happiny', 'vespiquen', 'petilil', 'lilligant', 'vullaby', 'mandibuzz', 'flabebe', 'floette', 'florges', 'bounsweet', 'steenee', 'tsareena', 'salazzle'],
            'genderless': ['magnemite', 'magneton', 'voltorb', 'electrode', 'staryu', 'starmie', 'ditto', 'porygon', 'porygon2', 'porygon-z', 'shedinja', 'lunatone', 'solrock', 'baltoy', 'claydol', 'beldum', 'metang', 'metagross', 'bronzor', 'bronzong', 'rotom', 'cryogonal', 'golett', 'golurk', 'klink', 'klang', 'klinklang', 'carbink', 'minior', 'dhelmise', 'sinistea', 'polteageist', 'falinks']
        };

        // Mythicals/Legendaries that can't be shiny from normal means
        this.shinyLocked = [
            'victini', 'keldeo', 'meloetta', 'hoopa', 'volcanion', 'magearna',
            'marshadow', 'zeraora', 'zarude', 'glastrier', 'spectrier', 'calyrex',
            'enamorus', 'wo-chien', 'chien-pao', 'ting-lu', 'chi-yu',
            'koraidon', 'miraidon', 'walking-wake', 'iron-leaves',
            'okidogi', 'munkidori', 'fezandipiti', 'ogerpon', 'terapagos', 'pecharunt'
        ];
    }

    // Main fix function
    fixPokemon(pokemon, options = {}) {
        const fixes = [];
        const fixed = { ...pokemon };

        // Fix species
        if (!fixed.species || typeof fixed.species !== 'string') {
            fixed.species = 'Pikachu';
            fixes.push({ field: 'species', issue: 'Missing or invalid species', fix: 'Set to Pikachu' });
        }

        // Fix level
        const levelFix = this.fixLevel(fixed);
        if (levelFix) fixes.push(levelFix);

        // Fix IVs
        const ivFixes = this.fixIVs(fixed);
        fixes.push(...ivFixes);

        // Fix EVs
        const evFixes = this.fixEVs(fixed);
        fixes.push(...evFixes);

        // Fix nature
        const natureFix = this.fixNature(fixed);
        if (natureFix) fixes.push(natureFix);

        // Fix moves
        const moveFixes = this.fixMoves(fixed);
        fixes.push(...moveFixes);

        // Fix ball
        const ballFix = this.fixBall(fixed, options.originGame);
        if (ballFix) fixes.push(ballFix);

        // Fix shiny status
        const shinyFix = this.fixShiny(fixed);
        if (shinyFix) fixes.push(shinyFix);

        // Fix gender
        const genderFix = this.fixGender(fixed);
        if (genderFix) fixes.push(genderFix);

        // Fix OT/TID/SID
        const trainerFixes = this.fixTrainerInfo(fixed);
        fixes.push(...trainerFixes);

        // Fix friendship/happiness
        const friendshipFix = this.fixFriendship(fixed);
        if (friendshipFix) fixes.push(friendshipFix);

        // Fix ability
        const abilityFix = this.fixAbility(fixed);
        if (abilityFix) fixes.push(abilityFix);

        // Fix met conditions
        const metFixes = this.fixMetConditions(fixed, options.originGame);
        fixes.push(...metFixes);

        return {
            success: true,
            pokemon: fixed,
            fixesApplied: fixes.length,
            fixes
        };
    }

    fixLevel(pokemon) {
        if (!pokemon.level || pokemon.level < 1 || pokemon.level > 100) {
            const oldLevel = pokemon.level;
            pokemon.level = Math.max(1, Math.min(100, pokemon.level || 100));
            return {
                field: 'level',
                issue: `Invalid level: ${oldLevel}`,
                fix: `Set to ${pokemon.level}`
            };
        }
        return null;
    }

    fixIVs(pokemon) {
        const fixes = [];
        if (!pokemon.ivs) {
            pokemon.ivs = { hp: 31, atk: 31, def: 31, spa: 31, spd: 31, spe: 31 };
            fixes.push({ field: 'ivs', issue: 'Missing IVs', fix: 'Set to perfect IVs' });
        } else {
            for (const stat of ['hp', 'atk', 'def', 'spa', 'spd', 'spe']) {
                const value = pokemon.ivs[stat];
                if (value === undefined || value < 0 || value > 31) {
                    const oldValue = value;
                    pokemon.ivs[stat] = Math.max(0, Math.min(31, value || 31));
                    fixes.push({
                        field: `ivs.${stat}`,
                        issue: `Invalid IV: ${oldValue}`,
                        fix: `Set to ${pokemon.ivs[stat]}`
                    });
                }
            }
        }
        return fixes;
    }

    fixEVs(pokemon) {
        const fixes = [];
        if (!pokemon.evs) {
            pokemon.evs = { hp: 0, atk: 0, def: 0, spa: 0, spd: 0, spe: 0 };
            fixes.push({ field: 'evs', issue: 'Missing EVs', fix: 'Set to 0' });
        } else {
            // Fix individual EVs
            for (const stat of ['hp', 'atk', 'def', 'spa', 'spd', 'spe']) {
                const value = pokemon.evs[stat];
                if (value === undefined || value < 0 || value > 252) {
                    const oldValue = value;
                    pokemon.evs[stat] = Math.max(0, Math.min(252, value || 0));
                    fixes.push({
                        field: `evs.${stat}`,
                        issue: `Invalid EV: ${oldValue}`,
                        fix: `Set to ${pokemon.evs[stat]}`
                    });
                }
            }

            // Fix total EVs
            const total = Object.values(pokemon.evs).reduce((a, b) => a + b, 0);
            if (total > 510) {
                const scale = 510 / total;
                for (const stat of ['hp', 'atk', 'def', 'spa', 'spd', 'spe']) {
                    pokemon.evs[stat] = Math.floor(pokemon.evs[stat] * scale);
                }
                fixes.push({
                    field: 'evs.total',
                    issue: `EV total too high: ${total}`,
                    fix: 'Scaled down to 510'
                });
            }
        }
        return fixes;
    }

    fixNature(pokemon) {
        if (!pokemon.nature || !this.natures.includes(pokemon.nature.toLowerCase())) {
            const oldNature = pokemon.nature;
            pokemon.nature = 'Hardy';
            return {
                field: 'nature',
                issue: `Invalid nature: ${oldNature}`,
                fix: 'Set to Hardy'
            };
        }
        return null;
    }

    fixMoves(pokemon) {
        const fixes = [];
        if (!pokemon.moves) {
            pokemon.moves = [];
            fixes.push({ field: 'moves', issue: 'Missing moves', fix: 'Initialized empty' });
        } else if (pokemon.moves.length > 4) {
            pokemon.moves = pokemon.moves.slice(0, 4);
            fixes.push({ field: 'moves', issue: 'Too many moves', fix: 'Trimmed to 4' });
        }

        // Remove duplicate moves
        const uniqueMoves = [...new Set(pokemon.moves)];
        if (uniqueMoves.length !== pokemon.moves.length) {
            pokemon.moves = uniqueMoves;
            fixes.push({ field: 'moves', issue: 'Duplicate moves', fix: 'Removed duplicates' });
        }

        return fixes;
    }

    fixBall(pokemon, originGame = 'gen9') {
        if (!pokemon.ball) {
            pokemon.ball = 'poke-ball';
            return { field: 'ball', issue: 'Missing ball', fix: 'Set to Poke Ball' };
        }

        const validBalls = this.validBalls[originGame] || this.validBalls.gen9;
        const ballLower = pokemon.ball.toLowerCase().replace(/\s+/g, '-');

        if (!validBalls.includes(ballLower)) {
            const oldBall = pokemon.ball;
            pokemon.ball = 'poke-ball';
            return {
                field: 'ball',
                issue: `Invalid ball for ${originGame}: ${oldBall}`,
                fix: 'Set to Poke Ball'
            };
        }

        return null;
    }

    fixShiny(pokemon) {
        const species = pokemon.species?.toLowerCase();
        if (pokemon.shiny && this.shinyLocked.includes(species)) {
            pokemon.shiny = false;
            return {
                field: 'shiny',
                issue: `${pokemon.species} is shiny-locked`,
                fix: 'Set shiny to false'
            };
        }
        return null;
    }

    fixGender(pokemon) {
        const species = pokemon.species?.toLowerCase();

        if (this.genderRatios['male-only'].includes(species)) {
            if (pokemon.gender !== 'male') {
                pokemon.gender = 'male';
                return { field: 'gender', issue: 'Species is male-only', fix: 'Set to male' };
            }
        } else if (this.genderRatios['female-only'].includes(species)) {
            if (pokemon.gender !== 'female') {
                pokemon.gender = 'female';
                return { field: 'gender', issue: 'Species is female-only', fix: 'Set to female' };
            }
        } else if (this.genderRatios['genderless'].includes(species)) {
            if (pokemon.gender) {
                pokemon.gender = null;
                return { field: 'gender', issue: 'Species is genderless', fix: 'Removed gender' };
            }
        }

        return null;
    }

    fixTrainerInfo(pokemon) {
        const fixes = [];

        if (!pokemon.ot || typeof pokemon.ot !== 'string' || pokemon.ot.length === 0) {
            pokemon.ot = 'PKHex';
            fixes.push({ field: 'ot', issue: 'Missing OT', fix: 'Set to PKHex' });
        } else if (pokemon.ot.length > 12) {
            pokemon.ot = pokemon.ot.substring(0, 12);
            fixes.push({ field: 'ot', issue: 'OT too long', fix: 'Trimmed to 12 chars' });
        }

        if (!pokemon.tid || pokemon.tid < 0 || pokemon.tid > 65535) {
            pokemon.tid = Math.floor(Math.random() * 65535);
            fixes.push({ field: 'tid', issue: 'Invalid TID', fix: `Set to ${pokemon.tid}` });
        }

        if (!pokemon.sid || pokemon.sid < 0 || pokemon.sid > 65535) {
            pokemon.sid = Math.floor(Math.random() * 65535);
            fixes.push({ field: 'sid', issue: 'Invalid SID', fix: `Set to ${pokemon.sid}` });
        }

        return fixes;
    }

    fixFriendship(pokemon) {
        if (pokemon.friendship === undefined || pokemon.friendship < 0 || pokemon.friendship > 255) {
            const oldValue = pokemon.friendship;
            pokemon.friendship = 255;
            return {
                field: 'friendship',
                issue: `Invalid friendship: ${oldValue}`,
                fix: 'Set to 255'
            };
        }
        return null;
    }

    fixAbility(pokemon) {
        // Basic check - just ensure ability exists
        if (!pokemon.ability) {
            pokemon.ability = null; // Will use default
            return { field: 'ability', issue: 'Missing ability', fix: 'Will use default' };
        }
        return null;
    }

    fixMetConditions(pokemon, originGame = 'gen9') {
        const fixes = [];

        if (!pokemon.metLevel || pokemon.metLevel < 1 || pokemon.metLevel > 100) {
            pokemon.metLevel = pokemon.level || 1;
            fixes.push({ field: 'metLevel', issue: 'Invalid met level', fix: `Set to ${pokemon.metLevel}` });
        }

        if (pokemon.metLevel > pokemon.level) {
            pokemon.metLevel = pokemon.level;
            fixes.push({ field: 'metLevel', issue: 'Met level higher than current', fix: `Set to ${pokemon.metLevel}` });
        }

        if (!pokemon.metDate) {
            pokemon.metDate = new Date().toISOString().split('T')[0];
            fixes.push({ field: 'metDate', issue: 'Missing met date', fix: `Set to ${pokemon.metDate}` });
        }

        return fixes;
    }

    // Batch fix multiple Pokemon
    batchFix(pokemonList, options = {}) {
        const results = [];
        for (const pokemon of pokemonList) {
            results.push(this.fixPokemon(pokemon, options));
        }

        const totalFixes = results.reduce((sum, r) => sum + r.fixesApplied, 0);

        return {
            success: true,
            count: results.length,
            totalFixes,
            results
        };
    }

    // Validate without fixing
    validate(pokemon) {
        const issues = [];

        // Level
        if (!pokemon.level || pokemon.level < 1 || pokemon.level > 100) {
            issues.push(`Invalid level: ${pokemon.level}`);
        }

        // IVs
        if (pokemon.ivs) {
            for (const [stat, value] of Object.entries(pokemon.ivs)) {
                if (value < 0 || value > 31) {
                    issues.push(`Invalid ${stat} IV: ${value}`);
                }
            }
        }

        // EVs
        if (pokemon.evs) {
            let total = 0;
            for (const [stat, value] of Object.entries(pokemon.evs)) {
                if (value < 0 || value > 252) {
                    issues.push(`Invalid ${stat} EV: ${value}`);
                }
                total += value;
            }
            if (total > 510) {
                issues.push(`EV total exceeds 510: ${total}`);
            }
        }

        // Moves
        if (pokemon.moves && pokemon.moves.length > 4) {
            issues.push('Too many moves');
        }

        // Shiny lock
        const species = pokemon.species?.toLowerCase();
        if (pokemon.shiny && this.shinyLocked.includes(species)) {
            issues.push(`${pokemon.species} cannot be shiny`);
        }

        return {
            valid: issues.length === 0,
            issues
        };
    }

    // Generate legal defaults
    generateLegalDefaults(species, options = {}) {
        return {
            species: species || 'Pikachu',
            level: options.level || 100,
            shiny: options.shiny || false,
            nature: options.nature || 'Hardy',
            ability: options.ability || null,
            item: options.item || null,
            ball: 'poke-ball',
            gender: null,
            ivs: { hp: 31, atk: 31, def: 31, spa: 31, spd: 31, spe: 31 },
            evs: { hp: 0, atk: 0, def: 0, spa: 0, spd: 0, spe: 0 },
            moves: options.moves || [],
            ot: options.ot || 'PKHex',
            tid: Math.floor(Math.random() * 65535),
            sid: Math.floor(Math.random() * 65535),
            language: options.language || 'ENG',
            friendship: 255,
            metLevel: options.level || 1,
            metLocation: 'Paldea',
            metDate: new Date().toISOString().split('T')[0],
            originGame: options.originGame || 'Scarlet/Violet'
        };
    }
}

module.exports = PokemonAutoLegality;
