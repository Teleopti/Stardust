using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Support.Security;

namespace Teleopti.Support.CodeTest.Tool
{
	[TestFixture]
	public class SecurityArgumentsTest
	{
		private string arguments =
			"-DStcp:s8v4m110k9.database.windows.net -APteleoptirnd_TeleoptiApp -ANteleoptirnd_TeleoptiAnalytics -CDteleoptirnd_TeleoptiAnalytics -CSServer=tcp:s8v4m110k9.database.windows.net;UID=teleopti;Password=T3l30pt1 -DUteleopti -DPT3l30pt1";

		[Test]
		public void ShouldDeliverConnectionStringBasedOnBaseConnstring()
		{
			var argsArray = arguments.Split(' ');
			var commandArgs = new CommandLineArgument(argsArray);
			var dbArgs =  commandArgs.GetDatabaseArguments();
			dbArgs.ApplicationDbConnectionString.Should().Not.Be.Empty();
		}
	}
}