using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

public partial class OptimalSetupRecommender : Form
{
    private readonly SaveFile SAV;
    private ComboBox cmbPokemon = null!;
    private ComboBox cmbRole = null!;
    private ComboBox cmbFormat = null!;
    private ListView lstRecommendations = null!;
    private Panel pnlNaturePreview = null!;
    private Panel pnlAbilityPreview = null!;
    private Panel pnlItemPreview = null!;
    private RichTextBox rtbExplanation = null!;
    private Button btnApplyRecommendation = null!;
    private Button btnAnalyze = null!;
    private ProgressBar pbOptimality = null!;
    private Label lblOptimalityScore = null!;

    // Role-based recommendations
    private static readonly Dictionary<string, RoleRecommendation> RoleRecommendations = new()
    {
        ["Physical Sweeper"] = new()
        {
            PreferredNatures = new[] { Nature.Jolly, Nature.Adamant },
            PreferredAbilities = new[] { "Moxie", "Technician", "Sheer Force", "Tough Claws", "Sword of Ruin" },
            PreferredItems = new[] { "Choice Band", "Life Orb", "Choice Scarf", "Focus Sash" },
            EVSpread = "252 Atk / 4 Def / 252 Spe",
            IVRequirements = "31/31/31/x/31/31"
        },
        ["Special Sweeper"] = new()
        {
            PreferredNatures = new[] { Nature.Timid, Nature.Modest },
            PreferredAbilities = new[] { "Protean", "Adaptability", "Beads of Ruin", "Transistor" },
            PreferredItems = new[] { "Choice Specs", "Life Orb", "Choice Scarf", "Expert Belt" },
            EVSpread = "252 SpA / 4 SpD / 252 Spe",
            IVRequirements = "31/x/31/31/31/31"
        },
        ["Physical Wall"] = new()
        {
            PreferredNatures = new[] { Nature.Impish, Nature.Bold },
            PreferredAbilities = new[] { "Regenerator", "Sturdy", "Unaware", "Iron Barbs" },
            PreferredItems = new[] { "Leftovers", "Rocky Helmet", "Heavy-Duty Boots", "Eviolite" },
            EVSpread = "252 HP / 252 Def / 4 SpD",
            IVRequirements = "31/31/31/x/31/31"
        },
        ["Special Wall"] = new()
        {
            PreferredNatures = new[] { Nature.Calm, Nature.Careful },
            PreferredAbilities = new[] { "Regenerator", "Natural Cure", "Thick Fat", "Storm Drain" },
            PreferredItems = new[] { "Leftovers", "Assault Vest", "Heavy-Duty Boots", "Black Sludge" },
            EVSpread = "252 HP / 4 Def / 252 SpD",
            IVRequirements = "31/x/31/31/31/31"
        },
        ["Support"] = new()
        {
            PreferredNatures = new[] { Nature.Bold, Nature.Calm, Nature.Impish },
            PreferredAbilities = new[] { "Intimidate", "Regenerator", "Prankster", "Effect Spore" },
            PreferredItems = new[] { "Sitrus Berry", "Safety Goggles", "Light Clay", "Leftovers" },
            EVSpread = "252 HP / 128 Def / 128 SpD",
            IVRequirements = "31/x/31/31/31/31"
        },
        ["Trick Room Attacker"] = new()
        {
            PreferredNatures = new[] { Nature.Brave, Nature.Quiet },
            PreferredAbilities = new[] { "Guts", "Sheer Force", "Iron Fist" },
            PreferredItems = new[] { "Life Orb", "Assault Vest", "Choice Band" },
            EVSpread = "252 HP / 252 Atk / 4 SpD",
            IVRequirements = "31/31/31/x/31/0"
        },
        ["Mixed Attacker"] = new()
        {
            PreferredNatures = new[] { Nature.Naive, Nature.Hasty, Nature.Mild, Nature.Rash },
            PreferredAbilities = new[] { "Protean", "Libero", "Download" },
            PreferredItems = new[] { "Life Orb", "Expert Belt", "Choice Specs" },
            EVSpread = "4 Atk / 252 SpA / 252 Spe",
            IVRequirements = "31/31/31/31/31/31"
        },
        ["Bulky Attacker"] = new()
        {
            PreferredNatures = new[] { Nature.Adamant, Nature.Modest },
            PreferredAbilities = new[] { "Intimidate", "Defiant", "Justified" },
            PreferredItems = new[] { "Assault Vest", "Leftovers", "Sitrus Berry" },
            EVSpread = "252 HP / 252 Atk / 4 Def",
            IVRequirements = "31/31/31/x/31/31"
        }
    };

