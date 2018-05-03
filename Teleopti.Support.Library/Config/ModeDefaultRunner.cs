using Teleopti.Support.Library.Config;

namespace Teleopti.Support.Tool.Tool
{
	public class ModeDebugCommand
	{
	}

	public class ModeDebugRunner : ModeDefaultRunner
	{
		public void Run(ModeDebugCommand command) => Run(new ConfigFiles("ConfigFiles.txt"));
	}

	public class ModeAzureCommand
	{
	}

	public class ModeAzureRunner : ModeDefaultRunner
	{
		public void Run(ModeAzureCommand command) => Run(new ConfigFiles("AzureConfigFiles.txt"));
	}

	public class ModeTestCommand
	{
	}

	public class ModeTestRunner : ModeDefaultRunner
	{
		public void Run(ModeTestCommand command) => Run(new ConfigFiles("BuildServerConfigFiles.txt"));
	}

	public abstract class ModeDefaultRunner
	{
		protected static void Run(ConfigFiles configFiles)
		{
			new RefreshConfigsRunner(new RefreshConfigFile(), () => configFiles).Execute();
			new ConfigurationRestorer()
				.Restore(
					new ConfigurationRestoreCommand
					{
						ConfigFiles = configFiles
					});
			new CustomReportsMover().Execute();
		}
	}
}