using NUnit.Framework;
using Teleopti.Support.Code.Tool;

namespace Teleopti.Support.CodeTest.Tool
{
    [TestFixture()]
    public class ConfigFileTagReplacerTest
    {
        
        [Test()]
        public void ReplaceTagsTest()
        {
            var args = new[] { "-DSserver", "-DBdatabase", "-ADanalyticsDB", "-BUburl", "-PWpassword" };
            ICommandLineArgument arg = new CommandLineArgument(args);
            var target = new ConfigFileTagReplacer(arg);
            // should not be any tags to replace, i hope
            target.ReplaceTags("ConfigFiles/ConfigFiles.txt");
        }
    }
}
