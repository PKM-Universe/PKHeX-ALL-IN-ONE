/**
 * Pokemon Trade Queue Integration
 * Direct integration with SysBot for automated trading
 */

const fs = require('fs');
const path = require('path');
const https = require('https');
const http = require('http');

class PokemonTradeQueue {
    constructor() {
        this.dataPath = path.join(__dirname, '..', 'Json');
        this.queueFile = path.join(this.dataPath, 'trade_queue.json');
        this.historyFile = path.join(this.dataPath, 'trade_history.json');
        this.botStatusFile = path.join(this.dataPath, 'trade_bot_status.json');

        // Trade types
        this.tradeTypes = {
            LINK: 'link',
            CLONE: 'clone',
            DUMP: 'dump',
            SEED: 'seed',
            BATCH: 'batch',
            GIVEAWAY: 'giveaway'
        };

        // Queue priorities
        this.priorities = {
            LOW: 0,
            NORMAL: 1,
            HIGH: 2,
            VIP: 3,
            IMMEDIATE: 4
        };

        // Bot configurations
        this.bots = new Map();
        this.loadBotConfigs();
    }

    loadBotConfigs() {
        try {
            const mappingPath = path.join(this.dataPath, 'channel_bot_mappings.json');
            if (fs.existsSync(mappingPath)) {
                const mappings = JSON.parse(fs.readFileSync(mappingPath, 'utf8'));
                for (const [channelId, config] of Object.entries(mappings)) {
                    this.bots.set(channelId, config);
                }
            }
        } catch (error) {
            console.error('Failed to load bot configs:', error);
        }
    }

    loadQueue() {
        try {
            if (fs.existsSync(this.queueFile)) {
                return JSON.parse(fs.readFileSync(this.queueFile, 'utf8'));
            }
        } catch (error) {
            console.error('Failed to load queue:', error);
        }
        return { pending: [], processing: [], completed: [], failed: [] };
    }

    saveQueue(queue) {
        try {
            fs.writeFileSync(this.queueFile, JSON.stringify(queue, null, 2));
            return true;
        } catch (error) {
            console.error('Failed to save queue:', error);
            return false;
        }
    }

    loadHistory() {
        try {
            if (fs.existsSync(this.historyFile)) {
                return JSON.parse(fs.readFileSync(this.historyFile, 'utf8'));
            }
        } catch (error) {
            console.error('Failed to load history:', error);
        }
        return [];
    }

    saveHistory(history) {
        try {
            // Keep last 1000 entries
            const trimmed = history.slice(-1000);
            fs.writeFileSync(this.historyFile, JSON.stringify(trimmed, null, 2));
            return true;
        } catch (error) {
            console.error('Failed to save history:', error);
            return false;
        }
    }

    generateTradeId() {
        return `TRD-${Date.now()}-${Math.random().toString(36).substring(2, 8).toUpperCase()}`;
    }

    // Create a trade request
    createTradeRequest(options) {
        const {
            userId,
            username,
            pokemon,
            tradeType = this.tradeTypes.LINK,
            priority = this.priorities.NORMAL,
            gameVersion = 'SV',
            switchCode = null,
            webhookUrl = null
        } = options;

        const tradeRequest = {
            id: this.generateTradeId(),
            userId,
            username,
            pokemon: Array.isArray(pokemon) ? pokemon : [pokemon],
            tradeType,
            priority,
            gameVersion,
            switchCode,
            webhookUrl,
            status: 'pending',
            createdAt: new Date().toISOString(),
            updatedAt: new Date().toISOString(),
            attempts: 0,
            maxAttempts: 3,
            estimatedWait: null,
            position: null,
            botAssigned: null
        };

        // Add to queue
        const queue = this.loadQueue();
        queue.pending.push(tradeRequest);

        // Sort by priority (higher first) then by creation time
        queue.pending.sort((a, b) => {
            if (b.priority !== a.priority) return b.priority - a.priority;
            return new Date(a.createdAt) - new Date(b.createdAt);
        });

        // Update positions
        queue.pending.forEach((req, index) => {
            req.position = index + 1;
            req.estimatedWait = this.estimateWaitTime(index);
        });

        this.saveQueue(queue);

        return {
            success: true,
            tradeId: tradeRequest.id,
            position: tradeRequest.position,
            estimatedWait: tradeRequest.estimatedWait,
            message: `Trade request created! Position: #${tradeRequest.position}`
        };
    }

