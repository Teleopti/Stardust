using System;
using System.Collections.Generic;
using System.IO;

namespace Teleopti.Support.Code.Tool
{
    public interface IConfigFileTagReplacer
    {
        void ReplaceTags(string fileToProcess, IList<SearchReplace> searchReplaces);
    }
    public class ConfigFileTagReplacer : IConfigFileTagReplacer
    {
        public void ReplaceTags(string fileToProcess, IList<SearchReplace> searchReplaces )
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileToProcess);
            var text = File.ReadAllText(path);
            foreach (var searchReplace in searchReplaces)
            {
                text = text.Replace(searchReplace.SearchFor, searchReplace.ReplaceWith);
            }
            File.WriteAllText(path, text);
        }
    }
}