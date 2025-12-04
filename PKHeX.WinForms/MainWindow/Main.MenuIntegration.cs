using System;
using System.Windows.Forms;
using PKHeX.Core;

namespace PKHeX.WinForms;

/// <summary>
/// Menu integration for revolutionary PKM-Universe features
/// </summary>
public partial class Main
{
    // Revolutionary Features Menu Handlers

    private void Menu_TradeBotIntegration_Click(object sender, EventArgs e)
    {
        if (!PKME_Tabs.EditsComplete)
        {
            WinFormsUtil.Alert("Please finish editing the current Pokemon first.");
            return;
        }

        var pk = PKME_Tabs.PreparePKM();
        using var form = new TradeBotIntegration(C_SAV.SAV, pk);
        if (form.ShowDialog() == DialogResult.OK)
        {
            WinFormsUtil.Alert("Pokemon sent to trade bot successfully!");
        }
    }

    private void Menu_CloudSync_Click(object sender, EventArgs e)
    {
        if (C_SAV.SAV == null || !C_SAV.SAV.HasBox)
        {
            WinFormsUtil.Alert("No save file with boxes loaded!");
            return;
        }

        using var form = new PokemonHomeCloudSync(C_SAV.SAV);
        if (form.ShowDialog() == DialogResult.OK)
        {
            C_SAV.ReloadSlots();
            WinFormsUtil.Alert("Cloud sync operation completed!");
        }
    }

    private void Menu_VGCTeamImporter_Click(object sender, EventArgs e)
    {
        if (C_SAV.SAV == null)
        {
            WinFormsUtil.Alert("No save file loaded!");
            return;
        }

        using var form = new VGCTeamImporter(C_SAV.SAV, PKME_Tabs);
        if (form.ShowDialog() == DialogResult.OK)
        {
            C_SAV.ReloadSlots();
            WinFormsUtil.Alert("Team imported successfully!");
        }
    }
}
