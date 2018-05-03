using Teleopti.Support.Library.Config;

namespace Teleopti.Support.Tool.Tool
{
	public class SetSettingCommand
	{
		public string SearchFor;
		public string ReplaceWith;
	}

	public class SettingsSetter
	{
		public void UpdateSettingsFile(SetSettingCommand command)
		{
			new SettingsFileManager().UpdateFile(command.SearchFor, command.ReplaceWith);
		}
	}
}