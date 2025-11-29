using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PKHeX.Core;
using PKHeX.WinForms.Controls;
using static PKHeX.Core.MessageStrings;

namespace PKHeX.WinForms;

public partial class BatchEditor : Form
{
    private readonly SaveFile SAV;

    // Mass Editing
    private Core.BatchEditor editor = new();
    private readonly EntityInstructionBuilder UC_Builder;

    private static string LastUsedCommands = string.Empty;

    // Batch Presets
    private static readonly Dictionary<string, string> BatchPresets = new()
    {
        // Shiny Operations
        ["Make All Shiny (Star)"] = ".IsShiny=false\n.SetShiny(Shiny.AlwaysStar)",
        ["Make All Shiny (Square)"] = ".IsShiny=false\n.SetShiny(Shiny.AlwaysSquare)",
        ["Make All Shiny (Random)"] = ".IsShiny=false\n.SetShiny()",
        ["Remove Shiny"] = ".IsShiny=true\n.SetShinySID()",

        // IV Operations
        ["Max All IVs (6IV)"] = ".IV_HP=31\n.IV_ATK=31\n.IV_DEF=31\n.IV_SPA=31\n.IV_SPD=31\n.IV_SPE=31",
        ["Zero All IVs"] = ".IV_HP=0\n.IV_ATK=0\n.IV_DEF=0\n.IV_SPA=0\n.IV_SPD=0\n.IV_SPE=0",
        ["Zero Attack IV (Special)"] = ".IV_ATK=0",
        ["Zero Speed IV (Trick Room)"] = ".IV_SPE=0",
        ["Hyper Train All"] = ".IV_HP=31\n.IV_ATK=31\n.IV_DEF=31\n.IV_SPA=31\n.IV_SPD=31\n.IV_SPE=31\n.HT_HP=true\n.HT_ATK=true\n.HT_DEF=true\n.HT_SPA=true\n.HT_SPD=true\n.HT_SPE=true",

        // EV Operations
        ["Clear All EVs"] = ".EV_HP=0\n.EV_ATK=0\n.EV_DEF=0\n.EV_SPA=0\n.EV_SPD=0\n.EV_SPE=0",
        ["Max Physical Sweeper EVs"] = ".EV_HP=0\n.EV_ATK=252\n.EV_DEF=0\n.EV_SPA=0\n.EV_SPD=4\n.EV_SPE=252",
        ["Max Special Sweeper EVs"] = ".EV_HP=0\n.EV_ATK=0\n.EV_DEF=0\n.EV_SPA=252\n.EV_SPD=4\n.EV_SPE=252",
        ["Max Bulk (Physical)"] = ".EV_HP=252\n.EV_ATK=0\n.EV_DEF=252\n.EV_SPA=0\n.EV_SPD=4\n.EV_SPE=0",
        ["Max Bulk (Special)"] = ".EV_HP=252\n.EV_ATK=0\n.EV_DEF=4\n.EV_SPA=0\n.EV_SPD=252\n.EV_SPE=0",
        ["Max Bulk (Mixed)"] = ".EV_HP=252\n.EV_ATK=0\n.EV_DEF=128\n.EV_SPA=0\n.EV_SPD=128\n.EV_SPE=0",

        // Level Operations
        ["Set Level 100"] = ".CurrentLevel=100",
        ["Set Level 50"] = ".CurrentLevel=50",
        ["Set Level 1"] = ".CurrentLevel=1",

        // Friendship/Happiness
        ["Max Friendship"] = ".OriginalTrainerFriendship=255\n.HandlingTrainerFriendship=255",
        ["Zero Friendship"] = ".OriginalTrainerFriendship=0\n.HandlingTrainerFriendship=0",

        // Ball Operations
        ["Set Poke Ball"] = ".Ball=4",
        ["Set Ultra Ball"] = ".Ball=2",
        ["Set Master Ball"] = ".Ball=1",
        ["Set Cherish Ball"] = ".Ball=16",
        ["Set Dream Ball"] = ".Ball=25",
        ["Set Beast Ball"] = ".Ball=26",

        // PP Operations
        ["Max All PP"] = ".HealPP()",
        ["Max PP Ups"] = ".Move1_PPUps=3\n.Move2_PPUps=3\n.Move3_PPUps=3\n.Move4_PPUps=3\n.HealPP()",

        // Language
        ["Set Language English"] = ".Language=2",
        ["Set Language Japanese"] = ".Language=1",
        ["Set Language French"] = ".Language=3",
        ["Set Language German"] = ".Language=4",
        ["Set Language Spanish"] = ".Language=7",
        ["Set Language Korean"] = ".Language=8",
        ["Set Language Chinese (Simplified)"] = ".Language=9",
        ["Set Language Chinese (Traditional)"] = ".Language=10",

        // Contest Stats (Gen 3-4)
        ["Max Contest Stats"] = ".ContestCool=255\n.ContestBeauty=255\n.ContestCute=255\n.ContestSmart=255\n.ContestTough=255\n.ContestSheen=255",
        ["Clear Contest Stats"] = ".ContestCool=0\n.ContestBeauty=0\n.ContestCute=0\n.ContestSmart=0\n.ContestTough=0\n.ContestSheen=0",

        // Pokerus
        ["Give Pokerus"] = ".PKRS_Strain=1\n.PKRS_Days=1",
        ["Cure Pokerus (Keep Immune)"] = ".PKRS_Days=0",
        ["Clear Pokerus"] = ".PKRS_Strain=0\n.PKRS_Days=0",

        // Gender
        ["Set Male"] = ".Gender=0",
        ["Set Female"] = ".Gender=1",

        // Misc
        ["Clear Nickname"] = ".IsNicknamed=false",
        ["Remove Held Item"] = ".HeldItem=0",
        ["Set Current Handler to OT"] = ".CurrentHandler=0",
        ["Set Current Handler to HT"] = ".CurrentHandler=1",
        ["Cure Status"] = ".Status_Condition=0",
        ["Full Heal (HP)"] = ".Stat_HPCurrent=999",

        // Legality Helpers
        ["Fix Checksums"] = ".RefreshChecksum()",
        ["Clear Illegal Ribbons"] = ".Ribbons=$suggestNone",

        // Gen 9 Specific (Tera)
        ["Set Tera Type Normal"] = ".TeraTypeOriginal=0",
        ["Set Tera Type Fighting"] = ".TeraTypeOriginal=1",
        ["Set Tera Type Flying"] = ".TeraTypeOriginal=2",
        ["Set Tera Type Poison"] = ".TeraTypeOriginal=3",
        ["Set Tera Type Ground"] = ".TeraTypeOriginal=4",
        ["Set Tera Type Rock"] = ".TeraTypeOriginal=5",
        ["Set Tera Type Bug"] = ".TeraTypeOriginal=6",
        ["Set Tera Type Ghost"] = ".TeraTypeOriginal=7",
        ["Set Tera Type Steel"] = ".TeraTypeOriginal=8",
        ["Set Tera Type Fire"] = ".TeraTypeOriginal=9",
        ["Set Tera Type Water"] = ".TeraTypeOriginal=10",
        ["Set Tera Type Grass"] = ".TeraTypeOriginal=11",
        ["Set Tera Type Electric"] = ".TeraTypeOriginal=12",
        ["Set Tera Type Psychic"] = ".TeraTypeOriginal=13",
        ["Set Tera Type Ice"] = ".TeraTypeOriginal=14",
        ["Set Tera Type Dragon"] = ".TeraTypeOriginal=15",
        ["Set Tera Type Dark"] = ".TeraTypeOriginal=16",
        ["Set Tera Type Fairy"] = ".TeraTypeOriginal=17",
        ["Set Tera Type Stellar"] = ".TeraTypeOriginal=18",

        // Filter Examples
        ["[Filter] Shiny Only"] = "=IsShiny=true",
        ["[Filter] Non-Shiny Only"] = "=IsShiny=false",
        ["[Filter] Legendary Only"] = "=IsLegendary=true",
        ["[Filter] Level 100 Only"] = "=CurrentLevel=100",
        ["[Filter] Has Item"] = "=HeldItem>0",
        ["[Filter] No Item"] = "=HeldItem=0",
    };

