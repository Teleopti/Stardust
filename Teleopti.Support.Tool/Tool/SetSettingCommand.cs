using Teleopti.Support.Library.Config;

namespace Teleopti.Support.Tool.Tool
{
	internal class SetSettingCommand : ISupportCommand
	{
		private readonly string _searchFor;
		private readonly string _replaceWith;

		public SetSettingCommand(string searchFor, string replaceWith)
		{
			_searchFor = searchFor;
			_replaceWith = replaceWith;
		}

		public void Execute()
		{
			new SettingsFileManager().UpdateFile(_searchFor, _replaceWith);
		}
	}
}