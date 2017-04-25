using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Logging;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Filter;

namespace Teleopti.Ccc.DomainTest.Common.Logging
{
    [TestFixture]
	[TestWithStaticDependenciesAvoidUse]
	public class AdvanceLoggingServiceTest
    {
        private SchedulingOptions _schedulingOptions;

        [SetUp]
        public void Setup()
        {
            _schedulingOptions = new SchedulingOptions();
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void ShouldListenToCorrectName()
        {
            var appender = setUpMemoryAppender("Teleopti.AdvanceLoggingService");
            AdvanceLoggingService.LogSchedulingInfo(_schedulingOptions,1,1,TestCode );
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

        public void TestCode()
        {
            
        }

        [Test]
        public void VerifyLogSchedulingInfo()
        {
            AdvanceLoggingService.LogSchedulingInfo(_schedulingOptions,1,1,TestCode );

            Assert.AreEqual(GlobalContext.Properties["GeneralOptions"], "Scheduling,Prefrences,Rotations");
            Assert.AreEqual(GlobalContext.Properties["Agents"], "1");
            Assert.AreEqual(GlobalContext.Properties["SkillDays"], "1");

        }

        [Test]
        public void VerifyLogOptimizationInfo()
        {
            AdvanceLoggingService.LogOptimizationInfo( new OptimizationPreferences(), 2, 2, TestCode);

            Assert.AreEqual(GlobalContext.Properties["GeneralOptions"], "StandardDeviation");
            Assert.AreEqual(GlobalContext.Properties["Agents"], "2");
            Assert.AreEqual(GlobalContext.Properties["SkillDays"], "2");

        }

        [Test]
        public void VerifyGeneralInfo()
        {
            AdvanceLoggingService.LogSchedulingInfo( _schedulingOptions , 2, 2, TestCode);

            Assert.AreEqual(GlobalContext.Properties["BU"], "Business unit used in test");
            Assert.AreEqual(GlobalContext.Properties["Agents"], "2");
            Assert.AreEqual(GlobalContext.Properties["SkillDays"], "2");

        }

    }
}
