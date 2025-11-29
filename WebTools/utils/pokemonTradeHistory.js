/**
 * Pokemon Trade History Tracker
 * Track all generated and traded Pokemon
 */

const fs = require('fs');
const path = require('path');

class PokemonTradeHistory {
    constructor() {
        this.dataPath = path.join(__dirname, '..', 'Json');
        this.historyFile = path.join(this.dataPath, 'trade_history_full.json');
        this.statsFile = path.join(this.dataPath, 'trade_statistics.json');
        this.maxHistorySize = 10000; // Maximum records to keep
    }

    loadHistory() {
        try {
            if (fs.existsSync(this.historyFile)) {
                return JSON.parse(fs.readFileSync(this.historyFile, 'utf8'));
            }
        } catch (error) {
            console.error('Failed to load trade history:', error);
        }
        return { trades: [], created: new Date().toISOString() };
    }

    saveHistory(history) {
        try {
            // Trim to max size
            if (history.trades.length > this.maxHistorySize) {
                history.trades = history.trades.slice(-this.maxHistorySize);
            }
            fs.writeFileSync(this.historyFile, JSON.stringify(history, null, 2));
            return true;
        } catch (error) {
            console.error('Failed to save trade history:', error);
            return false;
        }
    }

    loadStatistics() {
        try {
            if (fs.existsSync(this.statsFile)) {
                return JSON.parse(fs.readFileSync(this.statsFile, 'utf8'));
            }
        } catch (error) {
            console.error('Failed to load statistics:', error);
        }
        return this.createEmptyStats();
    }

    saveStatistics(stats) {
        try {
            fs.writeFileSync(this.statsFile, JSON.stringify(stats, null, 2));
            return true;
        } catch (error) {
            console.error('Failed to save statistics:', error);
            return false;
        }
    }

    createEmptyStats() {
        return {
            totalTrades: 0,
            successfulTrades: 0,
            failedTrades: 0,
            totalPokemonTraded: 0,
            uniqueSpecies: [],
            uniqueUsers: [],
            byGame: {},
            byMonth: {},
            topSpecies: {},
            shinyCount: 0,
            legendaryCount: 0,
            eventCount: 0,
            lastUpdated: new Date().toISOString()
        };
    }

    // Record a new trade
    recordTrade(tradeData) {
        const history = this.loadHistory();
        const stats = this.loadStatistics();

        const record = {
            id: this.generateTradeId(),
            timestamp: new Date().toISOString(),
            userId: tradeData.userId,
            username: tradeData.username,
            pokemon: tradeData.pokemon,
            species: tradeData.pokemon?.species || tradeData.species,
            shiny: tradeData.pokemon?.shiny || tradeData.shiny || false,
            level: tradeData.pokemon?.level || tradeData.level || 100,
            game: tradeData.game || 'SV',
            type: tradeData.type || 'link', // link, clone, giveaway, etc.
            status: tradeData.status || 'completed',
            source: tradeData.source || 'pkhex', // pkhex, event, import
            botId: tradeData.botId,
            tradeCode: tradeData.tradeCode,
            duration: tradeData.duration,
            notes: tradeData.notes
        };

        // Add to history
        history.trades.push(record);

        // Update statistics
        this.updateStatistics(stats, record);

        // Save
        this.saveHistory(history);
        this.saveStatistics(stats);

        return {
            success: true,
            tradeId: record.id,
            record
        };
    }

    updateStatistics(stats, record) {
        stats.totalTrades++;

        if (record.status === 'completed') {
            stats.successfulTrades++;
            stats.totalPokemonTraded++;

            // Track unique species
            if (record.species && !stats.uniqueSpecies.includes(record.species.toLowerCase())) {
                stats.uniqueSpecies.push(record.species.toLowerCase());
            }

            // Track unique users
            if (record.userId && !stats.uniqueUsers.includes(record.userId)) {
                stats.uniqueUsers.push(record.userId);
            }

            // Track by game
            if (record.game) {
                stats.byGame[record.game] = (stats.byGame[record.game] || 0) + 1;
            }

            // Track by month
            const month = record.timestamp.substring(0, 7); // YYYY-MM
            stats.byMonth[month] = (stats.byMonth[month] || 0) + 1;

            // Track top species
            if (record.species) {
                const species = record.species.toLowerCase();
                stats.topSpecies[species] = (stats.topSpecies[species] || 0) + 1;
            }

            // Track special Pokemon
            if (record.shiny) stats.shinyCount++;

            const legendaries = ['mewtwo', 'lugia', 'ho-oh', 'rayquaza', 'dialga', 'palkia', 'giratina', 'zekrom', 'reshiram', 'kyurem', 'xerneas', 'yveltal', 'zygarde', 'solgaleo', 'lunala', 'necrozma', 'zacian', 'zamazenta', 'eternatus', 'calyrex', 'koraidon', 'miraidon'];
            if (record.species && legendaries.includes(record.species.toLowerCase())) {
                stats.legendaryCount++;
            }

            if (record.source === 'event') stats.eventCount++;
        } else {
            stats.failedTrades++;
        }

        stats.lastUpdated = new Date().toISOString();
    }

