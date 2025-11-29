/**
 * Pokemon Breeding Calculator
 * IV inheritance, egg moves, nature passing, ability breeding, gender ratios
 * Based on Gen 8/9 breeding mechanics
 */

class PokemonBreedingCalculator {
    constructor() {
        // Gender ratios (male:female)
        this.genderRatios = {
            '100:0': ['nidoran-m', 'tauros', 'tyrogue', 'volbeat', 'throh', 'sawk', 'rufflet', 'braviary', 'impidimp', 'morgrem', 'grimmsnarl'],
            '0:100': ['nidoran-f', 'chansey', 'kangaskhan', 'jynx', 'miltank', 'illumise', 'happiny', 'blissey', 'petilil', 'lilligant', 'vullaby', 'mandibuzz', 'flabebe', 'floette', 'florges', 'bounsweet', 'steenee', 'tsareena', 'hatenna', 'hattrem', 'hatterene', 'milcery', 'alcremie', 'tinkatink', 'tinkatuff', 'tinkaton'],
            '87.5:12.5': ['bulbasaur', 'charmander', 'squirtle', 'chikorita', 'cyndaquil', 'totodile', 'treecko', 'torchic', 'mudkip', 'turtwig', 'chimchar', 'piplup', 'snivy', 'tepig', 'oshawott', 'chespin', 'fennekin', 'froakie', 'rowlet', 'litten', 'popplio', 'grookey', 'scorbunny', 'sobble', 'sprigatito', 'fuecoco', 'quaxly', 'eevee', 'amaura', 'tyrunt', 'omanyte', 'kabuto', 'lileep', 'anorith', 'cranidos', 'shieldon', 'tirtouga', 'archen'],
            '75:25': ['machop', 'abra', 'gastly', 'hitmonlee', 'hitmonchan', 'elekid', 'magby', 'riolu', 'timburr'],
            '50:50': 'default',
            '25:75': ['vulpix', 'clefairy', 'jigglypuff', 'skitty', 'buneary', 'glameow', 'smoochum', 'gothita', 'bounsweet'],
            'genderless': ['magnemite', 'magneton', 'magnezone', 'voltorb', 'electrode', 'staryu', 'starmie', 'porygon', 'porygon2', 'porygon-z', 'ditto', 'unown', 'baltoy', 'claydol', 'beldum', 'metang', 'metagross', 'bronzor', 'bronzong', 'rotom', 'klink', 'klang', 'klinklang', 'cryogonal', 'golett', 'golurk', 'carbink', 'minior', 'dhelmise', 'sinistea', 'polteageist', 'falinks']
        };

        // Egg groups
        this.eggGroups = {
            'monster': ['bulbasaur', 'charmander', 'squirtle', 'nidoran-f', 'nidoran-m', 'cubone', 'marowak', 'kangaskhan', 'larvitar', 'treecko', 'mudkip', 'aron', 'bagon', 'cranidos', 'gible', 'axew', 'tyrunt', 'jangmo-o', 'dreepy'],
            'water1': ['squirtle', 'psyduck', 'poliwag', 'slowpoke', 'seel', 'horsea', 'goldeen', 'staryu', 'totodile', 'marill', 'wooper', 'mudkip', 'lotad', 'corphish', 'feebas', 'spheal', 'piplup', 'buizel', 'shellos', 'oshawott', 'tympole', 'popplio', 'sobble', 'quaxly'],
            'water2': ['goldeen', 'magikarp', 'chinchou', 'remoraid', 'carvanha', 'wailmer', 'barboach', 'relicanth', 'luvdisc', 'finneon', 'basculin', 'alomomola', 'wishiwashi', 'arrokuda', 'veluza'],
            'water3': ['omanyte', 'kabuto', 'tentacool', 'shellder', 'krabby', 'corsola', 'clamperl', 'anorith', 'tirtouga', 'binacle', 'mareanie', 'wimpod', 'clobbopus'],
            'bug': ['caterpie', 'weedle', 'paras', 'venonat', 'scyther', 'pinsir', 'ledyba', 'spinarak', 'yanma', 'pineco', 'heracross', 'wurmple', 'surskit', 'nincada', 'volbeat', 'illumise', 'kricketot', 'combee', 'skorupi', 'venipede', 'dwebble', 'karrablast', 'shelmet', 'joltik', 'larvesta', 'scatterbug', 'grubbin', 'cutiefly', 'dewpider', 'wimpod', 'blipbug', 'sizzlipede', 'nymble', 'tarountula', 'rellor'],
            'flying': ['pidgey', 'spearow', 'farfetchd', 'doduo', 'hoothoot', 'togetic', 'natu', 'murkrow', 'delibird', 'wingull', 'taillow', 'swablu', 'starly', 'chatot', 'pidove', 'woobat', 'ducklett', 'rufflet', 'vullaby', 'fletchling', 'hawlucha', 'noibat', 'pikipek', 'oricorio', 'rookidee', 'cramorant', 'wattrel', 'flamigo', 'bombirdier', 'squawkabilly'],
            'field': ['rattata', 'pikachu', 'sandshrew', 'vulpix', 'growlithe', 'ponyta', 'eevee', 'cyndaquil', 'sentret', 'mareep', 'girafarig', 'dunsparce', 'snubbull', 'houndour', 'phanpy', 'stantler', 'zigzagoon', 'poochyena', 'skitty', 'electrike', 'numel', 'torkoal', 'spinda', 'absol', 'shinx', 'pachirisu', 'buneary', 'glameow', 'stunky', 'hippopotas', 'lillipup', 'purrloin', 'munna', 'blitzle', 'drilbur', 'minccino', 'deerling', 'litleo', 'espurr', 'furfrou', 'skiddo', 'stufful', 'rockruff', 'mudbray', 'yamper', 'wooloo', 'nickit', 'tandemaus', 'fidough', 'maschiff', 'shroodle', 'pawmi', 'lechonk', 'capsakid', 'greavard'],
            'fairy': ['clefairy', 'jigglypuff', 'pikachu', 'togepi', 'marill', 'snubbull', 'roselia', 'mawile', 'plusle', 'minun', 'pachirisu', 'cherubi', 'mime-jr', 'cottonee', 'petilil', 'flabebe', 'spritzee', 'swirlix', 'dedenne', 'klefki', 'carbink', 'comfey', 'mimikyu', 'hatenna', 'impidimp', 'milcery', 'alcremie', 'fidough', 'tinkatink'],
            'grass': ['bulbasaur', 'oddish', 'bellsprout', 'exeggcute', 'tangela', 'chikorita', 'hoppip', 'sunkern', 'treecko', 'lotad', 'seedot', 'shroomish', 'roselia', 'cacnea', 'tropius', 'turtwig', 'budew', 'cherubi', 'carnivine', 'snover', 'snivy', 'pansage', 'sewaddle', 'petilil', 'maractus', 'foongus', 'ferroseed', 'chespin', 'skiddo', 'phantump', 'pumpkaboo', 'rowlet', 'fomantis', 'bounsweet', 'comfey', 'grookey', 'gossifleur', 'applin', 'sprigatito', 'smoliv', 'bramblin', 'capsakid', 'toedscool'],
            'human-like': ['abra', 'machop', 'drowzee', 'mr-mime', 'jynx', 'electabuzz', 'magmar', 'tyrogue', 'smoochum', 'elekid', 'magby', 'makuhita', 'sableye', 'meditite', 'volbeat', 'illumise', 'spinda', 'cacnea', 'chimchar', 'buneary', 'lucario', 'croagunk', 'timburr', 'throh', 'sawk', 'scraggy', 'gothita', 'mienfoo', 'golett', 'pancham', 'hawlucha', 'stufful', 'passimian', 'toxel', 'impidimp', 'clobbopus', 'hatenna', 'riolu'],
            'mineral': ['geodude', 'onix', 'voltorb', 'rhyhorn', 'sudowoodo', 'slugma', 'corsola', 'nosepass', 'aron', 'lunatone', 'solrock', 'baltoy', 'lileep', 'anorith', 'relicanth', 'bonsly', 'bronzor', 'roggenrola', 'dwebble', 'trubbish', 'ferroseed', 'carbink', 'phantump', 'bergmite', 'rockruff', 'minior', 'rolycoly', 'stonjourner', 'nacli', 'klawf', 'glimmet'],
            'amorphous': ['grimer', 'gastly', 'koffing', 'misdreavus', 'slugma', 'shuppet', 'duskull', 'castform', 'chimecho', 'drifloon', 'stunky', 'spiritomb', 'yamask', 'frillish', 'litwick', 'phantump', 'pumpkaboo', 'honedge', 'inkay', 'espurr', 'sandygast', 'mimikyu', 'sinistea', 'dreepy', 'greavard', 'bramblin', 'gimmighoul'],
            'dragon': ['charmander', 'ekans', 'horsea', 'magikarp', 'dratini', 'treecko', 'swablu', 'seviper', 'feebas', 'bagon', 'gible', 'druddigon', 'deino', 'goomy', 'noibat', 'turtonator', 'drampa', 'jangmo-o', 'dreepy', 'applin', 'duraludon', 'frigibax', 'tatsugiri', 'cyclizar'],
            'undiscovered': ['baby', 'legendary', 'mythical', 'ultra-beast', 'paradox']
        };

        // Natures and their effects
        this.natures = {
            hardy: { plus: null, minus: null },
            lonely: { plus: 'atk', minus: 'def' },
            brave: { plus: 'atk', minus: 'spe' },
            adamant: { plus: 'atk', minus: 'spa' },
            naughty: { plus: 'atk', minus: 'spd' },
            bold: { plus: 'def', minus: 'atk' },
            docile: { plus: null, minus: null },
            relaxed: { plus: 'def', minus: 'spe' },
            impish: { plus: 'def', minus: 'spa' },
            lax: { plus: 'def', minus: 'spd' },
            timid: { plus: 'spe', minus: 'atk' },
            hasty: { plus: 'spe', minus: 'def' },
            serious: { plus: null, minus: null },
            jolly: { plus: 'spe', minus: 'spa' },
            naive: { plus: 'spe', minus: 'spd' },
            modest: { plus: 'spa', minus: 'atk' },
            mild: { plus: 'spa', minus: 'def' },
            quiet: { plus: 'spa', minus: 'spe' },
            bashful: { plus: null, minus: null },
            rash: { plus: 'spa', minus: 'spd' },
            calm: { plus: 'spd', minus: 'atk' },
            gentle: { plus: 'spd', minus: 'def' },
            sassy: { plus: 'spd', minus: 'spe' },
            careful: { plus: 'spd', minus: 'spa' },
            quirky: { plus: null, minus: null }
        };

        // Ability inheritance rules
        this.abilitySlots = {
            slot1: 0.8, // 80% chance to pass slot 1
            slot2: 0.8, // 80% chance to pass slot 2
            hidden: 0.6  // 60% chance to pass HA (female in normal breeding, either parent with Ditto)
        };

        // Items that affect breeding
        this.breedingItems = {
            'destiny-knot': { effect: 'passDownFiveIVs' },
            'everstone': { effect: 'passNature' },
            'power-weight': { stat: 'hp', effect: 'guaranteeIV' },
            'power-bracer': { stat: 'atk', effect: 'guaranteeIV' },
            'power-belt': { stat: 'def', effect: 'guaranteeIV' },
            'power-lens': { stat: 'spa', effect: 'guaranteeIV' },
            'power-band': { stat: 'spd', effect: 'guaranteeIV' },
            'power-anklet': { stat: 'spe', effect: 'guaranteeIV' },
            'light-ball': { effect: 'voltTacklePichu' },
            'luck-incense': { effect: 'happinyEgg' },
            'full-incense': { effect: 'munchlaxEgg' },
            'lax-incense': { effect: 'wynautEgg' },
            'odd-incense': { effect: 'mimeJrEgg' },
            'pure-incense': { effect: 'chimechoEgg' },
            'rock-incense': { effect: 'bonslyEgg' },
            'rose-incense': { effect: 'budewEgg' },
            'sea-incense': { effect: 'azurillEgg' },
            'wave-incense': { effect: 'mantykeEgg' }
        };

        // Egg move inheritance
        this.eggMoveRules = {
            gen8Plus: 'Both parents can pass egg moves',
            gen6to7: 'Only father passes egg moves (or same-species transfer)',
            gen5Below: 'Only father passes egg moves'
        };
    }

