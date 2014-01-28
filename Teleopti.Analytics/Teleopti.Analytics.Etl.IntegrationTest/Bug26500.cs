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
	public class Bug26500
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
			IPerson person2;
			BasicShiftSetup.AddPerson(out person2, "David J");
			BasicShiftSetup.AddThreeShifts("David J");
			
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
			Assert.That(factSchedules.Count(), Is.EqualTo(192));

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

			// we must manipulate readmodel so it "knows" that the dates on the person are updated
			// person has 3 days changed
			var readM = new ScheduleDayReadModelSetup { Model = readModel };
			Data.Apply(readM);
			readModel.Date = DateTime.Today.AddDays(-1);
			readM = new ScheduleDayReadModelSetup { Model = readModel };
			Data.Apply(readM);
			readModel.Date = DateTime.Today.AddDays(1);
			readM = new ScheduleDayReadModelSetup { Model = readModel };
			Data.Apply(readM);

			// person2 has only one day changed
			readModel.PersonId = person2.Id.GetValueOrDefault();
			readModel.Date = DateTime.Today;
			readM = new ScheduleDayReadModelSetup { Model = readModel };
			Data.Apply(readM);


            result = StepRunner.RunNightly(jobParameters);

			factSchedules = from s in db.fact_schedule select s;

			Assert.That(factSchedules.Count(), Is.EqualTo(192));

			
		}

		//[Test]
		//public void ShouldWorkForBrasil()
		//{
		//	// run this to get a date and time in mart.LastUpdatedPerStep
		//	var etlUpdateDate = new EtlReadModelSetup { BusinessUnit = TestState.BusinessUnit, StepName = "Schedules" };
		//	Data.Apply(etlUpdateDate);

		//	var brasilTimeZone = new BrasilTimeZone {TimeZoneId = 2};
		//	AnalyticsRunner.RunAnalyticsBaseData(new List<IAnalyticsDataSetup>{brasilTimeZone});

		//	IPerson person;
		//	SetupBasicForShifts(out person);

		//	Data.Person("Ola H").Apply(new BrasilianTimeZone());

		//	var readModel = new ScheduleDayReadModel
		//	{
		//		PersonId = person.Id.GetValueOrDefault(),
		//		ColorCode = 0,
		//		ContractTimeTicks = 500,
		//		Date = DateTime.Today,
		//		StartDateTime = DateTime.Today.AddDays(-15).AddHours(8),
		//		EndDateTime = DateTime.Today.AddDays(-15).AddHours(17),
		//		Label = "LABEL",
		//		NotScheduled = false,
		//		Workday = true,
		//		WorkTimeTicks = 600
		//	};

		//	// we must manipulate readmodel so it "knows" that the dates on the person are updated
		//	var readM = new ScheduleDayReadModelSetup { Model = readModel };
		//	Data.Apply(readM);
		//	readModel.Date = DateTime.Today.AddDays(-1);
		//	readM = new ScheduleDayReadModelSetup { Model = readModel };
		//	Data.Apply(readM);
		//	//readModel.Date = DateTime.Today.AddDays(1);
		//	//readM = new ScheduleDayReadModelSetup { Model = readModel };
		//	//Data.Apply(readM);

		//	var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
		//	dateList.Add(DateTime.Today.AddDays(-3), DateTime.Today.AddDays(3), JobCategoryType.Schedule);
		//	var jobParameters = new JobParameters(dateList, 1, "UTC", 15, "", "False", CultureInfo.CurrentCulture)
		//	{
		//		Helper =
		//			new JobHelper(new RaptorRepository(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, ""), null, null)
		//	};

		//	//transfer site, team contract etc from app to analytics
		//	var result = StepRunner.RunBasicStepsBeforeSchedule(jobParameters);

		//	JobStepBase step = new StageScheduleJobStep(jobParameters);
		//	step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

		//	step = new DimShiftLengthJobStep(jobParameters);
		//	step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

		//	step = new FactScheduleJobStep(jobParameters, false);
		//	step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

		//	// now it should have data on all three dates 96 interval
		//	var db = new AnalyticsContext(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix);

		//	var factSchedules = from s in db.fact_schedule select s;

		//	Assert.That(factSchedules.Count(), Is.EqualTo(96));
		//	step = new IntradayStageScheduleJobStep(jobParameters);
		//	step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

		//	step = new FactScheduleJobStep(jobParameters, true);
		//	step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);
		//	factSchedules = from s in db.fact_schedule select s;

			
		//	Assert.That(factSchedules.Count(), Is.EqualTo(96));
		//}

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