namespace Teleopti.Support.Library.Config
{
	public class ModeDeployWebCommand
	{
	}

	public class ModeDeployWebRunner : ModeDeployRunner
	{
		public void Run(ModeDeployWebCommand command) => Run(new ConfigFiles("DeployConfigFilesWeb.txt"));
	}

	public class ModeDeployWorkerCommand
	{
	}

	public class ModeDeployWorkerRunner : ModeDeployRunner
	{
		public void Run(ModeDeployWorkerCommand command) => Run(new ConfigFiles("DeployConfigFilesWorker.txt"));
	}

	public abstract class ModeDeployRunner
	{
		protected void Run(ConfigFiles configFiles)
		{
			new RefreshConfigsRunner(new RefreshConfigFile(), configFiles).Execute();
			new ConfigurationRestorer().Restore(
				new ConfigurationRestoreCommand
				{
					ConfigFiles = configFiles
				});
		}
	}
}