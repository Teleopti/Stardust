namespace Teleopti.Support.Security.Library
{

	public class ReportTextsCommand :ICommandLineCommand
	{
		public int Execute(UpgradeCommand commandLineArgument)
		{
			return TextLoader.LoadAllTextsToDatabase(commandLineArgument.AnalyticsConnectionString);
		}
	}
}