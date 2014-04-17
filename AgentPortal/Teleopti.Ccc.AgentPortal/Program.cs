using System;
using System.Windows.Forms;
using Teleopti.Ccc.AgentPortal.Helper;
using Teleopti.Ccc.AgentPortal.Main;

namespace Teleopti.Ccc.AgentPortal
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.AgentPortal.Helper.MessageBoxHelper.ShowErrorMessage(System.String,System.String)"), STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var settings = AppConfigHelper.LoadSettings();
            
            var logOnScreen = new LogOnScreen(settings);
            if (logOnScreen.ShowDialog() == DialogResult.OK)
            {
                Application.Run(new MainScreen());
            }
        }
    }
}