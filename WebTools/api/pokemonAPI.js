/**
 * Pokemon API Endpoints
 * REST API for external tools and integrations
 */

const express = require('express');
const fs = require('fs');
const path = require('path');
const cors = require('cors');

class PokemonAPI {
    constructor(port = 3001) {
        this.port = port;
        this.app = express();
        this.dataPath = path.join(__dirname, '..', 'Json');

        // Middleware
        this.app.use(cors());
        this.app.use(express.json());
        this.app.use(express.urlencoded({ extended: true }));

        // Rate limiting storage
        this.rateLimits = new Map();

        this.setupRoutes();
    }

    // Simple rate limiter
    rateLimit(ip, limit = 100, window = 60000) {
        const now = Date.now();
        const key = ip;

        if (!this.rateLimits.has(key)) {
            this.rateLimits.set(key, { count: 1, resetAt: now + window });
            return true;
        }

        const record = this.rateLimits.get(key);
        if (now > record.resetAt) {
            this.rateLimits.set(key, { count: 1, resetAt: now + window });
            return true;
        }

        if (record.count >= limit) {
            return false;
        }

        record.count++;
        return true;
    }

    setupRoutes() {
        // Rate limit middleware
        this.app.use((req, res, next) => {
            const ip = req.ip || req.connection.remoteAddress;
            if (!this.rateLimit(ip)) {
                return res.status(429).json({ error: 'Rate limit exceeded', retryAfter: 60 });
            }
            next();
        });

        // API info
        this.app.get('/api', (req, res) => {
            res.json({
                name: 'PKHex Web API',
                version: '1.0.0',
                endpoints: {
                    pokemon: '/api/pokemon',
                    species: '/api/species/:name',
                    events: '/api/events',
                    raids: '/api/raids',
                    smogon: '/api/smogon/:format/:pokemon',
                    locations: '/api/locations/:game',
                    validate: '/api/validate',
                    generate: '/api/generate',
                    showdown: '/api/showdown/parse',
                    showdownExport: '/api/showdown/export'
                }
            });
        });

        // Get all Pokemon species
        this.app.get('/api/pokemon', (req, res) => {
            try {
                const data = this.loadData('pokemon_complete_data.json');
                if (!data) {
                    return res.status(500).json({ error: 'Pokemon data not available' });
                }

                const { limit = 50, offset = 0, search } = req.query;

                let pokemon = Object.entries(data).map(([id, info]) => ({
                    id,
                    ...info
                }));

                if (search) {
                    pokemon = pokemon.filter(p =>
                        p.name?.toLowerCase().includes(search.toLowerCase()) ||
                        p.id?.toString().includes(search)
                    );
                }

                const total = pokemon.length;
                pokemon = pokemon.slice(parseInt(offset), parseInt(offset) + parseInt(limit));

                res.json({
                    total,
                    limit: parseInt(limit),
                    offset: parseInt(offset),
                    pokemon
                });
            } catch (error) {
                res.status(500).json({ error: error.message });
            }
        });

        // Get specific Pokemon
        this.app.get('/api/species/:name', (req, res) => {
            try {
                const { name } = req.params;
                const data = this.loadData('pokemon_complete_data.json');

                if (!data) {
                    return res.status(500).json({ error: 'Pokemon data not available' });
                }

                // Search by name or ID
                const pokemon = Object.entries(data).find(([id, info]) =>
                    info.name?.toLowerCase() === name.toLowerCase() ||
                    id === name
                );

                if (!pokemon) {
                    return res.status(404).json({ error: 'Pokemon not found' });
                }

                res.json({ id: pokemon[0], ...pokemon[1] });
            } catch (error) {
                res.status(500).json({ error: error.message });
            }
        });

        // Get event Pokemon
        this.app.get('/api/events', (req, res) => {
            try {
                const data = this.loadData('mystery_gift_expanded.json');
                if (!data || !data.events) {
                    return res.status(500).json({ error: 'Event data not available' });
                }

                const { species, year, shiny, limit = 50, offset = 0 } = req.query;

                let events = data.events;

                if (species) {
                    events = events.filter(e =>
                        e.species?.toLowerCase().includes(species.toLowerCase())
                    );
                }

                if (year) {
                    events = events.filter(e => e.year?.toString() === year);
                }

                if (shiny !== undefined) {
                    events = events.filter(e => e.shiny === (shiny === 'true'));
                }

                const total = events.length;
                events = events.slice(parseInt(offset), parseInt(offset) + parseInt(limit));

                res.json({
                    total,
                    limit: parseInt(limit),
                    offset: parseInt(offset),
                    events
                });
            } catch (error) {
                res.status(500).json({ error: error.message });
            }
        });

        // Get specific event
        this.app.get('/api/events/:id', (req, res) => {
            try {
                const { id } = req.params;
                const data = this.loadData('mystery_gift_expanded.json');

                if (!data || !data.events) {
                    return res.status(500).json({ error: 'Event data not available' });
                }

                const event = data.events[parseInt(id)];
                if (!event) {
                    return res.status(404).json({ error: 'Event not found' });
                }

                res.json(event);
            } catch (error) {
                res.status(500).json({ error: error.message });
            }
        });

        // Get raids
        this.app.get('/api/raids', (req, res) => {
            try {
                const data = this.loadData('raid_database.json');
                if (!data) {
                    return res.status(500).json({ error: 'Raid data not available' });
                }

                const { type, stars } = req.query;

                let result = data;

                if (type === 'tera') {
                    result = { teraRaids: data.teraRaids };
                } else if (type === 'max') {
                    result = { maxRaids: data.maxRaids };
                } else if (type === 'dynamax') {
                    result = { dynamaxAdventures: data.dynamaxAdventures };
                }

                if (stars && data.teraRaids) {
                    const starKey = `${stars}star`;
                    if (data.teraRaids[starKey]) {
                        result = { [starKey]: data.teraRaids[starKey] };
                    }
                }

                res.json(result);
            } catch (error) {
                res.status(500).json({ error: error.message });
            }
        });

        // Get Smogon movesets
        this.app.get('/api/smogon/:format/:pokemon', (req, res) => {
            try {
                const { format, pokemon } = req.params;
                const data = this.loadData('smogon_movesets.json');

                if (!data) {
                    return res.status(500).json({ error: 'Smogon data not available' });
                }

                if (!data[format]) {
                    return res.status(404).json({ error: `Format '${format}' not found` });
                }

                const pokemonData = data[format][pokemon.toLowerCase()];
                if (!pokemonData) {
                    return res.status(404).json({ error: `No sets found for ${pokemon} in ${format}` });
                }

                res.json({ format, pokemon, ...pokemonData });
            } catch (error) {
                res.status(500).json({ error: error.message });
            }
        });

        // Get all Smogon formats
        this.app.get('/api/smogon', (req, res) => {
            try {
                const data = this.loadData('smogon_movesets.json');
                if (!data) {
                    return res.status(500).json({ error: 'Smogon data not available' });
                }

                const formats = Object.keys(data).filter(k => k !== 'totalSets' && k !== 'lastUpdated');
                res.json({ formats, totalSets: data.totalSets });
            } catch (error) {
                res.status(500).json({ error: error.message });
            }
        });

        // Get locations
        this.app.get('/api/locations/:game', (req, res) => {
            try {
                const { game } = req.params;
                const data = this.loadData('location_database.json');

                if (!data) {
                    return res.status(500).json({ error: 'Location data not available' });
                }

                // Map game names to data structure
                const gameMap = {
                    'scarlet': 'gen9.paldea',
                    'violet': 'gen9.paldea',
                    'sword': 'gen8.swsh',
                    'shield': 'gen8.swsh',
                    'bdsp': 'gen8.bdsp',
                    'pla': 'gen8.pla',
                    'usum': 'gen7.usum'
                };

                const dataPath = gameMap[game.toLowerCase()] || game;
                const pathParts = dataPath.split('.');

                let result = data;
                for (const part of pathParts) {
                    result = result?.[part];
                }

                if (!result) {
                    return res.status(404).json({ error: `Locations for '${game}' not found` });
                }

                res.json({ game, locations: result });
            } catch (error) {
                res.status(500).json({ error: error.message });
            }
        });

        // Validate Pokemon
        this.app.post('/api/validate', (req, res) => {
            try {
                const pokemon = req.body;
                const issues = this.validatePokemon(pokemon);

                res.json({
                    valid: issues.length === 0,
                    issues,
                    pokemon
                });
            } catch (error) {
                res.status(400).json({ error: error.message });
            }
        });

        // Generate Pokemon
        this.app.post('/api/generate', (req, res) => {
            try {
                const config = req.body;
                const pokemon = this.generatePokemon(config);

                res.json(pokemon);
            } catch (error) {
                res.status(400).json({ error: error.message });
            }
        });

        // Parse Showdown format
        this.app.post('/api/showdown/parse', (req, res) => {
            try {
                const { text } = req.body;
                if (!text) {
                    return res.status(400).json({ error: 'Showdown text required' });
                }

                const pokemon = this.parseShowdown(text);
                res.json(pokemon);
            } catch (error) {
                res.status(400).json({ error: error.message });
            }
        });

        // Export to Showdown format
        this.app.post('/api/showdown/export', (req, res) => {
            try {
                const pokemon = req.body;
                const showdown = this.toShowdown(pokemon);

                res.json({ showdown });
            } catch (error) {
                res.status(400).json({ error: error.message });
            }
        });

        // Batch operations
        this.app.post('/api/batch/validate', (req, res) => {
            try {
                const { pokemon } = req.body;
                if (!Array.isArray(pokemon)) {
                    return res.status(400).json({ error: 'Pokemon array required' });
                }

                const results = pokemon.map(p => ({
                    pokemon: p,
                    valid: this.validatePokemon(p).length === 0,
                    issues: this.validatePokemon(p)
                }));

                res.json({ count: results.length, results });
            } catch (error) {
                res.status(400).json({ error: error.message });
            }
        });

        // Get trade queue status
        this.app.get('/api/queue/status', (req, res) => {
            try {
                const data = this.loadData('trade_bot_status.json');
                res.json(data || { error: 'Queue data not available' });
            } catch (error) {
                res.status(500).json({ error: error.message });
            }
        });

        // Health check
        this.app.get('/api/health', (req, res) => {
            res.json({
                status: 'healthy',
                uptime: process.uptime(),
                timestamp: new Date().toISOString()
            });
        });

        // 404 handler
        this.app.use((req, res) => {
            res.status(404).json({ error: 'Endpoint not found' });
        });

        // Error handler
        this.app.use((err, req, res, next) => {
            console.error('API Error:', err);
            res.status(500).json({ error: 'Internal server error' });
        });
    }

