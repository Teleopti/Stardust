using System;

namespace Teleopti.Support.Library.Config
{
	public class RefreshConfigsRunner
	{
		private readonly RefreshConfigFile _refreshConfigFile;
		private readonly ConfigFiles _configFiles;

		public RefreshConfigsRunner(RefreshConfigFile refreshConfigFile, ConfigFiles configFiles)
		{
			_refreshConfigFile = refreshConfigFile;
			_configFiles = configFiles;
		}

		public void Execute()
		{
			var searchReplaces = new SettingsFileManager().ReadFile();
			var file = _configFiles.FileContents();
			Array.ForEach(file, f => _refreshConfigFile.ReplaceFile(f, searchReplaces));
		}
	}
}