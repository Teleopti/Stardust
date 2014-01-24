using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Teleopti.Support.Tool
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {            
            // *********************************************************************************
            // Don't remove this, used for showing groups in a ListView /Henry
            System.Windows.Forms.Application.EnableVisualStyles();
            // *********************************************************************************

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