    loadData(filename) {
        try {
            const filePath = path.join(this.dataPath, filename);
            if (fs.existsSync(filePath)) {
                return JSON.parse(fs.readFileSync(filePath, 'utf8'));
            }
        } catch (error) {
            console.error(`Failed to load ${filename}:`, error);
        }
        return null;
    }

    validatePokemon(pokemon) {
        const issues = [];

        if (!pokemon.species) {
            issues.push('Species is required');
        }

        // Level check
        if (pokemon.level !== undefined) {
            if (pokemon.level < 1 || pokemon.level > 100) {
                issues.push('Level must be between 1 and 100');
            }
        }

        // EV checks
        if (pokemon.evs) {
            const evTotal = Object.values(pokemon.evs).reduce((a, b) => a + (parseInt(b) || 0), 0);
            if (evTotal > 510) {
                issues.push(`EV total (${evTotal}) exceeds maximum of 510`);
            }

            for (const [stat, value] of Object.entries(pokemon.evs)) {
                if (value > 252) {
                    issues.push(`${stat} EVs (${value}) exceed maximum of 252`);
                }
                if (value < 0) {
                    issues.push(`${stat} EVs cannot be negative`);
                }
            }
        }

        // IV checks
        if (pokemon.ivs) {
            for (const [stat, value] of Object.entries(pokemon.ivs)) {
                if (value > 31 || value < 0) {
                    issues.push(`${stat} IVs must be between 0 and 31`);
                }
            }
        }

        // Move count
        if (pokemon.moves && pokemon.moves.length > 4) {
            issues.push('Pokemon cannot have more than 4 moves');
        }

        // PP Ups
        if (pokemon.ppUps) {
            for (const pp of Object.values(pokemon.ppUps)) {
                if (pp > 3 || pp < 0) {
                    issues.push('PP Ups must be between 0 and 3');
                }
            }
        }

        return issues;
    }

