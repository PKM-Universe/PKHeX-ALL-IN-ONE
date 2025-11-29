/**
 * Pokemon Living Dex Tracker
 * Track completion across all forms, genders, shinies, and regional variants
 */

const fs = require('fs');
const path = require('path');

class PokemonLivingDex {
    constructor(savePath = null) {
        this.savePath = savePath || path.join(__dirname, '..', 'Json', 'living_dex_progress.json');
        this.progress = this.loadProgress();

        // National Dex count by generation
        this.generationRanges = {
            1: { start: 1, end: 151, name: 'Kanto' },
            2: { start: 152, end: 251, name: 'Johto' },
            3: { start: 252, end: 386, name: 'Hoenn' },
            4: { start: 387, end: 493, name: 'Sinnoh' },
            5: { start: 494, end: 649, name: 'Unova' },
            6: { start: 650, end: 721, name: 'Kalos' },
            7: { start: 722, end: 809, name: 'Alola' },
            8: { start: 810, end: 905, name: 'Galar/Hisui' },
            9: { start: 906, end: 1025, name: 'Paldea' }
        };

        // Pokemon with gender differences
        this.genderDifferences = [
            'venusaur', 'butterfree', 'rattata', 'raticate', 'pikachu', 'raichu',
            'nidoran', 'zubat', 'golbat', 'gloom', 'vileplume', 'kadabra',
            'alakazam', 'doduo', 'dodrio', 'hypno', 'rhyhorn', 'rhydon',
            'goldeen', 'seaking', 'scyther', 'magikarp', 'gyarados', 'eevee',
            'meganium', 'ledyba', 'ledian', 'xatu', 'sudowoodo', 'politoed',
            'aipom', 'wooper', 'quagsire', 'murkrow', 'wobbuffet', 'girafarig',
            'gligar', 'steelix', 'scizor', 'heracross', 'sneasel', 'ursaring',
            'piloswine', 'octillery', 'houndoom', 'donphan', 'torchic', 'combusken',
            'blaziken', 'beautifly', 'dustox', 'ludicolo', 'nuzleaf', 'shiftry',
            'meditite', 'medicham', 'roselia', 'gulpin', 'swalot', 'numel',
            'camerupt', 'cacturne', 'milotic', 'relicanth', 'starly', 'staravia',
            'staraptor', 'bidoof', 'bibarel', 'kricketot', 'kricketune', 'shinx',
            'luxio', 'luxray', 'roserade', 'combee', 'pachirisu', 'buizel',
            'floatzel', 'ambipom', 'gible', 'gabite', 'garchomp', 'hippopotas',
            'hippowdon', 'croagunk', 'toxicroak', 'finneon', 'lumineon', 'snover',
            'abomasnow', 'weavile', 'rhyperior', 'tangrowth', 'mamoswine',
            'unfezant', 'frillish', 'jellicent', 'pyroar', 'meowstic', 'indeedee'
        ];

        // Pokemon with regional forms
        this.regionalForms = {
            'rattata': ['alolan'],
            'raticate': ['alolan'],
            'raichu': ['alolan'],
            'sandshrew': ['alolan'],
            'sandslash': ['alolan'],
            'vulpix': ['alolan'],
            'ninetales': ['alolan'],
            'diglett': ['alolan'],
            'dugtrio': ['alolan'],
            'meowth': ['alolan', 'galarian'],
            'persian': ['alolan'],
            'geodude': ['alolan'],
            'graveler': ['alolan'],
            'golem': ['alolan'],
            'grimer': ['alolan'],
            'muk': ['alolan'],
            'exeggutor': ['alolan'],
            'marowak': ['alolan'],
            'ponyta': ['galarian'],
            'rapidash': ['galarian'],
            'slowpoke': ['galarian'],
            'slowbro': ['galarian'],
            'slowking': ['galarian'],
            'farfetchd': ['galarian'],
            'weezing': ['galarian'],
            'mr-mime': ['galarian'],
            'articuno': ['galarian'],
            'zapdos': ['galarian'],
            'moltres': ['galarian'],
            'corsola': ['galarian'],
            'zigzagoon': ['galarian'],
            'linoone': ['galarian'],
            'darumaka': ['galarian'],
            'darmanitan': ['galarian'],
            'yamask': ['galarian'],
            'stunfisk': ['galarian'],
            'growlithe': ['hisuian'],
            'arcanine': ['hisuian'],
            'voltorb': ['hisuian'],
            'electrode': ['hisuian'],
            'typhlosion': ['hisuian'],
            'qwilfish': ['hisuian'],
            'sneasel': ['hisuian'],
            'samurott': ['hisuian'],
            'lilligant': ['hisuian'],
            'zorua': ['hisuian'],
            'zoroark': ['hisuian'],
            'braviary': ['hisuian'],
            'sliggoo': ['hisuian'],
            'goodra': ['hisuian'],
            'avalugg': ['hisuian'],
            'decidueye': ['hisuian'],
            'wooper': ['paldean'],
            'tauros': ['paldean-combat', 'paldean-blaze', 'paldean-aqua']
        };

        // Pokemon with multiple forms
        this.formVariants = {
            'unown': ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
                      'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '!', '?'],
            'castform': ['normal', 'sunny', 'rainy', 'snowy'],
            'deoxys': ['normal', 'attack', 'defense', 'speed'],
            'burmy': ['plant', 'sandy', 'trash'],
            'wormadam': ['plant', 'sandy', 'trash'],
            'shellos': ['west', 'east'],
            'gastrodon': ['west', 'east'],
            'rotom': ['normal', 'heat', 'wash', 'frost', 'fan', 'mow'],
            'giratina': ['altered', 'origin'],
            'shaymin': ['land', 'sky'],
            'arceus': ['normal', 'fire', 'water', 'electric', 'grass', 'ice', 'fighting',
                       'poison', 'ground', 'flying', 'psychic', 'bug', 'rock', 'ghost',
                       'dragon', 'dark', 'steel', 'fairy'],
            'basculin': ['red-striped', 'blue-striped', 'white-striped'],
            'deerling': ['spring', 'summer', 'autumn', 'winter'],
            'sawsbuck': ['spring', 'summer', 'autumn', 'winter'],
            'tornadus': ['incarnate', 'therian'],
            'thundurus': ['incarnate', 'therian'],
            'landorus': ['incarnate', 'therian'],
            'kyurem': ['normal', 'white', 'black'],
            'keldeo': ['ordinary', 'resolute'],
            'meloetta': ['aria', 'pirouette'],
            'genesect': ['normal', 'douse', 'shock', 'burn', 'chill'],
            'vivillon': ['meadow', 'polar', 'tundra', 'continental', 'garden', 'elegant',
                         'icy-snow', 'modern', 'marine', 'archipelago', 'high-plains',
                         'sandstorm', 'river', 'monsoon', 'savanna', 'sun', 'ocean',
                         'jungle', 'fancy', 'pokeball'],
            'flabebe': ['red', 'yellow', 'orange', 'blue', 'white'],
            'floette': ['red', 'yellow', 'orange', 'blue', 'white', 'eternal'],
            'florges': ['red', 'yellow', 'orange', 'blue', 'white'],
            'furfrou': ['natural', 'heart', 'star', 'diamond', 'debutante',
                        'matron', 'dandy', 'la-reine', 'kabuki', 'pharaoh'],
            'pumpkaboo': ['small', 'average', 'large', 'super'],
            'gourgeist': ['small', 'average', 'large', 'super'],
            'hoopa': ['confined', 'unbound'],
            'oricorio': ['baile', 'pom-pom', 'pau', 'sensu'],
            'lycanroc': ['midday', 'midnight', 'dusk'],
            'wishiwashi': ['solo', 'school'],
            'silvally': ['normal', 'fire', 'water', 'electric', 'grass', 'ice', 'fighting',
                         'poison', 'ground', 'flying', 'psychic', 'bug', 'rock', 'ghost',
                         'dragon', 'dark', 'steel', 'fairy'],
            'minior': ['red', 'orange', 'yellow', 'green', 'blue', 'indigo', 'violet'],
            'necrozma': ['normal', 'dusk-mane', 'dawn-wings', 'ultra'],
            'magearna': ['normal', 'original'],
            'alcremie': ['vanilla-cream', 'ruby-cream', 'matcha-cream', 'mint-cream',
                         'lemon-cream', 'salted-cream', 'ruby-swirl', 'caramel-swirl', 'rainbow-swirl'],
            'toxtricity': ['amped', 'low-key'],
            'sinistea': ['phony', 'antique'],
            'polteageist': ['phony', 'antique'],
            'urshifu': ['single-strike', 'rapid-strike'],
            'calyrex': ['normal', 'ice-rider', 'shadow-rider'],
            'enamorus': ['incarnate', 'therian'],
            'squawkabilly': ['green', 'blue', 'yellow', 'white'],
            'palafin': ['zero', 'hero'],
            'tatsugiri': ['curly', 'droopy', 'stretchy'],
            'dudunsparce': ['two-segment', 'three-segment'],
            'gimmighoul': ['chest', 'roaming'],
            'ogerpon': ['teal-mask', 'wellspring-mask', 'hearthflame-mask', 'cornerstone-mask']
        };
    }

