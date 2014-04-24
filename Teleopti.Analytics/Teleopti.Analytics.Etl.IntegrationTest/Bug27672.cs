using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
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
	[Ignore,TestFixture]
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
			DateTime testDate = new DateTime(2013, 06, 15);

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
			dateList.Add(DateTime.Today.AddDays(-3),DateTime.Today.AddDays(3),JobCategoryType.Schedule);
			var jobParameters = new JobParameters(dateList, 1, "UTC",15,"","False",CultureInfo.CurrentCulture)
				{
					Helper =
						new JobHelper(new RaptorRepository(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, ""),null,null)
				};

			//transfer site, team contract etc from app to analytics
			var result = StepRunner.RunNightly(jobParameters);

			
			var factSchedules = SqlCommands.RowsInFactSchedule();
			Assert.That(factSchedules, Is.EqualTo(96));
            
            //run again now with fewer days
            dateList.Clear();
            dateList.Add(DateTime.Today.AddDays(0), DateTime.Today.AddDays(0), JobCategoryType.Schedule);
            jobParameters = new JobParameters(dateList, 1, "UTC", 15, "", "False", CultureInfo.CurrentCulture)
            {
                Helper =
                    new JobHelper(new RaptorRepository(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, ""), null, null)
            };
            
            result = StepRunner.RunNightly(jobParameters);

			factSchedules = SqlCommands.RowsInFactSchedule();
			Assert.That(factSchedules, Is.EqualTo(96));

		}
        [Test]
        public void ShouldWorkForCanberra()
        {
			DateTime testDate = new DateTime(2013, 06, 15);

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
            dateList.Add(DateTime.Today.AddDays(-3), DateTime.Today.AddDays(3), JobCategoryType.Schedule);
            var jobParameters = new JobParameters(dateList, 1, "UTC", 15, "", "False", CultureInfo.CurrentCulture)
            {
                Helper =
                    new JobHelper(new RaptorRepository(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, ""), null, null)
            };

            //transfer site, team contract etc from app to analytics
            var result = StepRunner.RunNightly(jobParameters);

            // now it should have data on all three dates on 96 interval
			var factSchedules = SqlCommands.RowsInFactSchedule();
			Assert.That(factSchedules, Is.EqualTo(96));

            //run again now with fewer days
            dateList.Clear();
            dateList.Add(DateTime.Today.AddDays(0), DateTime.Today.AddDays(0), JobCategoryType.Schedule);
            jobParameters = new JobParameters(dateList, 1, "UTC", 15, "", "False", CultureInfo.CurrentCulture)
            {
                Helper =
                    new JobHelper(new RaptorRepository(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, ""), null, null)
            };

            result = StepRunner.RunNightly(jobParameters);

			factSchedules = SqlCommands.RowsInFactSchedule();
			Assert.That(factSchedules, Is.EqualTo(96));

        }
	}
}