    public BatchEditor(PKM pk, SaveFile sav)
    {
        InitializeComponent();
        WinFormsUtil.TranslateInterface(this, Main.CurrentLanguage);
        var above = FLP_RB.Location;
        UC_Builder = new EntityInstructionBuilder(() => pk)
        {
            Location = new() { Y = above.Y + FLP_RB.Height + 4 - 1, X = above.X + 1 },
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Width = B_Add.Location.X - above.X - 2,
        };
        Controls.Add(UC_Builder);
        SAV = sav;
        DragDrop += TabMain_DragDrop;
        DragEnter += TabMain_DragEnter;

        RTB_Instructions.Text = LastUsedCommands;
        FormClosing += (_, _) => LastUsedCommands = RTB_Instructions.Text;

        // Initialize presets menu
        InitializePresetsMenu();
    }

    private void InitializePresetsMenu()
    {
        Menu_Presets.Items.Clear();

        // Group presets by category
        var categories = new Dictionary<string, List<KeyValuePair<string, string>>>
        {
            ["Shiny"] = new(),
            ["IVs"] = new(),
            ["EVs"] = new(),
            ["Level"] = new(),
            ["Friendship"] = new(),
            ["Ball"] = new(),
            ["PP"] = new(),
            ["Language"] = new(),
            ["Contest"] = new(),
            ["Pokerus"] = new(),
            ["Gender"] = new(),
            ["Tera Type"] = new(),
            ["Filters"] = new(),
            ["Misc"] = new(),
        };

        foreach (var preset in BatchPresets)
        {
            string category = "Misc";
            if (preset.Key.Contains("Shiny")) category = "Shiny";
            else if (preset.Key.Contains("IV") || preset.Key.Contains("Hyper")) category = "IVs";
            else if (preset.Key.Contains("EV") || preset.Key.Contains("Sweeper") || preset.Key.Contains("Bulk")) category = "EVs";
            else if (preset.Key.Contains("Level")) category = "Level";
            else if (preset.Key.Contains("Friendship")) category = "Friendship";
            else if (preset.Key.Contains("Ball")) category = "Ball";
            else if (preset.Key.Contains("PP")) category = "PP";
            else if (preset.Key.Contains("Language")) category = "Language";
            else if (preset.Key.Contains("Contest")) category = "Contest";
            else if (preset.Key.Contains("Pokerus") || preset.Key.Contains("PKRS")) category = "Pokerus";
            else if (preset.Key.Contains("Gender") || preset.Key.Contains("Male") || preset.Key.Contains("Female")) category = "Gender";
            else if (preset.Key.Contains("Tera")) category = "Tera Type";
            else if (preset.Key.Contains("[Filter]")) category = "Filters";

            categories[category].Add(preset);
        }

        foreach (var category in categories.Where(c => c.Value.Count > 0))
        {
            var menuItem = new ToolStripMenuItem(category.Key);
            foreach (var preset in category.Value)
            {
                var subItem = new ToolStripMenuItem(preset.Key.Replace("[Filter] ", ""), null, (_, _) => ApplyPreset(preset.Value));
                menuItem.DropDownItems.Add(subItem);
            }
            Menu_Presets.Items.Add(menuItem);
        }

        // Add separator and custom options
        Menu_Presets.Items.Add(new ToolStripSeparator());

        var savePreset = new ToolStripMenuItem("Save Current as Preset...", null, SaveCurrentPreset_Click);
        var loadPreset = new ToolStripMenuItem("Load from File...", null, LoadPresetFromFile_Click);
        Menu_Presets.Items.Add(savePreset);
        Menu_Presets.Items.Add(loadPreset);
    }

