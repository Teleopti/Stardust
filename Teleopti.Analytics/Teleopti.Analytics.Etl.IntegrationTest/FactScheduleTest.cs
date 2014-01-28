using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Analytics.Etl.IntegrationTest.Models;
using Teleopti.Analytics.Etl.IntegrationTest.TestData;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.Jobs;
using Teleopti.Analytics.Etl.Transformer.Job.MultipleDate;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Ccc.Domain.AgentInfo;
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
			IPerson person;
			BasicShiftSetup.SetupBasicForShifts();
			BasicShiftSetup.AddPerson(out person,"Ola H", "");
			BasicShiftSetup.AddOverlapping("Ola H");

			var period = new DateTimePeriod(DateTime.Today.AddDays(-14).ToUniversalTime(), DateTime.Today.AddDays(14).ToUniversalTime());
			var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
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
			BasicShiftSetup.AddThreeShifts("Ola H");

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

	}
}