    /**
     * Check if two Pokemon can breed
     */
    canBreed(parent1, parent2) {
        const result = {
            canBreed: false,
            reason: '',
            notes: []
        };

        // Check if either is Ditto
        const hasDitto = parent1.species?.toLowerCase() === 'ditto' || parent2.species?.toLowerCase() === 'ditto';

        // Check undiscovered egg group
        const undiscoveredSpecies = ['mew', 'mewtwo', 'lugia', 'ho-oh', 'celebi', 'kyogre', 'groudon', 'rayquaza', 'jirachi', 'deoxys', 'dialga', 'palkia', 'giratina', 'arceus', 'victini', 'reshiram', 'zekrom', 'kyurem', 'keldeo', 'meloetta', 'genesect', 'xerneas', 'yveltal', 'zygarde', 'diancie', 'hoopa', 'volcanion', 'cosmog', 'cosmoem', 'solgaleo', 'lunala', 'necrozma', 'magearna', 'marshadow', 'zeraora', 'zacian', 'zamazenta', 'eternatus', 'zarude', 'regieleki', 'regidrago', 'glastrier', 'spectrier', 'calyrex', 'koraidon', 'miraidon'];

        if (undiscoveredSpecies.includes(parent1.species?.toLowerCase()) ||
            undiscoveredSpecies.includes(parent2.species?.toLowerCase())) {
            result.reason = 'One or both Pokemon are in the Undiscovered egg group (legendaries/mythicals cannot breed)';
            return result;
        }

        // Both are Ditto
        if (parent1.species?.toLowerCase() === 'ditto' && parent2.species?.toLowerCase() === 'ditto') {
            result.reason = 'Two Ditto cannot breed together';
            return result;
        }

        // Check genderless (must breed with Ditto)
        const genderless = this.genderRatios.genderless;
        const parent1Genderless = genderless.includes(parent1.species?.toLowerCase());
        const parent2Genderless = genderless.includes(parent2.species?.toLowerCase());

        if ((parent1Genderless || parent2Genderless) && !hasDitto) {
            result.reason = 'Genderless Pokemon can only breed with Ditto';
            return result;
        }

        // Check compatible egg groups
        if (!hasDitto) {
            const parent1Groups = this.getEggGroups(parent1.species);
            const parent2Groups = this.getEggGroups(parent2.species);

            const hasCommonGroup = parent1Groups.some(g => parent2Groups.includes(g));
            if (!hasCommonGroup) {
                result.reason = `No compatible egg groups: ${parent1.species} (${parent1Groups.join('/')}) and ${parent2.species} (${parent2Groups.join('/')})`;
                return result;
            }

            // Check gender compatibility
            const parent1Gender = parent1.gender?.toLowerCase();
            const parent2Gender = parent2.gender?.toLowerCase();

            if (parent1Gender === parent2Gender && parent1Gender !== undefined) {
                result.reason = 'Same gender Pokemon cannot breed (need male and female)';
                return result;
            }
        }

        result.canBreed = true;
        result.reason = 'Compatible for breeding';

        // Add notes
        if (hasDitto) {
            result.notes.push('Ditto breeding: offspring will be the non-Ditto parent\'s species');
            result.notes.push('Hidden Ability can be passed from either parent when breeding with Ditto');
        }

        return result;
    }

