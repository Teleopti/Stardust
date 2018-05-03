using System;
using log4net;
using log4net.Config;
using Teleopti.Support.Tool.Tool;

namespace Teleopti.Support.Tool
{
	static class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			XmlConfigurator.Configure();
			if (args.Length.Equals(0))
			{
				// Don't remove this, used for showing groups in a ListView /Henry
				System.Windows.Forms.Application.EnableVisualStyles();
				new MainForm().ShowDialog();
			}
			else
			{
				try
				{
					var command = new CommandBuilder()
						.ParseCommandLine(args);

					switch (command)
					{
						case ModeDebugCommand c:
							new ModeDebugRunner().Run(c);
							break;
						case ModeDeployWebCommand c:
							new ModeDeployWebRunner().Run(c);
							break;
						case ModeDeployWorkerCommand c:
							new ModeDeployWorkerRunner().Run(c);
							break;
						case ModeAzureCommand c:
							new ModeAzureRunner().Run(c);
							break;
						case ModeTestCommand c:
							new ModeTestRunner().Run(c);
							break;
						case SetToggleModeCommand c:
							c.UpdateToggleMode();
							break;
						case HelpWindow c:
							c.ShowDialog();
							break;
						case ConfigurationBackupCommand c:
							new ConfigurationBackuper().Backup(c);
							break;
						case ConfigurationRestoreCommand c:
							new ConfigurationRestorer().Restore(c);
							break;
						case LoadSettingsCommand c:
							new SettingsLoader().LoadSettingsFile(c);
							break;
						case SetSettingCommand c:
							new SettingsSetter().UpdateSettingsFile(c);
							break;
						case ConfigServerCommand c:
							c.Execute();
							break;
						case SavePmConfigurationCommand c:
							c.Execute();
							break;
						case FixMyConfigCommand c:
							new FixMyConfigFixer().Fix(c);
							break;
						default:
							throw new ArgumentException("Unknown command");
					}
				}
				catch (Exception e)
				{
					LogManager.GetLogger(typeof(Program)).Error(e);
					throw;
				}
			}
		}
	}
}