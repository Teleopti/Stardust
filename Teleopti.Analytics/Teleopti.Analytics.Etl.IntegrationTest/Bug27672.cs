using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.MultipleDate;
using Teleopti.Analytics.Etl.IntegrationTest.TestData;
using Teleopti.Ccc.TestCommon;
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
			var testDate = new DateTime(2013, 06, 15);

			// run this to get a date and time in mart.LastUpdatedPerStep
			var etlUpdateDate = new EtlReadModelSetup { BusinessUnit = TestState.BusinessUnit, StepName = "Schedules" };
			Data.Apply(etlUpdateDate);


			AnalyticsRunner.RunAnalyticsBaseData(new List<IAnalyticsDataSetup>(), testDate);

			var cat = new ShiftCategoryConfigurable { Name = "Kattegat", Color = "Green" };
			var activityPhone = new ActivityConfigurable { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			var activityLunch = new ActivityConfigurable { Name = "Lunch", Color = "Red" };

			Data.Apply(cat);
			Data.Apply(activityPhone);
			Data.Apply(activityLunch);

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
						new JobHelperForTest(new RaptorRepository(InfraTestConfigReader.AnalyticsConnectionString, ""), null)
				};

			//transfer site, team contract etc from app to analytics
			StepRunner.RunNightly(jobParameters);


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
								new RaptorRepository(InfraTestConfigReader.AnalyticsConnectionString, ""), null
								)
						};

			StepRunner.RunNightly(jobParameters);

			factSchedules = SqlCommands.RowsInFactSchedule();
			Assert.That(factSchedules, Is.EqualTo(96));

		}
		[Test]
		public void ShouldWorkForCanberra()
		{
			var testDate = new DateTime(2013, 06, 15);

			// run this to get a date and time in mart.LastUpdatedPerStep
			var etlUpdateDate = new EtlReadModelSetup { BusinessUnit = TestState.BusinessUnit, StepName = "Schedules" };
			Data.Apply(etlUpdateDate);


			AnalyticsRunner.RunAnalyticsBaseData(new List<IAnalyticsDataSetup>(), testDate);

			var cat = new ShiftCategoryConfigurable { Name = "Kattegat", Color = "Green" };
			var activityPhone = new ActivityConfigurable { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			var activityLunch = new ActivityConfigurable { Name = "Lunch", Color = "Red" };

			Data.Apply(cat);
			Data.Apply(activityPhone);
			Data.Apply(activityLunch);

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
				new JobHelperForTest(new RaptorRepository(InfraTestConfigReader.AnalyticsConnectionString, ""), null)
						};

			//transfer site, team contract etc from app to analytics
			StepRunner.RunNightly(jobParameters);

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
								new JobHelperForTest(new RaptorRepository(InfraTestConfigReader.AnalyticsConnectionString, ""), null)
						};

			StepRunner.RunNightly(jobParameters);

			factSchedules = SqlCommands.RowsInFactSchedule();
			Assert.That(factSchedules, Is.EqualTo(96));

		}
	}
}