    generatePokemon(config) {
        const pokemon = {
            species: config.species || 'Pikachu',
            nickname: config.nickname || '',
            level: config.level || 100,
            shiny: config.shiny || false,
            nature: config.nature || 'Hardy',
            ability: config.ability || null,
            item: config.item || null,
            ball: config.ball || 'Poke Ball',
            gender: config.gender || 'Random',
            teraType: config.teraType || null,
            ivs: config.ivs || { hp: 31, atk: 31, def: 31, spa: 31, spd: 31, spe: 31 },
            evs: config.evs || { hp: 0, atk: 0, def: 0, spa: 0, spd: 0, spe: 0 },
            moves: config.moves || [],
            ot: config.ot || 'PKHex',
            tid: config.tid || Math.floor(Math.random() * 65535),
            sid: config.sid || Math.floor(Math.random() * 65535),
            language: config.language || 'ENG',
            originGame: config.originGame || 'Scarlet/Violet',
            metLevel: config.metLevel || config.level || 100,
            metLocation: config.metLocation || 'Paldea',
            metDate: config.metDate || new Date().toISOString().split('T')[0],
            friendship: config.friendship || 255,
            isEgg: config.isEgg || false,
            pokerus: config.pokerus || null
        };

        // Generate showdown format
        pokemon.showdownFormat = this.toShowdown(pokemon);

        return pokemon;
    }