    /**
     * Load progress from file
     */
    loadProgress() {
        try {
            if (fs.existsSync(this.savePath)) {
                return JSON.parse(fs.readFileSync(this.savePath, 'utf8'));
            }
        } catch (error) {
            console.error('Failed to load Living Dex progress:', error);
        }

        return {
            users: {},
            global: {
                totalPokemon: 1025,
                lastUpdated: new Date().toISOString()
            }
        };
    }

    /**
     * Save progress to file
     */
    saveProgress() {
        try {
            this.progress.global.lastUpdated = new Date().toISOString();
            fs.writeFileSync(this.savePath, JSON.stringify(this.progress, null, 2));
            return true;
        } catch (error) {
            console.error('Failed to save Living Dex progress:', error);
            return false;
        }
    }

    /**
     * Get or create user progress
     */
    getUserProgress(userId) {
        if (!this.progress.users[userId]) {
            this.progress.users[userId] = {
                owned: {},           // { dexNumber: { normal: true, shiny: false, male: true, female: false, forms: {} } }
                startedAt: new Date().toISOString(),
                lastUpdated: new Date().toISOString(),
                settings: {
                    trackShiny: true,
                    trackGender: true,
                    trackForms: true,
                    trackRegional: true
                }
            };
        }
        return this.progress.users[userId];
    }

