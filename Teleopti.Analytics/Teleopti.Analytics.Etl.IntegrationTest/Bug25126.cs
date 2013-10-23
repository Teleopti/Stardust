using System;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Common;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Generic;
using Teleopti.Ccc.TestCommon.TestData.Setups;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
    [TestFixture]
    public class Bug25126
    {
        [SetUp]
        public void Setup()
        {
            SetupFixtureForAssembly.BeginTest();
        }

        [TearDown]
        public void TearDown()
        {
            SetupFixtureForAssembly.EndTest();
        }
         
        [Test]
        public void ShouldWork()
        {
            var analyticsDataFactory = new AnalyticsDataFactory();
            var timeZones = new UtcAndCetTimeZones();
            var dates = new CurrentBeforeAndAfterWeekDates();
            var dataSource = new ExistingDatasources(timeZones);
			var businessUnit = new BusinessUnit(TestState.BusinessUnit, dataSource);
            var intervals = new QuarterOfAnHourInterval();
			//var person = TestState.TestDataFactory.Person("Ashley Andeen").Person;

            analyticsDataFactory.Setup(timeZones);
			analyticsDataFactory.Setup(new EternityAndNotDefinedDate());			
			analyticsDataFactory.Setup(dates);
			analyticsDataFactory.Setup(businessUnit);
            analyticsDataFactory.Setup(intervals);
            analyticsDataFactory.Setup(new FillBridgeTimeZoneFromData(dates, intervals, timeZones, dataSource));

            analyticsDataFactory.Persist();

            var scenario = new CommonScenario();
            Data.Apply(scenario);
            
            
            
            //var shift = new ShiftForDate(DateTime.Today, 9, scenario.Scenario, category, activityPhone, activityLunch);

            //Data.Person("Ashley Andeen").Apply(shift);

        }
    }
}