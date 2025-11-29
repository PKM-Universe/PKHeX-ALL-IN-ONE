/**
 * Pokemon Contest Stats Editor
 * Beauty/Cool/Cute/Smart/Tough editing for Gen 3-4 and BDSP/ORAS contests
 */

class PokemonContestStats {
    constructor() {
        this.stats = ['cool', 'beauty', 'cute', 'smart', 'tough'];
        this.sheen = 'sheen'; // Reflects Pokeblock/Poffin consumption

        // Contest ranks
        this.ranks = {
            normal: { required: 0, label: 'Normal Rank' },
            super: { required: 100, label: 'Super Rank' },
            hyper: { required: 200, label: 'Hyper Rank' },
            master: { required: 300, label: 'Master Rank' }
        };

        // Pokeblock effects (Gen 3)
        this.pokeblocks = {
            'red-pokeblock': { primary: 'cool', level: 1 },
            'blue-pokeblock': { primary: 'beauty', level: 1 },
            'pink-pokeblock': { primary: 'cute', level: 1 },
            'green-pokeblock': { primary: 'smart', level: 1 },
            'yellow-pokeblock': { primary: 'tough', level: 1 },
            'gold-pokeblock': { all: true, level: 2 },
            'rainbow-pokeblock': { all: true, level: 3 }
        };

        // Poffin effects (Gen 4)
        this.poffins = {
            'spicy-poffin': { primary: 'cool', secondary: null },
            'dry-poffin': { primary: 'beauty', secondary: null },
            'sweet-poffin': { primary: 'cute', secondary: null },
            'bitter-poffin': { primary: 'smart', secondary: null },
            'sour-poffin': { primary: 'tough', secondary: null },
            'spicy-dry-poffin': { primary: 'cool', secondary: 'beauty' },
            'spicy-sweet-poffin': { primary: 'cool', secondary: 'cute' },
            'spicy-bitter-poffin': { primary: 'cool', secondary: 'smart' },
            'spicy-sour-poffin': { primary: 'cool', secondary: 'tough' },
            'mild-poffin': { all: true, level: 1 },
            'rich-poffin': { all: true, level: 2 },
            'supreme-poffin': { all: true, level: 3 }
        };

        // Contest moves by category
        this.contestMoves = {
            cool: {
                moves: ['aerial-ace', 'air-cutter', 'blaze-kick', 'brave-bird', 'counter', 'double-edge', 'dragon-claw', 'earthquake', 'fire-blast', 'flamethrower', 'focus-blast', 'giga-impact', 'head-smash', 'hyper-beam', 'iron-head', 'metal-claw', 'outrage', 'rock-slide', 'sky-attack', 'stone-edge', 'thunder', 'thunderbolt'],
                effect: 'Startles Pokemon that made a same-type appeal'
            },
            beauty: {
                moves: ['aurora-beam', 'blizzard', 'bubble-beam', 'draco-meteor', 'dragon-pulse', 'flash-cannon', 'hydro-pump', 'ice-beam', 'moonblast', 'psychic', 'shadow-ball', 'signal-beam', 'surf', 'water-pulse', 'waterfall'],
                effect: 'Startles all Pokemon that have made an appeal'
            },
            cute: {
                moves: ['attract', 'baby-doll-eyes', 'charm', 'copycat', 'disarming-voice', 'draining-kiss', 'encore', 'fake-tears', 'follow-me', 'helping-hand', 'metronome', 'mimic', 'play-nice', 'pound', 'present', 'sweet-kiss', 'tail-whip', 'wish'],
                effect: 'Makes the next Pokemon nervous'
            },
            smart: {
                moves: ['agility', 'amnesia', 'barrier', 'baton-pass', 'calm-mind', 'confusion', 'cosmic-power', 'destiny-bond', 'detect', 'disable', 'dream-eater', 'future-sight', 'hypnosis', 'light-screen', 'lock-on', 'magic-coat', 'meditate', 'mind-reader', 'mirror-coat', 'nasty-plot', 'psychic', 'psych-up', 'reflect', 'rest', 'skill-swap', 'substitute', 'teleport', 'trick', 'trick-room'],
                effect: 'Startles the Pokemon that appealed before the user'
            },
            tough: {
                moves: ['arm-thrust', 'body-slam', 'brick-break', 'bulk-up', 'close-combat', 'cross-chop', 'earthquake', 'endure', 'facade', 'focus-punch', 'hammer-arm', 'iron-defense', 'mega-kick', 'mega-punch', 'protect', 'reversal', 'rock-tomb', 'rollout', 'seismic-toss', 'skull-bash', 'slam', 'stomp', 'strength', 'superpower', 'take-down', 'vital-throw']
            }
        };

        // Nature preferences for contest stats
        this.naturePreferences = {
            // Likes Spicy (Cool), Hates Dry (Beauty)
            lonely: { likes: 'spicy', hates: 'dry' },
            adamant: { likes: 'spicy', hates: 'dry' },
            naughty: { likes: 'spicy', hates: 'bitter' },
            brave: { likes: 'spicy', hates: 'sweet' },
            // Likes Sour (Tough), Hates Sweet (Cute)
            bold: { likes: 'sour', hates: 'spicy' },
            impish: { likes: 'sour', hates: 'dry' },
            lax: { likes: 'sour', hates: 'bitter' },
            relaxed: { likes: 'sour', hates: 'sweet' },
            // Likes Sweet (Cute), Hates Spicy (Cool)
            timid: { likes: 'sweet', hates: 'spicy' },
            hasty: { likes: 'sweet', hates: 'sour' },
            jolly: { likes: 'sweet', hates: 'dry' },
            naive: { likes: 'sweet', hates: 'bitter' },
            // Likes Dry (Beauty), Hates Sour (Tough)
            modest: { likes: 'dry', hates: 'spicy' },
            mild: { likes: 'dry', hates: 'sour' },
            quiet: { likes: 'dry', hates: 'sweet' },
            rash: { likes: 'dry', hates: 'bitter' },
            // Likes Bitter (Smart), Hates Sweet (Cute)
            calm: { likes: 'bitter', hates: 'spicy' },
            gentle: { likes: 'bitter', hates: 'sour' },
            careful: { likes: 'bitter', hates: 'dry' },
            sassy: { likes: 'bitter', hates: 'sweet' },
            // Neutral natures
            hardy: { likes: null, hates: null },
            docile: { likes: null, hates: null },
            bashful: { likes: null, hates: null },
            quirky: { likes: null, hates: null },
            serious: { likes: null, hates: null }
        };
    }

