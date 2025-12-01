using System;
using System.Windows.Forms;

namespace PKHeX.WinForms;

public partial class SplashScreen : Form
{
    public SplashScreen()
    {
        InitializeComponent();
    }

    public void UpdateStatus(string message)
    {
        if (InvokeRequired)
        {
            Invoke(() => UpdateStatus(message));
            return;
        }
        L_Status.Text = message;
    }

    private void SplashScreen_FormClosing(object sender, FormClosingEventArgs e)
    {
        // Allow closing
    }

    public void ForceClose()
    {
        if (InvokeRequired)
        {
            Invoke(ForceClose);
            return;
        }
        Close();
    }
}
