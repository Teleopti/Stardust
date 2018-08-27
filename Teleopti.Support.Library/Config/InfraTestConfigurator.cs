namespace Teleopti.Support.Library.Config
{
	public class InfraTestConfigCommand
	{
		public string Server;
		public string ApplicationDatabase = "InfraTest_CCC7";
		public string AnalyticsDatabase = "InfraTest_Analytics";
		public string ToggleMode = "ALL";
		public string SqlAuthString;
	}

	public class InfraTestConfigurator
	{
		public void Configure(InfraTestConfigCommand command)
		{
			new SettingsPreparer()
				.Prepare(new SettingPrepareCommand
				{
					Server = command.Server,
					ApplicationDatabase = command.ApplicationDatabase,
					AnalyticsDatabase = command.AnalyticsDatabase,
					ToggleMode = command.ToggleMode,
					SqlAuthString = command.SqlAuthString
				});

			new ModeTestRunner()
				.Run(new ModeTestCommand());
		}
	}
}