    /**
     * Get egg groups for a Pokemon
     */
    getEggGroups(species) {
        const speciesLower = species?.toLowerCase();
        const groups = [];

        for (const [group, members] of Object.entries(this.eggGroups)) {
            if (Array.isArray(members) && members.includes(speciesLower)) {
                groups.push(group);
            }
        }

        return groups.length > 0 ? groups : ['field']; // Default to field
    }

    /**
     * Calculate IV inheritance
     */
    calculateIVInheritance(parent1, parent2, options = {}) {
        const { destinyKnot = false, powerItem = null } = options;

        const stats = ['hp', 'atk', 'def', 'spa', 'spd', 'spe'];
        const result = {
            guaranteed: [],
            random: [],
            offspring: {}
        };

        // Determine number of IVs to inherit
        let ivsToInherit = destinyKnot ? 5 : 3;
        let guaranteedStats = [];

        // Power item guarantees specific stat
        if (powerItem) {
            const itemData = this.breedingItems[powerItem];
            if (itemData && itemData.stat) {
                guaranteedStats.push(itemData.stat);
                result.guaranteed.push({
                    stat: itemData.stat,
                    source: powerItem,
                    fromParent: 'holder'
                });
            }
        }

        // Select remaining stats to inherit
        const availableStats = stats.filter(s => !guaranteedStats.includes(s));
        const inheritCount = ivsToInherit - guaranteedStats.length;

        // Simulate inheritance
        for (const stat of stats) {
            if (guaranteedStats.includes(stat)) {
                // Inherited from power item holder
                const fromParent = Math.random() < 0.5 ? parent1 : parent2;
                result.offspring[stat] = fromParent.ivs?.[stat] || Math.floor(Math.random() * 32);
            } else if (result.guaranteed.length + result.random.length < ivsToInherit && Math.random() < 0.5) {
                // Randomly inherited
                const fromParent = Math.random() < 0.5 ? parent1 : parent2;
                result.offspring[stat] = fromParent.ivs?.[stat] || Math.floor(Math.random() * 32);
                result.random.push({ stat, fromParent: fromParent.name || fromParent.species });
            } else {
                // Random IV
                result.offspring[stat] = Math.floor(Math.random() * 32);
            }
        }

        return result;
    }

