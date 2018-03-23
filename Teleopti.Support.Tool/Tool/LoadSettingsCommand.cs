using Teleopti.Support.Library.Config;

namespace Teleopti.Support.Tool.Tool
{
	internal class LoadSettingsCommand : ISupportCommand
	{
		private readonly SettingsFileManager fileManager = new SettingsFileManager();
		private readonly string _file;

		public LoadSettingsCommand(string file)
		{
			_file = file;
		}

		public void Execute()
		{
			fileManager.LoadFile(_file);
		}
	}
}