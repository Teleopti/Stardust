using System;
using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.IntegrationTest.TestData;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.MultipleDate;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
	[TestFixture]
	[Category("LongRunning")]
	public class AnalyticsScheduleRepositoryTest //: DatabaseTest
	{
		private IAnalyticsScheduleRepository _target;
		private const string datasourceName = "Teleopti CCC Agg: Default log object";
		[SetUp]
		public void Setup()
		{
			_target = StatisticRepositoryFactory.CreateAnalytics();
			SetupFixtureForAssembly.BeginTest();
			AnalyticsRunner.DropAndCreateTestProcedures();
		}

		[TearDown]
		public void TearDown()
		{
			SetupFixtureForAssembly.EndTest();
		}

		[Test]
		public void ShouldLoadActivities()
		{
			var activityPhone = new ActivityConfigurable { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			var activityLunch = new ActivityConfigurable { Name = "Lunch", Color = "Red", InWorkTime = false };

			Data.Apply(activityPhone);
			Data.Apply(activityLunch);

			var jobParameters = getJobParameters();
			StepRunner.RunNightly(jobParameters);

			var acts = _target.Activities();
			acts.Count.Should().Be.EqualTo(3); //one undefined too
		}

		[Test]
		public void ShouldLoadAbsences()
		{
			var absenceFree = new AbsenceConfigurable { Name = "Free", Color = "LightGreen" };
			var absenceNelson = new AbsenceConfigurable { Name = "Nelson Mandela", Color = "Red" };

			Data.Apply(absenceFree);
			Data.Apply(absenceNelson);

			var jobParameters = getJobParameters();
			StepRunner.RunNightly(jobParameters);

			var acts = _target.Absences();
			acts.Count.Should().Be.EqualTo(3); //one undefined too
		}

		private static JobParameters getJobParameters()
		{
			const string timeZoneId = "W. Europe Standard Time";
			var testDate = new DateTime(2013, 06, 15);
			var period = new DateTimePeriod(testDate.AddDays(-14).ToUniversalTime(), testDate.AddDays(1).ToUniversalTime());
			var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
			dateList.Add(testDate.AddDays(-1), testDate.AddDays(1), JobCategoryType.Schedule);
			var jobParameters = new JobParameters(dateList, 1, "UTC", 15, "", "False", CultureInfo.CurrentCulture,
				new EtlToggleManager())
			{
				Helper =
					new JobHelper(new RaptorRepository(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, ""), null, null, null),
				DataSource = SqlCommands.DataSourceIdGet(datasourceName)
			};

            jobParameters.StateHolder.SetLoadBridgeTimeZonePeriod(period, timeZoneId);
			return jobParameters;
		}
	}
}