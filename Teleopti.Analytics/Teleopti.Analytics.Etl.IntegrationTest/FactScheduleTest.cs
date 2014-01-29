using System;
using System.Collections.Generic;
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
		public void ShouldWorkWithOverlappingShifts()
		{
			AnalyticsRunner.RunAnalyticsBaseData(new List<IAnalyticsDataSetup>());
			AnalyticsRunner.RunSysSetupTestData();
			const string timeZoneId = "W. Europe Standard Time";
			IPerson person;
			BasicShiftSetup.SetupBasicForShifts();
			BasicShiftSetup.AddPerson(out person,"Ola H", "");
			BasicShiftSetup.AddOverlapping("Ola H");

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

			Assert.That(factSchedules.Count(), Is.EqualTo(68));
			var time = DateTime.Today.AddHours(5);
			var last = from s in db.fact_schedule where s.shift_starttime == time select s;

			Assert.That(last.Count(), Is.EqualTo(32));
			time = DateTime.Today.AddDays(-1).AddHours(20);
			var first = from s in db.fact_schedule where s.shift_starttime == time select s;
			Assert.That(first.Count(), Is.EqualTo(36));
		}

		[Test]
		public void ShouldFindAdherence()
		{
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
			BasicShiftSetup.AddOverlapping("Ola H");

			var period = new DateTimePeriod(DateTime.Today.AddDays(-14).ToUniversalTime(), DateTime.Today.AddDays(14).ToUniversalTime());
			var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
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
			StepRunner.RunNightly(jobParameters);

			Assert.That(countIntervalsPerLocalDate(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, person, DateTime.Today), Is.EqualTo(34));
			Assert.That(countIntervalsPerLocalDate(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, person, DateTime.Today.AddDays(-1)), Is.EqualTo(38));

			Assert.That(sumScheduledReadyTime(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, person, DateTime.Today), Is.EqualTo(25200));
			Assert.That(sumScheduledReadyTime(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, person, DateTime.Today.AddDays(-1)), Is.EqualTo(28800));

			Assert.That(sumReadyTime(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, person, DateTime.Today), Is.EqualTo(3240));
			Assert.That(sumReadyTime(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, person, DateTime.Today.AddDays(-1)), Is.EqualTo(3240));
			
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

		private static int sumScheduledReadyTime(string connectionString, IPerson person, DateTime datelocal)
		{
			var sqlConnection = connectAndOpen(connectionString);

			string sql = string.Format("select sum(f.scheduled_ready_time_s) from mart.fact_schedule_deviation f " +
	"inner join mart.dim_person p on f.person_id = p.person_id " +
	"inner join mart.dim_time_zone tz on p.time_zone_id = tz.time_zone_id " +
	"join mart.bridge_time_zone btz on p.time_zone_id = btz.time_zone_id " +
		"and f.shift_startdate_id = btz.date_id " +
		"and f.shift_startinterval_id = btz.interval_id " +
	"join mart.dim_date d " +
	"on btz.local_date_id = d.date_id " +
	"where d.date_date = '{0}' and p.person_code = '{1}'", datelocal.Date, person.Id);
			using (var sqlCommand = new SqlCommand(sql, sqlConnection))
			{
				return Convert.ToInt32(sqlCommand.ExecuteScalar(), CultureInfo.CurrentCulture);
			}

		}

		private static int sumReadyTime(string connectionString, IPerson person, DateTime datelocal)
		{
			var sqlConnection = connectAndOpen(connectionString);

			string sql = string.Format("select sum(f.ready_time_s) from mart.fact_schedule_deviation f " +
	"inner join mart.dim_person p on f.person_id = p.person_id " +
	"inner join mart.dim_time_zone tz on p.time_zone_id = tz.time_zone_id " +
	"join mart.bridge_time_zone btz on p.time_zone_id = btz.time_zone_id " +
		"and f.shift_startdate_id = btz.date_id " +
		"and f.shift_startinterval_id = btz.interval_id " +
	"join mart.dim_date d " +
	"on btz.local_date_id = d.date_id " +
	"where d.date_date = '{0}' and p.person_code = '{1}'", datelocal.Date, person.Id);
			using (var sqlCommand = new SqlCommand(sql, sqlConnection))
			{
				return Convert.ToInt32(sqlCommand.ExecuteScalar(), CultureInfo.CurrentCulture);
			}

		}
    	
    	private static SqlConnection connectAndOpen(string connectionString)
        {
            var sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            return sqlConnection;
        }

	}
}
