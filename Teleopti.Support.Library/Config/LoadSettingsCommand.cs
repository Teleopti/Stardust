using Teleopti.Support.Library.Config;

namespace Teleopti.Support.Tool.Tool
{
	public class LoadSettingsCommand
	{
		public string File;
	}

	public class SettingsLoader
	{
		public void LoadSettingsFile(LoadSettingsCommand command)
		{
			new SettingsFileManager().LoadFile(command.File);
		}
	}
}