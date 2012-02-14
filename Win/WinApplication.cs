using System;
using System.Windows.Forms;
using log4net.Config;
using Teleopti.Ccc.Win.Main;

namespace Teleopti.Ccc.Win
{
    public static class WinApplication
    {

        /// <summary>
        /// The main entry point for the win application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            XmlConfigurator.Configure();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (LogOn())
                Application.Run(new MainScreen());
        }


        /// <summary>
        /// Loads the authentication form.
        /// </summary>
        /// <returns></returns>
        private static bool LogOn()
        {
            Teleopti.Ccc.Win.Main.LogOnScreen logOnScreen = new Teleopti.Ccc.Win.Main.LogOnScreen();
            return (logOnScreen.ShowDialog() == DialogResult.OK);
        }
    }
}