    estimateWaitTime(position) {
        // Average 3 minutes per trade
        const minutesPerTrade = 3;
        const totalMinutes = position * minutesPerTrade;

        if (totalMinutes < 60) {
            return `~${totalMinutes} minutes`;
        } else {
            const hours = Math.floor(totalMinutes / 60);
            const mins = totalMinutes % 60;
            return `~${hours}h ${mins}m`;
        }
    }

    // Get queue status
    getQueueStatus(userId = null) {
        const queue = this.loadQueue();

        const status = {
            pending: queue.pending.length,
            processing: queue.processing.length,
            completed: queue.completed.length,
            failed: queue.failed.length,
            total: queue.pending.length + queue.processing.length
        };

        if (userId) {
            const userTrades = queue.pending.filter(t => t.userId === userId);
            status.userTrades = userTrades.map(t => ({
                id: t.id,
                position: t.position,
                pokemon: t.pokemon.map(p => p.species || p).join(', '),
                estimatedWait: t.estimatedWait,
                status: t.status
            }));
        }

        return status;
    }

    // Get trade by ID
    getTrade(tradeId) {
        const queue = this.loadQueue();

        // Search all queues
        for (const status of ['pending', 'processing', 'completed', 'failed']) {
            const trade = queue[status].find(t => t.id === tradeId);
            if (trade) {
                return { found: true, trade, status };
            }
        }

        return { found: false };
    }

    // Cancel a trade
    cancelTrade(tradeId, userId) {
        const queue = this.loadQueue();

        const index = queue.pending.findIndex(t => t.id === tradeId && t.userId === userId);
        if (index === -1) {
            return { success: false, error: 'Trade not found or cannot be cancelled' };
        }

        const cancelled = queue.pending.splice(index, 1)[0];
        cancelled.status = 'cancelled';
        cancelled.updatedAt = new Date().toISOString();

        // Update positions
        queue.pending.forEach((req, i) => {
            req.position = i + 1;
            req.estimatedWait = this.estimateWaitTime(i);
        });

        this.saveQueue(queue);

        // Add to history
        const history = this.loadHistory();
        history.push(cancelled);
        this.saveHistory(history);

        return { success: true, message: 'Trade cancelled successfully' };
    }

    // Start processing a trade (called by bot)
    startTrade(tradeId, botId) {
        const queue = this.loadQueue();

        const index = queue.pending.findIndex(t => t.id === tradeId);
        if (index === -1) {
            return { success: false, error: 'Trade not found in pending queue' };
        }

        const trade = queue.pending.splice(index, 1)[0];
        trade.status = 'processing';
        trade.botAssigned = botId;
        trade.startedAt = new Date().toISOString();
        trade.updatedAt = new Date().toISOString();
        trade.attempts++;

        queue.processing.push(trade);

        // Update positions for remaining
        queue.pending.forEach((req, i) => {
            req.position = i + 1;
            req.estimatedWait = this.estimateWaitTime(i);
        });

        this.saveQueue(queue);

        return { success: true, trade };
    }

    // Complete a trade
    completeTrade(tradeId, result = {}) {
        const queue = this.loadQueue();

        const index = queue.processing.findIndex(t => t.id === tradeId);
        if (index === -1) {
            return { success: false, error: 'Trade not found in processing queue' };
        }

        const trade = queue.processing.splice(index, 1)[0];
        trade.status = 'completed';
        trade.completedAt = new Date().toISOString();
        trade.updatedAt = new Date().toISOString();
        trade.result = result;

        queue.completed.push(trade);

        // Keep only last 100 completed
        if (queue.completed.length > 100) {
            queue.completed = queue.completed.slice(-100);
        }

        this.saveQueue(queue);

        // Add to history
        const history = this.loadHistory();
        history.push(trade);
        this.saveHistory(history);

        // Send webhook if configured
        if (trade.webhookUrl) {
            this.sendWebhook(trade.webhookUrl, {
                type: 'trade_complete',
                trade
            });
        }

        return { success: true, trade };
    }

