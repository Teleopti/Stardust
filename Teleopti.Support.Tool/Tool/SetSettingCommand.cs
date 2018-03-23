using Teleopti.Support.Library.Config;

namespace Teleopti.Support.Tool.Tool
{
	internal class SetSettingCommand : ISupportCommand
	{
		private readonly SettingsFileManager fileManager = new SettingsFileManager();
		private readonly string _searchFor;
		private readonly string _replaceWith;

		public SetSettingCommand(string searchFor, string replaceWith)
		{
			_searchFor = searchFor;
			_replaceWith = replaceWith;
		}

		public void Execute()
		{
			fileManager.UpdateFile(_searchFor, _replaceWith);
		}
	}
}