    generateTradeId() {
        return `TH-${Date.now()}-${Math.random().toString(36).substring(2, 8).toUpperCase()}`;
    }

    // Get user's trade history
    getUserHistory(userId, options = {}) {
        const history = this.loadHistory();
        const { limit = 50, offset = 0, status = null, species = null } = options;

        let userTrades = history.trades.filter(t => t.userId === userId);

        if (status) {
            userTrades = userTrades.filter(t => t.status === status);
        }

        if (species) {
            userTrades = userTrades.filter(t =>
                t.species?.toLowerCase().includes(species.toLowerCase())
            );
        }

        // Sort by most recent first
        userTrades.sort((a, b) => new Date(b.timestamp) - new Date(a.timestamp));

        const total = userTrades.length;
        const paginated = userTrades.slice(offset, offset + limit);

        return {
            userId,
            total,
            limit,
            offset,
            trades: paginated.map(t => ({
                id: t.id,
                species: t.species,
                shiny: t.shiny,
                level: t.level,
                game: t.game,
                type: t.type,
                status: t.status,
                timestamp: t.timestamp
            }))
        };
    }

    // Get global statistics
    getGlobalStatistics() {
        const stats = this.loadStatistics();

        // Get top 10 species
        const sortedSpecies = Object.entries(stats.topSpecies)
            .sort((a, b) => b[1] - a[1])
            .slice(0, 10)
            .map(([species, count]) => ({
                species: species.charAt(0).toUpperCase() + species.slice(1),
                count
            }));

        // Get monthly trend (last 6 months)
        const months = Object.keys(stats.byMonth).sort().slice(-6);
        const monthlyTrend = months.map(month => ({
            month,
            trades: stats.byMonth[month]
        }));

        return {
            totalTrades: stats.totalTrades,
            successRate: stats.totalTrades > 0 ?
                ((stats.successfulTrades / stats.totalTrades) * 100).toFixed(1) + '%' : '0%',
            uniqueSpeciesTraded: stats.uniqueSpecies.length,
            uniqueUsers: stats.uniqueUsers.length,
            totalPokemonTraded: stats.totalPokemonTraded,
            shinyCount: stats.shinyCount,
            legendaryCount: stats.legendaryCount,
            eventCount: stats.eventCount,
            topSpecies: sortedSpecies,
            byGame: stats.byGame,
            monthlyTrend,
            lastUpdated: stats.lastUpdated
        };
    }

    // Get user statistics
    getUserStatistics(userId) {
        const history = this.loadHistory();
        const userTrades = history.trades.filter(t => t.userId === userId);

        if (userTrades.length === 0) {
            return {
                userId,
                totalTrades: 0,
                message: 'No trade history found'
            };
        }

        const completed = userTrades.filter(t => t.status === 'completed');
        const speciesCount = {};
        let shinyCount = 0;

        for (const trade of completed) {
            if (trade.species) {
                speciesCount[trade.species.toLowerCase()] =
                    (speciesCount[trade.species.toLowerCase()] || 0) + 1;
            }
            if (trade.shiny) shinyCount++;
        }

        const topSpecies = Object.entries(speciesCount)
            .sort((a, b) => b[1] - a[1])
            .slice(0, 5)
            .map(([species, count]) => ({
                species: species.charAt(0).toUpperCase() + species.slice(1),
                count
            }));

        // First and last trade
        const sortedTrades = userTrades.sort((a, b) =>
            new Date(a.timestamp) - new Date(b.timestamp)
        );

        return {
            userId,
            totalTrades: userTrades.length,
            successfulTrades: completed.length,
            successRate: ((completed.length / userTrades.length) * 100).toFixed(1) + '%',
            uniqueSpecies: Object.keys(speciesCount).length,
            shinyTraded: shinyCount,
            topSpecies,
            firstTrade: sortedTrades[0]?.timestamp,
            lastTrade: sortedTrades[sortedTrades.length - 1]?.timestamp
        };
    }

