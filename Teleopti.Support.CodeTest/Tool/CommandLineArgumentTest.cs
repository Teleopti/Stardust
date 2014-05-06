using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Support.Code.Tool;

namespace Teleopti.Support.CodeTest.Tool
{
    [TestFixture]
    public class CommandLineArgumentTest
    {
        
        [Test]
        public void ShouldSetPropertiesFromArguments()
        {
            var args = new[] { "-MODeploy", "-?" };
            ICommandLineArgument target = new CommandLineArgument(args);

            target.Mode.Type.Should().Be("Deploy");
    
            target.ShowHelp.Should().Be.True();
            target.Help.Should().Not.Be.Empty();
        }
  
    }
}
