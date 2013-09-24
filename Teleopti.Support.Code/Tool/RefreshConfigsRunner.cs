using System;
using System.Diagnostics;
using System.Globalization;
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

        public void RefreshThem(string mode)
        {
            var file = "ConfigFiles.txt";
            if (mode.ToUpper(CultureInfo.InvariantCulture).Equals("DEPLOY"))
                file = "DeployConfigFiles.txt";
            if (mode.ToUpper(CultureInfo.InvariantCulture).Equals("TEST"))
                file = "BuildServerConfigFiles.txt";
            
            var settings = _manager.GetReplaceList();
            
            try
            {
                _refreshConfigFile.ReadLinesFromString(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ConfigFiles\" + file)), settings, false);
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