    /**
     * Mark Pokemon as owned
     */
    markOwned(userId, dexNumber, options = {}) {
        const {
            shiny = false,
            gender = null,      // 'male', 'female', or null
            form = 'normal',
            regional = null     // 'alolan', 'galarian', 'hisuian', 'paldean'
        } = options;

        const userProgress = this.getUserProgress(userId);

        if (!userProgress.owned[dexNumber]) {
            userProgress.owned[dexNumber] = {
                normal: false,
                shiny: false,
                male: null,
                female: null,
                forms: {},
                regional: {}
            };
        }

        const entry = userProgress.owned[dexNumber];

        if (regional) {
            if (!entry.regional[regional]) {
                entry.regional[regional] = { normal: false, shiny: false };
            }
            entry.regional[regional][shiny ? 'shiny' : 'normal'] = true;
        } else if (form !== 'normal') {
            if (!entry.forms[form]) {
                entry.forms[form] = { normal: false, shiny: false };
            }
            entry.forms[form][shiny ? 'shiny' : 'normal'] = true;
        } else {
            entry[shiny ? 'shiny' : 'normal'] = true;
        }

        if (gender) {
            entry[gender] = true;
        }

        userProgress.lastUpdated = new Date().toISOString();
        this.saveProgress();

        return { success: true, entry: userProgress.owned[dexNumber] };
    }

    /**
     * Mark Pokemon as not owned
     */
    markNotOwned(userId, dexNumber, options = {}) {
        const userProgress = this.getUserProgress(userId);

        if (!userProgress.owned[dexNumber]) {
            return { success: true, message: 'Already not owned' };
        }

        const { shiny = false, form = 'normal', regional = null } = options;
        const entry = userProgress.owned[dexNumber];

        if (regional) {
            if (entry.regional[regional]) {
                entry.regional[regional][shiny ? 'shiny' : 'normal'] = false;
            }
        } else if (form !== 'normal') {
            if (entry.forms[form]) {
                entry.forms[form][shiny ? 'shiny' : 'normal'] = false;
            }
        } else {
            entry[shiny ? 'shiny' : 'normal'] = false;
        }

        userProgress.lastUpdated = new Date().toISOString();
        this.saveProgress();

        return { success: true };
    }

