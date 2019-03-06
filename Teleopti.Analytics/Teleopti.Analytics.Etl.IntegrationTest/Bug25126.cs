using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.MultipleDate;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.IntegrationTest.TestData;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Specific;

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
			var testDate = new DateTime(2013, 06, 15);

			// run this to get a date and time in mart.LastUpdatedPerStep
			var etlUpdateDate = new EtlReadModelSetup { BusinessUnit = TestState.BusinessUnit, StepName = "Schedules" };
			Data.Apply(etlUpdateDate);

			AnalyticsRunner.RunAnalyticsBaseData(new List<IAnalyticsDataSetup>(), testDate);

			IPerson person;
			BasicShiftSetup.SetupBasicForShifts();
			BasicShiftSetup.AddPerson(out person, "Ola H", "", testDate);

			var cat = new ShiftCategoryConfigurable { Name = "Kattegat", Color = "Green" };
			var activityPhone = new ActivitySpec { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			var activityLunch = new ActivitySpec { Name = "Lunch", Color = "Red" };

			Data.Apply(cat);
			Data.Apply(activityPhone);
			Data.Apply(activityLunch);

			BasicShiftSetup.AddThreeShifts("Ola H", cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity, testDate);

			var readModel = new ScheduleDayReadModel
			{
				PersonId = person.Id.GetValueOrDefault(),
				ColorCode = 0,
				ContractTimeTicks = 500,
				Date = DateTime.Today,
				StartDateTime = testDate.AddDays(-15).AddHours(8),
				EndDateTime = testDate.AddDays(-15).AddHours(17),
				Label = "LABEL",
				NotScheduled = false,
				Workday = true,
				WorkTimeTicks = 600
			};

			// we must manipulate readmodel so it "knows" that the dates on the person are updated
			var readM = new ScheduleDayReadModelSetup { Model = readModel };
			Data.Apply(readM);

			readModel.Date = DateTime.Today.AddDays(1);
			readM = new ScheduleDayReadModelSetup { Model = readModel };
			Data.Apply(readM);

			var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			dateList.Add(testDate.AddDays(-3), testDate.AddDays(3), JobCategoryType.Schedule);
			var jobParameters = new JobParameters(
				dateList, 1, "UTC", 15, "", "False",
				CultureInfo.CurrentCulture,
				new FakeContainerHolder(), false)
				{
					Helper =
						new JobHelperForTest(new RaptorRepository(InfraTestConfigReader.AnalyticsConnectionString(), null, null), null)
				};

			//transfer site, team contract etc from app to analytics
			var result = StepRunner.RunBasicStepsBeforeSchedule(jobParameters);


			JobStepBase step = new StageScheduleJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			step = new DimShiftLengthJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			step = new FactScheduleJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			step = new FactScheduleDayCountJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			// now it should have data on all three dates 96 interval
			var factSchedules = SqlCommands.RowsInFactSchedule();

			Assert.That(factSchedules, Is.EqualTo(96));
			step = new StageScheduleJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			step = new FactScheduleJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			factSchedules = SqlCommands.RowsInFactSchedule();
			// still it should have data on all three dates 96 interval, in the bug only 64 one day extra before the two was deleted
			Assert.That(factSchedules, Is.EqualTo(96));

			step = new FactScheduleDayCountJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);
		}

		[Test]
		public void ShouldWorkForBrasil()
		{
			var testDate = new DateTime(2013, 06, 15);

			// run this to get a date and time in mart.LastUpdatedPerStep
			var etlUpdateDate = new EtlReadModelSetup { BusinessUnit = TestState.BusinessUnit, StepName = "Schedules" };
			Data.Apply(etlUpdateDate);

			var brasilTimeZone = new ATimeZone("E. South America Standard Time") { TimeZoneId = 2 };
			AnalyticsRunner.RunAnalyticsBaseData(new List<IAnalyticsDataSetup> { brasilTimeZone }, testDate);

			var cat = new ShiftCategoryConfigurable { Name = "Kattegat", Color = "Green" };
			var activityPhone = new ActivitySpec { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			var activityLunch = new ActivitySpec { Name = "Lunch", Color = "Red" };

			Data.Apply(cat);
			Data.Apply(activityPhone);
			Data.Apply(activityLunch);

			IPerson person;
			BasicShiftSetup.SetupBasicForShifts();
			BasicShiftSetup.AddPerson(out person, "Ola H", "", testDate);
			BasicShiftSetup.AddThreeShifts("Ola H", cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity, testDate);

			Data.Person("Ola H").Apply(new BrasilianTimeZone());

			var readModel = new ScheduleDayReadModel
			{
				PersonId = person.Id.GetValueOrDefault(),
				ColorCode = 0,
				ContractTimeTicks = 500,
				Date = DateTime.Today,
				StartDateTime = testDate.AddDays(-15).AddHours(8),
				EndDateTime = testDate.AddDays(-15).AddHours(17),
				Label = "LABEL",
				NotScheduled = false,
				Workday = true,
				WorkTimeTicks = 600
			};

			// we must manipulate readmodel so it "knows" that the dates on the person are updated
			var readM = new ScheduleDayReadModelSetup { Model = readModel };
			Data.Apply(readM);
			readModel.Date = DateTime.Today.AddDays(1);
			readM = new ScheduleDayReadModelSetup { Model = readModel };
			Data.Apply(readM);

			var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			dateList.Add(testDate.AddDays(-3), testDate.AddDays(3), JobCategoryType.Schedule);
			var jobParameters = new JobParameters(
				dateList, 1, "UTC", 15, "", "False",
				CultureInfo.CurrentCulture,
				new FakeContainerHolder(), false)
			{
				Helper =
					new JobHelperForTest(new RaptorRepository(InfraTestConfigReader.AnalyticsConnectionString(), null, null), null)
			};

			//transfer site, team contract etc from app to analytics
			var result = StepRunner.RunBasicStepsBeforeSchedule(jobParameters);

			JobStepBase step = new StageScheduleJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			step = new DimShiftLengthJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			step = new FactScheduleJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			// now it should have data on all three dates 96 interval
			var factSchedules = SqlCommands.RowsInFactSchedule();

			Assert.That(factSchedules, Is.EqualTo(96));
			step = new StageScheduleJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			step = new FactScheduleJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);
			factSchedules = SqlCommands.RowsInFactSchedule();

			Assert.That(factSchedules, Is.EqualTo(96));
		}
	}
}