/**
 * Pokemon Damage Calculator
 * Calculates move damage with type effectiveness, STAB, weather, abilities, items, and more
 * Based on Gen 9 damage formula
 */

class PokemonDamageCalculator {
    constructor() {
        // Type effectiveness chart
        this.typeChart = {
            normal: { rock: 0.5, ghost: 0, steel: 0.5 },
            fire: { fire: 0.5, water: 0.5, grass: 2, ice: 2, bug: 2, rock: 0.5, dragon: 0.5, steel: 2 },
            water: { fire: 2, water: 0.5, grass: 0.5, ground: 2, rock: 2, dragon: 0.5 },
            electric: { water: 2, electric: 0.5, grass: 0.5, ground: 0, flying: 2, dragon: 0.5 },
            grass: { fire: 0.5, water: 2, grass: 0.5, poison: 0.5, ground: 2, flying: 0.5, bug: 0.5, rock: 2, dragon: 0.5, steel: 0.5 },
            ice: { fire: 0.5, water: 0.5, grass: 2, ice: 0.5, ground: 2, flying: 2, dragon: 2, steel: 0.5 },
            fighting: { normal: 2, ice: 2, poison: 0.5, flying: 0.5, psychic: 0.5, bug: 0.5, rock: 2, ghost: 0, dark: 2, steel: 2, fairy: 0.5 },
            poison: { grass: 2, poison: 0.5, ground: 0.5, rock: 0.5, ghost: 0.5, steel: 0, fairy: 2 },
            ground: { fire: 2, electric: 2, grass: 0.5, poison: 2, flying: 0, bug: 0.5, rock: 2, steel: 2 },
            flying: { electric: 0.5, grass: 2, fighting: 2, bug: 2, rock: 0.5, steel: 0.5 },
            psychic: { fighting: 2, poison: 2, psychic: 0.5, dark: 0, steel: 0.5 },
            bug: { fire: 0.5, grass: 2, fighting: 0.5, poison: 0.5, flying: 0.5, psychic: 2, ghost: 0.5, dark: 2, steel: 0.5, fairy: 0.5 },
            rock: { fire: 2, ice: 2, fighting: 0.5, ground: 0.5, flying: 2, bug: 2, steel: 0.5 },
            ghost: { normal: 0, psychic: 2, ghost: 2, dark: 0.5 },
            dragon: { dragon: 2, steel: 0.5, fairy: 0 },
            dark: { fighting: 0.5, psychic: 2, ghost: 2, dark: 0.5, fairy: 0.5 },
            steel: { fire: 0.5, water: 0.5, electric: 0.5, ice: 2, rock: 2, steel: 0.5, fairy: 2 },
            fairy: { fire: 0.5, fighting: 2, poison: 0.5, dragon: 2, dark: 2, steel: 0.5 }
        };

        // Nature stat modifiers
        this.natures = {
            hardy: {}, lonely: { atk: 1.1, def: 0.9 }, brave: { atk: 1.1, spe: 0.9 },
            adamant: { atk: 1.1, spa: 0.9 }, naughty: { atk: 1.1, spd: 0.9 },
            bold: { def: 1.1, atk: 0.9 }, docile: {}, relaxed: { def: 1.1, spe: 0.9 },
            impish: { def: 1.1, spa: 0.9 }, lax: { def: 1.1, spd: 0.9 },
            timid: { spe: 1.1, atk: 0.9 }, hasty: { spe: 1.1, def: 0.9 }, serious: {},
            jolly: { spe: 1.1, spa: 0.9 }, naive: { spe: 1.1, spd: 0.9 },
            modest: { spa: 1.1, atk: 0.9 }, mild: { spa: 1.1, def: 0.9 },
            quiet: { spa: 1.1, spe: 0.9 }, bashful: {}, rash: { spa: 1.1, spd: 0.9 },
            calm: { spd: 1.1, atk: 0.9 }, gentle: { spd: 1.1, def: 0.9 },
            sassy: { spd: 1.1, spe: 0.9 }, careful: { spd: 1.1, spa: 0.9 }, quirky: {}
        };

        // Weather effects
        this.weatherEffects = {
            sun: { fire: 1.5, water: 0.5 },
            rain: { water: 1.5, fire: 0.5 },
            sand: {},
            hail: {},
            snow: {}
        };

        // Terrain effects
        this.terrainEffects = {
            electric: { electric: 1.3 },
            grassy: { grass: 1.3 },
            psychic: { psychic: 1.3 },
            misty: { dragon: 0.5 }
        };

        // Move data (sample - comprehensive list)
        this.moves = {
            // Physical moves
            'tackle': { type: 'normal', category: 'physical', power: 40, accuracy: 100 },
            'quick-attack': { type: 'normal', category: 'physical', power: 40, accuracy: 100, priority: 1 },
            'return': { type: 'normal', category: 'physical', power: 102, accuracy: 100 },
            'body-slam': { type: 'normal', category: 'physical', power: 85, accuracy: 100 },
            'double-edge': { type: 'normal', category: 'physical', power: 120, accuracy: 100, recoil: 0.33 },
            'extreme-speed': { type: 'normal', category: 'physical', power: 80, accuracy: 100, priority: 2 },
            'facade': { type: 'normal', category: 'physical', power: 70, accuracy: 100, doubleOnStatus: true },
            'earthquake': { type: 'ground', category: 'physical', power: 100, accuracy: 100 },
            'close-combat': { type: 'fighting', category: 'physical', power: 120, accuracy: 100 },
            'iron-head': { type: 'steel', category: 'physical', power: 80, accuracy: 100 },
            'stone-edge': { type: 'rock', category: 'physical', power: 100, accuracy: 80, critRatio: 2 },
            'rock-slide': { type: 'rock', category: 'physical', power: 75, accuracy: 90 },
            'crunch': { type: 'dark', category: 'physical', power: 80, accuracy: 100 },
            'x-scissor': { type: 'bug', category: 'physical', power: 80, accuracy: 100 },
            'poison-jab': { type: 'poison', category: 'physical', power: 80, accuracy: 100 },
            'play-rough': { type: 'fairy', category: 'physical', power: 90, accuracy: 90 },
            'outrage': { type: 'dragon', category: 'physical', power: 120, accuracy: 100 },
            'dragon-claw': { type: 'dragon', category: 'physical', power: 80, accuracy: 100 },
            'flare-blitz': { type: 'fire', category: 'physical', power: 120, accuracy: 100, recoil: 0.33 },
            'fire-punch': { type: 'fire', category: 'physical', power: 75, accuracy: 100 },
            'waterfall': { type: 'water', category: 'physical', power: 80, accuracy: 100 },
            'aqua-jet': { type: 'water', category: 'physical', power: 40, accuracy: 100, priority: 1 },
            'ice-punch': { type: 'ice', category: 'physical', power: 75, accuracy: 100 },
            'icicle-crash': { type: 'ice', category: 'physical', power: 85, accuracy: 90 },
            'thunder-punch': { type: 'electric', category: 'physical', power: 75, accuracy: 100 },
            'wild-charge': { type: 'electric', category: 'physical', power: 90, accuracy: 100, recoil: 0.25 },
            'seed-bomb': { type: 'grass', category: 'physical', power: 80, accuracy: 100 },
            'wood-hammer': { type: 'grass', category: 'physical', power: 120, accuracy: 100, recoil: 0.33 },
            'zen-headbutt': { type: 'psychic', category: 'physical', power: 80, accuracy: 90 },
            'shadow-claw': { type: 'ghost', category: 'physical', power: 70, accuracy: 100, critRatio: 2 },
            'brave-bird': { type: 'flying', category: 'physical', power: 120, accuracy: 100, recoil: 0.33 },
            'drill-peck': { type: 'flying', category: 'physical', power: 80, accuracy: 100 },
            'knock-off': { type: 'dark', category: 'physical', power: 65, accuracy: 100, bonusOnItem: 1.5 },
            'sucker-punch': { type: 'dark', category: 'physical', power: 70, accuracy: 100, priority: 1 },
            'u-turn': { type: 'bug', category: 'physical', power: 70, accuracy: 100, switchOut: true },
            'volt-switch': { type: 'electric', category: 'special', power: 70, accuracy: 100, switchOut: true },
            'flip-turn': { type: 'water', category: 'physical', power: 60, accuracy: 100, switchOut: true },

            // Special moves
            'thunderbolt': { type: 'electric', category: 'special', power: 90, accuracy: 100 },
            'thunder': { type: 'electric', category: 'special', power: 110, accuracy: 70, rainAccuracy: 100 },
            'flamethrower': { type: 'fire', category: 'special', power: 90, accuracy: 100 },
            'fire-blast': { type: 'fire', category: 'special', power: 110, accuracy: 85 },
            'hydro-pump': { type: 'water', category: 'special', power: 110, accuracy: 80 },
            'surf': { type: 'water', category: 'special', power: 90, accuracy: 100 },
            'scald': { type: 'water', category: 'special', power: 80, accuracy: 100, burnChance: 30 },
            'ice-beam': { type: 'ice', category: 'special', power: 90, accuracy: 100 },
            'blizzard': { type: 'ice', category: 'special', power: 110, accuracy: 70, hailAccuracy: 100 },
            'energy-ball': { type: 'grass', category: 'special', power: 90, accuracy: 100 },
            'leaf-storm': { type: 'grass', category: 'special', power: 130, accuracy: 90, spaDrop: 2 },
            'giga-drain': { type: 'grass', category: 'special', power: 75, accuracy: 100, drain: 0.5 },
            'psychic': { type: 'psychic', category: 'special', power: 90, accuracy: 100 },
            'psyshock': { type: 'psychic', category: 'special', power: 80, accuracy: 100, targetsDef: true },
            'shadow-ball': { type: 'ghost', category: 'special', power: 80, accuracy: 100 },
            'dark-pulse': { type: 'dark', category: 'special', power: 80, accuracy: 100 },
            'dragon-pulse': { type: 'dragon', category: 'special', power: 85, accuracy: 100 },
            'draco-meteor': { type: 'dragon', category: 'special', power: 130, accuracy: 90, spaDrop: 2 },
            'flash-cannon': { type: 'steel', category: 'special', power: 80, accuracy: 100 },
            'sludge-bomb': { type: 'poison', category: 'special', power: 90, accuracy: 100 },
            'focus-blast': { type: 'fighting', category: 'special', power: 120, accuracy: 70 },
            'aura-sphere': { type: 'fighting', category: 'special', power: 80, accuracy: true },
            'moonblast': { type: 'fairy', category: 'special', power: 95, accuracy: 100 },
            'dazzling-gleam': { type: 'fairy', category: 'special', power: 80, accuracy: 100 },
            'air-slash': { type: 'flying', category: 'special', power: 75, accuracy: 95 },
            'hurricane': { type: 'flying', category: 'special', power: 110, accuracy: 70, rainAccuracy: 100 },
            'bug-buzz': { type: 'bug', category: 'special', power: 90, accuracy: 100 },
            'power-gem': { type: 'rock', category: 'special', power: 80, accuracy: 100 },
            'earth-power': { type: 'ground', category: 'special', power: 90, accuracy: 100 },
            'hyper-beam': { type: 'normal', category: 'special', power: 150, accuracy: 90, recharge: true },
            'giga-impact': { type: 'normal', category: 'physical', power: 150, accuracy: 90, recharge: true },
            'boomburst': { type: 'normal', category: 'special', power: 140, accuracy: 100 },
            'hyper-voice': { type: 'normal', category: 'special', power: 90, accuracy: 100, sound: true },

            // Signature/Special moves
            'blue-flare': { type: 'fire', category: 'special', power: 130, accuracy: 85 },
            'bolt-strike': { type: 'electric', category: 'physical', power: 130, accuracy: 85 },
            'fusion-flare': { type: 'fire', category: 'special', power: 100, accuracy: 100 },
            'fusion-bolt': { type: 'electric', category: 'physical', power: 100, accuracy: 100 },
            'origin-pulse': { type: 'water', category: 'special', power: 110, accuracy: 85 },
            'precipice-blades': { type: 'ground', category: 'physical', power: 120, accuracy: 85 },
            'dragon-ascent': { type: 'flying', category: 'physical', power: 120, accuracy: 100 },
            'photon-geyser': { type: 'psychic', category: 'special', power: 100, accuracy: 100, usesHigherOffense: true },
            'sunsteel-strike': { type: 'steel', category: 'physical', power: 100, accuracy: 100, ignoresAbility: true },
            'moongeist-beam': { type: 'ghost', category: 'special', power: 100, accuracy: 100, ignoresAbility: true },
            'behemoth-blade': { type: 'steel', category: 'physical', power: 100, accuracy: 100, doubleDynamax: true },
            'behemoth-bash': { type: 'steel', category: 'physical', power: 100, accuracy: 100, doubleDynamax: true },
            'astral-barrage': { type: 'ghost', category: 'special', power: 120, accuracy: 100 },
            'glacial-lance': { type: 'ice', category: 'physical', power: 120, accuracy: 100 },
            'tera-blast': { type: 'normal', category: 'special', power: 80, accuracy: 100, teraType: true }
        };

        // Ability effects on damage
        this.abilityEffects = {
            // Offensive abilities
            'huge-power': { atkMod: 2 },
            'pure-power': { atkMod: 2 },
            'gorilla-tactics': { atkMod: 1.5 },
            'hustle': { atkMod: 1.5, accMod: 0.8 },
            'guts': { atkModOnStatus: 1.5 },
            'toxic-boost': { atkModOnPoison: 1.5 },
            'flare-boost': { spaModOnBurn: 1.5 },
            'solar-power': { spaModInSun: 1.5 },
            'blaze': { fireModLowHP: 1.5 },
            'torrent': { waterModLowHP: 1.5 },
            'overgrow': { grassModLowHP: 1.5 },
            'swarm': { bugModLowHP: 1.5 },
            'technician': { weakMoveMod: 1.5, threshold: 60 },
            'tough-claws': { contactMod: 1.3 },
            'iron-fist': { punchMod: 1.2 },
            'strong-jaw': { biteMod: 1.5 },
            'mega-launcher': { pulseMod: 1.5 },
            'sheer-force': { removeSecondary: 1.3 },
            'adaptability': { stabMod: 2 },
            'aerilate': { normalToFlying: true, mod: 1.2 },
            'pixilate': { normalToFairy: true, mod: 1.2 },
            'refrigerate': { normalToIce: true, mod: 1.2 },
            'galvanize': { normalToElectric: true, mod: 1.2 },
            'normalize': { allToNormal: true },
            'sand-force': { sandMod: 1.3, types: ['rock', 'ground', 'steel'] },
            'analytic': { lastMod: 1.3 },
            'rivalry': { sameGenderMod: 1.25, oppGenderMod: 0.75 },
            'reckless': { recoilMod: 1.2 },
            'sniper': { critMod: 1.5 },
            'tinted-lens': { resistedMod: 2 },
            'neuroforce': { superEffectiveMod: 1.25 },

            // Defensive abilities
            'multiscale': { fullHPMod: 0.5 },
            'shadow-shield': { fullHPMod: 0.5 },
            'solid-rock': { superEffectiveMod: 0.75 },
            'filter': { superEffectiveMod: 0.75 },
            'prism-armor': { superEffectiveMod: 0.75 },
            'fur-coat': { physicalMod: 0.5 },
            'marvel-scale': { defModOnStatus: 1.5 },
            'ice-scales': { specialMod: 0.5 },
            'fluffy': { contactMod: 0.5, fireMod: 2 },
            'thick-fat': { fireIceMod: 0.5 },
            'heatproof': { fireMod: 0.5 },
            'water-bubble': { fireMod: 0.5, waterAtk: 2 },
            'dry-skin': { waterImmune: true, fireMod: 1.25 },
            'levitate': { groundImmune: true },
            'flash-fire': { fireImmune: true, fireBoost: true },
            'lightning-rod': { electricImmune: true, spaBoost: true },
            'motor-drive': { electricImmune: true, speBoost: true },
            'volt-absorb': { electricImmune: true, heal: true },
            'water-absorb': { waterImmune: true, heal: true },
            'sap-sipper': { grassImmune: true, atkBoost: true },
            'storm-drain': { waterImmune: true, spaBoost: true },
            'wonder-guard': { onlySuperEffective: true }
        };

        // Item effects
        this.itemEffects = {
            'choice-band': { atkMod: 1.5 },
            'choice-specs': { spaMod: 1.5 },
            'choice-scarf': { speMod: 1.5 },
            'life-orb': { damageMod: 1.3, recoil: 0.1 },
            'expert-belt': { superEffectiveMod: 1.2 },
            'muscle-band': { physicalMod: 1.1 },
            'wise-glasses': { specialMod: 1.1 },
            'assault-vest': { spdMod: 1.5 },
            'eviolite': { defSpdMod: 1.5, notFullyEvolved: true },
            'rocky-helmet': { contactDamage: 0.167 },
            'leftovers': { endTurnHeal: 0.0625 },
            'black-sludge': { poisonHeal: 0.0625, nonPoisonDamage: 0.0625 },
            'type-plates': { typeMod: 1.2 },
            'type-gems': { typeMod: 1.5, oneUse: true },
            'metronome': { repeatMod: [1, 1.2, 1.4, 1.6, 1.8, 2] },
            'punching-glove': { punchMod: 1.1, noContact: true }
        };
    }