    /**
     * Get completion statistics
     */
    getCompletionStats(userId) {
        const userProgress = this.getUserProgress(userId);
        const settings = userProgress.settings;

        const stats = {
            normal: { owned: 0, total: 1025, percentage: 0 },
            shiny: { owned: 0, total: 1025, percentage: 0 },
            byGeneration: {},
            forms: { owned: 0, total: 0, percentage: 0 },
            regional: { owned: 0, total: 0, percentage: 0 },
            gender: { complete: 0, total: 0, percentage: 0 },
            overall: { owned: 0, total: 0, percentage: 0 }
        };

        // Count normal Pokemon
        for (let i = 1; i <= 1025; i++) {
            const entry = userProgress.owned[i];
            if (entry?.normal) stats.normal.owned++;
            if (entry?.shiny && settings.trackShiny) stats.shiny.owned++;
        }

        stats.normal.percentage = ((stats.normal.owned / stats.normal.total) * 100).toFixed(2);
        stats.shiny.percentage = ((stats.shiny.owned / stats.shiny.total) * 100).toFixed(2);

        // By generation
        for (const [gen, range] of Object.entries(this.generationRanges)) {
            const genStats = { owned: 0, total: range.end - range.start + 1, name: range.name };

            for (let i = range.start; i <= range.end; i++) {
                if (userProgress.owned[i]?.normal) genStats.owned++;
            }

            genStats.percentage = ((genStats.owned / genStats.total) * 100).toFixed(2);
            stats.byGeneration[gen] = genStats;
        }

        // Forms
        if (settings.trackForms) {
            for (const [pokemon, forms] of Object.entries(this.formVariants)) {
                stats.forms.total += forms.length;
            }

            for (const entry of Object.values(userProgress.owned)) {
                if (entry?.forms) {
                    stats.forms.owned += Object.values(entry.forms).filter(f => f.normal).length;
                }
            }

            stats.forms.percentage = stats.forms.total > 0
                ? ((stats.forms.owned / stats.forms.total) * 100).toFixed(2)
                : 0;
        }

        // Regional forms
        if (settings.trackRegional) {
            for (const variants of Object.values(this.regionalForms)) {
                stats.regional.total += variants.length;
            }

            for (const entry of Object.values(userProgress.owned)) {
                if (entry?.regional) {
                    stats.regional.owned += Object.values(entry.regional).filter(r => r.normal).length;
                }
            }

            stats.regional.percentage = stats.regional.total > 0
                ? ((stats.regional.owned / stats.regional.total) * 100).toFixed(2)
                : 0;
        }

        // Gender differences
        if (settings.trackGender) {
            stats.gender.total = this.genderDifferences.length * 2; // Male + Female

            for (const entry of Object.values(userProgress.owned)) {
                if (entry?.male) stats.gender.complete++;
                if (entry?.female) stats.gender.complete++;
            }

            stats.gender.percentage = ((stats.gender.complete / stats.gender.total) * 100).toFixed(2);
        }

        // Overall (based on settings)
        stats.overall.total = stats.normal.total;
        stats.overall.owned = stats.normal.owned;

        if (settings.trackShiny) {
            stats.overall.total += stats.shiny.total;
            stats.overall.owned += stats.shiny.owned;
        }
        if (settings.trackForms) {
            stats.overall.total += stats.forms.total;
            stats.overall.owned += stats.forms.owned;
        }
        if (settings.trackRegional) {
            stats.overall.total += stats.regional.total;
            stats.overall.owned += stats.regional.owned;
        }

        stats.overall.percentage = ((stats.overall.owned / stats.overall.total) * 100).toFixed(2);

        return stats;
    }

    /**
     * Get missing Pokemon
     */
    getMissing(userId, options = {}) {
        const { generation = null, shiny = false, limit = 50 } = options;
        const userProgress = this.getUserProgress(userId);
        const missing = [];

        let startDex = 1;
        let endDex = 1025;

        if (generation && this.generationRanges[generation]) {
            startDex = this.generationRanges[generation].start;
            endDex = this.generationRanges[generation].end;
        }

        for (let i = startDex; i <= endDex && missing.length < limit; i++) {
            const entry = userProgress.owned[i];
            const key = shiny ? 'shiny' : 'normal';

            if (!entry || !entry[key]) {
                missing.push({
                    dexNumber: i,
                    type: shiny ? 'shiny' : 'normal'
                });
            }
        }

        return missing;
    }

