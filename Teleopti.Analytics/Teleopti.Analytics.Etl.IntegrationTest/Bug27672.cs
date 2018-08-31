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
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Specific;

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
			DateTime testDate = DateTime.Today.AddDays(-30);

			// run this to get a date and time in mart.LastUpdatedPerStep
			var etlUpdateDate = new EtlReadModelSetup { BusinessUnit = TestState.BusinessUnit, StepName = "Schedules" };
			Data.Apply(etlUpdateDate);


			AnalyticsRunner.RunAnalyticsBaseData(new List<IAnalyticsDataSetup>(){new BusinessUnit(TestState.BusinessUnit, ExistingDatasources.DefaultRaptorDefaultDatasourceId)}, testDate);

			var cat = new ShiftCategoryConfigurable { Name = "Kattegat", Color = "Green" };
			var activityPhone = new ActivitySpec { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			var activityLunch = new ActivitySpec { Name = "Lunch", Color = "Red" };

			Data.Apply(cat);
			Data.Apply(activityPhone);
			Data.Apply(activityLunch);
			var analyticsDataFactory = new AnalyticsDataFactory();
			analyticsDataFactory.Setup(new ShiftCategory(1, cat.ShiftCategory.Id.Value, cat.Name, cat.ShiftCategory.DisplayColor, ExistingDatasources.DefaultRaptorDefaultDatasourceId, 1));
			analyticsDataFactory.Setup(new Activity(1, activityPhone.Activity.Id.Value, activityPhone.Name, activityPhone.Activity.DisplayColor, ExistingDatasources.DefaultRaptorDefaultDatasourceId, 1));
			analyticsDataFactory.Setup(new Activity(2, activityLunch.Activity.Id.Value, activityLunch.Name, activityLunch.Activity.DisplayColor, ExistingDatasources.DefaultRaptorDefaultDatasourceId, 1));
			analyticsDataFactory.Persist();

			IPerson person;
			BasicShiftSetup.SetupBasicForShifts();
			BasicShiftSetup.AddPerson(out person, "Ola H", "", testDate);
			BasicShiftSetup.AddThreeShifts("Ola H", cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity, testDate);

			var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			dateList.Add(testDate.AddDays(-3), testDate.AddDays(3), JobCategoryType.Schedule);
			var jobParameters = new JobParameters(
				dateList, 1, "UTC", 15, "", "False",
				CultureInfo.CurrentCulture,
				new FakeContainerHolder(), false
				)
				{
					Helper =
						new JobHelperForTest(new RaptorRepository(InfraTestConfigReader.AnalyticsConnectionString, null, null), null)
				};

			//transfer site, team contract etc from app to analytics
			var result = new List<Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult>();
			JobStepBase step = new StageScheduleJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);
			step = new DimShiftLengthJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);
			step = new FactScheduleJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);


			var factSchedules = SqlCommands.RowsInFactSchedule();
			Assert.That(factSchedules, Is.EqualTo(96));

			//run again now with fewer days
			dateList.Clear();
			dateList.Add(testDate.AddDays(0), testDate.AddDays(0), JobCategoryType.Schedule);
			jobParameters = new JobParameters(
				dateList, 1, "UTC", 15, "", "False",
				CultureInfo.CurrentCulture,
				new FakeContainerHolder(), false
				)
						{
							Helper = new JobHelperForTest(
								new RaptorRepository(InfraTestConfigReader.AnalyticsConnectionString, null, null), null
								)
						};

			StepRunner.RunNightly(jobParameters);

			factSchedules = SqlCommands.RowsInFactSchedule();
			Assert.That(factSchedules, Is.EqualTo(96));

		}
		[Test]
		public void ShouldWorkForCanberra()
		{
			DateTime testDate = DateTime.Today.AddDays(-30);

			// run this to get a date and time in mart.LastUpdatedPerStep
			var etlUpdateDate = new EtlReadModelSetup { BusinessUnit = TestState.BusinessUnit, StepName = "Schedules" };
			Data.Apply(etlUpdateDate);


			AnalyticsRunner.RunAnalyticsBaseData(new List<IAnalyticsDataSetup>(){new BusinessUnit(TestState.BusinessUnit, ExistingDatasources.DefaultRaptorDefaultDatasourceId)}, testDate);

			var cat = new ShiftCategoryConfigurable { Name = "Kattegat", Color = "Green" };
			var activityPhone = new ActivitySpec { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			var activityLunch = new ActivitySpec { Name = "Lunch", Color = "Red" };

			Data.Apply(cat);
			Data.Apply(activityPhone);
			Data.Apply(activityLunch);
			var analyticsDataFactory = new AnalyticsDataFactory();
			analyticsDataFactory.Setup(new ShiftCategory(1, cat.ShiftCategory.Id.Value, cat.Name, cat.ShiftCategory.DisplayColor, ExistingDatasources.DefaultRaptorDefaultDatasourceId, 1));
			analyticsDataFactory.Setup(new Activity(1, activityPhone.Activity.Id.Value, activityPhone.Name, activityPhone.Activity.DisplayColor, ExistingDatasources.DefaultRaptorDefaultDatasourceId, 1));
			analyticsDataFactory.Setup(new Activity(2, activityLunch.Activity.Id.Value, activityLunch.Name, activityLunch.Activity.DisplayColor, ExistingDatasources.DefaultRaptorDefaultDatasourceId, 1));
			analyticsDataFactory.Persist();

			IPerson person;
			BasicShiftSetup.SetupBasicForShifts();
			BasicShiftSetup.AddPerson(out person, "Ola H", "", testDate);
			Data.Person("Ola H").Apply(new AustralianTimeZone());
			BasicShiftSetup.AddThreeShifts("Ola H", cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity, testDate);

			var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			dateList.Add(testDate.AddDays(-3), testDate.AddDays(3), JobCategoryType.Schedule);
			var jobParameters = new JobParameters(
				dateList, 1, "UTC", 15, "", "False",
				CultureInfo.CurrentCulture,
				new FakeContainerHolder(), false
				)
						{
							Helper =
				new JobHelperForTest(new RaptorRepository(InfraTestConfigReader.AnalyticsConnectionString, null, null), null)
						};

			//transfer site, team contract etc from app to analytics
			var result = new List<Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult>();
			JobStepBase step = new StageScheduleJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);
			step = new DimShiftLengthJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);
			step = new FactScheduleJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);

			// now it should have data on all three dates on 96 interval
			var factSchedules = SqlCommands.RowsInFactSchedule();
			Assert.That(factSchedules, Is.EqualTo(96));

			//run again now with fewer days
			dateList.Clear();
			dateList.Add(testDate.AddDays(0), testDate.AddDays(0), JobCategoryType.Schedule);
			jobParameters = new JobParameters(
				dateList, 1, "UTC", 15, "", "False",
				CultureInfo.CurrentCulture,
				new FakeContainerHolder(), false
				)
						{
							Helper =
								new JobHelperForTest(new RaptorRepository(InfraTestConfigReader.AnalyticsConnectionString, null, null), null)
						};

			StepRunner.RunNightly(jobParameters);

			factSchedules = SqlCommands.RowsInFactSchedule();
			Assert.That(factSchedules, Is.EqualTo(96));

		}
	}
}