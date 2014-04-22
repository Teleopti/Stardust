using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.InfrastructureTest.WebReports.DailyMetricsForDay;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.WebReports.DetailedAdherenceForDay
{
	[TestFixture]
	public class TotalAdherenceForDateTest : WebReportTest
	{
		private const int scheduledReadyTimeOneMinutes = 1;
		private const int scheduledReadyTimeTwoMinutes = 3;
		private const int deviationScheduleReadyOneSeconds = 60;
		private const int deviationScheduleReadyTwoSeconds = 120;

		protected override void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory)
		{
			analyticsDataFactory.Setup(new FactSchedule(PersonId, Today.DateId, Today.DateId, 0, scheduledReadyTimeOneMinutes, 1, ScenarioId));
			analyticsDataFactory.Setup(new FactSchedule(PersonId, Today.DateId, Today.DateId, 0, scheduledReadyTimeTwoMinutes, 2, ScenarioId));
			analyticsDataFactory.Setup(new FactScheduleDeviation(Today.DateId, Today.DateId, 1, PersonId, 0, 0, deviationScheduleReadyOneSeconds, 0, true));
			analyticsDataFactory.Setup(new FactScheduleDeviation(Today.DateId, Today.DateId, 2, PersonId, 0, 0, deviationScheduleReadyTwoSeconds, 0, true));
		}

		[Test]
		public void ShouldReturnTotalAdherenceForDate()
		{
			var expectedPercentage = new Percent(0.25);
			Target(
				(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository) =>
					new DetailedAdherenceForDayQuery(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository))
				.Execute(Today.Date).Last()
				.TotalAdherence.Should().Be.EqualTo(expectedPercentage);
		}
	}

	[TestFixture]
	public class IntervalsPerDayForDateTest : WebReportTest
	{
		private const int scheduledReadyTimeOneMinutes = 1;
		private const int scheduledReadyTimeTwoMinutes = 3;
		private const int deviationScheduleReadyOneSeconds = 60;
		private const int deviationScheduleReadyTwoSeconds = 120;

		protected override void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory)
		{
			analyticsDataFactory.Setup(new FactSchedule(PersonId, Today.DateId, Today.DateId, 0, scheduledReadyTimeOneMinutes, 1, ScenarioId));
			analyticsDataFactory.Setup(new FactSchedule(PersonId, Today.DateId, Today.DateId, 0, scheduledReadyTimeTwoMinutes, 2, ScenarioId));
			analyticsDataFactory.Setup(new FactScheduleDeviation(Today.DateId, Today.DateId, 1, PersonId, 0, 0, deviationScheduleReadyOneSeconds, 0, true));
			analyticsDataFactory.Setup(new FactScheduleDeviation(Today.DateId, Today.DateId, 2, PersonId, 0, 0, deviationScheduleReadyTwoSeconds, 0, true));
		}

		[Test]
		public void ShouldReturnIntervalsPerDayForDate()
		{
			Target(
				(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository) =>
					new DetailedAdherenceForDayQuery(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository))
				.Execute(Today.Date).Last()
				.IntervalsPerDay.Should().Be.EqualTo(96);
		}
	}

	[TestFixture]
	public class IntervalIdForDateTest : WebReportTest
	{
		private const int scheduledReadyTimeOneMinutes = 1;
		private const int scheduledReadyTimeTwoMinutes = 3;
		private const int deviationScheduleReadyOneSeconds = 60;
		private const int deviationScheduleReadyTwoSeconds = 120;

		protected override void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory)
		{
			analyticsDataFactory.Setup(new FactSchedule(PersonId, Today.DateId, Today.DateId, 0, scheduledReadyTimeOneMinutes, 1, ScenarioId));
			analyticsDataFactory.Setup(new FactSchedule(PersonId, Today.DateId, Today.DateId, 0, scheduledReadyTimeTwoMinutes, 2, ScenarioId));
			analyticsDataFactory.Setup(new FactScheduleDeviation(Today.DateId, Today.DateId, 1, PersonId, 0, 0, deviationScheduleReadyOneSeconds, 0, true));
			analyticsDataFactory.Setup(new FactScheduleDeviation(Today.DateId, Today.DateId, 2, PersonId, 0, 0, deviationScheduleReadyTwoSeconds, 0, true));
		}

		[Test]
		public void ShouldReturnIntervalIdForDate()
		{
			Target(
				(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository) =>
					new DetailedAdherenceForDayQuery(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository))
				.Execute(Today.Date).First()
				.IntervalId.Should().Be.EqualTo(9);
		}
	}

	[TestFixture]
	public class IntervalCounterForDateTest : WebReportTest
	{
		private const int scheduledReadyTimeOneMinutes = 1;
		private const int scheduledReadyTimeTwoMinutes = 3;
		private const int deviationScheduleReadyOneSeconds = 60;
		private const int deviationScheduleReadyTwoSeconds = 120;

		protected override void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory)
		{
			analyticsDataFactory.Setup(new FactSchedule(PersonId, Today.DateId, Today.DateId, 0, scheduledReadyTimeOneMinutes, 1, ScenarioId));
			analyticsDataFactory.Setup(new FactSchedule(PersonId, Today.DateId, Today.DateId, 0, scheduledReadyTimeTwoMinutes, 2, ScenarioId));
			analyticsDataFactory.Setup(new FactScheduleDeviation(Today.DateId, Today.DateId, 1, PersonId, 0, 0, deviationScheduleReadyOneSeconds, 0, true));
			analyticsDataFactory.Setup(new FactScheduleDeviation(Today.DateId, Today.DateId, 2, PersonId, 0, 0, deviationScheduleReadyTwoSeconds, 0, true));
		}

		[Test]
		public void ShouldReturnIntervalCounterForDate()
		{
			Target(
				(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository) =>
					new DetailedAdherenceForDayQuery(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository))
				.Execute(Today.Date).First()
				.IntervalCounter.Should().Be.EqualTo(1);
		}
	}

	[TestFixture]
	public class AdherenceForDateTest : WebReportTest
	{
		private const int scheduledReadyTimeOneMinutes = 1;
		private const int scheduledReadyTimeTwoMinutes = 3;
		private const int deviationScheduleReadyOneSeconds = 60;
		private const int deviationScheduleReadyTwoSeconds = 120;

		protected override void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory)
		{
			analyticsDataFactory.Setup(new FactSchedule(PersonId, Today.DateId, Today.DateId, 0, scheduledReadyTimeOneMinutes, 1, ScenarioId));
			analyticsDataFactory.Setup(new FactSchedule(PersonId, Today.DateId, Today.DateId, 0, scheduledReadyTimeTwoMinutes, 2, ScenarioId));
			analyticsDataFactory.Setup(new FactScheduleDeviation(Today.DateId, Today.DateId, 1, PersonId, 0, 0, deviationScheduleReadyOneSeconds, 0, true));
			analyticsDataFactory.Setup(new FactScheduleDeviation(Today.DateId, Today.DateId, 2, PersonId, 0, 0, deviationScheduleReadyTwoSeconds, 0, true));
		}

		[Test]
		public void ShouldReturnAdherenceForDate()
		{
			Target(
				(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository) =>
					new DetailedAdherenceForDayQuery(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository))
				.Execute(Today.Date).Last()
				.Adherence.Should().Be.EqualTo(0.333);
		}
	}

	[TestFixture]
	public class DeviationForDateTest : WebReportTest
	{
		private const int scheduledReadyTimeOneMinutes = 1;
		private const int scheduledReadyTimeTwoMinutes = 3;
		private const int deviationScheduleReadyOneSeconds = 60;
		private const int deviationScheduleReadyTwoSeconds = 120;

		protected override void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory)
		{
			analyticsDataFactory.Setup(new FactSchedule(PersonId, Today.DateId, Today.DateId, 0, scheduledReadyTimeOneMinutes, 1, ScenarioId));
			analyticsDataFactory.Setup(new FactSchedule(PersonId, Today.DateId, Today.DateId, 0, scheduledReadyTimeTwoMinutes, 2, ScenarioId));
			analyticsDataFactory.Setup(new FactScheduleDeviation(Today.DateId, Today.DateId, 1, PersonId, 0, 0, deviationScheduleReadyOneSeconds, 0, true));
			analyticsDataFactory.Setup(new FactScheduleDeviation(Today.DateId, Today.DateId, 2, PersonId, 0, 0, deviationScheduleReadyTwoSeconds, 0, true));
		}

		[Test]
		public void ShouldReturnDeviationForDate()
		{
			Target(
				(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository) =>
					new DetailedAdherenceForDayQuery(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository))
				.Execute(Today.Date).Last()
				.Deviation.Should().Be.EqualTo(2);
		}
	}

	[TestFixture]
	public class DisplayColorForDateTest : WebReportTest
	{
		private const int scheduledReadyTimeOneMinutes = 1;
		private const int scheduledReadyTimeTwoMinutes = 3;
		private const int deviationScheduleReadyOneSeconds = 60;
		private const int deviationScheduleReadyTwoSeconds = 120;

		protected override void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory)
		{
			analyticsDataFactory.Setup(new FactSchedule(PersonId, Today.DateId, Today.DateId, 0, scheduledReadyTimeOneMinutes, 1, ScenarioId));
			analyticsDataFactory.Setup(new FactSchedule(PersonId, Today.DateId, Today.DateId, 0, scheduledReadyTimeTwoMinutes, 2, ScenarioId));
			analyticsDataFactory.Setup(new FactScheduleDeviation(Today.DateId, Today.DateId, 1, PersonId, 0, 0, deviationScheduleReadyOneSeconds, 0, true));
			analyticsDataFactory.Setup(new FactScheduleDeviation(Today.DateId, Today.DateId, 2, PersonId, 0, 0, deviationScheduleReadyTwoSeconds, 0, true));
		}

		[Test]
		public void ShouldReturnDisplayColorForDate()
		{
			Target(
				(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository) =>
					new DetailedAdherenceForDayQuery(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository))
				.Execute(Today.Date).First()
				.DisplayColor.Should().Be.EqualTo(Color.Empty);
		}
	}

	[TestFixture]
	public class DisplayNameForDateTest : WebReportTest
	{
		private const int scheduledReadyTimeOneMinutes = 1;
		private const int scheduledReadyTimeTwoMinutes = 3;
		private const int deviationScheduleReadyOneSeconds = 60;
		private const int deviationScheduleReadyTwoSeconds = 120;

		protected override void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory)
		{
			analyticsDataFactory.Setup(new FactSchedule(PersonId, Today.DateId, Today.DateId, 0, scheduledReadyTimeOneMinutes, 1, ScenarioId));
			analyticsDataFactory.Setup(new FactSchedule(PersonId, Today.DateId, Today.DateId, 0, scheduledReadyTimeTwoMinutes, 2, ScenarioId));
			analyticsDataFactory.Setup(new FactScheduleDeviation(Today.DateId, Today.DateId, 1, PersonId, 0, 0, deviationScheduleReadyOneSeconds, 0, true));
			analyticsDataFactory.Setup(new FactScheduleDeviation(Today.DateId, Today.DateId, 2, PersonId, 0, 0, deviationScheduleReadyTwoSeconds, 0, true));
		}

		[Test]
		public void ShouldReturnDisplayNameForDate()
		{
			Target(
				(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository) =>
					new DetailedAdherenceForDayQuery(loggedOnUser, currentDataSource, currentBusinessUnit, globalSettingDataRepository))
				.Execute(Today.Date).First()
				.DisplayName.Should().Be.Null();
		}
	}
}