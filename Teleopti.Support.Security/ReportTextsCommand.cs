namespace Teleopti.Support.Security
{

	public class ReportTextsCommand :ICommandLineCommand
	{
		public int Execute(IDatabaseArguments commandLineArgument)
		{
			return TextLoader.LoadAllTextsToDatabase(commandLineArgument.AnalyticsDbConnectionString);
		}
	}
}