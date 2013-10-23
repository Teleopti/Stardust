﻿using System;
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

            var site = new SiteConfigurable {BusinessUnit = TestState.BusinessUnit, Name = "Västerhaninge"};
            var team = new TeamConfigurable {Name = "Yellow", Site = "Västerhaninge"};
            var contract = new CommonContract();
            var cc = new CommonContractSchedule();
            var scenario = new CommonScenario();
            var cat = new ShiftCategoryConfigurable{Name = "Kattegat"};
            var act = new ActivityConfigurable{Name = "Phone"};
            var act2 = new ActivityConfigurable{Name = "Lunch"};
            Data.Apply(site);
            Data.Apply(team);
            Data.Apply(contract);
            Data.Apply(cc);
            Data.Apply(scenario);
            Data.Apply(cat);
            Data.Apply(act);
            Data.Apply(act2);

            var shift = new ShiftForDate(DateTime.Today, 9, scenario.Scenario, cat.ShiftCategory, act.Activity, act2.Activity);
            //var pp = new PersonPeriodConfigurable{BudgetGroup = "",Contract = contract.Contract.Description.Name, ContractSchedule = cc.ContractSchedule.Description.Name,ppp}
            
            Data.Person("Ola H").Apply(new StockholmTimeZone());
            Data.Person("Ola H").Apply(shift);


        }
    }
}