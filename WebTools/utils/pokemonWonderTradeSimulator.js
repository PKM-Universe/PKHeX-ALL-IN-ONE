/**
 * Pokemon Wonder Trade Simulator
 * Random trade simulation with rarity tiers, IV generation, and special encounters
 */

const fs = require('fs');
const path = require('path');

class PokemonWonderTradeSimulator {
    constructor() {
        this.rarityTiers = {
            common: { weight: 50, label: 'Common', color: '#808080' },
            uncommon: { weight: 25, label: 'Uncommon', color: '#1eff00' },
            rare: { weight: 15, label: 'Rare', color: '#0070dd' },
            epic: { weight: 7, label: 'Epic', color: '#a335ee' },
            legendary: { weight: 2.5, label: 'Legendary', color: '#ff8000' },
            mythical: { weight: 0.5, label: 'Mythical', color: '#e6cc80' }
        };

        this.pokemonPools = {
            common: ['pidgey', 'rattata', 'caterpie', 'weedle', 'zubat', 'geodude', 'magikarp', 'tentacool', 'psyduck', 'poliwag', 'abra', 'machop', 'gastly', 'krabby', 'voltorb', 'goldeen', 'staryu', 'hoothoot', 'ledyba', 'spinarak', 'mareep', 'wooper', 'zigzagoon', 'wurmple', 'taillow', 'wingull', 'ralts', 'whismur', 'aron', 'bidoof', 'starly', 'shinx', 'patrat', 'lillipup', 'pidove', 'roggenrola', 'fletchling', 'bunnelby', 'rookidee', 'wooloo', 'lechonk'],
            uncommon: ['pikachu', 'vulpix', 'growlithe', 'ponyta', 'slowpoke', 'eevee', 'snorlax', 'cyndaquil', 'totodile', 'chikorita', 'togepi', 'larvitar', 'treecko', 'torchic', 'mudkip', 'bagon', 'beldum', 'turtwig', 'chimchar', 'piplup', 'gible', 'riolu', 'snivy', 'tepig', 'oshawott', 'axew', 'deino', 'froakie', 'honedge', 'goomy', 'rowlet', 'litten', 'popplio', 'dreepy', 'sprigatito', 'fuecoco', 'quaxly'],
            rare: ['bulbasaur', 'charmander', 'squirtle', 'dratini', 'lapras', 'porygon', 'aerodactyl', 'heracross', 'skarmory', 'feebas', 'absol', 'spiritomb', 'rotom', 'zorua', 'larvesta', 'tyrunt', 'amaura', 'mimikyu', 'duraludon', 'frigibax', 'charcadet'],
            epic: ['dragonite', 'tyranitar', 'salamence', 'metagross', 'garchomp', 'lucario', 'hydreigon', 'volcarona', 'goodra', 'dragapult', 'ditto', 'kingambit', 'baxcalibur', 'roaring-moon', 'iron-valiant'],
            legendary: ['articuno', 'zapdos', 'moltres', 'raikou', 'entei', 'suicune', 'lugia', 'ho-oh', 'regirock', 'regice', 'registeel', 'latias', 'latios', 'kyogre', 'groudon', 'rayquaza', 'dialga', 'palkia', 'giratina', 'reshiram', 'zekrom', 'kyurem', 'xerneas', 'yveltal', 'solgaleo', 'lunala', 'zacian', 'zamazenta', 'koraidon', 'miraidon'],
            mythical: ['mew', 'celebi', 'jirachi', 'deoxys', 'manaphy', 'darkrai', 'shaymin', 'arceus', 'victini', 'keldeo', 'meloetta', 'genesect', 'diancie', 'hoopa', 'volcanion', 'magearna', 'marshadow', 'zeraora', 'zarude']
        };

        this.natures = ['hardy', 'lonely', 'brave', 'adamant', 'naughty', 'bold', 'docile', 'relaxed', 'impish', 'lax', 'timid', 'hasty', 'serious', 'jolly', 'naive', 'modest', 'mild', 'quiet', 'bashful', 'rash', 'calm', 'gentle', 'sassy', 'careful', 'quirky'];
        this.balls = { common: ['poke-ball', 'great-ball'], uncommon: ['ultra-ball', 'nest-ball', 'net-ball'], rare: ['dusk-ball', 'timer-ball', 'luxury-ball'], epic: ['dream-ball', 'beast-ball'], legendary: ['master-ball', 'cherish-ball'], mythical: ['cherish-ball'] };
        this.historyPath = path.join(__dirname, '..', 'Json', 'wonder_trade_history.json');
    }

