using System;
using System.Collections.Generic;

namespace Teleopti.Support.Code.Tool
{
    public class RefreshConfigFile
    {
        private readonly IConfigFileTagReplacer _configFileTagReplacer;

        public RefreshConfigFile(IConfigFileTagReplacer configFileTagReplacer)
        {
            _configFileTagReplacer = configFileTagReplacer;
        }

        public void ReplaceFile(string oldFilePath, string newFilePath, IList<SearchReplace> searchReplaces, bool doNotOverWrite)
        {
            if (System.IO.File.Exists(oldFilePath) && doNotOverWrite ) return;
            System.IO.File.Copy(newFilePath, oldFilePath,true);
            _configFileTagReplacer.ReplaceTags(oldFilePath,searchReplaces);
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
    }
}