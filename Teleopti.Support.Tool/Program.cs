using System;
using Teleopti.Support.Code.Tool;
using log4net.Config;

namespace Teleopti.Support.Tool
{
    static class Program
    {
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {   
            XmlConfigurator.Configure();
	        if (args.Length.Equals(0))
			{
				// *********************************************************************************
				// Don't remove this, used for showing groups in a ListView /Henry
				System.Windows.Forms.Application.EnableVisualStyles();
				// *********************************************************************************

		        new MainForm().ShowDialog();
	        }
            else
            {
				var commandLineArgument = new CommandLineArgument(args, x => new HelpWindow(x));
				commandLineArgument.Command.Execute(commandLineArgument.Mode);
            }
        }

		
    }

	
}