    /**
     * Calculate nature inheritance
     */
    calculateNatureInheritance(parent1, parent2, options = {}) {
        const { parent1Everstone = false, parent2Everstone = false } = options;

        if (parent1Everstone && parent2Everstone) {
            // Both have Everstone - 50/50 chance
            return {
                nature: Math.random() < 0.5 ? parent1.nature : parent2.nature,
                source: '50/50 between both Everstone holders'
            };
        } else if (parent1Everstone) {
            return {
                nature: parent1.nature,
                source: `${parent1.name || parent1.species} (Everstone)`
            };
        } else if (parent2Everstone) {
            return {
                nature: parent2.nature,
                source: `${parent2.name || parent2.species} (Everstone)`
            };
        } else {
            // Random nature
            const natures = Object.keys(this.natures);
            return {
                nature: natures[Math.floor(Math.random() * natures.length)],
                source: 'Random'
            };
        }
    }

    /**
     * Calculate ability inheritance
     */
    calculateAbilityInheritance(parent1, parent2, options = {}) {
        const { breedingWithDitto = false, femaleParent = null } = options;

        const result = {
            possibleAbilities: [],
            hiddenAbilityChance: 0,
            notes: []
        };

        // Determine which parent passes ability
        const abilityParent = breedingWithDitto ?
            (parent1.species?.toLowerCase() === 'ditto' ? parent2 : parent1) :
            (femaleParent || parent1);

        const hasHA = abilityParent.ability?.toLowerCase().includes('hidden') || abilityParent.hasHiddenAbility;

        if (hasHA) {
            result.hiddenAbilityChance = breedingWithDitto ? 0.6 : 0.6;
            result.possibleAbilities.push({
                ability: abilityParent.hiddenAbility || abilityParent.ability,
                chance: result.hiddenAbilityChance,
                type: 'Hidden Ability'
            });
            result.possibleAbilities.push({
                ability: 'Standard Ability',
                chance: 1 - result.hiddenAbilityChance,
                type: 'Normal Ability (Slot 1 or 2)'
            });
            result.notes.push('Hidden Ability has 60% chance to pass');
        } else {
            result.possibleAbilities.push({
                ability: abilityParent.ability || 'Standard Ability',
                chance: 0.8,
                type: 'Same ability slot'
            });
            result.possibleAbilities.push({
                ability: 'Other standard ability',
                chance: 0.2,
                type: 'Different ability slot'
            });
            result.notes.push('80% chance to pass same ability slot');
        }

        if (breedingWithDitto) {
            result.notes.push('When breeding with Ditto, the non-Ditto parent passes ability');
        } else {
            result.notes.push('Female parent passes ability in standard breeding');
        }

        return result;
    }

