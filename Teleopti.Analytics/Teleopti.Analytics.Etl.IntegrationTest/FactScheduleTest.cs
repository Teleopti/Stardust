using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Data.SqlClient;
using NUnit.Framework;
using Teleopti.Analytics.Etl.IntegrationTest.Models;
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
            AnalyticsRunner.RunAnalyticsBaseData(new List<IAnalyticsDataSetup>());
            AnalyticsRunner.RunSysSetupTestData();
            const string timeZoneId = "W. Europe Standard Time";
            IPerson person;
            BasicShiftSetup.SetupBasicForShifts();
            BasicShiftSetup.AddPerson(out person, "Ola H", "");


			var cat = new ShiftCategoryConfigurable { Name = "Kattegat", Color = "Green" };
			var activityPhone = new ActivityConfigurable { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			var activityLunch = new ActivityConfigurable { Name = "Lunch", Color = "Red" };

			Data.Apply(cat);
			Data.Apply(activityPhone);
			Data.Apply(activityLunch);

            //Add overlapping
            BasicShiftSetup.AddShift("Ola H", DateTime.Today.AddDays(-2), 21, 11, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);
			BasicShiftSetup.AddShift("Ola H", DateTime.Today.AddDays(-1), 6, 8, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);
			BasicShiftSetup.AddShift("Ola H", DateTime.Today.AddDays(0), 9, 8, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);

            var period = new DateTimePeriod(DateTime.Today.AddDays(-14).ToUniversalTime(), DateTime.Today.AddDays(14).ToUniversalTime());
            var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
            dateList.Add(DateTime.Today.AddDays(-3), DateTime.Today.AddDays(3), JobCategoryType.Schedule);
            var jobParameters = new JobParameters(dateList, 1, "UTC", 15, "", "False", CultureInfo.CurrentCulture)
            {
                Helper =
                    new JobHelper(new RaptorRepository(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, ""), null, null),
                DataSource = 2
            };

            jobParameters.StateHolder.SetLoadBridgeTimeZonePeriod(period);
            StepRunner.RunNightly(jobParameters);

	        const string phone = "Phone";
			var schedule = reportDataScheduledTimePerAgent(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, DateTime.Today.AddDays(-2), DateTime.Today.AddDays(0), 1, person, timeZoneId, phone);
            
            Assert.That(schedule.Rows.Count, Is.EqualTo(3));
	        foreach (DataRow row in schedule.Rows)
	        {
		        if ((row["date"]).Equals(DateTime.Today.AddDays(-2)))
		        {
			        Assert.That((row["activity_absence_name"]), Is.EqualTo(phone));
			        Assert.That((row["scheduled_contract_time_m"]), Is.EqualTo(180));
		        }
				if ((row["date"]).Equals(DateTime.Today.AddDays(-1)))
				{
					Assert.That((row["activity_absence_name"]), Is.EqualTo(phone));
					Assert.That((row["scheduled_contract_time_m"]), Is.EqualTo(840));
				}
				if ((row["date"]).Equals(DateTime.Today.AddDays(0)))
				{
					Assert.That((row["activity_absence_name"]), Is.EqualTo(phone));
					Assert.That((row["scheduled_contract_time_m"]), Is.EqualTo(420));
				}
	        }
		}

		[Test]
		public void ShouldWorkWithOverlappingShifts()
		{
			AnalyticsRunner.RunAnalyticsBaseData(new List<IAnalyticsDataSetup>());
			AnalyticsRunner.RunSysSetupTestData();
			const string timeZoneId = "W. Europe Standard Time";
			IPerson person;
			BasicShiftSetup.SetupBasicForShifts();
			BasicShiftSetup.AddPerson(out person,"Ola H", "");

			var cat = new ShiftCategoryConfigurable { Name = "Kattegat", Color = "Green" };
			var activityPhone = new ActivityConfigurable { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			var activityLunch = new ActivityConfigurable { Name = "Lunch", Color = "Red" };

			Data.Apply(cat);
			Data.Apply(activityPhone);
			Data.Apply(activityLunch);


			//Add overlapping
			BasicShiftSetup.AddShift("Ola H", DateTime.Today.AddDays(-2), 21, 11, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);
			BasicShiftSetup.AddShift("Ola H", DateTime.Today.AddDays(-1), 6, 8, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);

			var period = new DateTimePeriod(DateTime.Today.AddDays(-14).ToUniversalTime(), DateTime.Today.AddDays(14).ToUniversalTime());
			var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
			dateList.Add(DateTime.Today.AddDays(-3), DateTime.Today.AddDays(3), JobCategoryType.Schedule);
			var jobParameters = new JobParameters(dateList, 1, "UTC", 15, "", "False", CultureInfo.CurrentCulture)
			{
				Helper =
					new JobHelper(new RaptorRepository(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, ""), null, null),DataSource = 2
			};

			jobParameters.StateHolder.SetLoadBridgeTimeZonePeriod(period);
			StepRunner.RunNightly(jobParameters);

			// now it should have data on all three dates, 96 interval
			var db = new AnalyticsContext(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix);

			var factSchedules = from s in db.fact_schedule select s;

			Assert.That(factSchedules.Count(), Is.EqualTo(76));
			var time = DateTime.Today.AddDays(-1).AddHours(5);
			var last = from s in db.fact_schedule where s.shift_starttime == time select s;

			Assert.That(last.Count(), Is.EqualTo(32));
			time = DateTime.Today.AddDays(-2).AddHours(20);
			var first = from s in db.fact_schedule where s.shift_starttime == time select s;
			Assert.That(first.Count(), Is.EqualTo(44));
		}

		[Test]
		public void ShouldFindAdherence()
		{
			const string timeZoneId = "W. Europe Standard Time";
			AnalyticsRunner.RunAnalyticsBaseData(new List<IAnalyticsDataSetup>());
			AnalyticsRunner.RunSysSetupTestData();

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
			BasicShiftSetup.AddPerson(out person, "Ola H", "Ola H");

			var cat = new ShiftCategoryConfigurable { Name = "Kattegat", Color = "Green" };
			var activityPhone = new ActivityConfigurable { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			var activityLunch = new ActivityConfigurable { Name = "Lunch", Color = "Red" };

			Data.Apply(cat);
			Data.Apply(activityPhone);
			Data.Apply(activityLunch);


			//Add overlapping
			BasicShiftSetup.AddShift("Ola H", DateTime.Today.AddDays(-2), 21, 11, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);
			BasicShiftSetup.AddShift("Ola H", DateTime.Today.AddDays(-1), 6, 8, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);

			var period = new DateTimePeriod(DateTime.Today.AddDays(-14).ToUniversalTime(), DateTime.Today.AddDays(14).ToUniversalTime());
			var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
			dateList.Add(DateTime.Today.AddDays(-3), DateTime.Today.AddDays(3), JobCategoryType.Schedule);
			dateList.Add(DateTime.Today.AddDays(-3), DateTime.Today.AddDays(3), JobCategoryType.AgentStatistics);
			dateList.Add(DateTime.Today.AddDays(-3), DateTime.Today.AddDays(3), JobCategoryType.Forecast);
			dateList.Add(DateTime.Today.AddDays(-3), DateTime.Today.AddDays(3), JobCategoryType.QueueStatistics);
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
			assertOverlapping(person, timeZoneId, "Nightly");

			//Re-Add overlapping
			RemovePersonSchedule.RemoveAssignmentAndReadmodel(BasicShiftSetup.Scenario.Scenario, "Ola H", DateTime.Today.AddDays(-1), person);
			RemovePersonSchedule.RemoveAssignmentAndReadmodel(BasicShiftSetup.Scenario.Scenario, "Ola H", DateTime.Today.AddDays(-2), person);
			BasicShiftSetup.AddShift("Ola H", DateTime.Today.AddDays(-2), 21, 11, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);
			BasicShiftSetup.AddShift("Ola H", DateTime.Today.AddDays(-1), 6, 8, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);

			//Intraday
			StepRunner.RunIntraday(jobParameters);
			assertOverlapping(person, timeZoneId, "Intraday");

			//Edit shifts and remove Overlapping
			RemovePersonSchedule.RemoveAssignmentAndReadmodel(BasicShiftSetup.Scenario.Scenario, "Ola H", DateTime.Today.AddDays(-1), person);
			RemovePersonSchedule.RemoveAssignmentAndReadmodel(BasicShiftSetup.Scenario.Scenario, "Ola H", DateTime.Today.AddDays(-2), person);
			BasicShiftSetup.AddShift("Ola H", DateTime.Today.AddDays(-2), 17, 8, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);
			BasicShiftSetup.AddShift("Ola H", DateTime.Today.AddDays(-1), 6, 8, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);

			//Intraday
			StepRunner.RunIntraday(jobParameters);

			//Assert on Intraday, shifts back to normal
			Assert.That(countIntervalsPerLocalDate(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, person, DateTime.Today.AddDays(-1)), Is.EqualTo(35));
			Assert.That(countIntervalsPerLocalDate(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, person, DateTime.Today.AddDays(-2)), Is.EqualTo(32));
			var column = "deviation_schedule_s";
			Assert.That(sumFactScheduleDeviation(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, person, DateTime.Today.AddDays(-1), column), Is.EqualTo(24540));
            Assert.That(sumFactScheduleDeviation(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, person, DateTime.Today.AddDays(-2), column), Is.EqualTo(21960));
			column = "scheduled_ready_time_s";
			Assert.That(sumFactScheduleDeviation(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, person, DateTime.Today.AddDays(-1), column), Is.EqualTo(25200));
			Assert.That(sumFactScheduleDeviation(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, person, DateTime.Today.AddDays(-2), column), Is.EqualTo(25200));
			column = "ready_time_s";
			Assert.That(sumFactScheduleDeviation(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, person, DateTime.Today.AddDays(-1), column), Is.EqualTo(4740));
			Assert.That(sumFactScheduleDeviation(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, person, DateTime.Today.AddDays(-2), column), Is.EqualTo(3240));
			column = "deviation_schedule_ready_s";
			Assert.That(sumFactScheduleDeviation(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, person, DateTime.Today.AddDays(-1), column), Is.EqualTo(22500));
			Assert.That(sumFactScheduleDeviation(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, person, DateTime.Today.AddDays(-2), column), Is.EqualTo(21960));
		}

		private static int countIntervalsPerLocalDate(string connectionString, IPerson person, DateTime datelocal)
		{
			var sqlConnection = connectAndOpen(connectionString);

            string sql = string.Format("select count(1) from mart.fact_schedule_deviation f " +
	"inner join mart.dim_person p on f.person_id = p.person_id " +
	"inner join mart.dim_time_zone tz on p.time_zone_id = tz.time_zone_id " +
	"join mart.bridge_time_zone btz on p.time_zone_id = btz.time_zone_id " +
		"and f.shift_startdate_id = btz.date_id " +
		"and f.shift_startinterval_id = btz.interval_id " +
	"join mart.dim_date d " +
	"on btz.local_date_id = d.date_id " +
	"where d.date_date = '{0}' and p.person_code = '{1}'", datelocal.Date, person.Id);
            using(var sqlCommand = new SqlCommand(sql, sqlConnection))
            {
                return Convert.ToInt32(sqlCommand.ExecuteScalar(), CultureInfo.CurrentCulture);
            }

		}

        private static int sumFactScheduleDeviation(string connectionString, IPerson person, DateTime datelocal, string columnName)
        {
            var sqlConnection = connectAndOpen(connectionString);

            string sql = string.Format("select sum(f.{2}) from mart.fact_schedule_deviation f " +
    "inner join mart.dim_person p on f.person_id = p.person_id " +
    "inner join mart.dim_time_zone tz on p.time_zone_id = tz.time_zone_id " +
    "join mart.bridge_time_zone btz on p.time_zone_id = btz.time_zone_id " +
        "and f.shift_startdate_id = btz.date_id " +
        "and f.shift_startinterval_id = btz.interval_id " +
    "join mart.dim_date d " +
    "on btz.local_date_id = d.date_id " +
	"where d.date_date = '{0}' and p.person_code = '{1}'", datelocal.Date, person.Id, columnName);
            using (var sqlCommand = new SqlCommand(sql, sqlConnection))
            {
                return Convert.ToInt32(sqlCommand.ExecuteScalar(), CultureInfo.CurrentCulture);
            }

        }


        private static DataTable reportDataAgentScheduleAdherence(string connectionString, DateTime date_from, DateTime date_to, int adherence_id, IPerson person, string timeZoneId)
		{
			var sqlConnection = connectAndOpen(connectionString);
            var reportResourceKey = "ResReportAdherencePerDay";
			var dtResult = new DataSet();
			SqlCommand command = sqlConnection.CreateCommand();
			SqlDataAdapter sqlAdapter = new SqlDataAdapter(command);
			command.CommandType = System.Data.CommandType.StoredProcedure;
			command.CommandText = "mart.report_data_agent_schedule_adherence_for_test";
			command.Parameters.AddWithValue("@date_from", date_from);
			command.Parameters.AddWithValue("@date_to", date_to);
			command.Parameters.AddWithValue("@adherence_id", adherence_id);
			command.Parameters.AddWithValue("@agent_code", person.Id);
			command.Parameters.AddWithValue("@time_zone_code", timeZoneId);
            command.Parameters.AddWithValue("@report_resource_key", reportResourceKey);
			sqlAdapter.Fill(dtResult);
			return dtResult.Tables[0];
		}

        private static DataTable reportDataScheduledTimePerAgent(string connectionString, DateTime date_from, DateTime date_to, int adherence_id, IPerson person, string timeZoneId, string activity)
        {
            var sqlConnection = connectAndOpen(connectionString);

            var dtResult = new DataSet();
            var reportResourceKey = "ResReportScheduledTimePerAgent";
            SqlCommand command = sqlConnection.CreateCommand();
            SqlDataAdapter sqlAdapter = new SqlDataAdapter(command);
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = "mart.report_data_agent_schedule_adherence_for_test";
            command.Parameters.AddWithValue("@date_from", date_from);
            command.Parameters.AddWithValue("@date_to", date_to);
            command.Parameters.AddWithValue("@adherence_id", adherence_id);
			command.Parameters.AddWithValue("@agent_code", person.Id);
            command.Parameters.AddWithValue("@time_zone_code", timeZoneId);
            command.Parameters.AddWithValue("@report_resource_key", reportResourceKey);
	        command.Parameters.AddWithValue("@activity_set", activity);
            sqlAdapter.Fill(dtResult);
            return dtResult.Tables[0];
        }


		private static SqlConnection connectAndOpen(string connectionString)
        {
            var sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            return sqlConnection;
        }

		public void assertOverlapping(IPerson person, string timeZoneId, string ETLType)
		{
			//Tests for "Ready Time vs. Schedule Ready Time"
			var adheranceId = 1;
			var adherance = reportDataAgentScheduleAdherence(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, DateTime.Today.AddDays(-2), DateTime.Today.AddDays(-1), adheranceId, person, timeZoneId);
			
			Assert.That(adherance.Rows.Count, Is.EqualTo(81));
			foreach (DataRow row in adherance.Rows)
			{
				var rownumber = (int)row["date_interval_counter"];
				switch (rownumber)
				{
					case 1:
						{
							Assert.That((row["date"]), Is.EqualTo(DateTime.Today.AddDays(-2)));
							Assert.That((row["interval_id"]), Is.EqualTo(78));
							Assert.That((row["interval_name"]), Is.EqualTo("19:30-19:45"));
							Assert.That((row["deviation_tot_m"]), Is.EqualTo(543));
							break;
						}
					case 7:
						{
							Assert.That((row["date"]), Is.EqualTo(DateTime.Today.AddDays(-2)), "60% interval");
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
							Assert.That((row["date"]), Is.EqualTo(DateTime.Today.AddDays(-2)), "100% interval");
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
							Assert.That((row["date"]), Is.EqualTo(DateTime.Today.AddDays(-1)));
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
			adheranceId = 2;
			adherance = reportDataAgentScheduleAdherence(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, DateTime.Today.AddDays(-2), DateTime.Today.AddDays(-1), adheranceId, person, timeZoneId);

			Assert.That(adherance.Rows.Count, Is.EqualTo(81));
			foreach (DataRow row in adherance.Rows)
			{
				var rownumber = (int)row["date_interval_counter"];
				switch (rownumber)
				{
					case 1:
						{
							Assert.That((row["date"]), Is.EqualTo(DateTime.Today.AddDays(-2)));
							Assert.That((row["interval_id"]), Is.EqualTo(78), "First interval_id yesterday");
							Assert.That((row["interval_name"]), Is.EqualTo("19:30-19:45"), "First interval_name yesterday");
							break;
						}
					case 46:
						{
							Assert.That((row["date"]), Is.EqualTo(DateTime.Today.AddDays(-1)));
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
			Assert.That(countIntervalsPerLocalDate(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, person, DateTime.Today.AddDays(-1)), Is.EqualTo(35), "ETL."+ ETLType + " count intervals for Day-1");
            Assert.That(countIntervalsPerLocalDate(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, person, DateTime.Today.AddDays(-2)), Is.EqualTo(46), "ETL." + ETLType + " count intervals for Day-2");
			var column = "deviation_schedule_s";
			Assert.That(sumFactScheduleDeviation(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, person, DateTime.Today.AddDays(-1), column), Is.EqualTo(24540), "ETL." + ETLType + " " + column + " for Day-1");
            Assert.That(sumFactScheduleDeviation(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, person, DateTime.Today.AddDays(-2), column), Is.EqualTo(33660), "ETL." + ETLType + " " + column + " for Day-2");
            column = "scheduled_ready_time_s";
            Assert.That(sumFactScheduleDeviation(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, person, DateTime.Today.AddDays(-1), column), Is.EqualTo(25200), "ETL." + ETLType + " " + column + " for Day-1");
            Assert.That(sumFactScheduleDeviation(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, person, DateTime.Today.AddDays(-2), column), Is.EqualTo(36000), "ETL." + ETLType + " " + column + " for Day-2");
            column = "ready_time_s";
            Assert.That(sumFactScheduleDeviation(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, person, DateTime.Today.AddDays(-1), column), Is.EqualTo(4740), "ETL." + ETLType + " " + column + " for Day-1");
            Assert.That(sumFactScheduleDeviation(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, person, DateTime.Today.AddDays(-2), column), Is.EqualTo(4500), "ETL." + ETLType + " " + column + " for Day-2");
            column = "deviation_schedule_ready_s";
            Assert.That(sumFactScheduleDeviation(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, person, DateTime.Today.AddDays(-1), column), Is.EqualTo(22500), "ETL." + ETLType + " " + column + " for Day-1");
            Assert.That(sumFactScheduleDeviation(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, person, DateTime.Today.AddDays(-2), column), Is.EqualTo(32580), "ETL." + ETLType + " " + column + " for Day-2");

			
		}


	}
}