    parseShowdown(text) {
        const lines = text.trim().split('\n');
        const pokemon = {
            species: '',
            nickname: null,
            item: null,
            ability: null,
            level: 100,
            shiny: false,
            nature: 'Hardy',
            teraType: null,
            evs: { hp: 0, atk: 0, def: 0, spa: 0, spd: 0, spe: 0 },
            ivs: { hp: 31, atk: 31, def: 31, spa: 31, spd: 31, spe: 31 },
            moves: []
        };

        // Parse first line
        const firstLine = lines[0];
        if (firstLine.includes('@')) {
            const [namePart, itemPart] = firstLine.split('@').map(s => s.trim());
            pokemon.item = itemPart;

            const nickMatch = namePart.match(/(.+)\s*\((.+)\)/);
            if (nickMatch) {
                pokemon.nickname = nickMatch[1].trim();
                pokemon.species = nickMatch[2].trim();
            } else {
                pokemon.species = namePart;
            }
        } else {
            const nickMatch = firstLine.match(/(.+)\s*\((.+)\)/);
            if (nickMatch) {
                pokemon.nickname = nickMatch[1].trim();
                pokemon.species = nickMatch[2].trim();
            } else {
                pokemon.species = firstLine.trim();
            }
        }

        // Parse remaining lines
        for (let i = 1; i < lines.length; i++) {
            const line = lines[i].trim();

            if (line.startsWith('Ability:')) {
                pokemon.ability = line.replace('Ability:', '').trim();
            } else if (line.startsWith('Level:')) {
                pokemon.level = parseInt(line.replace('Level:', '').trim());
            } else if (line.startsWith('Shiny:')) {
                pokemon.shiny = line.toLowerCase().includes('yes');
            } else if (line.startsWith('Tera Type:')) {
                pokemon.teraType = line.replace('Tera Type:', '').trim();
            } else if (line.startsWith('EVs:')) {
                this.parseStats(line.replace('EVs:', ''), pokemon.evs);
            } else if (line.startsWith('IVs:')) {
                this.parseStats(line.replace('IVs:', ''), pokemon.ivs);
            } else if (line.includes('Nature')) {
                const natureMatch = line.match(/(\w+)\s+Nature/);
                if (natureMatch) pokemon.nature = natureMatch[1];
            } else if (line.startsWith('-')) {
                pokemon.moves.push(line.replace('-', '').trim());
            }
        }

        return pokemon;
    }

    parseStats(text, statsObj) {
        const parts = text.trim().split('/').map(s => s.trim());
        for (const part of parts) {
            const match = part.match(/(\d+)\s*(HP|Atk|Def|SpA|SpD|Spe)/i);
            if (match) {
                const statMap = { hp: 'hp', atk: 'atk', def: 'def', spa: 'spa', spd: 'spd', spe: 'spe' };
                const stat = statMap[match[2].toLowerCase()];
                if (stat) statsObj[stat] = parseInt(match[1]);
            }
        }
    }

    toShowdown(pokemon) {
        const lines = [];

        let line1 = pokemon.species;
        if (pokemon.nickname && pokemon.nickname !== pokemon.species) {
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
            const statNames = { hp: 'HP', atk: 'Atk', def: 'Def', spa: 'SpA', spd: 'SpD', spe: 'Spe' };
            for (const [stat, value] of Object.entries(pokemon.evs)) {
                if (value > 0) evParts.push(`${value} ${statNames[stat]}`);
            }
            if (evParts.length > 0) lines.push(`EVs: ${evParts.join(' / ')}`);
        }

        if (pokemon.nature) lines.push(`${pokemon.nature} Nature`);

        // IVs (only non-perfect)
        if (pokemon.ivs) {
            const ivParts = [];
            const statNames = { hp: 'HP', atk: 'Atk', def: 'Def', spa: 'SpA', spd: 'SpD', spe: 'Spe' };
            for (const [stat, value] of Object.entries(pokemon.ivs)) {
                if (value < 31) ivParts.push(`${value} ${statNames[stat]}`);
            }
            if (ivParts.length > 0) lines.push(`IVs: ${ivParts.join(' / ')}`);
        }

        // Moves
        if (pokemon.moves) {
            for (const move of pokemon.moves) {
                lines.push(`- ${move}`);
            }
        }

        return lines.join('\n');
    }

    start() {
        return new Promise((resolve) => {
            this.server = this.app.listen(this.port, () => {
                console.log(`Pokemon API running on port ${this.port}`);
                resolve(this.server);
            });
        });
    }

    stop() {
        if (this.server) {
            this.server.close();
        }
    }
}

// Export for use as module or standalone
module.exports = PokemonAPI;

// Start if run directly
if (require.main === module) {
    const api = new PokemonAPI(process.env.API_PORT || 3001);
    api.start();
}
