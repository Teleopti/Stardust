using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Analytics.Etl.IntegrationTest.TestData;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.MultipleDate;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
	[TestFixture]
	public class FactScheduleTest
	{
		private const int aheranceTypeReadyTime = 1;
		private const int aheranceTypeSchedule = 2;
		private const int aheranceTypeContract = 3;

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
        public void ShouldHaveCorrectScheduleInReports()
        {
			DateTime testDate = new DateTime(2013, 06, 15);
			AnalyticsRunner.RunAnalyticsBaseData(new List<IAnalyticsDataSetup>(), testDate);

			AnalyticsRunner.RunSysSetupTestData(testDate);
            const string timeZoneId = "W. Europe Standard Time";
            IPerson person;
            BasicShiftSetup.SetupBasicForShifts();
			BasicShiftSetup.AddPerson(out person, "Ola H", "", testDate);


			var cat = new ShiftCategoryConfigurable { Name = "Kattegat", Color = "Green" };
			var activityPhone = new ActivityConfigurable { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			var activityLunch = new ActivityConfigurable { Name = "Lunch", Color = "Red" };

			Data.Apply(cat);
			Data.Apply(activityPhone);
			Data.Apply(activityLunch);

            //Add overlapping
            BasicShiftSetup.AddShift("Ola H", testDate.AddDays(-2), 21, 11, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);
			BasicShiftSetup.AddShift("Ola H", testDate.AddDays(-1), 6, 8, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);
			BasicShiftSetup.AddShift("Ola H", testDate.AddDays(0), 9, 8, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);

            var period = new DateTimePeriod(testDate.AddDays(-14).ToUniversalTime(), testDate.AddDays(14).ToUniversalTime());
            var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
            dateList.Add(testDate.AddDays(-3), testDate.AddDays(3), JobCategoryType.Schedule);
            var jobParameters = new JobParameters(dateList, 1, "UTC", 15, "", "False", CultureInfo.CurrentCulture)
            {
                Helper =
                    new JobHelper(new RaptorRepository(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, ""), null, null),
                DataSource = 2
            };

            jobParameters.StateHolder.SetLoadBridgeTimeZonePeriod(period);
            StepRunner.RunNightly(jobParameters);

	        const string phone = "Phone";
			var schedule = SqlCommands.ReportDataScheduledTimePerAgent(testDate.AddDays(-2), testDate.AddDays(0), 1, person, timeZoneId, phone);
            
            Assert.That(schedule.Rows.Count, Is.EqualTo(3));
	        foreach (DataRow row in schedule.Rows)
	        {
		        if ((row["date"]).Equals(testDate.AddDays(-2)))
		        {
			        Assert.That((row["activity_absence_name"]), Is.EqualTo(phone));
			        Assert.That((row["scheduled_contract_time_m"]), Is.EqualTo(180));
		        }
				if ((row["date"]).Equals(testDate.AddDays(-1)))
				{
					Assert.That((row["activity_absence_name"]), Is.EqualTo(phone));
					Assert.That((row["scheduled_contract_time_m"]), Is.EqualTo(840));
				}
				if ((row["date"]).Equals(testDate.AddDays(0)))
				{
					Assert.That((row["activity_absence_name"]), Is.EqualTo(phone));
					Assert.That((row["scheduled_contract_time_m"]), Is.EqualTo(420));
				}
	        }
		}

		[Test]
		[Ignore]
		public void ShouldWorkWithOverlappingShifts()
		{
			DateTime testDate = new DateTime(2013, 06, 15);
			AnalyticsRunner.RunAnalyticsBaseData(new List<IAnalyticsDataSetup>(), testDate);

			AnalyticsRunner.RunSysSetupTestData(testDate);
			const string timeZoneId = "W. Europe Standard Time";
			IPerson person;
			BasicShiftSetup.SetupBasicForShifts();
			BasicShiftSetup.AddPerson(out person, "Ola H", "", testDate);

			var cat = new ShiftCategoryConfigurable { Name = "Kattegat", Color = "Green" };
			var activityPhone = new ActivityConfigurable { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			var activityLunch = new ActivityConfigurable { Name = "Lunch", Color = "Red" };

			Data.Apply(cat);
			Data.Apply(activityPhone);
			Data.Apply(activityLunch);


			//Add overlapping
			BasicShiftSetup.AddShift("Ola H", testDate.AddDays(-2), 21, 11, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);
			BasicShiftSetup.AddShift("Ola H", testDate.AddDays(-1), 6, 8, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);

			var period = new DateTimePeriod(testDate.AddDays(-14).ToUniversalTime(), testDate.AddDays(14).ToUniversalTime());
			var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
			dateList.Add(testDate.AddDays(-3), testDate.AddDays(3), JobCategoryType.Schedule);
			var jobParameters = new JobParameters(dateList, 1, "UTC", 15, "", "False", CultureInfo.CurrentCulture)
			{
				Helper =
					new JobHelper(new RaptorRepository(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, ""), null, null),DataSource = 2
			};

			jobParameters.StateHolder.SetLoadBridgeTimeZonePeriod(period);
			StepRunner.RunNightly(jobParameters);

			// now it should have data on all three dates, 96 interval
			var factSchedules = SqlCommands.RowsInFactSchedule();

			Assert.That(factSchedules, Is.EqualTo(76));
			var time = testDate.AddDays(-1).AddHours(4); //2013-06-14 04:00
			var last = SqlCommands.RowsInFactSchedule(time);

			Assert.That(last, Is.EqualTo(32));
			time = testDate.AddDays(-2).AddHours(19);
			var first = SqlCommands.RowsInFactSchedule(time);
			Assert.That(first, Is.EqualTo(44));
		}

		[Test]
		public void ShouldFindAdherence()
		{
			DateTime testDate = new DateTime(2013, 06, 15);
			const string timeZoneId = "W. Europe Standard Time";

			AnalyticsRunner.RunAnalyticsBaseData(new List<IAnalyticsDataSetup>(), testDate);
			AnalyticsRunner.RunSysSetupTestData(testDate);

		    var el = new ExternalLogonConfigurable
		    {
		        AcdLogOnAggId = 52,
		        AcdLogOnMartId = 1,
		        AcdLogOnOriginalId = "152",
		        AcdLogOnName = "Ola H"
		    };
            Data.Apply(el);

            IPerson person;
			BasicShiftSetup.SetupBasicForShifts();
			BasicShiftSetup.AddPerson(out person, "Ola H", "Ola H", testDate);

			var cat = new ShiftCategoryConfigurable { Name = "Kattegat", Color = "Green" };
			var activityPhone = new ActivityConfigurable { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			var activityLunch = new ActivityConfigurable { Name = "Lunch", Color = "Red" };

			Data.Apply(cat);
			Data.Apply(activityPhone);
			Data.Apply(activityLunch);


			//Add overlapping
			BasicShiftSetup.AddShift("Ola H", testDate.AddDays(-2), 21, 11, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);
			BasicShiftSetup.AddShift("Ola H", testDate.AddDays(-1), 6, 8, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);

			var period = new DateTimePeriod(testDate.AddDays(-14).ToUniversalTime(), testDate.AddDays(14).ToUniversalTime());
			var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
			dateList.Add(testDate.AddDays(-3), testDate.AddDays(3), JobCategoryType.Schedule);
			dateList.Add(testDate.AddDays(-3), testDate.AddDays(3), JobCategoryType.AgentStatistics);
			dateList.Add(testDate.AddDays(-3), testDate.AddDays(3), JobCategoryType.Forecast);
			dateList.Add(testDate.AddDays(-3), testDate.AddDays(3), JobCategoryType.QueueStatistics);
			var jobParameters = new JobParameters(dateList, 1, "UTC", 15, "", "False", CultureInfo.CurrentCulture)
			{
				Helper =
					new JobHelper(new RaptorRepository(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, ""), null, null),
				DataSource = 2
			};

			jobParameters.StateHolder.SetLoadBridgeTimeZonePeriod(period);

			//Run ETL.Intraday first time just to set "LastUpdatedPerStep"
			StepRunner.RunIntraday(jobParameters);

			//Nightly
			StepRunner.RunNightly(jobParameters);
			assertOverlapping(person, timeZoneId, "Nightly", testDate);

			//Re-Add overlapping
			RemovePersonSchedule.RemoveAssignmentAndReadmodel(BasicShiftSetup.Scenario.Scenario, "Ola H", testDate.AddDays(-1), person);
			RemovePersonSchedule.RemoveAssignmentAndReadmodel(BasicShiftSetup.Scenario.Scenario, "Ola H", testDate.AddDays(-2), person);
			BasicShiftSetup.AddShift("Ola H", testDate.AddDays(-2), 21, 11, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);
			BasicShiftSetup.AddShift("Ola H", testDate.AddDays(-1), 6, 8, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);

			//Intraday
			StepRunner.RunIntraday(jobParameters);
			assertOverlapping(person, timeZoneId, "Intraday", testDate);

			//Edit shifts and remove Overlapping
			RemovePersonSchedule.RemoveAssignmentAndReadmodel(BasicShiftSetup.Scenario.Scenario, "Ola H", testDate.AddDays(-1), person);
			RemovePersonSchedule.RemoveAssignmentAndReadmodel(BasicShiftSetup.Scenario.Scenario, "Ola H", testDate.AddDays(-2), person);
			BasicShiftSetup.AddShift("Ola H", testDate.AddDays(-2), 17, 8, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);
			BasicShiftSetup.AddShift("Ola H", testDate.AddDays(-1), 6, 8, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);

			//Intraday
			StepRunner.RunIntraday(jobParameters);

			//Assert on Intraday, shifts back to normal
			Assert.That(SqlCommands.CountIntervalsPerLocalDate(person, testDate.AddDays(-1)), Is.EqualTo(35));
			Assert.That(SqlCommands.CountIntervalsPerLocalDate(person, testDate.AddDays(-2)), Is.EqualTo(32));
			var column = "deviation_schedule_s";
			Assert.That(SqlCommands.SumFactScheduleDeviation(person, testDate.AddDays(-1), column), Is.EqualTo(24540));
            Assert.That(SqlCommands.SumFactScheduleDeviation(person, testDate.AddDays(-2), column), Is.EqualTo(21960));
			column = "scheduled_ready_time_s";
			Assert.That(SqlCommands.SumFactScheduleDeviation(person, testDate.AddDays(-1), column), Is.EqualTo(25200));
			Assert.That(SqlCommands.SumFactScheduleDeviation(person, testDate.AddDays(-2), column), Is.EqualTo(25200));
			column = "ready_time_s";
			Assert.That(SqlCommands.SumFactScheduleDeviation(person, testDate.AddDays(-1), column), Is.EqualTo(4740));
			Assert.That(SqlCommands.SumFactScheduleDeviation(person, testDate.AddDays(-2), column), Is.EqualTo(3240));
			column = "deviation_schedule_ready_s";
			Assert.That(SqlCommands.SumFactScheduleDeviation(person, testDate.AddDays(-1), column), Is.EqualTo(22500));
			Assert.That(SqlCommands.SumFactScheduleDeviation(person, testDate.AddDays(-2), column), Is.EqualTo(21960));
		}


		public void assertOverlapping(IPerson person, string timeZoneId, string ETLType, DateTime testDate)
		{
			//Tests for "Ready Time vs. Schedule Ready Time"
			var adheranceId = aheranceTypeReadyTime;
			var adherance = SqlCommands.ReportDataAgentScheduleAdherence(testDate.AddDays(-2), testDate.AddDays(-1), adheranceId, person, timeZoneId);
			
			Assert.That(adherance.Rows.Count, Is.EqualTo(81));
			foreach (DataRow row in adherance.Rows)
			{
				var rownumber = (int)row["date_interval_counter"];
				switch (rownumber)
				{
					case 1:
						{
							Assert.That((row["date"]), Is.EqualTo(testDate.AddDays(-2)));
							Assert.That((row["interval_id"]), Is.EqualTo(78));
							Assert.That((row["interval_name"]), Is.EqualTo("19:30-19:45"));
							Assert.That((row["deviation_tot_m"]), Is.EqualTo(543));
							break;
						}
					case 7:
						{
							Assert.That((row["date"]), Is.EqualTo(testDate.AddDays(-2)), "60% interval");
							Assert.That((row["interval_id"]), Is.EqualTo(88), "60% interval");
							Assert.That((row["interval_name"]), Is.EqualTo("22:00-22:15"), "60% interval");
							Assert.That((row["adherence"]), Is.EqualTo(0.6), "60% interval");
							Assert.That((row["deviation_m"]), Is.EqualTo(6), "60% interval");
							Assert.That((row["adherence_calc_s"]), Is.EqualTo(900), "60% interval");
							Assert.That((row["ready_time_m"]), Is.EqualTo(9), "60% interval");
							break;
						}
					case 8:
						{
							Assert.That((row["date"]), Is.EqualTo(testDate.AddDays(-2)), "100% interval");
							Assert.That((row["interval_id"]), Is.EqualTo(89), "100% interval");
							Assert.That((row["interval_name"]), Is.EqualTo("22:15-22:30"), "100% interval");
							Assert.That((row["adherence"]), Is.EqualTo(1), "100% interval");
							Assert.That((row["deviation_m"]), Is.EqualTo(0), "100% interval");
							Assert.That((row["adherence_calc_s"]), Is.EqualTo(900), "100% interval");
							Assert.That((row["ready_time_m"]), Is.EqualTo(15), "100% interval");
							break;
						}

					case 46:
						{
							Assert.That((row["date"]), Is.EqualTo(testDate.AddDays(-1)));
							Assert.That((row["interval_id"]), Is.EqualTo(31));
							Assert.That((row["interval_name"]), Is.EqualTo("07:45-08:00"));
							break;
						}

					case 79:
						{
							Assert.That((row["ready_time_m"]), Is.EqualTo(4));
							break;
						}

					case 81:
						{
							Assert.That((row["ready_time_m"]), Is.EqualTo(9));
							break;
						}

				}
			}

			//Tests for "Ready Time vs. Schedule Time", e.g. the punish if over performing agent
			adheranceId = aheranceTypeSchedule;
			adherance = SqlCommands.ReportDataAgentScheduleAdherence(testDate.AddDays(-2), testDate.AddDays(-1), adheranceId, person, timeZoneId);

			Assert.That(adherance.Rows.Count, Is.EqualTo(81));
			foreach (DataRow row in adherance.Rows)
			{
				var rownumber = (int)row["date_interval_counter"];
				switch (rownumber)
				{
					case 1:
						{
							Assert.That((row["date"]), Is.EqualTo(testDate.AddDays(-2)));
							Assert.That((row["interval_id"]), Is.EqualTo(78), "First interval_id yesterday");
							Assert.That((row["interval_name"]), Is.EqualTo("19:30-19:45"), "First interval_name yesterday");
							break;
						}
					case 46:
						{
							Assert.That((row["date"]), Is.EqualTo(testDate.AddDays(-1)));
							Assert.That((row["interval_id"]), Is.EqualTo(31), "First interval_id today");
							Assert.That((row["interval_name"]), Is.EqualTo("07:45-08:00"), "First interval_name today");
							break;
						}

					case 79:
						{
							Assert.That((row["ready_time_m"]), Is.EqualTo(4));
							break;
						}

					case 81:
						{
							Assert.That((row["ready_time_m"]), Is.EqualTo(9));
							break;
						}

				}
			}


			//Asserts on fact_schedule_deviation
			Assert.That(SqlCommands.CountIntervalsPerLocalDate(person, testDate.AddDays(-1)), Is.EqualTo(35), "ETL."+ ETLType + " count intervals for Day-1");
            Assert.That(SqlCommands.CountIntervalsPerLocalDate(person, testDate.AddDays(-2)), Is.EqualTo(46), "ETL." + ETLType + " count intervals for Day-2");
			var column = "deviation_schedule_s";
			Assert.That(SqlCommands.SumFactScheduleDeviation(person, testDate.AddDays(-1), column), Is.EqualTo(24540), "ETL." + ETLType + " " + column + " for Day-1");
            Assert.That(SqlCommands.SumFactScheduleDeviation(person, testDate.AddDays(-2), column), Is.EqualTo(33660), "ETL." + ETLType + " " + column + " for Day-2");
            column = "scheduled_ready_time_s";
            Assert.That(SqlCommands.SumFactScheduleDeviation(person, testDate.AddDays(-1), column), Is.EqualTo(25200), "ETL." + ETLType + " " + column + " for Day-1");
            Assert.That(SqlCommands.SumFactScheduleDeviation(person, testDate.AddDays(-2), column), Is.EqualTo(36000), "ETL." + ETLType + " " + column + " for Day-2");
            column = "ready_time_s";
            Assert.That(SqlCommands.SumFactScheduleDeviation(person, testDate.AddDays(-1), column), Is.EqualTo(4740), "ETL." + ETLType + " " + column + " for Day-1");
            Assert.That(SqlCommands.SumFactScheduleDeviation(person, testDate.AddDays(-2), column), Is.EqualTo(4500), "ETL." + ETLType + " " + column + " for Day-2");
            column = "deviation_schedule_ready_s";
            Assert.That(SqlCommands.SumFactScheduleDeviation(person, testDate.AddDays(-1), column), Is.EqualTo(22500), "ETL." + ETLType + " " + column + " for Day-1");
            Assert.That(SqlCommands.SumFactScheduleDeviation(person, testDate.AddDays(-2), column), Is.EqualTo(32580), "ETL." + ETLType + " " + column + " for Day-2");

			
		}


	}
}
