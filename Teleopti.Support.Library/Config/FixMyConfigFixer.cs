using System.IO;
using Teleopti.Support.Library.Folders;

namespace Teleopti.Support.Library.Config
{
	public class FixMyConfigCommand
	{
		public string Server;
		public string ApplicationDatabase;
		public string AnalyticsDatabase;
		public string ToggleMode = "ALL";
	}

	public class FixMyConfigFixer
	{
		public void Fix(FixMyConfigCommand command)
		{
			new SettingsPreparer()
				.Prepare(new SettingPrepareCommand
				{
					Server = command.Server,
					ApplicationDatabase = command.ApplicationDatabase,
					AnalyticsDatabase = command.AnalyticsDatabase,
					ToggleMode = command.ToggleMode,
					MachineKeyValidationKey = "754534E815EF6164CE788E521F845BA4953509FA45E321715FDF5B92C5BD30759C1669B4F0DABA17AC7BBF729749CE3E3203606AB49F20C19D342A078A3903D1",
					MachineKeyDecryptionKey = "3E1AD56713339011EB29E98D1DF3DBE1BADCF353938C3429"
				});

			new ModeDebugRunner()
				.Run(new ModeDebugCommand());
		}
	}
}