    private void ApplyPreset(string commands)
    {
        var tb = RTB_Instructions;
        if (tb.Text.Length != 0 && !tb.Text.EndsWith('\n'))
            tb.AppendText(Environment.NewLine);
        tb.AppendText(commands);
    }

    private void B_Presets_Click(object? sender, EventArgs e)
    {
        Menu_Presets.Show(B_Presets, 0, B_Presets.Height);
    }

    private void B_Clear_Click(object? sender, EventArgs e)
    {
        RTB_Instructions.Clear();
    }

    private void SaveCurrentPreset_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(RTB_Instructions.Text))
        {
            WinFormsUtil.Alert("No commands to save!");
            return;
        }

        using var sfd = new SaveFileDialog
        {
            Filter = "Batch Preset (*.txt)|*.txt|All Files (*.*)|*.*",
            FileName = "BatchPreset.txt"
        };

        if (sfd.ShowDialog() != DialogResult.OK)
            return;

        File.WriteAllText(sfd.FileName, RTB_Instructions.Text);
        WinFormsUtil.Alert("Preset saved!", sfd.FileName);
    }

    private void LoadPresetFromFile_Click(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog
        {
            Filter = "Batch Preset (*.txt)|*.txt|All Files (*.*)|*.*"
        };

        if (ofd.ShowDialog() != DialogResult.OK)
            return;

        var content = File.ReadAllText(ofd.FileName);
        ApplyPreset(content);
    }

    private void B_Open_Click(object sender, EventArgs e)
    {
        if (!B_Go.Enabled)
            return;
        using var fbd = new FolderBrowserDialog();
        if (fbd.ShowDialog() != DialogResult.OK)
            return;

        TB_Folder.Text = fbd.SelectedPath;
        TB_Folder.Visible = true;
    }

    private void B_SAV_Click(object sender, EventArgs e)
    {
        TB_Folder.Text = string.Empty;
        TB_Folder.Visible = false;
    }

    private void B_Go_Click(object sender, EventArgs e)
    {
        RunBackgroundWorker();
    }

    private void B_Add_Click(object sender, EventArgs e)
    {
        var s = UC_Builder.Create();
        if (s.Length == 0)
        { WinFormsUtil.Alert(MsgBEPropertyInvalid); return; }

        // If we already have text, add a new line (except if the last line is blank).
        var tb = RTB_Instructions;
        var batchText = tb.Text;
        if (batchText.Length != 0 && !batchText.EndsWith('\n'))
            tb.AppendText(Environment.NewLine);
        RTB_Instructions.AppendText(s);
    }

    private static void TabMain_DragEnter(object? sender, DragEventArgs? e)
    {
        if (e?.Data is null)
            return;
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effect = DragDropEffects.Copy;
    }

    private void TabMain_DragDrop(object? sender, DragEventArgs? e)
    {
        if (e?.Data?.GetData(DataFormats.FileDrop) is not string[] { Length: not 0 } files)
            return;
        if (!Directory.Exists(files[0]))
            return;

        TB_Folder.Text = files[0];
        TB_Folder.Visible = true;
        RB_Boxes.Checked = RB_Party.Checked = false;
        RB_Path.Checked = true;
    }

    private void RunBackgroundWorker()
    {
        ReadOnlySpan<char> text = RTB_Instructions.Text;
        if (StringInstructionSet.HasEmptyLine(text))
        { WinFormsUtil.Error(MsgBEInstructionInvalid); return; }

        var sets = StringInstructionSet.GetBatchSets(text);
        if (Array.Exists(sets, s => s.Filters.Any(z => string.IsNullOrWhiteSpace(z.PropertyValue))))
        { WinFormsUtil.Error(MsgBEFilterEmpty); return; }

        if (Array.Exists(sets, z => z.Instructions.Count == 0))
        { WinFormsUtil.Error(MsgBEInstructionNone); return; }

        var emptyVal = sets.SelectMany(s => s.Instructions.Where(z => string.IsNullOrWhiteSpace(z.PropertyValue))).ToArray();
        if (emptyVal.Length != 0)
        {
            string props = string.Join(", ", emptyVal.Select(z => z.PropertyName));
            string invalid = MsgBEPropertyEmpty + Environment.NewLine + props;
            if (DialogResult.Yes != WinFormsUtil.Prompt(MessageBoxButtons.YesNo, invalid, MsgContinue))
                return;
        }

        string? destPath = null;
        if (RB_Path.Checked)
        {
            WinFormsUtil.Alert(MsgExportFolder, MsgExportFolderAdvice);
            using var fbd = new FolderBrowserDialog();
            var dr = fbd.ShowDialog();
            if (dr != DialogResult.OK)
                return;

            destPath = fbd.SelectedPath;
        }

        FLP_RB.Enabled = RTB_Instructions.Enabled = B_Go.Enabled = false;

        foreach (var set in sets)
        {
            BatchEditing.ScreenStrings(set.Filters);
            BatchEditing.ScreenStrings(set.Instructions);
        }
        RunBatchEdit(sets, TB_Folder.Text, destPath);
    }

    private void RunBatchEdit(StringInstructionSet[] sets, string source, string? destination)
    {
        editor = new Core.BatchEditor();
        bool finished = false, displayed = false; // hack cuz DoWork event isn't cleared after completion
        b.DoWork += (_, _) =>
        {
            if (finished)
                return;
            if (RB_Boxes.Checked)
                RunBatchEditSaveFile(sets, boxes: true);
            else if (RB_Party.Checked)
                RunBatchEditSaveFile(sets, party: true);
            else if (destination is not null)
                RunBatchEditFolder(sets, source, destination);
            finished = true;
        };
        b.ProgressChanged += (_, e) => SetProgressBar(e.ProgressPercentage);
        b.RunWorkerCompleted += (_, _) =>
        {
            string result = editor.GetEditorResults(sets);
            if (!displayed) WinFormsUtil.Alert(result);
            displayed = true;
            FLP_RB.Enabled = RTB_Instructions.Enabled = B_Go.Enabled = true;
            SetupProgressBar(0);
        };
        b.RunWorkerAsync();
    }

    private void RunBatchEditFolder(IReadOnlyCollection<StringInstructionSet> sets, string source, string destination)
    {
        var files = Directory.GetFiles(source, "*", SearchOption.AllDirectories);
        SetupProgressBar(files.Length * sets.Count);
        foreach (var set in sets)
            ProcessFolder(files, destination, set.Filters, set.Instructions);
    }

    private void RunBatchEditSaveFile(IReadOnlyCollection<StringInstructionSet> sets, bool boxes = false, bool party = false)
    {
        if (party)
        {
            var data = new List<SlotCache>(SAV.PartyCount);
            SlotInfoLoader.AddPartyData(SAV, data);
            process(data);
            foreach (var slot in data)
                slot.Source.WriteTo(SAV, slot.Entity, EntityImportSettings.None);
        }
        if (boxes)
        {
            var data = new List<SlotCache>(SAV.SlotCount);
            SlotInfoLoader.AddBoxData(SAV, data);
            process(data);
            foreach (var slot in data)
                slot.Source.WriteTo(SAV, slot.Entity, EntityImportSettings.None);
        }
        void process(IList<SlotCache> d)
        {
            SetupProgressBar(d.Count * sets.Count);
            foreach (var set in sets)
                ProcessSAV(d, set.Filters, set.Instructions);
        }
    }

    // Progress Bar
    private void SetupProgressBar(int count) => PB_Show.BeginInvoke(() =>
    {
        PB_Show.Minimum = 0;
        PB_Show.Step = 1;
        PB_Show.Value = 0;
        PB_Show.Maximum = count;
    });

    private void SetProgressBar(int position) => PB_Show.BeginInvoke(() => PB_Show.Value = position);

    private void ProcessSAV(IList<SlotCache> data, IReadOnlyList<StringInstruction> Filters, IReadOnlyList<StringInstruction> Instructions)
    {
        if (data.Count == 0)
            return;

        // Pull out any filter meta instructions from the filters.
        var filterMeta = Filters.Where(f => BatchFilters.FilterMeta.Any(z => z.IsMatch(f.PropertyName))).ToArray();
        if (filterMeta.Length != 0)
            Filters = Filters.Except(filterMeta).ToArray();

        var max = SAV.MaxSpeciesID;

        for (int i = 0; i < data.Count; i++)
        {
            var entry = data[i];
            var pk = entry.Entity;

            // Ignore empty/invalid slots.
            var spec = pk.Species;
            if (spec == 0 || spec > max)
            {
                b.ReportProgress(i);
                continue;
            }

            if (entry.Source is SlotInfoBox info && SAV.GetBoxSlotFlags(info.Box, info.Slot).IsOverwriteProtected())
                editor.AddSkipped();
            else if (!BatchEditing.IsFilterMatchMeta(filterMeta, entry))
                editor.AddSkipped();
            else
                editor.Process(pk, Filters, Instructions);

            b.ReportProgress(i);
        }
    }

    private void ProcessFolder(IReadOnlyList<string> files, string destDir, IReadOnlyList<StringInstruction> pkFilters, IReadOnlyList<StringInstruction> instructions)
    {
        var filterMeta = pkFilters.Where(f => BatchFilters.FilterMeta.Any(z => z.IsMatch(f.PropertyName))).ToArray();
        if (filterMeta.Length != 0)
            pkFilters = pkFilters.Except(filterMeta).ToArray();

        for (int i = 0; i < files.Count; i++)
        {
            TryProcess(files[i], destDir, filterMeta, pkFilters, instructions);
            b.ReportProgress(i);
        }
    }

    private void TryProcess(string source, string destDir, IReadOnlyList<StringInstruction> metaFilters, IReadOnlyList<StringInstruction> pkFilters, IReadOnlyList<StringInstruction> instructions)
    {
        var fi = new FileInfo(source);
        if (!EntityDetection.IsSizePlausible(fi.Length))
            return;

        byte[] data = File.ReadAllBytes(source);
        _ = FileUtil.TryGetPKM(data, out var pk, fi.Extension, SAV);
        if (pk is null)
            return;

        var info = new SlotInfoFileSingle(source);
        var entry = new SlotCache(info, pk);
        if (!BatchEditing.IsFilterMatchMeta(metaFilters, entry))
        {
            editor.AddSkipped();
            return;
        }

        if (editor.Process(pk, pkFilters, instructions))
            File.WriteAllBytes(Path.Combine(destDir, Path.GetFileName(source)), pk.DecryptedPartyData);
    }
}
