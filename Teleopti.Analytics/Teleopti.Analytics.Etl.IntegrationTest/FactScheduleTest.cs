using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Authentication;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs;
using Teleopti.Analytics.Etl.Common.Transformer.Job.MultipleDate;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.IntegrationTest.TestData;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

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
		public void ShouldHaveCorrectScheduleInReports()
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
			var activityPhone = new ActivityConfigurable { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			var activityLunch = new ActivityConfigurable { Name = "Lunch", Color = "Red", InWorkTime = false };

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
			var jobParameters = new JobParameters(
				dateList, 1, "UTC", 15, "", "False",
				CultureInfo.CurrentCulture,
				new FakeContainerHolder(), false
				)
			{
				Helper =
					new JobHelper(new RaptorRepository(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, ""), null, null, null),
				DataSource = SqlCommands.DataSourceIdGet(datasourceName)
			};

			jobParameters.StateHolder.SetLoadBridgeTimeZonePeriod(period, person.PermissionInformation.DefaultTimeZone().Id);
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
			var testDate = new DateTime(2013, 06, 15);
			AnalyticsRunner.RunAnalyticsBaseData(new List<IAnalyticsDataSetup>(), testDate);
			const int queueId = 19;

			AnalyticsRunner.RunSysSetupTestData(testDate, queueId);
			const string timeZoneId = "W. Europe Standard Time";
			IPerson person;
			BasicShiftSetup.SetupBasicForShifts();
			BasicShiftSetup.AddPerson(out person, "Ola H", "", testDate);

			var cat = new ShiftCategoryConfigurable { Name = "Kattegat", Color = "Green" };
			var activityPhone = new ActivityConfigurable { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			var activityLunch = new ActivityConfigurable { Name = "Lunch", Color = "Red", InWorkTime = false };

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
				Helper =
					new JobHelper(new RaptorRepository(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, ""), null, null, null),
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
		public void ShouldFindAdherence()
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
			var activityPhone = new ActivityConfigurable { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			var activityLunch = new ActivityConfigurable { Name = "Lunch", Color = "Red", InWorkTime = false };

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
			var jobParameters = new JobParameters(
				dateList, 1, "UTC", 15, "", "False",
				CultureInfo.CurrentCulture,
				new FakeContainerHolder(), false
				)
			{
				Helper =
					new JobHelper(new RaptorRepository(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, ""), null, null, null),
				DataSource = SqlCommands.DataSourceIdGet(datasourceName)
			};

			jobParameters.StateHolder.SetLoadBridgeTimeZonePeriod(period, person.PermissionInformation.DefaultTimeZone().Id);

			//Run ETL.Intraday first time just to set "LastUpdatedPerStep"
			StepRunner.RunIntraday(jobParameters);

			//Nightly
			StepRunner.RunNightly(jobParameters);
			AssertOverlapping(person, timeZoneId, "Nightly", testDate);

			//Re-Add overlapping
			RemovePersonSchedule.RemoveAssignmentAndReadmodel(BasicShiftSetup.Scenario.Scenario, "Ola H", testDate.AddDays(-1), person);
			RemovePersonSchedule.RemoveAssignmentAndReadmodel(BasicShiftSetup.Scenario.Scenario, "Ola H", testDate.AddDays(-2), person);
			BasicShiftSetup.AddShift("Ola H", testDate.AddDays(-2), 21, 11, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);
			BasicShiftSetup.AddShift("Ola H", testDate.AddDays(-1), 6, 8, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);

			//Intraday
			StepRunner.RunIntraday(jobParameters);
			AssertOverlapping(person, timeZoneId, "Intraday", testDate);

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

			var adherance = SqlCommands.ReportDataAgentScheduleAdherence(testDate.AddDays(-2), testDate.AddDays(-1), aheranceTypeReadyTime, person, timeZoneId);

			var cellValue = IntervalValueGet(adherance, testDate.AddDays(-1), "deviation_m", "09:15-09:30", person);
			Assert.That(cellValue, Is.EqualTo(0));

		}

		[Test]
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
			var activityPhone = new ActivityConfigurable { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			var activityLunch = new ActivityConfigurable { Name = "Lunch", Color = "Red", InWorkTime = false };

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

			var user = MockRepository.GenerateMock<IPerson>();
			var permission = MockRepository.GenerateMock<IPermissionInformation>();
			user.Stub(x => x.PermissionInformation).Return(permission);
			permission.Stub(x => x.HasAccessToAllBusinessUnits()).Return(true);
			var dataSource = SetupFixtureForAssembly.DataSource;
			var jobHelper = new JobHelper(null, null, null,
				new LogOnHelperFake(dataSource, user));
			//var jobHelper = new JobHelper(null, null, null,new LogOnHelper(""));
			jobHelper.LogOffTeleoptiCccDomain();
			var fakeManager = new FakeContainerHolder();
			fakeManager.EnableToggle(Toggles.ETL_OnlyLatestQueueAgentStatistics_30787);
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
			jobHelper.SelectDataSourceContainer(jobHelper.TenantCollection[0].DataSourceName);
			var jobRunner = new JobRunner();
			var nightlyJob = new JobBase(jobParameters, new NightlyJobCollection(jobParameters), "Nightly", true, true);
			var jobResultCollection = new List<IJobResult>();

			nightlyJob.StepList.Remove(nightlyJob.StepList.FirstOrDefault(x => x.Name.Equals("Statistics Update Notification")));
			var jobListResult = jobRunner.Run(nightlyJob, jobResultCollection, new List<IJobStep>());

			Assert.That(jobListResult[0].HasError, Is.False);

			//save the current datasource
			var gen = new GeneralFunctions(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix);
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

		public static int IntervalValueGet(DataTable adherance, DateTime testDate, String testColumn, String intervalName, IPerson person)
		{
			foreach (DataRow dtRow in adherance.Rows)
			{
				if ((dtRow["date"]).Equals(testDate))
				{
					foreach (DataColumn dc in adherance.Columns)
					{
						if (dc.ColumnName.Equals(testColumn))
						{
							if ((dtRow["interval_name"]).ToString().Equals(intervalName))
							{
								return Convert.ToInt32(dtRow[testColumn]);
							}
						}
					}
				}
			}
			return -9999;
		}

		public void AssertOverlapping(IPerson person, string timeZoneId, string etlType, DateTime testDate)
		{
			//Tests for "Ready Time vs. Schedule Ready Time"
			var adherance = SqlCommands.ReportDataAgentScheduleAdherence(testDate.AddDays(-2), testDate.AddDays(-1), aheranceTypeReadyTime, person, timeZoneId);

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
			adherance = SqlCommands.ReportDataAgentScheduleAdherence(testDate.AddDays(-2), testDate.AddDays(-1), aheranceTypeSchedule, person, timeZoneId);

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
			Assert.That(SqlCommands.CountIntervalsPerLocalDate(person, testDate.AddDays(-1)), Is.EqualTo(35), "ETL." + etlType + " count intervals for Day-1");
			Assert.That(SqlCommands.CountIntervalsPerLocalDate(person, testDate.AddDays(-2)), Is.EqualTo(46), "ETL." + etlType + " count intervals for Day-2");
			var column = "deviation_schedule_s";
			Assert.That(SqlCommands.SumFactScheduleDeviation(person, testDate.AddDays(-1), column), Is.EqualTo(24540), "ETL." + etlType + " " + column + " for Day-1");
			Assert.That(SqlCommands.SumFactScheduleDeviation(person, testDate.AddDays(-2), column), Is.EqualTo(33660), "ETL." + etlType + " " + column + " for Day-2");
			column = "scheduled_ready_time_s";
			Assert.That(SqlCommands.SumFactScheduleDeviation(person, testDate.AddDays(-1), column), Is.EqualTo(25200), "ETL." + etlType + " " + column + " for Day-1");
			Assert.That(SqlCommands.SumFactScheduleDeviation(person, testDate.AddDays(-2), column), Is.EqualTo(36000), "ETL." + etlType + " " + column + " for Day-2");
			column = "ready_time_s";
			Assert.That(SqlCommands.SumFactScheduleDeviation(person, testDate.AddDays(-1), column), Is.EqualTo(4740), "ETL." + etlType + " " + column + " for Day-1");
			Assert.That(SqlCommands.SumFactScheduleDeviation(person, testDate.AddDays(-2), column), Is.EqualTo(4500), "ETL." + etlType + " " + column + " for Day-2");
			column = "deviation_schedule_ready_s";
			Assert.That(SqlCommands.SumFactScheduleDeviation(person, testDate.AddDays(-1), column), Is.EqualTo(22500), "ETL." + etlType + " " + column + " for Day-1");
			Assert.That(SqlCommands.SumFactScheduleDeviation(person, testDate.AddDays(-2), column), Is.EqualTo(32580), "ETL." + etlType + " " + column + " for Day-2");


		}
	}

	public class LogOnHelperFake : ILogOnHelper
	{
		private readonly DataSourceContainer _choosenDb;
		private IList<IBusinessUnit> _buList;
		private readonly List<ITenantName> _tenantNames;
		private IAvailableBusinessUnitsProvider _availableBusinessUnitsProvider;
	

		public LogOnHelperFake(IDataSource dataSource, IPerson person)
		{
			_choosenDb = new DataSourceContainer(dataSource, person);
			_tenantNames = new List<ITenantName> { new TenantName { DataSourceName = dataSource.DataSourceName } };
			_availableBusinessUnitsProvider = new AvailableBusinessUnitsProvider(new RepositoryFactory());
		}

		public IList<IBusinessUnit> GetBusinessUnitCollection()
		{
			if (_buList == null)
			{
				_buList = new List<IBusinessUnit>(_availableBusinessUnitsProvider.AvailableBusinessUnits(_choosenDb.User, _choosenDb.DataSource));
			}

			//Trace.WriteLine("No allowed business unit found in current database.");
			if (_buList == null || _buList.Count == 0)
			{
				throw new AuthenticationException("No allowed business unit found in current database '" +
											_choosenDb + "'.");
			}

			return _buList;
		}

		public List<ITenantName> TenantCollection
		{
			get
			{
				return _tenantNames;
			}
		}

		public IDataSourceContainer SelectedDataSourceContainer { get { return _choosenDb; } }

		public bool SetBusinessUnit(IBusinessUnit businessUnit)
		{
			LicenseActivator.ProvideLicenseActivator();
			return true;
		}

		public bool SelectDataSourceContainer(string dataSourceName)
		{
		//	var person = new LoadUserUnauthorized().LoadFullPersonInSeperateTransaction(_choosenDb.DataSource.Application, SuperUser.Id_AvoidUsing_This);
		//	_choosenDb.SetUser(person);
			return true;
		}

		public void LogOff()
		{
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}
	}
}
