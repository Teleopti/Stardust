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
			string val;
			if (configurations.TryGetValue(ServerConfiguration.FrameAncestors, out val))
			{
				new FrameAncestorsUpdator().Update(val);
			}
			if (configurations.TryGetValue(ServerConfiguration.InstrumentationKey, out val) && !string.IsNullOrEmpty(val))
			{
				new ApplicationInsightsInstrumentationKeyUpdator().Update(val);
			}
		}
	}

	public class ServerConfiguration
	{
		public const string FrameAncestors = "FrameAncestors";
		public const string InstrumentationKey = "InstrumentationKey";
	}
}