    rollRarity() {
        const roll = Math.random() * 100;
        let cumulative = 0;
        for (const [tier, data] of Object.entries(this.rarityTiers)) {
            cumulative += data.weight;
            if (roll < cumulative) return tier;
        }
        return 'common';
    }

    generateIVs(rarity) {
        const ivs = { hp: 0, atk: 0, def: 0, spa: 0, spd: 0, spe: 0 };
        const stats = Object.keys(ivs);
        for (const stat of stats) ivs[stat] = Math.floor(Math.random() * 32);
        const perfectCount = { mythical: 6, legendary: 5, epic: 4, rare: 3, uncommon: 2, common: Math.random() < 0.3 ? 1 : 0 }[rarity];
        const shuffled = stats.sort(() => Math.random() - 0.5);
        for (let i = 0; i < perfectCount; i++) ivs[shuffled[i]] = 31;
        return ivs;
    }

    generatePokemon(options = {}) {
        const rarity = options.guaranteedRarity || this.rollRarity();
        const pool = this.pokemonPools[rarity];
        const species = pool[Math.floor(Math.random() * pool.length)];
        const shiny = options.guaranteedShiny || Math.random() < ({ mythical: 0.01, legendary: 0.005, epic: 0.002, rare: 0.001, uncommon: 0.0005, common: 0.000244 }[rarity]);
        const ivs = this.generateIVs(rarity);
        const level = { mythical: 50 + Math.floor(Math.random() * 30), legendary: 50 + Math.floor(Math.random() * 30), epic: 30 + Math.floor(Math.random() * 30), rare: 20 + Math.floor(Math.random() * 20), uncommon: 10 + Math.floor(Math.random() * 15), common: 1 + Math.floor(Math.random() * 10) }[rarity];

        return {
            species, level, shiny, rarity,
            rarityLabel: this.rarityTiers[rarity].label,
            rarityColor: this.rarityTiers[rarity].color,
            nature: this.natures[Math.floor(Math.random() * this.natures.length)],
            ivs, ivTotal: Object.values(ivs).reduce((a, b) => a + b, 0),
            ball: this.balls[rarity][Math.floor(Math.random() * this.balls[rarity].length)],
            ot: ['Ash', 'Misty', 'Brock', 'Red', 'Blue', 'Cynthia', 'Leon', 'WonderTrader', 'PKMaster'][Math.floor(Math.random() * 9)],
            tid: Math.floor(Math.random() * 999999),
            tradedAt: new Date().toISOString()
        };
    }

    trade(userPokemon = null) {
        const received = this.generatePokemon();
        const result = { sent: userPokemon, received, timestamp: new Date().toISOString() };
        this.saveToHistory(result);
        return result;
    }

    bulkTrade(count = 10) {
        const results = [];
        for (let i = 0; i < count; i++) results.push(this.trade());
        const summary = { total: count, byRarity: {}, shinies: 0, bestIVTotal: 0, bestPokemon: null };
        for (const r of results) {
            summary.byRarity[r.received.rarity] = (summary.byRarity[r.received.rarity] || 0) + 1;
            if (r.received.shiny) summary.shinies++;
            if (r.received.ivTotal > summary.bestIVTotal) { summary.bestIVTotal = r.received.ivTotal; summary.bestPokemon = r.received; }
        }
        return { trades: results, summary };
    }

    saveToHistory(trade) {
        try {
            let history = fs.existsSync(this.historyPath) ? JSON.parse(fs.readFileSync(this.historyPath, 'utf8')) : [];
            history.push({ received: trade.received.species, rarity: trade.received.rarity, shiny: trade.received.shiny, ivTotal: trade.received.ivTotal, timestamp: trade.timestamp });
            if (history.length > 1000) history = history.slice(-1000);
            fs.writeFileSync(this.historyPath, JSON.stringify(history, null, 2));
        } catch (e) { console.error('Failed to save history:', e); }
    }

    getStatistics() {
        try {
            if (!fs.existsSync(this.historyPath)) return { error: 'No history' };
            const history = JSON.parse(fs.readFileSync(this.historyPath, 'utf8'));
            const stats = { totalTrades: history.length, byRarity: {}, shinies: 0, averageIVs: 0 };
            let totalIVs = 0;
            for (const t of history) {
                stats.byRarity[t.rarity] = (stats.byRarity[t.rarity] || 0) + 1;
                if (t.shiny) stats.shinies++;
                totalIVs += t.ivTotal || 0;
            }
            stats.averageIVs = (totalIVs / history.length).toFixed(1);
            stats.shinyRate = `1/${Math.round(history.length / Math.max(stats.shinies, 1))}`;
            return stats;
        } catch (e) { return { error: e.message }; }
    }
}

module.exports = PokemonWonderTradeSimulator;
