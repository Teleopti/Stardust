using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs;
using Teleopti.Analytics.Etl.Common.Transformer.Job.MultipleDate;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.IntegrationTest.TestData;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;

using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;
using Person = Teleopti.Ccc.Domain.Common.Person;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
	[TestFixture]
	public class FactScheduleTest
	{
		private const int aheranceTypeReadyTime = 1;
		private const int aheranceTypeSchedule = 2;
		private const string datasourceName = "Teleopti CCC Agg: Default log object";

		[SetUp]
		public void Setup()
		{
			SetupFixtureForAssembly.BeginTest();
			AnalyticsRunner.DropAndCreateTestProcedures();
		}

		[TearDown]
		public void TearDown()
		{
			SetupFixtureForAssembly.EndTest();
		}

		[Test]
		[Ignore("Reason mandatory for NUnit 3")]
		public void ShouldWorkWithOverlappingShifts()
		{
			var testDate = new DateTime(2013, 06, 15);
			AnalyticsRunner.RunAnalyticsBaseData(new List<IAnalyticsDataSetup>(), testDate);
			const int queueId = 19;

			AnalyticsRunner.RunSysSetupTestData(testDate, queueId);
			const string timeZoneId = "W. Europe Standard Time";
			IPerson person;
			BasicShiftSetup.SetupBasicForShifts();
			BasicShiftSetup.AddPerson(out person, "Ola H", "", testDate);

			var cat = new ShiftCategoryConfigurable { Name = "Kattegat", Color = "Green" };
			var activityPhone = new ActivitySpec { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			var activityLunch = new ActivitySpec { Name = "Lunch", Color = "Red", InWorkTime = false };

			Data.Apply(cat);
			Data.Apply(activityPhone);
			Data.Apply(activityLunch);


			//Add overlapping
			BasicShiftSetup.AddShift("Ola H", testDate.AddDays(-2), 21, 11, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);
			BasicShiftSetup.AddShift("Ola H", testDate.AddDays(-1), 6, 8, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);

			var period = new DateTimePeriod(testDate.AddDays(-14).ToUniversalTime(), testDate.AddDays(14).ToUniversalTime());
			var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
			dateList.Add(testDate.AddDays(-3), testDate.AddDays(3), JobCategoryType.Schedule);
			var jobParameters = new JobParameters(
				dateList, 1, "UTC", 15, "", "False",
				CultureInfo.CurrentCulture,
				new FakeContainerHolder(), false
				)
			{
				Helper = new JobHelperForTest(new RaptorRepository(InfraTestConfigReader.AnalyticsConnectionString, null, null), null),
				DataSource = SqlCommands.DataSourceIdGet(datasourceName)
			};

			jobParameters.StateHolder.SetLoadBridgeTimeZonePeriod(period, person.PermissionInformation.DefaultTimeZone().Id);
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
		[Ignore("Integration test in the way of refactoring")]
		// too long
		// mocks and fakes in an integration test that are in the way of refactoring
		// can not understand the purpose with too many asserts
		// can not understand the reason because it does not give any information other than returning false
		// ...
		public void TestCtiPlattformUpdate()
		{
			var testDate = new DateTime(2013, 06, 15);
			const string timeZoneId = "W. Europe Standard Time";
			const int queueId = 19;

			AnalyticsRunner.RunAnalyticsBaseData(new List<IAnalyticsDataSetup>(), testDate);
			AnalyticsRunner.RunSysSetupTestData(testDate, queueId);


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
			var activityPhone = new ActivitySpec { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			var activityLunch = new ActivitySpec { Name = "Lunch", Color = "Red", InWorkTime = false };

			Data.Apply(cat);
			Data.Apply(activityPhone);
			Data.Apply(activityLunch);

			//Add basic shift 09:00 - 17:00
			BasicShiftSetup.AddShift("Ola H", testDate.AddDays(-1), 9, 8, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);

			var period = new DateTimePeriod(testDate.AddDays(-14).ToUniversalTime(), testDate.AddDays(14).ToUniversalTime());
			var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
			dateList.Add(testDate.AddDays(-3), testDate.AddDays(3), JobCategoryType.Schedule);
			dateList.Add(testDate.AddDays(-3), testDate.AddDays(-3), JobCategoryType.AgentStatistics);
			dateList.Add(testDate.AddDays(-3), testDate.AddDays(-3), JobCategoryType.Forecast);
			dateList.Add(testDate.AddDays(-3), testDate.AddDays(-3), JobCategoryType.QueueStatistics);

			var user = new Person();
			user.PermissionInformation.AddApplicationRole(new ApplicationRole
			{
				AvailableData = new AvailableData {AvailableDataRange = AvailableDataRangeOption.Everyone}
			});
			var dataSource = SetupFixtureForAssembly.DataSource;
			JobHelper jobHelper = null;
			//var jobHelper = new JobHelperForTest(null, null, new LogOnHelperFake(dataSource, user));
			//var jobHelper = new JobHelper(null, null, null,new LogOnHelper(""));
			jobHelper.LogOffTeleoptiCccDomain();
			var fakeManager = new FakeContainerHolder();
			var jobParameters = new JobParameters(
				dateList, 1, "UTC", 15, "", "False",
				CultureInfo.CurrentCulture,
				fakeManager,
				false
				)
			{
				Helper = jobHelper,
				DataSource = SqlCommands.DataSourceIdGet(datasourceName)
			};
			jobParameters.StateHolder.SetLoadBridgeTimeZonePeriod(period, person.PermissionInformation.DefaultTimeZone().Id);
			jobHelper.SelectDataSourceContainer(dataSource.DataSourceName);
			var jobRunner = new JobRunner();
			var nightlyJob = new JobBase(jobParameters, new NightlyJobCollection(jobParameters), "Nightly", true, true);
			var jobResultCollection = new List<IJobResult>();

			nightlyJob.StepList.Remove(nightlyJob.StepList.FirstOrDefault(x => x.Name.Equals("Statistics Update Notification")));
			var jobListResult = jobRunner.Run(nightlyJob, jobResultCollection, new List<IJobStep>());

			Assert.That(jobListResult[0].HasError, Is.False);

			//save the current datasource
			var gen = new GeneralFunctions(new GeneralInfrastructure(new BaseConfigurationRepository()));
			gen.SetConnectionString(InfraTestConfigReader.AnalyticsConnectionString);
			gen.SaveDataSource(jobParameters.DataSource, SqlCommands.TimezoneIdGet(timeZoneId));
			SqlCommands.EtlJobIntradaySettingsReset(testDate.AddDays(-3));

			jobParameters.NowForTestPurpose = testDate.AddDays(0);

			var intradayJob = new JobBase(jobParameters, new IntradayJobCollection(jobParameters), "Intraday", true, true);
			intradayJob.StepList.Remove(intradayJob.StepList.FirstOrDefault(x => x.Name.Equals("Statistics Update Notification")));
			jobResultCollection = new List<IJobResult>(); //reset

			jobListResult = jobRunner.Run(intradayJob, jobResultCollection, new List<IJobStep>());
			Assert.That(jobListResult[0].HasError, Is.False);
			//check max interval value before (59)
			Assert.That(SqlCommands.MaxIntervalLogObjectDetail(2, jobParameters.DataSource), Is.EqualTo(59));
			AnalyticsRunner.AddOneInterval(el.AcdLogOnAggId, el.AcdLogOnName, queueId, 8, null, null);
			//check interval max value after 60
			Assert.That(SqlCommands.MaxIntervalLogObjectDetail(2, jobParameters.DataSource), Is.EqualTo(60));
			//run intraday and check that agg and mart is synced
			jobResultCollection = new List<IJobResult>(); //reset
			jobListResult = jobRunner.Run(intradayJob, jobResultCollection, new List<IJobStep>());
			Assert.That(jobListResult[0].HasError, Is.False);

			Assert.That(SqlCommands.IntradayDetailSynced(1, jobParameters.DataSource), Is.True);
			Assert.That(SqlCommands.IntradayDetailSynced(2, jobParameters.DataSource), Is.True);

			//add intervals to check devation in sync
			BasicShiftSetup.AddShift("Ola H", testDate.AddDays(0), 9, 8, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);

			var startInterval = 32;
			var stopInterval = 68;

			Assert.That(SqlCommands.CountIntervalsPerLocalDate(person, testDate.AddDays(0)), Is.EqualTo(0));

			while (startInterval <= stopInterval)
			{
				AnalyticsRunner.AddOneInterval(el.AcdLogOnAggId, el.AcdLogOnName, queueId, 8, testDate, startInterval);
				startInterval++;
			}
			Assert.That(SqlCommands.MaxIntervalLogObjectDetail(2, jobParameters.DataSource), Is.EqualTo(stopInterval));

			//run intraday and check that agg and mart is synced
			jobResultCollection = new List<IJobResult>(); //reset
			jobListResult = jobRunner.Run(intradayJob, jobResultCollection, new List<IJobStep>());
			Assert.That(jobListResult[0].HasError, Is.False);
			//check deviation is loaded for today and that all sync dates/intervals are OK. Number of intervals in deviation is now 36
			Assert.That(SqlCommands.SumFactScheduleDeviation(person, testDate.AddDays(0), "deviation_schedule_s"), Is.EqualTo(16080));
			Assert.That(SqlCommands.IntradayDetailSynced(1, jobParameters.DataSource), Is.True);
			Assert.That(SqlCommands.IntradayDetailSynced(2, jobParameters.DataSource), Is.True);
			Assert.That(SqlCommands.CountIntervalsPerLocalDate(person, testDate.AddDays(0)), Is.EqualTo(37));

			//Add one extra interval outside shift (and of shift)
			stopInterval = stopInterval + 1;
			AnalyticsRunner.AddOneInterval(el.AcdLogOnAggId, el.AcdLogOnName, queueId, 8, null, null);
			Assert.That(SqlCommands.MaxIntervalLogObjectDetail(2, jobParameters.DataSource), Is.EqualTo(stopInterval));

			//move @now ahead and run intraday again and verfiy agg and mart is synced, Number of intervals in deviation is now 37
			jobParameters.NowForTestPurpose = testDate.AddDays(0).AddHours(20);
			jobResultCollection = new List<IJobResult>(); //reset
			jobListResult = jobRunner.Run(intradayJob, jobResultCollection, new List<IJobStep>());
			Assert.That(jobListResult[0].HasError, Is.False);
			Assert.That(SqlCommands.IntradayDetailSynced(1, jobParameters.DataSource), Is.True);
			Assert.That(SqlCommands.IntradayDetailSynced(2, jobParameters.DataSource), Is.True);

			Assert.That(SqlCommands.CountIntervalsPerLocalDate(person, testDate.AddDays(0)), Is.EqualTo(38));
		}
	}
}
