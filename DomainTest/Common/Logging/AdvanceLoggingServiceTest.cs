using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Logging;
using log4net.Appender;
using log4net.Config;
using log4net.Filter;

namespace Teleopti.Ccc.DomainTest.Common.Logging
{
    [TestFixture]
    public class AdvanceLoggingServiceTest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void ShouldListenToCorrectName()
        {
            var appender = setUpMemoryAppender("Teleopti.AdvanceLoggingService");
            //AdvanceLoggingService.LogSchedulingInfo(null,1,1, TODO);
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
