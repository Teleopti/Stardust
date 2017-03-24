using Teleopti.Support.Tool.DataLayer;

namespace Teleopti.Support.Tool.Tool
{
	public class ConfigServerCommand : ISupportCommand
	{
		private readonly DBHelper _dbHelper;

		public ConfigServerCommand(string connectionString)
		{
			_dbHelper = new DBHelper(connectionString);
		}

		public void Execute(ModeFile modeFile)
		{
			var configurations = _dbHelper.GetServerConfigurations();
			foreach (var configuration in configurations)
			{
				if (configuration.Key == ServerConfiguration.FrameAncestors)
				{
					new FrameAncestorsUpdator().Update(configuration.Value);
				}
			}
		}
	}

	public class ServerConfiguration
	{
		public const string FrameAncestors = "FrameAncestors";
	}
}