    createContestStats(values = {}) {
        return {
            cool: Math.min(255, Math.max(0, values.cool || 0)),
            beauty: Math.min(255, Math.max(0, values.beauty || 0)),
            cute: Math.min(255, Math.max(0, values.cute || 0)),
            smart: Math.min(255, Math.max(0, values.smart || 0)),
            tough: Math.min(255, Math.max(0, values.tough || 0)),
            sheen: Math.min(255, Math.max(0, values.sheen || 0))
        };
    }

    setContestStat(pokemon, stat, value) {
        if (!this.stats.includes(stat) && stat !== 'sheen') {
            return { error: `Invalid stat: ${stat}. Must be one of: ${this.stats.join(', ')}, sheen` };
        }

        if (!pokemon.contestStats) {
            pokemon.contestStats = this.createContestStats();
        }

        pokemon.contestStats[stat] = Math.min(255, Math.max(0, value));
        return { success: true, stat, value: pokemon.contestStats[stat] };
    }

    setAllContestStats(pokemon, value) {
        if (!pokemon.contestStats) {
            pokemon.contestStats = this.createContestStats();
        }

        const clampedValue = Math.min(255, Math.max(0, value));
        for (const stat of this.stats) {
            pokemon.contestStats[stat] = clampedValue;
        }

        return { success: true, value: clampedValue };
    }

    maxContestStats(pokemon) {
        pokemon.contestStats = this.createContestStats({
            cool: 255, beauty: 255, cute: 255, smart: 255, tough: 255, sheen: 255
        });
        return { success: true, stats: pokemon.contestStats };
    }

    feedPokeblock(pokemon, blockType) {
        const block = this.pokeblocks[blockType];
        if (!block) {
            return { error: `Unknown Pokeblock: ${blockType}` };
        }

        if (!pokemon.contestStats) {
            pokemon.contestStats = this.createContestStats();
        }

        // Check sheen limit
        if (pokemon.contestStats.sheen >= 255) {
            return { error: 'Pokemon\'s sheen is maxed. Cannot feed more Pokeblocks.' };
        }

        const changes = {};
        const baseIncrease = block.level * 10;

        if (block.all) {
            for (const stat of this.stats) {
                const increase = Math.min(baseIncrease, 255 - pokemon.contestStats[stat]);
                pokemon.contestStats[stat] += increase;
                changes[stat] = increase;
            }
        } else {
            const increase = Math.min(baseIncrease * 2, 255 - pokemon.contestStats[block.primary]);
            pokemon.contestStats[block.primary] += increase;
            changes[block.primary] = increase;
        }

        // Increase sheen
        const sheenIncrease = Math.min(block.level * 5, 255 - pokemon.contestStats.sheen);
        pokemon.contestStats.sheen += sheenIncrease;
        changes.sheen = sheenIncrease;

        return { success: true, fed: blockType, changes };
    }

