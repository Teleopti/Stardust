using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Teleopti.Analytics.Etl.IntegrationTest.Models;
using Teleopti.Analytics.Etl.IntegrationTest.TestData;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.MultipleDate;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Specific;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
	[TestFixture]
	public class Bug27672
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
		public void ShouldWorkForStockholm()
		{
			// run this to get a date and time in mart.LastUpdatedPerStep
			var etlUpdateDate = new EtlReadModelSetup { BusinessUnit = TestState.BusinessUnit, StepName = "Schedules" };
			Data.Apply(etlUpdateDate);

			AnalyticsRunner.RunAnalyticsBaseData(new List<IAnalyticsDataSetup>());

			IPerson person;
			BasicShiftSetup.SetupBasicForShifts();
			BasicShiftSetup.AddPerson(out person, "Ola H");
			BasicShiftSetup.AddThreeShifts("Ola H");
			
			var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			dateList.Add(DateTime.Today.AddDays(-3),DateTime.Today.AddDays(3),JobCategoryType.Schedule);
			var jobParameters = new JobParameters(dateList, 1, "UTC",15,"","False",CultureInfo.CurrentCulture)
				{
					Helper =
						new JobHelper(new RaptorRepository(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, ""),null,null)
				};

			//transfer site, team contract etc from app to analytics
			var result = StepRunner.RunNightly(jobParameters);

			// now it should have data on all three dates on both persons 192 interval
			var db = new AnalyticsContext(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix);
			var factSchedules = from s in db.fact_schedule select s;
			Assert.That(factSchedules.Count(), Is.EqualTo(96));
            
            //run again now with fewer days
            dateList.Clear();
            dateList.Add(DateTime.Today.AddDays(0), DateTime.Today.AddDays(0), JobCategoryType.Schedule);
            jobParameters = new JobParameters(dateList, 1, "UTC", 15, "", "False", CultureInfo.CurrentCulture)
            {
                Helper =
                    new JobHelper(new RaptorRepository(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, ""), null, null)
            };
            
            result = StepRunner.RunNightly(jobParameters);

			factSchedules = from s in db.fact_schedule select s;

			Assert.That(factSchedules.Count(), Is.EqualTo(96));

		}
        [Test]
        public void ShouldWorkForCanberra()
        {
            // run this to get a date and time in mart.LastUpdatedPerStep
            var etlUpdateDate = new EtlReadModelSetup { BusinessUnit = TestState.BusinessUnit, StepName = "Schedules" };
            Data.Apply(etlUpdateDate);

            AnalyticsRunner.RunAnalyticsBaseData(new List<IAnalyticsDataSetup>());

            IPerson person;
            BasicShiftSetup.SetupBasicForShifts();
            BasicShiftSetup.AddPerson(out person, "Ola H");
            Data.Person("Ola H").Apply(new AustralianTimeZone());
            BasicShiftSetup.AddThreeShifts("Ola H");

            var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            dateList.Add(DateTime.Today.AddDays(-3), DateTime.Today.AddDays(3), JobCategoryType.Schedule);
            var jobParameters = new JobParameters(dateList, 1, "UTC", 15, "", "False", CultureInfo.CurrentCulture)
            {
                Helper =
                    new JobHelper(new RaptorRepository(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, ""), null, null)
            };

            //transfer site, team contract etc from app to analytics
            var result = StepRunner.RunNightly(jobParameters);

            // now it should have data on all three dates on both persons 192 interval
            var db = new AnalyticsContext(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix);
            var factSchedules = from s in db.fact_schedule select s;
            Assert.That(factSchedules.Count(), Is.EqualTo(96));

            //run again now with fewer days
            dateList.Clear();
            dateList.Add(DateTime.Today.AddDays(0), DateTime.Today.AddDays(0), JobCategoryType.Schedule);
            jobParameters = new JobParameters(dateList, 1, "UTC", 15, "", "False", CultureInfo.CurrentCulture)
            {
                Helper =
                    new JobHelper(new RaptorRepository(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, ""), null, null)
            };

            result = StepRunner.RunNightly(jobParameters);

            factSchedules = from s in db.fact_schedule select s;

            Assert.That(factSchedules.Count(), Is.EqualTo(96));

        }

      

		private static void SetupBasicForShifts(out IPerson person, string name, string extraName)
		{
			var site = new SiteConfigurable { BusinessUnit = TestState.BusinessUnit.Name, Name = "Västerhaninge" + extraName };
			var team = new TeamConfigurable { Name = "Yellow" + extraName, Site = "Västerhaninge" + extraName };
			var contract = new ContractConfigurable { Name = "Kontrakt" + extraName };
			var cc = new ContractScheduleConfigurable { Name = "Kontraktsschema" + extraName };
			var ppp = new PartTimePercentageConfigurable { Name = "ppp" + extraName };
			var scenario = new ScenarioConfigurable
			{
				EnableReporting = true,
				Name = "Scenario",
				BusinessUnit = TestState.BusinessUnit.Name
			};
			var cat = new ShiftCategoryConfigurable { Name = "Kattegat" };
			var act = new ActivityConfigurable { Name = "Phone" };
			var act2 = new ActivityConfigurable { Name = "Lunch" };
			Data.Apply(site);
			Data.Apply(team);
			Data.Apply(contract);
			Data.Apply(cc);
			Data.Apply(ppp);
			Data.Apply(scenario);
			Data.Apply(cat);
			Data.Apply(act);
			Data.Apply(act2);

			var shift = new ShiftForDate(DateTime.Today.AddDays(-1), 9, scenario.Scenario, cat.ShiftCategory, act.Activity,
										 act2.Activity);
			var shift2 = new ShiftForDate(DateTime.Today, 9, scenario.Scenario, cat.ShiftCategory, act.Activity, act2.Activity);
			var shift3 = new ShiftForDate(DateTime.Today.AddDays(1), 9, scenario.Scenario, cat.ShiftCategory, act.Activity,
										  act2.Activity);
			var pp = new PersonPeriodConfigurable
			{
				BudgetGroup = "",
				Contract = contract.Contract.Description.Name,
				ContractSchedule = cc.ContractSchedule.Description.Name,
				PartTimePercentage = ppp.Name,
				Team = team.Name,
				StartDate = DateTime.Today.AddDays(-6)
			};

			person = TestState.TestDataFactory.Person(name).Person;
			Data.Person(name).Apply(shift);
			Data.Person(name).Apply(shift2);
			Data.Person(name).Apply(shift3);
			Data.Person(name).Apply(pp);
			Data.Person(name).Apply(new StockholmTimeZone());
		}

		private static void SetupOneShiftOnNewPerson(out IPerson person)
		{
			var site = new SiteConfigurable { BusinessUnit = TestState.BusinessUnit.Name, Name = "Västerhaninge" };
			var team = new TeamConfigurable { Name = "Yellow", Site = "Västerhaninge" };
			var contract = new ContractConfigurable { Name = "Kontrakt1" };
			var cc = new ContractScheduleConfigurable { Name = "Kontraktsschema1" };
			var ppp = new PartTimePercentageConfigurable { Name = "ppp1" };
			var scenario = new ScenarioConfigurable
			{
				EnableReporting = true,
				Name = "Scenario",
				BusinessUnit = TestState.BusinessUnit.Name
			};
			var cat = new ShiftCategoryConfigurable { Name = "Kattegat" };
			var act = new ActivityConfigurable { Name = "Phone" };
			var act2 = new ActivityConfigurable { Name = "Lunch" };
			Data.Apply(site);
			Data.Apply(team);
			Data.Apply(contract);
			Data.Apply(cc);
			Data.Apply(ppp);
			Data.Apply(scenario);
			Data.Apply(cat);
			Data.Apply(act);
			Data.Apply(act2);

			var shift = new ShiftForDate(DateTime.Today.AddDays(-1), 9, scenario.Scenario, cat.ShiftCategory, act.Activity,
										 act2.Activity);
			
			var pp = new PersonPeriodConfigurable
			{
				BudgetGroup = "",
				Contract = contract.Contract.Description.Name,
				ContractSchedule = cc.ContractSchedule.Description.Name,
				PartTimePercentage = ppp.Name,
				Team = team.Name,
				StartDate = DateTime.Today.AddDays(-6)
			};

			person = TestState.TestDataFactory.Person("David J").Person;
			Data.Person("David J").Apply(shift);
			Data.Person("David J").Apply(pp);
			Data.Person("David J").Apply(new StockholmTimeZone());
		}
	}
}