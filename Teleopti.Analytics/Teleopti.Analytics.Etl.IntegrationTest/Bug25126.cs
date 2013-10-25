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
using Teleopti.Ccc.TestCommon.TestData.Common;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Generic;
using Teleopti.Ccc.TestCommon.TestData.Setups;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

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
		 
		[Test, Ignore("This is failing as it should now, removing too many days before the insert")]
		public void ShouldWorkForStockholm()
		{
			// run this to get a date and time in mart.LastUpdatedPerStep
			var etlUpdateDate = new EtlReadModelSetup { BusinessUnit = TestState.BusinessUnit, StepName = "Schedules" };
			Data.Apply(etlUpdateDate);

			AnalyticsRunner.RunAnalyticsBaseData(new List<IAnalyticsDataSetup>());

			var site = new SiteConfigurable {BusinessUnit = TestState.BusinessUnit, Name = "Västerhaninge"};
			var team = new TeamConfigurable {Name = "Yellow", Site = "Västerhaninge"};
			var contract = new CommonContract();
			var cc = new CommonContractSchedule();
			var ppp = new PartTimePercentageConfigurable{Name = "ppp"};
			var scenario = new CommonScenario{EnableReporting = true};
			var cat = new ShiftCategoryConfigurable{Name = "Kattegat"};
			var act = new ActivityConfigurable{Name = "Phone"};
			var act2 = new ActivityConfigurable{Name = "Lunch"};
			Data.Apply(site);
			Data.Apply(team);
			Data.Apply(contract);
			Data.Apply(cc);
			Data.Apply(ppp);
			Data.Apply(scenario);
			Data.Apply(cat);
			Data.Apply(act);
			Data.Apply(act2);

			var shift = new ShiftForDate(DateTime.Today.AddDays(-1), 9, scenario.Scenario, cat.ShiftCategory, act.Activity, act2.Activity);
			var shift2 = new ShiftForDate(DateTime.Today, 9, scenario.Scenario, cat.ShiftCategory, act.Activity, act2.Activity);
			var shift3 = new ShiftForDate(DateTime.Today.AddDays(1), 9, scenario.Scenario, cat.ShiftCategory, act.Activity, act2.Activity);
			var pp = new PersonPeriodConfigurable
				{
					BudgetGroup = "",
					Contract = contract.Contract.Description.Name,
					ContractSchedule = cc.ContractSchedule.Description.Name,
					PartTimePercentage = ppp.Name,
					Team = team.Name,
					StartDate = DateTime.Today.AddDays(-6)
				};

			var person = TestState.TestDataFactory.Person("Ola H").Person;
			Data.Person("Ola H").Apply(shift);
			Data.Person("Ola H").Apply(shift2);
			Data.Person("Ola H").Apply(shift3);
			Data.Person("Ola H").Apply(pp);
			Data.Person("Ola H").Apply(new StockholmTimeZone());

			var readModel = new ScheduleDayReadModel
			{
				PersonId = person.Id.GetValueOrDefault(),
				ColorCode = 0,
				ContractTimeTicks = 500,
				Date = DateTime.Today,
				StartDateTime = DateTime.Today.AddDays(-15).AddHours(8),
				EndDateTime = DateTime.Today.AddDays(-15).AddHours(17),
				Label = "LABEL",
				NotScheduled = false,
				Workday = true,
				WorkTimeTicks = 600
			};

			var readM = new ScheduleDayReadModelSetup { Model = readModel };
			Data.Apply(readM);
			//readModel.Date = DateTime.Today.AddDays(-1);
			//readM = new ScheduleDayReadModelSetup { Model = readModel };
			//Data.Apply(readM);
			readModel.Date = DateTime.Today.AddDays(1);
			readM = new ScheduleDayReadModelSetup { Model = readModel };
			Data.Apply(readM);

			var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			dateList.Add(DateTime.Today.AddDays(-3),DateTime.Today.AddDays(3),JobCategoryType.Schedule);
			var jobParameters = new JobParameters(dateList, 1, "UTC",15,"","False",CultureInfo.CurrentCulture)
				{
					Helper =
						new JobHelper(new RaptorRepository(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, ""),null,null)
				};

			var result = StepRunner.RunBasicStepsBeforeSchedule(jobParameters);

			// we must manipulate readmodel so it "knows" that the dates on the person are updated
			JobStepBase step = new StageScheduleJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			step = new FactScheduleJobStep(jobParameters,false);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			// now it should have data on all three dates 96 interval
			var db = new AnalyticsContext(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix);
			
			var query = from s in db.fact_schedule select s;

			Assert.That(query.Count(), Is.EqualTo(96));
			step = new IntradayStageScheduleJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			step = new FactScheduleJobStep(jobParameters, true);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);
			query = from s in db.fact_schedule select s;

			// still it should have data on all three dates 96 interval, in the bug only 64 one day before deleted
			Assert.That(query.Count(), Is.EqualTo(96));
		}

		[Test]
		public void ShouldWorkForBrasil()
		{
			// run this to get a date and time in mart.LastUpdatedPerStep
			var etlUpdateDate = new EtlReadModelSetup { BusinessUnit = TestState.BusinessUnit, StepName = "Schedules" };
			Data.Apply(etlUpdateDate);

			var brasilTimeZone = new BrasilTimeZone {TimeZoneId = 2};
			AnalyticsRunner.RunAnalyticsBaseData(new List<IAnalyticsDataSetup>{brasilTimeZone});

			var site = new SiteConfigurable { BusinessUnit = TestState.BusinessUnit, Name = "Brasilia" };
			var team = new TeamConfigurable { Name = "Yellow", Site = "Brasilia" };
			var contract = new CommonContract();
			var cc = new CommonContractSchedule();
			var ppp = new PartTimePercentageConfigurable { Name = "ppp" };
			var scenario = new CommonScenario { EnableReporting = true };
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

			var shift = new ShiftForDate(DateTime.Today.AddDays(-1), 9, scenario.Scenario, cat.ShiftCategory, act.Activity, act2.Activity);
			var shift2 = new ShiftForDate(DateTime.Today, 9, scenario.Scenario, cat.ShiftCategory, act.Activity, act2.Activity);
			var shift3 = new ShiftForDate(DateTime.Today.AddDays(1), 9, scenario.Scenario, cat.ShiftCategory, act.Activity, act2.Activity);
			var pp = new PersonPeriodConfigurable
			{
				BudgetGroup = "",
				Contract = contract.Contract.Description.Name,
				ContractSchedule = cc.ContractSchedule.Description.Name,
				PartTimePercentage = ppp.Name,
				Team = team.Name,
				StartDate = DateTime.Today.AddDays(-6)
			};

			var person = TestState.TestDataFactory.Person("Ola H").Person;
			Data.Person("Ola H").Apply(shift);
			Data.Person("Ola H").Apply(shift2);
			Data.Person("Ola H").Apply(shift3);
			Data.Person("Ola H").Apply(pp);
			Data.Person("Ola H").Apply(new BrasilianTimeZone());

			var readModel = new ScheduleDayReadModel
			{
				PersonId = person.Id.GetValueOrDefault(),
				ColorCode = 0,
				ContractTimeTicks = 500,
				Date = DateTime.Today,
				StartDateTime = DateTime.Today.AddDays(-15).AddHours(8),
				EndDateTime = DateTime.Today.AddDays(-15).AddHours(17),
				Label = "LABEL",
				NotScheduled = false,
				Workday = true,
				WorkTimeTicks = 600
			};

			var readM = new ScheduleDayReadModelSetup { Model = readModel };
			Data.Apply(readM);
			readModel.Date = DateTime.Today.AddDays(-1);
			readM = new ScheduleDayReadModelSetup { Model = readModel };
			Data.Apply(readM);
			//readModel.Date = DateTime.Today.AddDays(1);
			//readM = new ScheduleDayReadModelSetup { Model = readModel };
			//Data.Apply(readM);

			var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			dateList.Add(DateTime.Today.AddDays(-3), DateTime.Today.AddDays(3), JobCategoryType.Schedule);
			var jobParameters = new JobParameters(dateList, 1, "UTC", 15, "", "False", CultureInfo.CurrentCulture)
			{
				Helper =
					new JobHelper(new RaptorRepository(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, ""), null, null)
			};

			var result = StepRunner.RunBasicStepsBeforeSchedule(jobParameters);

			// we must manipulate readmodel so it "knows" that the dates on the person are updated
			JobStepBase step = new StageScheduleJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			step = new FactScheduleJobStep(jobParameters, false);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			// now it should have data on all three dates 96 interval
			var db = new AnalyticsContext(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix);

			var query = from s in db.fact_schedule select s;

			Assert.That(query.Count(), Is.EqualTo(96));
			step = new IntradayStageScheduleJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			step = new FactScheduleJobStep(jobParameters, true);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);
			query = from s in db.fact_schedule select s;

			// still it should have data on all three dates 96 interval, in the bug only 64 one day before deleted
			Assert.That(query.Count(), Is.EqualTo(96));
		}
		
	}
}