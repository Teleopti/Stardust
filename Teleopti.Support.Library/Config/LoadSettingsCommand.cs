namespace Teleopti.Support.Library.Config
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