    // Pokemon-specific ability recommendations
    private static readonly Dictionary<string, string[]> PokemonAbilities = new()
    {
        ["Garchomp"] = new[] { "Rough Skin", "Sand Veil" },
        ["Dragonite"] = new[] { "Multiscale", "Inner Focus" },
        ["Tyranitar"] = new[] { "Sand Stream", "Unnerve" },
        ["Salamence"] = new[] { "Intimidate", "Moxie" },
        ["Metagross"] = new[] { "Clear Body", "Light Metal" },
        ["Incineroar"] = new[] { "Intimidate", "Blaze" },
        ["Gholdengo"] = new[] { "Good as Gold" },
        ["Flutter Mane"] = new[] { "Protosynthesis" }
    };

    public OptimalSetupRecommender(SaveFile sav)
    {
        SAV = sav;
        InitializeComponent();
        LoadPokemonList();
    }

    private void InitializeComponent()
    {
        Text = "Optimal Setup Recommender - AI-Powered Build Suggestions";
        Size = new Size(1200, 800);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 40);
        Font = new Font("Segoe UI", 9F);

        // Selection Panel
        var grpSelection = new GroupBox
        {
            Text = "Pokemon Selection",
            Location = new Point(20, 20),
            Size = new Size(400, 150),
            ForeColor = Color.FromArgb(100, 200, 255)
        };