    /**
     * Calculate stat from base, IV, EV, level, and nature
     */
    calculateStat(base, iv, ev, level, nature, statName, isHP = false) {
        if (isHP) {
            if (base === 1) return 1; // Shedinja
            return Math.floor(((2 * base + iv + Math.floor(ev / 4)) * level) / 100) + level + 10;
        }

        let natureMod = 1;
        if (this.natures[nature]) {
            if (this.natures[nature][statName] === 1.1) natureMod = 1.1;
            else if (this.natures[nature][statName] === 0.9) natureMod = 0.9;
        }

        return Math.floor((Math.floor(((2 * base + iv + Math.floor(ev / 4)) * level) / 100) + 5) * natureMod);
    }

    /**
     * Get type effectiveness multiplier
     */
    getTypeEffectiveness(attackType, defenderTypes) {
        let multiplier = 1;
        for (const defType of defenderTypes) {
            if (this.typeChart[attackType] && this.typeChart[attackType][defType] !== undefined) {
                multiplier *= this.typeChart[attackType][defType];
            }
        }
        return multiplier;
    }

    /**
     * Calculate damage
     * @param {Object} attacker - Attacking Pokemon
     * @param {Object} defender - Defending Pokemon
     * @param {Object} move - Move being used
     * @param {Object} options - Battle conditions
     */
    calculateDamage(attacker, defender, move, options = {}) {
        const {
            weather = null,
            terrain = null,
            criticalHit = false,
            screens = { reflect: false, lightScreen: false, auroraVeil: false },
            statBoosts = { attacker: {}, defender: {} },
            isBurned = false,
            isCharged = false, // For Electric moves
            helpingHand = false,
            friendGuard = false,
            doubles = false
        } = options;

        // Get move data
        const moveData = typeof move === 'string' ? this.moves[move.toLowerCase()] : move;
        if (!moveData) {
            return { error: `Move "${move}" not found` };
        }

        // Determine attacking and defending stats
        const isPhysical = moveData.category === 'physical';
        const usesTargetDef = moveData.targetsDef || isPhysical;

        let attackStat = isPhysical ? 'atk' : 'spa';
        let defenseStat = usesTargetDef ? 'def' : 'spd';

        // Calculate actual stats
        const attackerAtk = this.calculateStat(
            attacker.baseStats?.[attackStat] || attacker.stats?.[attackStat] || 100,
            attacker.ivs?.[attackStat] || 31,
            attacker.evs?.[attackStat] || 0,
            attacker.level || 100,
            attacker.nature || 'hardy',
            attackStat
        );

        const defenderDef = this.calculateStat(
            defender.baseStats?.[defenseStat] || defender.stats?.[defenseStat] || 100,
            defender.ivs?.[defenseStat] || 31,
            defender.evs?.[defenseStat] || 0,
            defender.level || 100,
            defender.nature || 'hardy',
            defenseStat
        );

        // Apply stat boosts
        const atkBoost = statBoosts.attacker?.[attackStat] || 0;
        const defBoost = statBoosts.defender?.[defenseStat] || 0;

        const atkMultiplier = atkBoost >= 0 ? (2 + atkBoost) / 2 : 2 / (2 - atkBoost);
        const defMultiplier = defBoost >= 0 ? (2 + defBoost) / 2 : 2 / (2 - defBoost);

        const effectiveAtk = Math.floor(attackerAtk * (criticalHit && atkBoost < 0 ? 1 : atkMultiplier));
        const effectiveDef = Math.floor(defenderDef * (criticalHit && defBoost > 0 ? 1 : defMultiplier));

        // Get move type (may be modified by abilities)
        let moveType = moveData.type;
        const attackerAbility = attacker.ability?.toLowerCase();

        if (attackerAbility === 'aerilate' && moveType === 'normal') moveType = 'flying';
        else if (attackerAbility === 'pixilate' && moveType === 'normal') moveType = 'fairy';
        else if (attackerAbility === 'refrigerate' && moveType === 'normal') moveType = 'ice';
        else if (attackerAbility === 'galvanize' && moveType === 'normal') moveType = 'electric';

        // Calculate base damage
        const level = attacker.level || 100;
        const basePower = moveData.power;

        let baseDamage = Math.floor(Math.floor(Math.floor(2 * level / 5 + 2) * basePower * effectiveAtk / effectiveDef) / 50) + 2;

        // Apply modifiers
        let modifier = 1;

        // Spread move modifier (doubles)
        if (doubles && moveData.spread) {
            modifier *= 0.75;
        }

        // Weather
        if (weather) {
            if (weather === 'sun' && moveType === 'fire') modifier *= 1.5;
            else if (weather === 'sun' && moveType === 'water') modifier *= 0.5;
            else if (weather === 'rain' && moveType === 'water') modifier *= 1.5;
            else if (weather === 'rain' && moveType === 'fire') modifier *= 0.5;
        }

        // Terrain
        if (terrain && !attacker.isAirborne) {
            if (terrain === 'electric' && moveType === 'electric') modifier *= 1.3;
            else if (terrain === 'grassy' && moveType === 'grass') modifier *= 1.3;
            else if (terrain === 'psychic' && moveType === 'psychic') modifier *= 1.3;
            else if (terrain === 'misty' && moveType === 'dragon') modifier *= 0.5;
        }

        // Critical hit
        if (criticalHit) {
            modifier *= 1.5;
        }

        // STAB
        const attackerTypes = attacker.types || [attacker.type1, attacker.type2].filter(Boolean);
        let stabMultiplier = 1;
        if (attackerTypes.includes(moveType)) {
            stabMultiplier = attackerAbility === 'adaptability' ? 2 : 1.5;
        }
        modifier *= stabMultiplier;

        // Type effectiveness
        const defenderTypes = defender.types || [defender.type1, defender.type2].filter(Boolean);
        const effectiveness = this.getTypeEffectiveness(moveType, defenderTypes);
        modifier *= effectiveness;

        // Burn (physical moves only, except Facade and Guts)
        if (isBurned && isPhysical && attackerAbility !== 'guts' && moveData.name !== 'facade') {
            modifier *= 0.5;
        }

        // Screens
        if (!criticalHit) {
            if (isPhysical && (screens.reflect || screens.auroraVeil)) {
                modifier *= doubles ? 0.667 : 0.5;
            }
            if (!isPhysical && (screens.lightScreen || screens.auroraVeil)) {
                modifier *= doubles ? 0.667 : 0.5;
            }
        }

        // Ability modifiers
        if (attackerAbility) {
            const abilityEffect = this.abilityEffects[attackerAbility];
            if (abilityEffect) {
                if (abilityEffect.atkMod && isPhysical) modifier *= abilityEffect.atkMod;
                if (abilityEffect.spaMod && !isPhysical) modifier *= abilityEffect.spaMod;
                if (abilityEffect.contactMod && moveData.contact) modifier *= abilityEffect.contactMod;
                if (abilityEffect.weakMoveMod && basePower <= abilityEffect.threshold) modifier *= abilityEffect.weakMoveMod;
            }
        }

        // Item modifiers
        const attackerItem = attacker.item?.toLowerCase();
        if (attackerItem) {
            if (attackerItem === 'choice-band' && isPhysical) modifier *= 1.5;
            if (attackerItem === 'choice-specs' && !isPhysical) modifier *= 1.5;
            if (attackerItem === 'life-orb') modifier *= 1.3;
            if (attackerItem === 'expert-belt' && effectiveness > 1) modifier *= 1.2;
        }

        // Helping Hand
        if (helpingHand) modifier *= 1.5;

        // Friend Guard
        if (friendGuard) modifier *= 0.75;

        // Calculate damage range (random factor 0.85 to 1.00)
        const minDamage = Math.floor(baseDamage * modifier * 0.85);
        const maxDamage = Math.floor(baseDamage * modifier);

        // Calculate percentage damage
        const defenderHP = this.calculateStat(
            defender.baseStats?.hp || defender.stats?.hp || 100,
            defender.ivs?.hp || 31,
            defender.evs?.hp || 0,
            defender.level || 100,
            defender.nature || 'hardy',
            'hp',
            true
        );

        const minPercent = (minDamage / defenderHP * 100).toFixed(1);
        const maxPercent = (maxDamage / defenderHP * 100).toFixed(1);

        // Determine KO chance
        let koChance = 'No KO';
        if (minDamage >= defenderHP) {
            koChance = 'Guaranteed OHKO';
        } else if (maxDamage >= defenderHP) {
            const rolls = 16;
            let ohkoRolls = 0;
            for (let i = 0; i < rolls; i++) {
                const roll = Math.floor(baseDamage * modifier * (0.85 + i * 0.01));
                if (roll >= defenderHP) ohkoRolls++;
            }
            koChance = `${Math.round(ohkoRolls / rolls * 100)}% chance to OHKO`;
        } else if (maxDamage * 2 >= defenderHP) {
            koChance = 'Possible 2HKO';
        } else if (maxDamage * 3 >= defenderHP) {
            koChance = 'Possible 3HKO';
        }

        return {
            move: moveData.name || move,
            moveType,
            category: moveData.category,
            basePower,
            damage: { min: minDamage, max: maxDamage },
            percentage: { min: parseFloat(minPercent), max: parseFloat(maxPercent) },
            effectiveness,
            effectivenessText: this.getEffectivenessText(effectiveness),
            criticalHit,
            stab: stabMultiplier > 1,
            koChance,
            defenderHP,
            details: {
                attackStat: effectiveAtk,
                defenseStat: effectiveDef,
                weather,
                terrain,
                screens,
                burned: isBurned
            }
        };
    }

