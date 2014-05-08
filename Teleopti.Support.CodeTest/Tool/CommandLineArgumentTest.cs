using NUnit.Framework;
using Rhino.Mocks;
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
	        var mockCommand = MockRepository.GenerateMock<ISupportCommand>();
            var args = new[] { "-MODeploy", "-?" };
            ICommandLineArgument target = new CommandLineArgument(args,_ => mockCommand);

            target.Mode.Type.Should().Be("Deploy");
    
            target.Command.Should().Be.SameInstanceAs(mockCommand);
        }
  
    }
}
