using System;
using System.Collections.Generic;
using System.IO;

namespace Teleopti.Support.Code.Tool
{
    public interface IRefreshConfigFile
    {
        void ReadLinesFromString(string allFilesToRefresh, IList<SearchReplace> searchReplaces, bool doNotOverWrite);
    }

    public class RefreshConfigFile : IRefreshConfigFile
    {
        private readonly IConfigFileTagReplacer _configFileTagReplacer;
        private readonly IMachineKeyChecker _machineKeyChecker;

        public RefreshConfigFile(IConfigFileTagReplacer configFileTagReplacer, IMachineKeyChecker machineKeyChecker)
        {
            _configFileTagReplacer = configFileTagReplacer;
            _machineKeyChecker = machineKeyChecker;
        }

        public void ReplaceFile(string oldFilePath, string newFilePath, IList<SearchReplace> searchReplaces, bool doNotOverWrite)
        {
            oldFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, oldFilePath);
            newFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, newFilePath);

            var dir = GetDirectories(oldFilePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(oldFilePath) && doNotOverWrite ) return;
            File.Copy(newFilePath, oldFilePath,true);
            _configFileTagReplacer.ReplaceTags(oldFilePath,searchReplaces);
           _machineKeyChecker.CheckForMachineKey(oldFilePath);
        }

        public void SplitAndReplace(string oldAndNewFile, IList<SearchReplace> searchReplaces, bool doNotOverWrite)
        {
            var files = oldAndNewFile.Split(',');
            if (files.GetUpperBound(0).Equals(1))
                ReplaceFile(files[0], files[1], searchReplaces, doNotOverWrite);
        }

        public void ReadLinesFromString(string allFilesToRefresh,IList<SearchReplace> searchReplaces, bool doNotOverWrite)
        {
            foreach (var myString in allFilesToRefresh.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                SplitAndReplace(myString, searchReplaces, doNotOverWrite);
        }


        public string GetDirectories(string fullPath)
        {
            var pos = fullPath.LastIndexOf(@"\", StringComparison.InvariantCulture);
            if (pos == -1) return @".";
            return fullPath.Substring(0, pos);
        }
    }
}