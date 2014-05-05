using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using log4net;
using log4net.Appender;

namespace Teleopti.Support.Code.Tool
{
	public class SaveSsoRunner
	{
		public void Save(ModeFile mode)
		{
			var file = mode.FileContents();
			var authenticationBridgeConfig = file.FirstOrDefault(f => f.Contains("AuthenticationBridge"));
			if (string.IsNullOrEmpty(authenticationBridgeConfig))
			{
				return;
			}

			var filePath = authenticationBridgeConfig.Substring(0,authenticationBridgeConfig.IndexOf(','));
			var directory = Path.GetDirectoryName(filePath);

			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SavedForSSOConfiguration");
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			var tosave = new List<string>();
			var config = File.ReadAllLines(filePath);
			var counter = 0;
			var write = false;
			foreach (var line in config)
			{
				if (line.Contains("<!-- custom section ends here-->"))
				{
					write = false;
					continue;
				}
				if (write)
					tosave.Add(line);
				if (line.Contains("<!-- custom section starts here-->"))
				{
					counter ++;
					tosave.Add(counter.ToString());
					write = true;
				}
				
			}

			File.WriteAllLines(Path.Combine(path,"SSOConfiguration.txt"),tosave);
		}
	}

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