        var lblPokemon = new Label { Text = "Pokemon:", Location = new Point(15, 30), Size = new Size(70, 25), ForeColor = Color.White };
        cmbPokemon = new ComboBox
        {
            Location = new Point(90, 27),
            Size = new Size(200, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbPokemon.SelectedIndexChanged += CmbPokemon_SelectedIndexChanged;

        var lblRole = new Label { Text = "Target Role:", Location = new Point(15, 65), Size = new Size(75, 25), ForeColor = Color.White };
        cmbRole = new ComboBox
        {
            Location = new Point(90, 62),
            Size = new Size(200, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbRole.Items.AddRange(RoleRecommendations.Keys.ToArray());
        cmbRole.SelectedIndex = 0;

        var lblFormat = new Label { Text = "Format:", Location = new Point(15, 100), Size = new Size(70, 25), ForeColor = Color.White };
        cmbFormat = new ComboBox
        {
            Location = new Point(90, 97),
            Size = new Size(200, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(45, 45, 65),
            ForeColor = Color.White
        };
        cmbFormat.Items.AddRange(new[] { "VGC Doubles", "Singles OU", "Battle Stadium", "Anything Goes" });
        cmbFormat.SelectedIndex = 0;

        grpSelection.Controls.AddRange(new Control[] { lblPokemon, cmbPokemon, lblRole, cmbRole, lblFormat, cmbFormat });

        // Analyze Button
        btnAnalyze = new Button
        {
            Text = "🔍 GENERATE RECOMMENDATIONS",
            Location = new Point(440, 20),
            Size = new Size(250, 50),
            BackColor = Color.FromArgb(60, 120, 180),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Bold", 11F),
            FlatStyle = FlatStyle.Flat
        };
        btnAnalyze.Click += BtnAnalyze_Click;

        // Optimality Score
        var pnlScore = new Panel
        {
            Location = new Point(710, 20),
            Size = new Size(250, 80),
            BackColor = Color.FromArgb(35, 35, 55)
        };

        var lblScoreTitle = new Label
        {
            Text = "Current Build Optimality",
            Location = new Point(10, 5),
            Size = new Size(230, 20),
            ForeColor = Color.Gray,
            TextAlign = ContentAlignment.MiddleCenter
        };

        lblOptimalityScore = new Label
        {
            Text = "---",
            Location = new Point(10, 25),
            Size = new Size(230, 30),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Bold", 18F),
            TextAlign = ContentAlignment.MiddleCenter
        };

        pbOptimality = new ProgressBar
        {
            Location = new Point(10, 55),
            Size = new Size(230, 15),
            Maximum = 100
        };

        pnlScore.Controls.AddRange(new Control[] { lblScoreTitle, lblOptimalityScore, pbOptimality });

        // Apply Button
        btnApplyRecommendation = new Button
        {
            Text = "✓ Apply Selected Setup",
            Location = new Point(980, 20),
            Size = new Size(180, 50),
            BackColor = Color.FromArgb(60, 180, 80),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Bold", 10F),
            FlatStyle = FlatStyle.Flat,
            Enabled = false
        };
        btnApplyRecommendation.Click += BtnApply_Click;

        // Recommendations List
        var grpRecommendations = new GroupBox
        {
            Text = "Recommended Setup",
            Location = new Point(20, 180),
            Size = new Size(550, 350),
            ForeColor = Color.FromArgb(100, 255, 150)
        };

        lstRecommendations = new ListView
        {
            Location = new Point(15, 25),
            Size = new Size(520, 310),
            View = View.Details,
            FullRowSelect = true,
            BackColor = Color.FromArgb(35, 35, 55),
            ForeColor = Color.White
        };
        lstRecommendations.Columns.Add("Category", 100);
        lstRecommendations.Columns.Add("Recommendation", 200);
        lstRecommendations.Columns.Add("Current", 100);
        lstRecommendations.Columns.Add("Match", 60);

        grpRecommendations.Controls.Add(lstRecommendations);

        // Preview Panels
        var grpPreviews = new GroupBox
        {
            Text = "Setup Preview",
            Location = new Point(590, 180),
            Size = new Size(570, 150),
            ForeColor = Color.FromArgb(255, 200, 100)
        };

        pnlNaturePreview = new Panel
        {
            Location = new Point(15, 25),
            Size = new Size(175, 110),
            BackColor = Color.FromArgb(40, 40, 60)
        };
        CreatePreviewPanel(pnlNaturePreview, "NATURE", "---", "---");

        pnlAbilityPreview = new Panel
        {
            Location = new Point(200, 25),
            Size = new Size(175, 110),
            BackColor = Color.FromArgb(40, 40, 60)
        };
        CreatePreviewPanel(pnlAbilityPreview, "ABILITY", "---", "---");

        pnlItemPreview = new Panel
        {
            Location = new Point(385, 25),
            Size = new Size(175, 110),
            BackColor = Color.FromArgb(40, 40, 60)
        };
        CreatePreviewPanel(pnlItemPreview, "ITEM", "---", "---");

        grpPreviews.Controls.AddRange(new Control[] { pnlNaturePreview, pnlAbilityPreview, pnlItemPreview });

        // Explanation Panel
        var grpExplanation = new GroupBox
        {
            Text = "Why This Setup?",
            Location = new Point(590, 340),
            Size = new Size(570, 190),
            ForeColor = Color.FromArgb(200, 200, 200)
        };

        rtbExplanation = new RichTextBox
        {
            Location = new Point(15, 25),
            Size = new Size(540, 150),
            BackColor = Color.FromArgb(30, 30, 45),
            ForeColor = Color.White,
            Font = new Font("Consolas", 9F),
            ReadOnly = true,
            BorderStyle = BorderStyle.None
        };

        grpExplanation.Controls.Add(rtbExplanation);

        // EV/IV Summary Panel
        var grpStats = new GroupBox
        {
            Text = "Recommended Stats",
            Location = new Point(20, 540),
            Size = new Size(1140, 180),
            ForeColor = Color.FromArgb(100, 200, 255)
        };

        var pnlEVs = new Panel
        {
            Location = new Point(15, 25),
            Size = new Size(1110, 140),
            BackColor = Color.FromArgb(35, 35, 55)
        };
        pnlEVs.Name = "pnlEVs";
        pnlEVs.Paint += PnlEVs_Paint;

        grpStats.Controls.Add(pnlEVs);

        Controls.AddRange(new Control[] { grpSelection, btnAnalyze, pnlScore, btnApplyRecommendation, grpRecommendations, grpPreviews, grpExplanation, grpStats });
    }

    private void CreatePreviewPanel(Panel panel, string title, string value, string effect)
    {
        var lblTitle = new Label
        {
            Text = title,
            Location = new Point(5, 5),
            Size = new Size(165, 20),
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 8F),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblValue = new Label
        {
            Name = "lblValue",
            Text = value,
            Location = new Point(5, 30),
            Size = new Size(165, 35),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Bold", 14F),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblEffect = new Label
        {
            Name = "lblEffect",
            Text = effect,
            Location = new Point(5, 70),
            Size = new Size(165, 35),
            ForeColor = Color.Cyan,
            Font = new Font("Segoe UI", 8F),
            TextAlign = ContentAlignment.MiddleCenter
        };

        panel.Controls.AddRange(new Control[] { lblTitle, lblValue, lblEffect });
    }

    private PKM? currentPokemon;
    private RoleRecommendation? currentRecommendation;

    private void LoadPokemonList()
    {
        cmbPokemon.Items.Clear();

        var party = SAV.PartyData.Where(p => p.Species != 0).ToList();
        foreach (var pk in party)
        {
            var name = SpeciesName.GetSpeciesName(pk.Species, 2);
            cmbPokemon.Items.Add(new PokemonListItem { Pokemon = pk, Display = name });
        }

        if (cmbPokemon.Items.Count > 0)
            cmbPokemon.SelectedIndex = 0;
    }

    private void CmbPokemon_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (cmbPokemon.SelectedItem is PokemonListItem item)
        {
            currentPokemon = item.Pokemon;
            AutoDetectRole(item.Pokemon);
        }
    }

    private void AutoDetectRole(PKM pk)
    {
        int atk = pk.PersonalInfo.ATK;
        int spa = pk.PersonalInfo.SPA;
        int def = pk.PersonalInfo.DEF;
        int spd = pk.PersonalInfo.SPD;
        int spe = pk.PersonalInfo.SPE;

        string detectedRole;
        if (atk > spa && spe >= 100 && atk >= 100) detectedRole = "Physical Sweeper";
        else if (spa > atk && spe >= 100 && spa >= 100) detectedRole = "Special Sweeper";
        else if (def >= 100 && spd < 80) detectedRole = "Physical Wall";
        else if (spd >= 100 && def < 80) detectedRole = "Special Wall";
        else if (atk > spa && spe < 50) detectedRole = "Trick Room Attacker";
        else if (Math.Abs(atk - spa) < 20 && (atk + spa) / 2 >= 90) detectedRole = "Mixed Attacker";
        else detectedRole = "Bulky Attacker";

        var index = cmbRole.Items.IndexOf(detectedRole);
        if (index >= 0) cmbRole.SelectedIndex = index;
    }

    private void BtnAnalyze_Click(object? sender, EventArgs e)
    {
        if (currentPokemon == null)
        {
            MessageBox.Show("Please select a Pokemon!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        GenerateRecommendations();
    }

    private void GenerateRecommendations()
    {
        lstRecommendations.Items.Clear();
        rtbExplanation.Clear();

        var role = cmbRole.SelectedItem?.ToString() ?? "Physical Sweeper";
        currentRecommendation = RoleRecommendations[role];
        var pokemonName = (cmbPokemon.SelectedItem as PokemonListItem)?.Display ?? "";

        int matchScore = 0;
        int totalChecks = 4;

        // Nature recommendation
        var recommendedNature = currentRecommendation.PreferredNatures.First();
        var currentNature = (Nature)currentPokemon!.Nature;
        bool natureMatch = currentRecommendation.PreferredNatures.Contains(currentNature);
        matchScore += natureMatch ? 1 : 0;

        var natureItem = new ListViewItem("Nature");
        natureItem.SubItems.Add(recommendedNature.ToString());
        natureItem.SubItems.Add(currentNature.ToString());
        natureItem.SubItems.Add(natureMatch ? "✓" : "✗");
        natureItem.ForeColor = natureMatch ? Color.LightGreen : Color.Salmon;
        lstRecommendations.Items.Add(natureItem);

        // Ability recommendation
        var pokemonAbilities = PokemonAbilities.GetValueOrDefault(pokemonName, currentRecommendation.PreferredAbilities);
        var recommendedAbility = pokemonAbilities.First();

        var abilityItem = new ListViewItem("Ability");
        abilityItem.SubItems.Add(recommendedAbility);
        abilityItem.SubItems.Add("---");
        abilityItem.SubItems.Add("?");
        lstRecommendations.Items.Add(abilityItem);

        // Item recommendation
        var recommendedItem = currentRecommendation.PreferredItems.First();
        var itemItem = new ListViewItem("Held Item");
        itemItem.SubItems.Add(recommendedItem);
        itemItem.SubItems.Add("---");
        itemItem.SubItems.Add("?");
        lstRecommendations.Items.Add(itemItem);

        // EV Spread
        var evItem = new ListViewItem("EV Spread");
        evItem.SubItems.Add(currentRecommendation.EVSpread);
        evItem.SubItems.Add($"{currentPokemon.EV_HP}/{currentPokemon.EV_ATK}/{currentPokemon.EV_DEF}/{currentPokemon.EV_SPA}/{currentPokemon.EV_SPD}/{currentPokemon.EV_SPE}");
        lstRecommendations.Items.Add(evItem);

        // IV Requirements
        var ivItem = new ListViewItem("IV Requirements");
        ivItem.SubItems.Add(currentRecommendation.IVRequirements);
        ivItem.SubItems.Add($"{currentPokemon.IV_HP}/{currentPokemon.IV_ATK}/{currentPokemon.IV_DEF}/{currentPokemon.IV_SPA}/{currentPokemon.IV_SPD}/{currentPokemon.IV_SPE}");
        lstRecommendations.Items.Add(ivItem);

        // Update preview panels
        UpdatePreviewPanel(pnlNaturePreview, recommendedNature.ToString(), GetNatureEffect(recommendedNature));
        UpdatePreviewPanel(pnlAbilityPreview, recommendedAbility, GetAbilityEffect(recommendedAbility));
        UpdatePreviewPanel(pnlItemPreview, recommendedItem, GetItemEffect(recommendedItem));

        // Calculate optimality score
        int optimalityScore = (int)((double)matchScore / totalChecks * 100);
        CheckIVOptimality(ref optimalityScore);
        CheckEVOptimality(ref optimalityScore);

        lblOptimalityScore.Text = $"{optimalityScore}%";
        lblOptimalityScore.ForeColor = optimalityScore >= 80 ? Color.LightGreen : optimalityScore >= 50 ? Color.Yellow : Color.Salmon;
        pbOptimality.Value = optimalityScore;

        // Generate explanation
        GenerateExplanation(role, recommendedNature, recommendedAbility, recommendedItem);

        // Redraw EV panel
        var pnlEVs = Controls.Find("pnlEVs", true).FirstOrDefault();
        pnlEVs?.Invalidate();

        btnApplyRecommendation.Enabled = true;
    }

    private void CheckIVOptimality(ref int score)
    {
        if (currentPokemon == null) return;

        int perfectIVs = 0;
        if (currentPokemon.IV_HP == 31) perfectIVs++;
        if (currentPokemon.IV_ATK == 31) perfectIVs++;
        if (currentPokemon.IV_DEF == 31) perfectIVs++;
        if (currentPokemon.IV_SPA == 31) perfectIVs++;
        if (currentPokemon.IV_SPD == 31) perfectIVs++;
        if (currentPokemon.IV_SPE == 31) perfectIVs++;

        score += (perfectIVs * 5); // Up to 30 extra points
    }

    private void CheckEVOptimality(ref int score)
    {
        if (currentPokemon == null) return;

        int totalEVs = currentPokemon.EV_HP + currentPokemon.EV_ATK + currentPokemon.EV_DEF +
                       currentPokemon.EV_SPA + currentPokemon.EV_SPD + currentPokemon.EV_SPE;

        if (totalEVs >= 500) score += 10;
        else if (totalEVs >= 400) score += 5;
    }

    private void UpdatePreviewPanel(Panel panel, string value, string effect)
    {
        var lblValue = panel.Controls.OfType<Label>().FirstOrDefault(l => l.Name == "lblValue");
        var lblEffect = panel.Controls.OfType<Label>().FirstOrDefault(l => l.Name == "lblEffect");

        if (lblValue != null) lblValue.Text = value;
        if (lblEffect != null) lblEffect.Text = effect;
    }

    private string GetNatureEffect(Nature nature) => nature switch
    {
        Nature.Jolly => "+Spe / -SpA",
        Nature.Adamant => "+Atk / -SpA",
        Nature.Timid => "+Spe / -Atk",
        Nature.Modest => "+SpA / -Atk",
        Nature.Bold => "+Def / -Atk",
        Nature.Impish => "+Def / -SpA",
        Nature.Calm => "+SpD / -Atk",
        Nature.Careful => "+SpD / -SpA",
        Nature.Brave => "+Atk / -Spe",
        Nature.Quiet => "+SpA / -Spe",
        Nature.Naive => "+Spe / -SpD",
        Nature.Hasty => "+Spe / -Def",
        _ => "Neutral"
    };

    private string GetAbilityEffect(string ability) => ability switch
    {
        "Intimidate" => "-1 Atk to opponents",
        "Moxie" => "+1 Atk on KO",
        "Multiscale" => "50% dmg at full HP",
        "Rough Skin" => "1/8 contact dmg",
        "Regenerator" => "Heal 33% on switch",
        "Protosynthesis" => "Boost best stat (Sun)",
        "Good as Gold" => "Status immunity",
        _ => "Special effect"
    };

    private string GetItemEffect(string item) => item switch
    {
        "Choice Band" => "1.5x Atk, locked move",
        "Choice Specs" => "1.5x SpA, locked move",
        "Choice Scarf" => "1.5x Spe, locked move",
        "Life Orb" => "1.3x dmg, 10% recoil",
        "Leftovers" => "Heal 1/16 per turn",
        "Focus Sash" => "Survive OHKO at full",
        "Assault Vest" => "1.5x SpD, no status",
        _ => "Various effects"
    };

    private void GenerateExplanation(string role, Nature nature, string ability, string item)
    {
        rtbExplanation.SelectionColor = Color.Cyan;
        rtbExplanation.AppendText($"ROLE: {role}\n\n");

        rtbExplanation.SelectionColor = Color.Yellow;
        rtbExplanation.AppendText("WHY THIS NATURE?\n");
        rtbExplanation.SelectionColor = Color.White;
        rtbExplanation.AppendText($"{nature} is optimal for {role}s because it boosts the ");
        rtbExplanation.AppendText($"most important stat while reducing an unused one.\n\n");

        rtbExplanation.SelectionColor = Color.Yellow;
        rtbExplanation.AppendText("WHY THIS ABILITY?\n");
        rtbExplanation.SelectionColor = Color.White;
        rtbExplanation.AppendText($"{ability} synergizes well with the {role} playstyle.\n\n");

        rtbExplanation.SelectionColor = Color.Yellow;
        rtbExplanation.AppendText("WHY THIS ITEM?\n");
        rtbExplanation.SelectionColor = Color.White;
        rtbExplanation.AppendText($"{item} maximizes {role} effectiveness in competitive play.\n");
    }

    private void PnlEVs_Paint(object? sender, PaintEventArgs e)
    {
        if (currentPokemon == null || currentRecommendation == null) return;

        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        string[] statNames = { "HP", "Attack", "Defense", "Sp. Atk", "Sp. Def", "Speed" };
        int[] currentEVs = { currentPokemon.EV_HP, currentPokemon.EV_ATK, currentPokemon.EV_DEF, currentPokemon.EV_SPA, currentPokemon.EV_SPD, currentPokemon.EV_SPE };

        // Parse recommended EVs
        var evParts = currentRecommendation.EVSpread.Split('/').Select(s => s.Trim()).ToArray();
        int[] recommendedEVs = new int[6];
        for (int i = 0; i < evParts.Length && i < 6; i++)
        {
            var num = new string(evParts[i].TakeWhile(char.IsDigit).ToArray());
            if (int.TryParse(num, out int val))
            {
                if (evParts[i].Contains("HP")) recommendedEVs[0] = val;
                else if (evParts[i].Contains("Atk")) recommendedEVs[1] = val;
                else if (evParts[i].Contains("Def")) recommendedEVs[2] = val;
                else if (evParts[i].Contains("SpA")) recommendedEVs[3] = val;
                else if (evParts[i].Contains("SpD")) recommendedEVs[4] = val;
                else if (evParts[i].Contains("Spe")) recommendedEVs[5] = val;
            }
        }

        int barWidth = 150;
        int barHeight = 20;
        int spacing = 22;
        int startX = 80;
        int startY = 10;

        for (int i = 0; i < 6; i++)
        {
            int y = startY + i * spacing;

            // Stat name
            g.DrawString(statNames[i], Font, Brushes.White, 10, y);

            // Current EV bar (background)
            g.FillRectangle(new SolidBrush(Color.FromArgb(50, 50, 70)), startX, y, barWidth, barHeight);

            // Current EV fill
            int currentWidth = (int)((double)currentEVs[i] / 252 * barWidth);
            g.FillRectangle(new SolidBrush(Color.FromArgb(100, 150, 200)), startX, y, currentWidth, barHeight);

            // Recommended outline
            int recWidth = (int)((double)recommendedEVs[i] / 252 * barWidth);
            g.DrawRectangle(new Pen(Color.Cyan, 2), startX, y, recWidth, barHeight);

            // EV values
            g.DrawString($"{currentEVs[i]}/{recommendedEVs[i]}", Font, Brushes.White, startX + barWidth + 10, y);
        }

        // Legend
        g.DrawString("■ Current   □ Recommended", Font, Brushes.Gray, 400, 10);
    }

    private void BtnApply_Click(object? sender, EventArgs e)
    {
        if (currentPokemon == null || currentRecommendation == null) return;

        var result = MessageBox.Show(
            "Apply the recommended Nature and EVs to this Pokemon?",
            "Confirm Changes",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            // Apply nature
            var nature = currentRecommendation.PreferredNatures.First();
            currentPokemon.Nature = nature;
            currentPokemon.StatNature = nature;

            // Apply EVs (parse from recommendation)
            ApplyRecommendedEVs();

            MessageBox.Show("Recommendations applied successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            GenerateRecommendations(); // Refresh
        }
    }

    private void ApplyRecommendedEVs()
    {
        if (currentRecommendation == null || currentPokemon == null) return;

        // Reset EVs
        currentPokemon.EV_HP = 0;
        currentPokemon.EV_ATK = 0;
        currentPokemon.EV_DEF = 0;
        currentPokemon.EV_SPA = 0;
        currentPokemon.EV_SPD = 0;
        currentPokemon.EV_SPE = 0;

        var evParts = currentRecommendation.EVSpread.Split('/').Select(s => s.Trim()).ToArray();
        foreach (var part in evParts)
        {
            var num = new string(part.TakeWhile(char.IsDigit).ToArray());
            if (int.TryParse(num, out int val))
            {
                if (part.Contains("HP")) currentPokemon.EV_HP = val;
                else if (part.Contains("Atk")) currentPokemon.EV_ATK = val;
                else if (part.Contains("Def")) currentPokemon.EV_DEF = val;
                else if (part.Contains("SpA")) currentPokemon.EV_SPA = val;
                else if (part.Contains("SpD")) currentPokemon.EV_SPD = val;
                else if (part.Contains("Spe")) currentPokemon.EV_SPE = val;
            }
        }
    }

    private class PokemonListItem
    {
        public PKM Pokemon { get; set; } = null!;
        public string Display { get; set; } = "";
        public override string ToString() => Display;
    }

    private class RoleRecommendation
    {
        public Nature[] PreferredNatures { get; set; } = Array.Empty<Nature>();
        public string[] PreferredAbilities { get; set; } = Array.Empty<string>();
        public string[] PreferredItems { get; set; } = Array.Empty<string>();
        public string EVSpread { get; set; } = "";
        public string IVRequirements { get; set; } = "";
    }
}