    /**
     * Calculate egg moves
     */
    calculateEggMoves(parent1, parent2, offspringSpecies, options = {}) {
        const { gen = 9 } = options;

        const result = {
            possibleMoves: [],
            inheritedFrom: [],
            notes: []
        };

        // Get parent moves
        const parent1Moves = parent1.moves || [];
        const parent2Moves = parent2.moves || [];

        // In Gen 8+, both parents can pass egg moves
        if (gen >= 8) {
            result.notes.push('Both parents can pass egg moves (Gen 8+ mechanics)');

            // Check which moves are egg moves for the offspring
            const allParentMoves = [...new Set([...parent1Moves, ...parent2Moves])];

            for (const move of allParentMoves) {
                result.possibleMoves.push({
                    move,
                    from: parent1Moves.includes(move) && parent2Moves.includes(move) ? 'Both parents' :
                          parent1Moves.includes(move) ? (parent1.name || parent1.species) : (parent2.name || parent2.species)
                });
            }

            result.notes.push('Gen 8+: Egg moves can also be transferred between same-species Pokemon at the daycare');
        } else {
            result.notes.push('Only the male/father passes egg moves (Gen 7 and earlier)');

            const fatherMoves = parent1.gender?.toLowerCase() === 'male' ? parent1Moves : parent2Moves;
            for (const move of fatherMoves) {
                result.possibleMoves.push({
                    move,
                    from: 'Father'
                });
            }
        }

        return result;
    }

