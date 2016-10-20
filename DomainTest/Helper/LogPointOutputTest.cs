using log4net.Appender;
using log4net.Config;
using log4net.Filter;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.DomainTest.Common;

namespace Teleopti.Ccc.DomainTest.Helper
{
    [TestFixture]
    public class LogPointOutputTest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void ShouldListenToCorrectName()
        {
            var appender = setUpMemoryAppender("Teleopti.LogPointOutput");
            LogPointOutput.LogInfo("Foo", "starting");
            appender.GetEvents().Should().Not.Be.Empty();
        }


        
        private static MemoryAppender setUpMemoryAppender(string listeningTo)
        {
            var filter = new LoggerMatchFilter { LoggerToMatch = listeningTo };
            var memAppender = new MemoryAppender();
            memAppender.AddFilter(filter);
            memAppender.AddFilter(new DenyAllFilter());
            BasicConfigurator.Configure(memAppender);
            return memAppender;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), TearDown]
        public void Teardown()
        {
            BasicConfigurator.Configure(new DoNothingAppender());
        }
        
    }

    
}