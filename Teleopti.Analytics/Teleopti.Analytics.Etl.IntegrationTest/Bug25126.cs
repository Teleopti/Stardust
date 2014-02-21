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
		public void ShouldWorkForStockholm()
		{
			// run this to get a date and time in mart.LastUpdatedPerStep
			var etlUpdateDate = new EtlReadModelSetup { BusinessUnit = TestState.BusinessUnit, StepName = "Schedules" };
			Data.Apply(etlUpdateDate);

			AnalyticsRunner.RunAnalyticsBaseData(new List<IAnalyticsDataSetup>());

			IPerson person;
            BasicShiftSetup.SetupBasicForShifts();
            BasicShiftSetup.AddPerson(out person, "Ola H","");

			var cat = new ShiftCategoryConfigurable { Name = "Kattegat", Color = "Green" };
			var activityPhone = new ActivityConfigurable { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			var activityLunch = new ActivityConfigurable { Name = "Lunch", Color = "Red" };

			Data.Apply(cat);
			Data.Apply(activityPhone);
			Data.Apply(activityLunch);

            BasicShiftSetup.AddThreeShifts("Ola H", cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);

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

			//transfer site, team contract etc from app to analytics
			var result = StepRunner.RunBasicStepsBeforeSchedule(jobParameters);

			
			JobStepBase step = new StageScheduleJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			step = new DimShiftLengthJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			step = new FactScheduleJobStep(jobParameters,false);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			step = new FactScheduleDayCountJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			// now it should have data on all three dates 96 interval
			var db = new AnalyticsContext(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix);
			
			var factSchedules = from s in db.fact_schedule select s;

			Assert.That(factSchedules.Count(), Is.EqualTo(96));
			step = new IntradayStageScheduleJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			step = new FactScheduleJobStep(jobParameters, true);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			factSchedules = from s in db.fact_schedule select s;

			// still it should have data on all three dates 96 interval, in the bug only 64 one day extra before the two was deleted
			Assert.That(factSchedules.Count(), Is.EqualTo(96));

			step = new FactScheduleDayCountJobStep(jobParameters, true);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);
		}

		[Test]
		public void ShouldWorkForBrasil()
		{
			// run this to get a date and time in mart.LastUpdatedPerStep
			var etlUpdateDate = new EtlReadModelSetup { BusinessUnit = TestState.BusinessUnit, StepName = "Schedules" };
			Data.Apply(etlUpdateDate);

			var brasilTimeZone = new BrasilTimeZone {TimeZoneId = 2};
			AnalyticsRunner.RunAnalyticsBaseData(new List<IAnalyticsDataSetup>{brasilTimeZone});

			var cat = new ShiftCategoryConfigurable { Name = "Kattegat", Color = "Green" };
			var activityPhone = new ActivityConfigurable { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			var activityLunch = new ActivityConfigurable { Name = "Lunch", Color = "Red" };

			Data.Apply(cat);
			Data.Apply(activityPhone);
			Data.Apply(activityLunch);

			IPerson person;
            BasicShiftSetup.SetupBasicForShifts();
            BasicShiftSetup.AddPerson(out person, "Ola H","");
			BasicShiftSetup.AddThreeShifts("Ola H", cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);

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

			// we must manipulate readmodel so it "knows" that the dates on the person are updated
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

			//transfer site, team contract etc from app to analytics
			var result = StepRunner.RunBasicStepsBeforeSchedule(jobParameters);

			JobStepBase step = new StageScheduleJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			step = new DimShiftLengthJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			step = new FactScheduleJobStep(jobParameters, false);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			// now it should have data on all three dates 96 interval
			var db = new AnalyticsContext(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix);

			var factSchedules = from s in db.fact_schedule select s;

			Assert.That(factSchedules.Count(), Is.EqualTo(96));
			step = new IntradayStageScheduleJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			step = new FactScheduleJobStep(jobParameters, true);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);
			factSchedules = from s in db.fact_schedule select s;

			
			Assert.That(factSchedules.Count(), Is.EqualTo(96));
		}

		
	}
}