    /**
     * Calculate offspring details
     */
    calculateOffspring(parent1, parent2, options = {}) {
        const breedingCheck = this.canBreed(parent1, parent2);
        if (!breedingCheck.canBreed) {
            return { error: breedingCheck.reason };
        }

        const isDittoBreeding = parent1.species?.toLowerCase() === 'ditto' || parent2.species?.toLowerCase() === 'ditto';
        const nonDittoParent = isDittoBreeding ?
            (parent1.species?.toLowerCase() === 'ditto' ? parent2 : parent1) : null;

        // Determine offspring species
        let offspringSpecies;
        if (isDittoBreeding) {
            offspringSpecies = nonDittoParent.species;
        } else {
            // Female determines species (usually)
            const femaleParent = parent1.gender?.toLowerCase() === 'female' ? parent1 : parent2;
            offspringSpecies = femaleParent.species;
        }

        // Calculate all inheritance
        const ivResult = this.calculateIVInheritance(parent1, parent2, {
            destinyKnot: options.destinyKnot,
            powerItem: options.powerItem
        });

        const natureResult = this.calculateNatureInheritance(parent1, parent2, {
            parent1Everstone: options.parent1Everstone,
            parent2Everstone: options.parent2Everstone
        });

        const abilityResult = this.calculateAbilityInheritance(parent1, parent2, {
            breedingWithDitto: isDittoBreeding,
            femaleParent: parent1.gender?.toLowerCase() === 'female' ? parent1 : parent2
        });

        const eggMoveResult = this.calculateEggMoves(parent1, parent2, offspringSpecies, {
            gen: options.gen || 9
        });

        // Calculate shiny odds
        const shinyOdds = this.calculateShinyOdds(options);

        return {
            species: offspringSpecies,
            level: 1,
            ivs: ivResult.offspring,
            ivInheritance: ivResult,
            nature: natureResult,
            ability: abilityResult,
            eggMoves: eggMoveResult,
            shinyOdds,
            ball: this.determineBall(parent1, parent2),
            eggCycles: this.getEggCycles(offspringSpecies),
            hatchSteps: this.getHatchSteps(offspringSpecies)
        };
    }

    /**
     * Calculate shiny odds
     */
    calculateShinyOdds(options = {}) {
        const { masudaMethod = false, shinyCharm = false } = options;

        let baseOdds = 1 / 4096;
        let rolls = 1;

        if (masudaMethod) rolls += 6;
        if (shinyCharm) rolls += 2;

        const effectiveOdds = 1 - Math.pow(1 - baseOdds, rolls);

        return {
            odds: `1/${Math.round(1 / effectiveOdds)}`,
            percentage: (effectiveOdds * 100).toFixed(4) + '%',
            masudaMethod,
            shinyCharm,
            rolls
        };
    }

    /**
     * Determine Poke Ball inheritance
     */
    determineBall(parent1, parent2) {
        const isDittoBreeding = parent1.species?.toLowerCase() === 'ditto' || parent2.species?.toLowerCase() === 'ditto';

        if (isDittoBreeding) {
            const nonDittoParent = parent1.species?.toLowerCase() === 'ditto' ? parent2 : parent1;
            return {
                ball: nonDittoParent.ball || 'poke-ball',
                source: `${nonDittoParent.name || nonDittoParent.species} (non-Ditto parent)`
            };
        }

        // Same species - 50/50
        if (parent1.species?.toLowerCase() === parent2.species?.toLowerCase()) {
            return {
                ball: Math.random() < 0.5 ? (parent1.ball || 'poke-ball') : (parent2.ball || 'poke-ball'),
                source: '50/50 between both parents (same species)'
            };
        }

        // Different species - female's ball
        const femaleParent = parent1.gender?.toLowerCase() === 'female' ? parent1 : parent2;
        return {
            ball: femaleParent.ball || 'poke-ball',
            source: `${femaleParent.name || femaleParent.species} (female parent)`
        };
    }