    // Fail a trade
    failTrade(tradeId, reason) {
        const queue = this.loadQueue();

        const index = queue.processing.findIndex(t => t.id === tradeId);
        if (index === -1) {
            return { success: false, error: 'Trade not found in processing queue' };
        }

        const trade = queue.processing.splice(index, 1)[0];

        // Retry if under max attempts
        if (trade.attempts < trade.maxAttempts) {
            trade.status = 'pending';
            trade.lastError = reason;
            trade.updatedAt = new Date().toISOString();
            queue.pending.unshift(trade); // Put at front of queue
        } else {
            trade.status = 'failed';
            trade.failedAt = new Date().toISOString();
            trade.updatedAt = new Date().toISOString();
            trade.failReason = reason;
            queue.failed.push(trade);

            // Keep only last 50 failed
            if (queue.failed.length > 50) {
                queue.failed = queue.failed.slice(-50);
            }
        }

        this.saveQueue(queue);

        // Add to history
        const history = this.loadHistory();
        history.push(trade);
        this.saveHistory(history);

        return { success: true, trade, retried: trade.attempts < trade.maxAttempts };
    }

    // Generate Showdown format for SysBot
    toShowdownForBot(pokemon) {
        const lines = [];

        let line1 = pokemon.species;
        if (pokemon.nickname) {
            line1 = `${pokemon.nickname} (${pokemon.species})`;
        }
        if (pokemon.item) {
            line1 += ` @ ${pokemon.item}`;
        }
        lines.push(line1);

        if (pokemon.ability) lines.push(`Ability: ${pokemon.ability}`);
        if (pokemon.level && pokemon.level !== 100) lines.push(`Level: ${pokemon.level}`);
        if (pokemon.shiny) lines.push('Shiny: Yes');
        if (pokemon.teraType) lines.push(`Tera Type: ${pokemon.teraType}`);

        // EVs
        if (pokemon.evs) {
            const evParts = [];
            const names = { hp: 'HP', atk: 'Atk', def: 'Def', spa: 'SpA', spd: 'SpD', spe: 'Spe' };
            for (const [stat, value] of Object.entries(pokemon.evs)) {
                if (value > 0) evParts.push(`${value} ${names[stat]}`);
            }
            if (evParts.length > 0) lines.push(`EVs: ${evParts.join(' / ')}`);
        }

        if (pokemon.nature) lines.push(`${pokemon.nature} Nature`);

        // IVs
        if (pokemon.ivs) {
            const ivParts = [];
            const names = { hp: 'HP', atk: 'Atk', def: 'Def', spa: 'SpA', spd: 'SpD', spe: 'Spe' };
            for (const [stat, value] of Object.entries(pokemon.ivs)) {
                if (value < 31) ivParts.push(`${value} ${names[stat]}`);
            }
            if (ivParts.length > 0) lines.push(`IVs: ${ivParts.join(' / ')}`);
        }

        // Moves
        if (pokemon.moves) {
            for (const move of pokemon.moves) {
                lines.push(`- ${move}`);
            }
        }

        // SysBot specific
        if (pokemon.ot) lines.push(`.OT ${pokemon.ot}`);
        if (pokemon.tid) lines.push(`.TID ${pokemon.tid}`);
        if (pokemon.sid) lines.push(`.SID ${pokemon.sid}`);
        if (pokemon.ball) lines.push(`.Ball ${pokemon.ball}`);
        if (pokemon.language) lines.push(`.Language ${pokemon.language}`);
        if (pokemon.originGame) lines.push(`.Version ${pokemon.originGame}`);

        return lines.join('\n');
    }

    // Get next trade for a bot
    getNextTrade(botId, gameVersion = null) {
        const queue = this.loadQueue();

        // Find first compatible pending trade
        let trade = null;
        let index = -1;

        for (let i = 0; i < queue.pending.length; i++) {
            const t = queue.pending[i];
            if (!gameVersion || t.gameVersion === gameVersion) {
                trade = t;
                index = i;
                break;
            }
        }

        if (!trade) {
            return { success: false, message: 'No trades available' };
        }

        // Start the trade
        return this.startTrade(trade.id, botId);
    }

