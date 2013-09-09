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
            var text = File.ReadAllText(fileToProcess);
            foreach (var searchReplace in searchReplaces)
            {
                text = text.Replace(searchReplace.SearchFor, searchReplace.ReplaceWith);
            }
            File.WriteAllText(fileToProcess, text);
        }
    }
}