    /**
     * Get egg cycles for species
     */
    getEggCycles(species) {
        // Simplified - actual data would come from database
        const fastHatch = ['magikarp', 'pidgey', 'rattata', 'caterpie', 'weedle', 'zubat', 'geodude'];
        const slowHatch = ['dratini', 'larvitar', 'bagon', 'beldum', 'gible', 'deino', 'dreepy', 'eevee'];
        const verySlow = ['chansey', 'happiny', 'blissey', 'munchlax', 'snorlax'];

        const speciesLower = species?.toLowerCase();

        if (fastHatch.includes(speciesLower)) return 10;
        if (slowHatch.includes(speciesLower)) return 40;
        if (verySlow.includes(speciesLower)) return 60;
        return 20; // Default
    }

    /**
     * Get hatch steps
     */
    getHatchSteps(species) {
        const cycles = this.getEggCycles(species);
        return cycles * 257; // Each cycle is ~257 steps
    }

    /**
     * Find optimal parents for breeding target
     */
    findOptimalParents(targetIVs, availablePokemon) {
        const results = [];

        for (let i = 0; i < availablePokemon.length; i++) {
            for (let j = i + 1; j < availablePokemon.length; j++) {
                const parent1 = availablePokemon[i];
                const parent2 = availablePokemon[j];

                if (!this.canBreed(parent1, parent2).canBreed) continue;

                // Count matching perfect IVs
                let matchingIVs = 0;
                const stats = ['hp', 'atk', 'def', 'spa', 'spd', 'spe'];

                for (const stat of stats) {
                    const targetIV = targetIVs[stat];
                    if (targetIV !== undefined) {
                        if (parent1.ivs?.[stat] === targetIV || parent2.ivs?.[stat] === targetIV) {
                            matchingIVs++;
                        }
                    }
                }

                results.push({
                    parent1: parent1.name || parent1.species,
                    parent2: parent2.name || parent2.species,
                    matchingIVs,
                    coverage: (matchingIVs / Object.keys(targetIVs).length * 100).toFixed(1) + '%'
                });
            }
        }

        return results.sort((a, b) => b.matchingIVs - a.matchingIVs);
    }

    /**
     * Generate breeding guide
     */
    generateBreedingGuide(targetPokemon) {
        const guide = {
            target: targetPokemon,
            steps: [],
            tips: []
        };

        // Step 1: Get a Ditto
        guide.steps.push({
            step: 1,
            action: 'Obtain a Ditto with good IVs',
            details: '6IV Ditto is ideal for breeding any Pokemon',
            items: ['Destiny Knot (to pass 5 IVs)']
        });

        // Step 2: Get target species
        guide.steps.push({
            step: 2,
            action: `Obtain ${targetPokemon.species}`,
            details: targetPokemon.hasHiddenAbility ?
                'Make sure it has the Hidden Ability if desired' :
                'Any ability works',
            items: targetPokemon.hasHiddenAbility ? ['Ability Patch (if needed)'] : []
        });

        // Step 3: Nature
        if (targetPokemon.nature) {
            guide.steps.push({
                step: 3,
                action: `Get ${targetPokemon.nature} nature`,
                details: `Use Everstone on parent with ${targetPokemon.nature} nature`,
                items: ['Everstone']
            });
        }

        // Step 4: IVs
        guide.steps.push({
            step: 4,
            action: 'Breed for perfect IVs',
            details: 'Replace parent with better offspring each generation',
            items: ['Destiny Knot', 'Everstone', 'Power Items (optional)']
        });

        // Step 5: Egg moves
        if (targetPokemon.eggMoves && targetPokemon.eggMoves.length > 0) {
            guide.steps.push({
                step: 5,
                action: 'Get egg moves',
                details: `Need parent with: ${targetPokemon.eggMoves.join(', ')}`,
                items: []
            });
        }

        // Tips
        guide.tips = [
            'Use Flame Body/Magma Armor ability Pokemon in party to halve egg hatch time',
            'Oval Charm increases egg generation rate',
            'Shiny Charm + Masuda Method = ~1/512 shiny odds',
            'In Gen 8+, you can transfer egg moves between same-species Pokemon'
        ];

        return guide;
    }
}

module.exports = PokemonBreedingCalculator;
