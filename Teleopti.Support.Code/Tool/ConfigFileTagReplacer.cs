using System.IO;

namespace Teleopti.Support.Code.Tool
{
    public interface IConfigFileTagReplacer
    {
        void ReplaceTags(string fileToProcess);
    }
    public class ConfigFileTagReplacer : IConfigFileTagReplacer
    {
        private readonly ICommandLineArgument _commandLineArgument;

        public ConfigFileTagReplacer(ICommandLineArgument commandLineArgument)
        {
            _commandLineArgument = commandLineArgument;
        }

        public void ReplaceTags(string fileToProcess)
        {
            var text = File.ReadAllText(fileToProcess);
            text = text.Replace("some text", "new value");
            foreach (var searchReplace in _commandLineArgument.GetSearchReplaceList())
            {
                text = text.Replace(searchReplace.SearchFor, searchReplace.ReplaceWith);
            }
            File.WriteAllText(fileToProcess, text);
        }
    }
}