    feedPoffin(pokemon, poffinType, nature) {
        const poffin = this.poffins[poffinType];
        if (!poffin) {
            return { error: `Unknown Poffin: ${poffinType}` };
        }

        if (!pokemon.contestStats) {
            pokemon.contestStats = this.createContestStats();
        }

        if (pokemon.contestStats.sheen >= 255) {
            return { error: 'Pokemon\'s sheen is maxed. Cannot feed more Poffins.' };
        }

        const preference = this.naturePreferences[nature?.toLowerCase()];
        const changes = {};
        let baseIncrease = (poffin.level || 1) * 15;

        // Apply nature modifier
        const applyNatureModifier = (stat, increase) => {
            const flavorMap = { cool: 'spicy', beauty: 'dry', cute: 'sweet', smart: 'bitter', tough: 'sour' };
            const flavor = flavorMap[stat];

            if (preference) {
                if (preference.likes === flavor) increase = Math.floor(increase * 1.1);
                if (preference.hates === flavor) increase = Math.floor(increase * 0.9);
            }

            return increase;
        };

        if (poffin.all) {
            for (const stat of this.stats) {
                let increase = applyNatureModifier(stat, baseIncrease);
                increase = Math.min(increase, 255 - pokemon.contestStats[stat]);
                pokemon.contestStats[stat] += increase;
                changes[stat] = increase;
            }
        } else {
            // Primary stat
            let primaryIncrease = applyNatureModifier(poffin.primary, baseIncrease * 2);
            primaryIncrease = Math.min(primaryIncrease, 255 - pokemon.contestStats[poffin.primary]);
            pokemon.contestStats[poffin.primary] += primaryIncrease;
            changes[poffin.primary] = primaryIncrease;

            // Secondary stat
            if (poffin.secondary) {
                let secondaryIncrease = applyNatureModifier(poffin.secondary, baseIncrease);
                secondaryIncrease = Math.min(secondaryIncrease, 255 - pokemon.contestStats[poffin.secondary]);
                pokemon.contestStats[poffin.secondary] += secondaryIncrease;
                changes[poffin.secondary] = secondaryIncrease;
            }
        }

        // Increase sheen
        const sheenIncrease = Math.min(10, 255 - pokemon.contestStats.sheen);
        pokemon.contestStats.sheen += sheenIncrease;
        changes.sheen = sheenIncrease;

        return { success: true, fed: poffinType, changes, nature };
    }

    getContestReadiness(pokemon, category) {
        if (!pokemon.contestStats) {
            return { ready: false, reason: 'No contest stats', currentValue: 0 };
        }

        const value = pokemon.contestStats[category];
        let recommendedRank = 'normal';

        for (const [rank, data] of Object.entries(this.ranks)) {
            if (value >= data.required) {
                recommendedRank = rank;
            }
        }

        return {
            category,
            currentValue: value,
            recommendedRank,
            rankLabel: this.ranks[recommendedRank].label,
            maxed: value >= 255
        };
    }

    getRecommendedMoves(pokemon, category) {
        const categoryMoves = this.contestMoves[category];
        if (!categoryMoves) {
            return { error: `Invalid category: ${category}` };
        }

        const pokemonMoves = pokemon.moves || [];
        const matching = pokemonMoves.filter(m => categoryMoves.moves.includes(m.toLowerCase()));

        return {
            category,
            availableMoves: categoryMoves.moves,
            pokemonHas: matching,
            effect: categoryMoves.effect,
            recommendation: matching.length >= 2 ? 'Good moveset for this contest' : 'Consider learning more moves for this category'
        };
    }

    getBestCategory(pokemon) {
        if (!pokemon.contestStats) {
            return { error: 'No contest stats' };
        }

        let best = null;
        let bestValue = -1;

        for (const stat of this.stats) {
            if (pokemon.contestStats[stat] > bestValue) {
                bestValue = pokemon.contestStats[stat];
                best = stat;
            }
        }

        return {
            bestCategory: best,
            value: bestValue,
            percentage: ((bestValue / 255) * 100).toFixed(1)
        };
    }

    getContestSummary(pokemon) {
        if (!pokemon.contestStats) {
            return { error: 'No contest stats', stats: this.createContestStats() };
        }

        const summary = {
            pokemon: pokemon.nickname || pokemon.species,
            stats: { ...pokemon.contestStats },
            total: 0,
            percentage: 0,
            bestCategory: null,
            canCompete: {}
        };

        // Calculate totals
        for (const stat of this.stats) {
            summary.total += pokemon.contestStats[stat];
        }
        summary.percentage = ((summary.total / (255 * 5)) * 100).toFixed(1);

        // Find best category
        const best = this.getBestCategory(pokemon);
        summary.bestCategory = best.bestCategory;

        // Check competition readiness
        for (const stat of this.stats) {
            summary.canCompete[stat] = this.getContestReadiness(pokemon, stat);
        }

        return summary;
    }

    generateVisualStats(pokemon) {
        if (!pokemon.contestStats) {
            return 'No contest stats';
        }

        const lines = [];
        lines.push('Contest Stats:');
        lines.push('─────────────────────────────');

        for (const stat of this.stats) {
            const value = pokemon.contestStats[stat];
            const barLength = Math.floor(value / 255 * 20);
            const bar = '█'.repeat(barLength) + '░'.repeat(20 - barLength);
            const statName = stat.charAt(0).toUpperCase() + stat.slice(1);
            lines.push(`${statName.padEnd(7)}: [${bar}] ${value}/255`);
        }

        lines.push('─────────────────────────────');
        lines.push(`Sheen:   [${('█'.repeat(Math.floor(pokemon.contestStats.sheen / 255 * 20)) + '░'.repeat(20 - Math.floor(pokemon.contestStats.sheen / 255 * 20)))}] ${pokemon.contestStats.sheen}/255`);

        return lines.join('\n');
    }
}

module.exports = PokemonContestStats;