    /**
     * Get missing forms
     */
    getMissingForms(userId, pokemonName) {
        const userProgress = this.getUserProgress(userId);
        const forms = this.formVariants[pokemonName.toLowerCase()];

        if (!forms) {
            return { error: 'No form variants for this Pokemon' };
        }

        const missing = [];
        const dexNumber = this.getDexNumber(pokemonName);
        const entry = userProgress.owned[dexNumber];

        for (const form of forms) {
            if (!entry?.forms?.[form]?.normal) {
                missing.push({ form, shiny: false });
            }
            if (userProgress.settings.trackShiny && !entry?.forms?.[form]?.shiny) {
                missing.push({ form, shiny: true });
            }
        }

        return { pokemon: pokemonName, missing };
    }

    /**
     * Get missing regional forms
     */
    getMissingRegional(userId) {
        const userProgress = this.getUserProgress(userId);
        const missing = [];

        for (const [pokemon, variants] of Object.entries(this.regionalForms)) {
            const dexNumber = this.getDexNumber(pokemon);
            const entry = userProgress.owned[dexNumber];

            for (const variant of variants) {
                if (!entry?.regional?.[variant]?.normal) {
                    missing.push({ pokemon, variant, shiny: false });
                }
                if (userProgress.settings.trackShiny && !entry?.regional?.[variant]?.shiny) {
                    missing.push({ pokemon, variant, shiny: true });
                }
            }
        }

        return missing;
    }

    /**
     * Get dex number (simplified lookup)
     */
    getDexNumber(pokemonName) {
        const dexLookup = {
            'bulbasaur': 1, 'ivysaur': 2, 'venusaur': 3, 'charmander': 4, 'charmeleon': 5,
            'charizard': 6, 'squirtle': 7, 'wartortle': 8, 'blastoise': 9, 'caterpie': 10,
            'pikachu': 25, 'raichu': 26, 'sandshrew': 27, 'sandslash': 28, 'vulpix': 37,
            'ninetales': 38, 'meowth': 52, 'persian': 53, 'geodude': 74, 'graveler': 75,
            'golem': 76, 'ponyta': 77, 'rapidash': 78, 'slowpoke': 79, 'slowbro': 80,
            'farfetchd': 83, 'grimer': 87, 'muk': 88, 'exeggutor': 103, 'marowak': 105,
            'mr-mime': 122, 'articuno': 144, 'zapdos': 145, 'moltres': 146, 'rattata': 19,
            'raticate': 20, 'diglett': 50, 'dugtrio': 51, 'weezing': 110, 'corsola': 222,
            'zigzagoon': 263, 'linoone': 264, 'darumaka': 554, 'darmanitan': 555,
            'yamask': 562, 'stunfisk': 618, 'slowking': 199, 'growlithe': 58, 'arcanine': 59,
            'voltorb': 100, 'electrode': 101, 'typhlosion': 157, 'qwilfish': 211,
            'sneasel': 215, 'samurott': 503, 'lilligant': 549, 'zorua': 570, 'zoroark': 571,
            'braviary': 628, 'sliggoo': 705, 'goodra': 706, 'avalugg': 713, 'decidueye': 724,
            'wooper': 194, 'tauros': 128, 'unown': 201, 'castform': 351, 'deoxys': 386,
            'burmy': 412, 'wormadam': 413, 'shellos': 422, 'gastrodon': 423, 'rotom': 479,
            'giratina': 487, 'shaymin': 492, 'arceus': 493, 'basculin': 550, 'deerling': 585,
            'sawsbuck': 586, 'tornadus': 641, 'thundurus': 642, 'landorus': 645, 'kyurem': 646,
            'keldeo': 647, 'meloetta': 648, 'genesect': 649, 'vivillon': 666, 'flabebe': 669,
            'floette': 670, 'florges': 671, 'furfrou': 676, 'pumpkaboo': 710, 'gourgeist': 711,
            'hoopa': 720, 'oricorio': 741, 'lycanroc': 745, 'wishiwashi': 746, 'silvally': 773,
            'minior': 774, 'necrozma': 800, 'magearna': 801, 'alcremie': 869, 'toxtricity': 849,
            'sinistea': 854, 'polteageist': 855, 'urshifu': 892, 'calyrex': 898, 'enamorus': 905,
            'squawkabilly': 931, 'palafin': 964, 'tatsugiri': 952, 'dudunsparce': 982,
            'gimmighoul': 999, 'ogerpon': 1017
        };
        return dexLookup[pokemonName.toLowerCase()] || null;
    }

