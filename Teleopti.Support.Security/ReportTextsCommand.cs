namespace Teleopti.Support.Security
{

	public class ReportTextsCommand :ICommandLineCommand
	{
		public int Execute(CommandLineArgument commandLineArgument)
		{
			return TextLoader.LoadAllTextsToDatabase(commandLineArgument.DestinationConnectionString);
		}
	}
}