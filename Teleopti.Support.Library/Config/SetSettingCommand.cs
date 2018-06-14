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
		public void UpdateSettingsFile(params SetSettingCommand[] commands)
		{
			var manager = new SettingsFileManager();
			commands.ForEach(command => manager.UpdateFile(command.SearchFor, command.ReplaceWith));
		}
	}
}