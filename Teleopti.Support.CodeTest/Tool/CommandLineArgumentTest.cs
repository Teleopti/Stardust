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
            var args = new[] { "-DSserver", "-DBdatabase", "-ADanalyticsDB", "-BUburl", "-PWpassword", "-USola", "-?" };
            ICommandLineArgument target = new CommandLineArgument(args);

            target.AnalyticsDatabase.Should().Be("analyticsDB");
            target.AppDatabase.Should().Be("database");
            target.AppServer.Should().Be("server");
            target.BrokerUrl.Should().Be("burl");
            target.Password.Should().Be("password");
            target.UserId.Should().Be("ola");
            target.UseIntegratedSecurity.Should().Be.False();
            target.ConnectionString.Should().Be("Data Source=server;Initial Catalog=database;User ID=ola;Password=password");
            target.ShowHelp.Should().Be.True();
            target.Help.Should().Not.Be.Empty();
        }

        [Test]
        public void ShouldSetIntegratedFromArguments()
        {
            var args = new[] { "-DSserver", "-DBdatabase", "-ADanalyticsDB", "-BUburl", "-EE", };
            ICommandLineArgument target = new CommandLineArgument(args);

            target.AnalyticsDatabase.Should().Be("analyticsDB");
            target.AppDatabase.Should().Be("database");
            target.AppServer.Should().Be("server");
            target.BrokerUrl.Should().Be("burl");
            target.UseIntegratedSecurity.Should().Be.True();
            target.ConnectionString.Should().Be("Data Source=server;Initial Catalog=database;Integrated Security=True");
        }

        
    }
}
