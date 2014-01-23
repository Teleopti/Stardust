using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.WebReports.DailyMetricsForDay
{
	[TestFixture, Explicit("Not yet done")]
	public class AdherenceTest : WebReportTest
	{
		private const int scheduledReadyTimeOneMinutes = 1;
		private const int scheduledReadyTimeTwoMinutes = 1;
		private const int deviationScheduleReadyOneSeconds = 120;
		private const int deviationScheduleReadyTwoSeconds = 120;


		protected override void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory)
		{
			analyticsDataFactory.Setup(new FactSchedule(PersonId, Today.DateId, scheduledReadyTimeOneMinutes, 1, ScenarioId));
			analyticsDataFactory.Setup(new FactSchedule(PersonId, Today.DateId, scheduledReadyTimeTwoMinutes, 2, ScenarioId));
			analyticsDataFactory.Setup(new FactScheduleDeviation(Today.DateId, 1, PersonId, deviationScheduleReadyOneSeconds, true));
			analyticsDataFactory.Setup(new FactScheduleDeviation(Today.DateId, 2, PersonId, deviationScheduleReadyTwoSeconds, true));
		}

		[Test]
		public void ShouldReturnAdherenceForAdherenceType1()
		{
			const int expectedPercentage = 25;
			Target().Execute(new DateOnlyPeriod(2000, 1, 1, 2020, 1, 1), 1, 1, SetupFixtureForAssembly.loggedOnPerson)
				.Adherence.Should().Be.EqualTo(expectedPercentage);
		}

		[Test]
		public void ShouldReturnAdherenceForAdherenceType2()
		{
			const int expectedPercentage = 31;
			Target().Execute(new DateOnlyPeriod(2000, 1, 1, 2020, 1, 1), 1, 1, SetupFixtureForAssembly.loggedOnPerson)
				.Adherence.Should().Be.EqualTo(expectedPercentage);
		}

		[Test]
		public void ShouldReturnAdherenceForAdherenceType3()
		{
			const int expectedPercentage = 33;
			Target().Execute(new DateOnlyPeriod(2000, 1, 1, 2020, 1, 1), 1, 1, SetupFixtureForAssembly.loggedOnPerson)
				.Adherence.Should().Be.EqualTo(expectedPercentage);
		}
	}
}