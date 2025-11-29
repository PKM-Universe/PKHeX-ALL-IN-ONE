/**
 * Pokemon Ribbon Editor
 * Full ribbon collection management across all generations
 */

class PokemonRibbonEditor {
    constructor() {
        this.ribbons = {
            // Gen 3 Contest Ribbons
            contest: {
                'cool-ribbon': { gen: 3, type: 'contest', rank: 'normal', description: 'Won Cool Contest' },
                'cool-ribbon-super': { gen: 3, type: 'contest', rank: 'super', description: 'Won Cool Contest Super Rank' },
                'cool-ribbon-hyper': { gen: 3, type: 'contest', rank: 'hyper', description: 'Won Cool Contest Hyper Rank' },
                'cool-ribbon-master': { gen: 3, type: 'contest', rank: 'master', description: 'Won Cool Contest Master Rank' },
                'beauty-ribbon': { gen: 3, type: 'contest', rank: 'normal', description: 'Won Beauty Contest' },
                'beauty-ribbon-super': { gen: 3, type: 'contest', rank: 'super', description: 'Won Beauty Contest Super Rank' },
                'beauty-ribbon-hyper': { gen: 3, type: 'contest', rank: 'hyper', description: 'Won Beauty Contest Hyper Rank' },
                'beauty-ribbon-master': { gen: 3, type: 'contest', rank: 'master', description: 'Won Beauty Contest Master Rank' },
                'cute-ribbon': { gen: 3, type: 'contest', rank: 'normal', description: 'Won Cute Contest' },
                'cute-ribbon-super': { gen: 3, type: 'contest', rank: 'super', description: 'Won Cute Contest Super Rank' },
                'cute-ribbon-hyper': { gen: 3, type: 'contest', rank: 'hyper', description: 'Won Cute Contest Hyper Rank' },
                'cute-ribbon-master': { gen: 3, type: 'contest', rank: 'master', description: 'Won Cute Contest Master Rank' },
                'smart-ribbon': { gen: 3, type: 'contest', rank: 'normal', description: 'Won Smart Contest' },
                'smart-ribbon-super': { gen: 3, type: 'contest', rank: 'super', description: 'Won Smart Contest Super Rank' },
                'smart-ribbon-hyper': { gen: 3, type: 'contest', rank: 'hyper', description: 'Won Smart Contest Hyper Rank' },
                'smart-ribbon-master': { gen: 3, type: 'contest', rank: 'master', description: 'Won Smart Contest Master Rank' },
                'tough-ribbon': { gen: 3, type: 'contest', rank: 'normal', description: 'Won Tough Contest' },
                'tough-ribbon-super': { gen: 3, type: 'contest', rank: 'super', description: 'Won Tough Contest Super Rank' },
                'tough-ribbon-hyper': { gen: 3, type: 'contest', rank: 'hyper', description: 'Won Tough Contest Hyper Rank' },
                'tough-ribbon-master': { gen: 3, type: 'contest', rank: 'master', description: 'Won Tough Contest Master Rank' }
            },

            // Battle Tower/Facility Ribbons
            tower: {
                'winning-ribbon': { gen: 3, type: 'tower', description: 'Won 56 battles in Battle Tower' },
                'victory-ribbon': { gen: 3, type: 'tower', description: 'Won 100 battles in Battle Tower' },
                'ability-ribbon': { gen: 4, type: 'tower', description: 'Cleared Battle Tower' },
                'great-ability-ribbon': { gen: 4, type: 'tower', description: 'Cleared Battle Tower (50 streak)' },
                'double-ability-ribbon': { gen: 4, type: 'tower', description: 'Cleared Battle Tower Doubles' },
                'multi-ability-ribbon': { gen: 4, type: 'tower', description: 'Cleared Battle Tower Multi' },
                'pair-ability-ribbon': { gen: 4, type: 'tower', description: 'Cleared Battle Tower Link Multi' },
                'world-ability-ribbon': { gen: 4, type: 'tower', description: 'Cleared Battle Frontier Brain' },
                'battle-tree-great-ribbon': { gen: 7, type: 'tower', description: 'Cleared Battle Tree Super Singles/Doubles' },
                'battle-tree-master-ribbon': { gen: 7, type: 'tower', description: 'Cleared Battle Tree Master Rank' },
                'tower-master-ribbon': { gen: 8, type: 'tower', description: 'Reached Master Ball tier in Battle Tower' }
            },

            // Champion/League Ribbons
            champion: {
                'champion-ribbon': { gen: 3, type: 'champion', description: 'Entered Hall of Fame (Hoenn)' },
                'sinnoh-champion-ribbon': { gen: 4, type: 'champion', description: 'Entered Hall of Fame (Sinnoh)' },
                'kalos-champion-ribbon': { gen: 6, type: 'champion', description: 'Entered Hall of Fame (Kalos)' },
                'alola-champion-ribbon': { gen: 7, type: 'champion', description: 'Became Champion (Alola)' },
                'galar-champion-ribbon': { gen: 8, type: 'champion', description: 'Became Champion (Galar)' },
                'paldea-champion-ribbon': { gen: 9, type: 'champion', description: 'Became Champion (Paldea)' }
            },

            // Event/Special Ribbons
            event: {
                'event-ribbon': { gen: 3, type: 'event', description: 'Special Event Pokemon' },
                'classic-ribbon': { gen: 4, type: 'event', description: 'Event Pokemon from older games' },
                'premier-ribbon': { gen: 4, type: 'event', description: 'Special Premier Event' },
                'wishing-ribbon': { gen: 6, type: 'event', description: 'Received from Pokemon Center' },
                'battle-champion-ribbon': { gen: 6, type: 'event', description: 'VGC World Champion' },
                'regional-champion-ribbon': { gen: 6, type: 'event', description: 'VGC Regional Champion' },
                'national-champion-ribbon': { gen: 6, type: 'event', description: 'VGC National Champion' },
                'world-champion-ribbon': { gen: 6, type: 'event', description: 'VGC World Champion' },
                'birthday-ribbon': { gen: 6, type: 'event', description: 'Pokemon Center Birthday Event' },
                'souvenir-ribbon': { gen: 8, type: 'event', description: 'In-game event souvenir' }
            },

            // Memory/Effort Ribbons
            memory: {
                'effort-ribbon': { gen: 3, type: 'memory', description: 'Maxed EVs (510 total)' },
                'alert-ribbon': { gen: 3, type: 'memory', description: 'High Speed IV' },
                'shock-ribbon': { gen: 3, type: 'memory', description: 'High Sp.Atk IV' },
                'downcast-ribbon': { gen: 3, type: 'memory', description: 'High Sp.Def IV' },
                'careless-ribbon': { gen: 3, type: 'memory', description: 'High Attack IV' },
                'relax-ribbon': { gen: 3, type: 'memory', description: 'High Defense IV' },
                'snooze-ribbon': { gen: 3, type: 'memory', description: 'High HP IV' },
                'smile-ribbon': { gen: 3, type: 'memory', description: 'From Ribbon Syndicate' },
                'gorgeous-ribbon': { gen: 4, type: 'memory', description: 'Purchased (10,000)' },
                'royal-ribbon': { gen: 4, type: 'memory', description: 'Purchased (100,000)' },
                'gorgeous-royal-ribbon': { gen: 4, type: 'memory', description: 'Purchased (999,999)' },
                'footprint-ribbon': { gen: 4, type: 'memory', description: 'Max Friendship' },
                'best-friends-ribbon': { gen: 6, type: 'memory', description: 'Max Affection' },
                'training-ribbon': { gen: 6, type: 'memory', description: 'Completed Super Training' },
                'skillful-battler-ribbon': { gen: 6, type: 'memory', description: 'Won Battle Maison' },
                'expert-battler-ribbon': { gen: 6, type: 'memory', description: 'Won Battle Maison (50 streak)' }
            },

            // Gen 8+ Marks (function as ribbons)
            marks: {
                'lunchtime-mark': { gen: 8, type: 'mark', description: 'Caught at noon', title: 'the Peckish' },
                'sleepy-time-mark': { gen: 8, type: 'mark', description: 'Caught at night', title: 'the Sleepy' },
                'dusk-mark': { gen: 8, type: 'mark', description: 'Caught at dusk', title: 'the Pokemon' },
                'dawn-mark': { gen: 8, type: 'mark', description: 'Caught at dawn', title: 'the Early Riser' },
                'cloudy-mark': { gen: 8, type: 'mark', description: 'Caught in clouds', title: 'the Cloud Watcher' },
                'rainy-mark': { gen: 8, type: 'mark', description: 'Caught in rain', title: 'the Sodden' },
                'stormy-mark': { gen: 8, type: 'mark', description: 'Caught in thunderstorm', title: 'the Pokemon' },
                'snowy-mark': { gen: 8, type: 'mark', description: 'Caught in snow', title: 'the Snow Frolicker' },
                'blizzard-mark': { gen: 8, type: 'mark', description: 'Caught in blizzard', title: 'the Pokemon' },
                'dry-mark': { gen: 8, type: 'mark', description: 'Caught in harsh sun', title: 'the Pokemon' },
                'sandstorm-mark': { gen: 8, type: 'mark', description: 'Caught in sandstorm', title: 'the Pokemon' },
                'misty-mark': { gen: 8, type: 'mark', description: 'Caught in fog', title: 'the Pokemon' },
                'rare-mark': { gen: 8, type: 'mark', description: 'Rare encounter', title: 'the Recluse' },
                'uncommon-mark': { gen: 8, type: 'mark', description: 'Uncommon personality', title: 'the Pokemon' },
                'rowdy-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Rowdy' },
                'absent-minded-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Spacey' },
                'jittery-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Pokemon' },
                'excited-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Pokemon' },
                'charismatic-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Pokemon' },
                'calmness-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Pokemon' },
                'intense-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Pokemon' },
                'zoned-out-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Pokemon' },
                'joyful-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Pokemon' },
                'angry-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Pokemon' },
                'smiley-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Pokemon' },
                'teary-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Pokemon' },
                'upbeat-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Pokemon' },
                'peeved-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Pokemon' },
                'intellectual-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Pokemon' },
                'ferocious-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Pokemon' },
                'crafty-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Pokemon' },
                'scowling-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Pokemon' },
                'kindly-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Pokemon' },
                'flustered-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Pokemon' },
                'pumped-up-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Pokemon' },
                'zero-energy-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Pokemon' },
                'prideful-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Pokemon' },
                'unsure-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Pokemon' },
                'humble-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Pokemon' },
                'thorny-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Pokemon' },
                'vigor-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Pokemon' },
                'slump-mark': { gen: 8, type: 'mark', description: 'Personality mark', title: 'the Pokemon' },
                'mightiest-mark': { gen: 8, type: 'mark', description: 'From 7-star Tera Raid', title: 'the Unrivaled' },
                'titan-mark': { gen: 9, type: 'mark', description: 'Titan Pokemon defeated', title: 'the Former Titan' },
                'partner-mark': { gen: 9, type: 'mark', description: 'From special event', title: 'the Pokemon' },
                'gourmand-mark': { gen: 9, type: 'mark', description: 'High picnic friendship', title: 'the Pokemon' }
            }
        };
    }

    getAllRibbons() {
        const all = [];
        for (const [category, ribbons] of Object.entries(this.ribbons)) {
            for (const [id, data] of Object.entries(ribbons)) {
                all.push({ id, category, ...data });
            }
        }
        return all;
    }

    getRibbonsByGen(gen) {
        return this.getAllRibbons().filter(r => r.gen === gen);
    }

    getRibbonsByType(type) {
        return this.getAllRibbons().filter(r => r.type === type);
    }

    addRibbon(pokemon, ribbonId) {
        if (!pokemon.ribbons) pokemon.ribbons = [];
        const ribbon = this.getAllRibbons().find(r => r.id === ribbonId);
        if (!ribbon) return { error: `Unknown ribbon: ${ribbonId}` };
        if (pokemon.ribbons.includes(ribbonId)) return { error: 'Pokemon already has this ribbon' };
        pokemon.ribbons.push(ribbonId);
        return { success: true, ribbon };
    }

    removeRibbon(pokemon, ribbonId) {
        if (!pokemon.ribbons) return { error: 'Pokemon has no ribbons' };
        const index = pokemon.ribbons.indexOf(ribbonId);
        if (index === -1) return { error: 'Pokemon does not have this ribbon' };
        pokemon.ribbons.splice(index, 1);
        return { success: true };
    }

    getRibbonCount(pokemon) {
        return (pokemon.ribbons || []).length;
    }

    getRibbonDetails(pokemon) {
        const ribbons = pokemon.ribbons || [];
        return ribbons.map(id => {
            const ribbon = this.getAllRibbons().find(r => r.id === id);
            return ribbon || { id, description: 'Unknown ribbon' };
        });
    }

    canHaveRibbon(pokemon, ribbonId) {
        const ribbon = this.getAllRibbons().find(r => r.id === ribbonId);
        if (!ribbon) return { canHave: false, reason: 'Unknown ribbon' };

        // Check event ribbons
        if (ribbon.type === 'event' && !pokemon.isEventPokemon) {
            return { canHave: false, reason: 'Only event Pokemon can have this ribbon' };
        }

        // Check champion ribbons
        if (ribbon.type === 'champion') {
            return { canHave: true, reason: 'Any Pokemon that entered Hall of Fame can have this' };
        }

        return { canHave: true };
    }

    setAllContestRibbons(pokemon) {
        const contestRibbons = this.getRibbonsByType('contest');
        for (const ribbon of contestRibbons) {
            this.addRibbon(pokemon, ribbon.id);
        }
        return { success: true, added: contestRibbons.length };
    }

    setAllChampionRibbons(pokemon) {
        const championRibbons = this.getRibbonsByType('champion');
        for (const ribbon of championRibbons) {
            this.addRibbon(pokemon, ribbon.id);
        }
        return { success: true, added: championRibbons.length };
    }

    getActiveTitle(pokemon) {
        if (!pokemon.activeRibbon) return null;
        const ribbon = this.getAllRibbons().find(r => r.id === pokemon.activeRibbon);
        if (ribbon?.title) {
            return `${pokemon.nickname || pokemon.species} ${ribbon.title}`;
        }
        return null;
    }

    setActiveTitle(pokemon, ribbonId) {
        if (!pokemon.ribbons?.includes(ribbonId)) {
            return { error: 'Pokemon does not have this ribbon' };
        }
        const ribbon = this.getAllRibbons().find(r => r.id === ribbonId);
        if (!ribbon?.title) {
            return { error: 'This ribbon does not have a title' };
        }
        pokemon.activeRibbon = ribbonId;
        return { success: true, title: this.getActiveTitle(pokemon) };
    }

    getSummary(pokemon) {
        const ribbons = this.getRibbonDetails(pokemon);
        const byType = {};

        for (const ribbon of ribbons) {
            if (!byType[ribbon.type]) byType[ribbon.type] = [];
            byType[ribbon.type].push(ribbon);
        }

        return {
            pokemon: pokemon.nickname || pokemon.species,
            totalRibbons: ribbons.length,
            byType,
            activeTitle: this.getActiveTitle(pokemon)
        };
    }
}

module.exports = PokemonRibbonEditor;
