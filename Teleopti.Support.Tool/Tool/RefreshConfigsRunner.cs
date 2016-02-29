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
        private readonly IRefreshConfigFile _refreshConfigFile;

        public RefreshConfigsRunner(IRefreshConfigFile refreshConfigFile)
        {
            _refreshConfigFile = refreshConfigFile;
        }

        public void Execute(ModeFile mode)
        {
	        var searchReplaces = new SettingsFileManager().ReadFile();
            
            try
            {
				var file = mode.FileContents();
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