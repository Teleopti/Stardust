namespace Teleopti.Support.Security
{

	public class ReportTextsCommand :ICommandLineCommand
	{
		public int Execute(UpgradeCommand commandLineArgument)
		{
			return TextLoader.LoadAllTextsToDatabase(commandLineArgument.AnalyticsDbConnectionString);
		}
	}
}