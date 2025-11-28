using System.Diagnostics;
using System.Windows.Forms;

namespace PKHeX.WinForms;

public partial class About : Form
{
    public About(AboutPage index = AboutPage.Changelog)
    {
        InitializeComponent();
        WinFormsUtil.TranslateInterface(this, Main.CurrentLanguage);
        RTB_Changelog.Text = Properties.Resources.changelog;
        RTB_Shortcuts.Text = Properties.Resources.shortcuts;
        TC_About.SelectedIndex = (int)index;

        // Set up link click handlers
        LL_Discord.LinkClicked += (_, _) => OpenUrl("https://discord.gg/pkm-universe");
        LL_Kofi.LinkClicked += (_, _) => OpenUrl("https://ko-fi.com/pokemonlover8888");
        LL_GitHub.LinkClicked += (_, _) => OpenUrl("https://github.com/PKM-Universe/PKHeX-ALL-IN-ONE");
    }

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch { }
    }
}

public enum AboutPage
{
    Shortcuts,
    Changelog,
}
