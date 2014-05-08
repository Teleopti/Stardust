using System;
using System.Diagnostics;
using System.IO;
using log4net;
using log4net.Appender;

namespace Teleopti.Support.Code.Tool
{
	public class RefreshConfigsRunner
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RefreshConfigsRunner));
        private readonly ISettingsFileManager _manager;
        private readonly IRefreshConfigFile _refreshConfigFile;

        public RefreshConfigsRunner(ISettingsFileManager manager, IRefreshConfigFile refreshConfigFile)
        {
            _manager = manager;
            _refreshConfigFile = refreshConfigFile;
        }

        public void RefreshThem(ModeFile mode)
        {
	        var settings = _manager.GetReplaceList();
            
            try
            {
				var file = mode.FileContents();
				Array.ForEach(file,f => _refreshConfigFile.SplitAndReplace(f, settings, false));
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