    /**
     * Get effectiveness description
     */
    getEffectivenessText(effectiveness) {
        if (effectiveness === 0) return 'No effect';
        if (effectiveness < 0.5) return 'Barely effective (0.25x)';
        if (effectiveness < 1) return 'Not very effective (0.5x)';
        if (effectiveness === 1) return 'Normal effectiveness';
        if (effectiveness < 4) return 'Super effective (2x)';
        return 'Super effective (4x)';
    }

    /**
     * Calculate all moves damage
     */
    calculateAllMoves(attacker, defender, options = {}) {
        const results = [];
        const moves = attacker.moves || [];

        for (const move of moves) {
            const result = this.calculateDamage(attacker, defender, move, options);
            if (!result.error) {
                results.push(result);
            }
        }

        return results.sort((a, b) => b.damage.max - a.damage.max);
    }

    /**
     * Find best move against defender
     */
    findBestMove(attacker, defender, options = {}) {
        const results = this.calculateAllMoves(attacker, defender, options);
        return results[0] || null;
    }

    /**
     * Calculate speed comparison
     */
    calculateSpeed(pokemon, options = {}) {
        const { weather, terrain, statBoost = 0, paralyzed = false, tailwind = false } = options;

        let speed = this.calculateStat(
            pokemon.baseStats?.spe || pokemon.stats?.spe || 100,
            pokemon.ivs?.spe || 31,
            pokemon.evs?.spe || 0,
            pokemon.level || 100,
            pokemon.nature || 'hardy',
            'spe'
        );

        // Stat boost
        const speedMultiplier = statBoost >= 0 ? (2 + statBoost) / 2 : 2 / (2 - statBoost);
        speed = Math.floor(speed * speedMultiplier);

        // Item
        const item = pokemon.item?.toLowerCase();
        if (item === 'choice-scarf') speed = Math.floor(speed * 1.5);
        if (item === 'iron-ball') speed = Math.floor(speed * 0.5);

        // Ability
        const ability = pokemon.ability?.toLowerCase();
        if (ability === 'chlorophyll' && weather === 'sun') speed *= 2;
        if (ability === 'swift-swim' && weather === 'rain') speed *= 2;
        if (ability === 'sand-rush' && weather === 'sand') speed *= 2;
        if (ability === 'slush-rush' && weather === 'hail') speed *= 2;
        if (ability === 'surge-surfer' && terrain === 'electric') speed *= 2;
        if (ability === 'unburden' && pokemon.itemConsumed) speed *= 2;

        // Paralysis
        if (paralyzed && ability !== 'quick-feet') speed = Math.floor(speed * 0.5);

        // Tailwind
        if (tailwind) speed *= 2;

        return Math.floor(speed);
    }

    /**
     * Compare speeds
     */
    compareSpeed(pokemon1, pokemon2, options1 = {}, options2 = {}) {
        const speed1 = this.calculateSpeed(pokemon1, options1);
        const speed2 = this.calculateSpeed(pokemon2, options2);

        return {
            pokemon1: { name: pokemon1.name || pokemon1.species, speed: speed1 },
            pokemon2: { name: pokemon2.name || pokemon2.species, speed: speed2 },
            faster: speed1 > speed2 ? pokemon1.name || pokemon1.species :
                    speed2 > speed1 ? pokemon2.name || pokemon2.species : 'Speed tie',
            difference: Math.abs(speed1 - speed2)
        };
    }
}

module.exports = PokemonDamageCalculator;
