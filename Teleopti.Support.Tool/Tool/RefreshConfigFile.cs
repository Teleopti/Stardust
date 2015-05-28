using System;
using System.Collections.Generic;
using System.IO;

namespace Teleopti.Support.Code.Tool
{
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
            if (files.Length.Equals(2))
                ReplaceFile(files[0], files[1], searchReplaces, doNotOverWrite);
        }

        public string GetDirectories(string fullPath)
        {
            var pos = fullPath.LastIndexOf(@"\", StringComparison.InvariantCulture);
            if (pos == -1) return @".";
            return fullPath.Substring(0, pos);
        }
    }

	public interface IRefreshConfigFile
	{
		void SplitAndReplace(string oldAndNewFile, IList<SearchReplace> searchReplaces, bool doNotOverWrite);
	}
}