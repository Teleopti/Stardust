using System;
using System.Diagnostics;
using System.IO;
using log4net;
using log4net.Appender;
using Teleopti.Support.Library.Config;

namespace Teleopti.Support.Tool.Tool
{
	public class RefreshConfigsRunner : ISupportCommand
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RefreshConfigsRunner));
        private readonly RefreshConfigFile _refreshConfigFile;
		private readonly Func<ConfigFiles> _configFiles;

		public RefreshConfigsRunner(RefreshConfigFile refreshConfigFile, Func<ConfigFiles> configFiles)
		{
			_refreshConfigFile = refreshConfigFile;
			_configFiles = configFiles;
		}

        public void Execute()
        {
	        var searchReplaces = new SettingsFileManager().ReadFile();
            
            try
            {
				var file = _configFiles().FileContents();
	            Array.ForEach(file, f => _refreshConfigFile.ReplaceFile(f, searchReplaces));
            }
            catch (Exception e)
            {
                Logger.Error("Failed to process the config files", e);
                foreach (IAppender appender in LogManager.GetRepository().GetAppenders())
                {
                    var theAppender = appender as FileAppender;
                    if (theAppender != null)
                    {
                        var fileAppender = theAppender;
                        string filePath = fileAppender.File;
                        if (new FileInfo(filePath).Length != 0)
                        {
                            Process.Start("Notepad.exe", filePath);
                        }
                    }
                }
            }


        }

	    
    }
}