    // Batch trade request
    createBatchTrade(userId, username, pokemonList, options = {}) {
        const results = [];

        for (const pokemon of pokemonList) {
            const result = this.createTradeRequest({
                userId,
                username,
                pokemon,
                tradeType: options.tradeType || this.tradeTypes.BATCH,
                priority: options.priority || this.priorities.NORMAL,
                gameVersion: options.gameVersion || 'SV'
            });
            results.push(result);
        }

        return {
            success: true,
            count: results.length,
            trades: results
        };
    }

    // Get user trade history
    getUserHistory(userId, limit = 20) {
        const history = this.loadHistory();
        const userHistory = history
            .filter(t => t.userId === userId)
            .slice(-limit)
            .reverse();

        return {
            count: userHistory.length,
            trades: userHistory.map(t => ({
                id: t.id,
                pokemon: t.pokemon.map(p => p.species || p).join(', '),
                status: t.status,
                createdAt: t.createdAt,
                completedAt: t.completedAt || t.failedAt
            }))
        };
    }

    // Get bot status
    getBotStatus() {
        try {
            if (fs.existsSync(this.botStatusFile)) {
                return JSON.parse(fs.readFileSync(this.botStatusFile, 'utf8'));
            }
        } catch (error) {
            console.error('Failed to load bot status:', error);
        }
        return {};
    }

    // Clear old data
    cleanup() {
        const queue = this.loadQueue();

        // Remove completed trades older than 24 hours
        const yesterday = new Date(Date.now() - 24 * 60 * 60 * 1000);
        queue.completed = queue.completed.filter(t =>
            new Date(t.completedAt) > yesterday
        );

        // Remove failed trades older than 7 days
        const weekAgo = new Date(Date.now() - 7 * 24 * 60 * 60 * 1000);
        queue.failed = queue.failed.filter(t =>
            new Date(t.failedAt) > weekAgo
        );

        this.saveQueue(queue);

        return { success: true, message: 'Cleanup completed' };
    }

    // Send webhook notification
    async sendWebhook(url, data) {
        try {
            const urlObj = new URL(url);
            const protocol = urlObj.protocol === 'https:' ? https : http;

            const postData = JSON.stringify(data);

            const options = {
                hostname: urlObj.hostname,
                port: urlObj.port,
                path: urlObj.pathname,
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Content-Length': Buffer.byteLength(postData)
                }
            };

            return new Promise((resolve, reject) => {
                const req = protocol.request(options, (res) => {
                    resolve({ status: res.statusCode });
                });

                req.on('error', reject);
                req.write(postData);
                req.end();
            });
        } catch (error) {
            console.error('Webhook error:', error);
            return { error: error.message };
        }
    }

    // Statistics
    getStatistics() {
        const queue = this.loadQueue();
        const history = this.loadHistory();

        const now = new Date();
        const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
        const thisWeek = new Date(now - 7 * 24 * 60 * 60 * 1000);

        const todayTrades = history.filter(t =>
            new Date(t.createdAt) >= today
        );

        const weekTrades = history.filter(t =>
            new Date(t.createdAt) >= thisWeek
        );

        const successRate = history.length > 0 ?
            (history.filter(t => t.status === 'completed').length / history.length * 100).toFixed(1) :
            100;

        return {
            queue: {
                pending: queue.pending.length,
                processing: queue.processing.length,
                completed: queue.completed.length,
                failed: queue.failed.length
            },
            today: {
                total: todayTrades.length,
                completed: todayTrades.filter(t => t.status === 'completed').length,
                failed: todayTrades.filter(t => t.status === 'failed').length
            },
            thisWeek: {
                total: weekTrades.length,
                completed: weekTrades.filter(t => t.status === 'completed').length,
                failed: weekTrades.filter(t => t.status === 'failed').length
            },
            allTime: {
                total: history.length,
                successRate: `${successRate}%`
            }
        };
    }
}

module.exports = PokemonTradeQueue;