    // Search trade history
    searchHistory(query, options = {}) {
        const history = this.loadHistory();
        const { limit = 50, offset = 0 } = options;

        const queryLower = query.toLowerCase();
        let results = history.trades.filter(t =>
            t.species?.toLowerCase().includes(queryLower) ||
            t.username?.toLowerCase().includes(queryLower) ||
            t.id?.toLowerCase().includes(queryLower)
        );

        results.sort((a, b) => new Date(b.timestamp) - new Date(a.timestamp));

        const total = results.length;
        results = results.slice(offset, offset + limit);

        return {
            query,
            total,
            limit,
            offset,
            results
        };
    }

    // Get recent trades
    getRecentTrades(limit = 20) {
        const history = this.loadHistory();

        const recent = history.trades
            .sort((a, b) => new Date(b.timestamp) - new Date(a.timestamp))
            .slice(0, limit)
            .map(t => ({
                id: t.id,
                species: t.species,
                shiny: t.shiny,
                username: t.username,
                game: t.game,
                status: t.status,
                timestamp: t.timestamp
            }));

        return { trades: recent };
    }

    // Get trade by ID
    getTradeById(tradeId) {
        const history = this.loadHistory();
        const trade = history.trades.find(t => t.id === tradeId);

        if (!trade) {
            return { found: false };
        }

        return { found: true, trade };
    }

    // Export user history
    exportUserHistory(userId, format = 'json') {
        const userHistory = this.getUserHistory(userId, { limit: 10000 });

        if (format === 'csv') {
            const headers = ['ID', 'Species', 'Shiny', 'Level', 'Game', 'Type', 'Status', 'Timestamp'];
            const rows = userHistory.trades.map(t => [
                t.id, t.species, t.shiny ? 'Yes' : 'No', t.level,
                t.game, t.type, t.status, t.timestamp
            ]);

            return {
                format: 'csv',
                content: [headers, ...rows].map(r => r.join(',')).join('\n')
            };
        }

        return {
            format: 'json',
            content: JSON.stringify(userHistory, null, 2)
        };
    }

    // Clean old records
    cleanOldRecords(daysToKeep = 90) {
        const history = this.loadHistory();
        const cutoffDate = new Date();
        cutoffDate.setDate(cutoffDate.getDate() - daysToKeep);

        const originalCount = history.trades.length;
        history.trades = history.trades.filter(t =>
            new Date(t.timestamp) > cutoffDate
        );

        const removedCount = originalCount - history.trades.length;
        this.saveHistory(history);

        return {
            success: true,
            removedCount,
            remainingCount: history.trades.length
        };
    }

    // Get daily activity
    getDailyActivity(days = 30) {
        const history = this.loadHistory();
        const activity = {};

        const cutoffDate = new Date();
        cutoffDate.setDate(cutoffDate.getDate() - days);

        for (const trade of history.trades) {
            const tradeDate = new Date(trade.timestamp);
            if (tradeDate < cutoffDate) continue;

            const dateKey = trade.timestamp.substring(0, 10); // YYYY-MM-DD
            activity[dateKey] = (activity[dateKey] || 0) + 1;
        }

        // Fill in missing days
        const result = [];
        const current = new Date();
        for (let i = days - 1; i >= 0; i--) {
            const date = new Date(current);
            date.setDate(date.getDate() - i);
            const dateKey = date.toISOString().substring(0, 10);
            result.push({
                date: dateKey,
                trades: activity[dateKey] || 0
            });
        }

        return { days, activity: result };
    }

    // Get leaderboard
    getLeaderboard(type = 'trades', limit = 10) {
        const history = this.loadHistory();
        const userStats = {};

        for (const trade of history.trades) {
            if (trade.status !== 'completed') continue;

            const userId = trade.userId;
            if (!userId) continue;

            if (!userStats[userId]) {
                userStats[userId] = {
                    userId,
                    username: trade.username,
                    trades: 0,
                    shiny: 0,
                    uniqueSpecies: new Set()
                };
            }

            userStats[userId].trades++;
            if (trade.shiny) userStats[userId].shiny++;
            if (trade.species) userStats[userId].uniqueSpecies.add(trade.species.toLowerCase());
        }

        // Convert to array and sort
        let leaderboard = Object.values(userStats).map(u => ({
            userId: u.userId,
            username: u.username,
            trades: u.trades,
            shiny: u.shiny,
            uniqueSpecies: u.uniqueSpecies.size
        }));

        // Sort based on type
        switch (type) {
            case 'shiny':
                leaderboard.sort((a, b) => b.shiny - a.shiny);
                break;
            case 'species':
                leaderboard.sort((a, b) => b.uniqueSpecies - a.uniqueSpecies);
                break;
            default:
                leaderboard.sort((a, b) => b.trades - a.trades);
        }

        return {
            type,
            leaderboard: leaderboard.slice(0, limit)
        };
    }
}

module.exports = PokemonTradeHistory;