    /**
     * Bulk import Pokemon
     */
    bulkImport(userId, pokemonList) {
        const results = { success: 0, failed: 0, errors: [] };

        for (const pokemon of pokemonList) {
            try {
                const dexNumber = pokemon.dexNumber || this.getDexNumber(pokemon.species);
                if (dexNumber) {
                    this.markOwned(userId, dexNumber, {
                        shiny: pokemon.shiny || false,
                        gender: pokemon.gender || null,
                        form: pokemon.form || 'normal',
                        regional: pokemon.regional || null
                    });
                    results.success++;
                } else {
                    results.failed++;
                    results.errors.push(`Unknown Pokemon: ${pokemon.species}`);
                }
            } catch (error) {
                results.failed++;
                results.errors.push(error.message);
            }
        }

        return results;
    }

    /**
     * Export progress
     */
    exportProgress(userId, format = 'json') {
        const userProgress = this.getUserProgress(userId);
        const stats = this.getCompletionStats(userId);

        if (format === 'json') {
            return JSON.stringify({
                progress: userProgress,
                stats: stats,
                exportedAt: new Date().toISOString()
            }, null, 2);
        }

        if (format === 'csv') {
            let csv = 'DexNumber,Normal,Shiny,Male,Female\n';
            for (let i = 1; i <= 1025; i++) {
                const entry = userProgress.owned[i] || {};
                csv += `${i},${entry.normal || false},${entry.shiny || false},${entry.male || ''},${entry.female || ''}\n`;
            }
            return csv;
        }

        return null;
    }

    /**
     * Generate visual progress bar
     */
    generateProgressBar(percentage, length = 20) {
        const filled = Math.round((percentage / 100) * length);
        const empty = length - filled;
        return `[${'█'.repeat(filled)}${'░'.repeat(empty)}] ${percentage}%`;
    }

    /**
     * Get detailed summary
     */
    getSummary(userId) {
        const stats = this.getCompletionStats(userId);
        const lines = [];

        lines.push('═══════════════════════════════════════');
        lines.push('         LIVING DEX PROGRESS           ');
        lines.push('═══════════════════════════════════════');
        lines.push('');

        lines.push(`Normal:   ${this.generateProgressBar(parseFloat(stats.normal.percentage))}`);
        lines.push(`          ${stats.normal.owned}/${stats.normal.total} Pokemon`);
        lines.push('');

        lines.push(`Shiny:    ${this.generateProgressBar(parseFloat(stats.shiny.percentage))}`);
        lines.push(`          ${stats.shiny.owned}/${stats.shiny.total} Pokemon`);
        lines.push('');

        lines.push('─── By Generation ───');
        for (const [gen, genStats] of Object.entries(stats.byGeneration)) {
            lines.push(`Gen ${gen} (${genStats.name}): ${genStats.owned}/${genStats.total} (${genStats.percentage}%)`);
        }
        lines.push('');

        lines.push(`Forms:    ${stats.forms.owned}/${stats.forms.total} (${stats.forms.percentage}%)`);
        lines.push(`Regional: ${stats.regional.owned}/${stats.regional.total} (${stats.regional.percentage}%)`);
        lines.push('');

        lines.push('═══════════════════════════════════════');
        lines.push(`OVERALL:  ${this.generateProgressBar(parseFloat(stats.overall.percentage))}`);
        lines.push(`          ${stats.overall.owned}/${stats.overall.total} Total`);
        lines.push('═══════════════════════════════════════');

        return lines.join('\n');
    }

    /**
     * Update user settings
     */
    updateSettings(userId, settings) {
        const userProgress = this.getUserProgress(userId);
        userProgress.settings = { ...userProgress.settings, ...settings };
        this.saveProgress();
        return userProgress.settings;
    }
}

module.exports = PokemonLivingDex;
