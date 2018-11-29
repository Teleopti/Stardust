using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Historical
{
	public class HistoricalDataTest
	{
		[Test]
		public void ShouldLoadHistoricalDataFromStatistics()
		{
			var dailyStatisticsAggregator = MockRepository.GenerateMock<IDailyStatisticsProvider>();
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkill("skill1"));
			var startDate = new DateOnly(2015,1,1);
			var period = new DateOnlyPeriod(startDate, new DateOnly(2015,2,1));
			var dailyStatistics = new[] {new DailyStatistic(startDate, 100, 1.1, 2.2)};
			dailyStatisticsAggregator.Stub(x => x.LoadDailyStatistics(workload, period)).Return(dailyStatistics);
			var target = new HistoricalData(dailyStatisticsAggregator);
			var result = target.Fetch(workload, period);

			result.TaskOwnerDayCollection.Single().TotalStatisticCalculatedTasks.Should().Be.EqualTo(100);
			result.TaskOwnerDayCollection.Single().TotalStatisticAverageTaskTime.Should().Be.EqualTo(TimeSpan.FromSeconds(1.1));
			result.TaskOwnerDayCollection.Single().TotalStatisticAverageAfterTaskTime.Should().Be.EqualTo(TimeSpan.FromSeconds(2.2));
		}
	}
}