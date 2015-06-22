using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Methods
{
	[TestFixture]
	public class TeleoptiClassicBaseTest
	{
		[Test]
		public void ShouldForecastTasksUsingIndexesCorrectly()
		{
			var skill = SkillFactory.CreateSkill("testSkill");
			skill.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
			var workload = WorkloadFactory.CreateWorkload(skill);

			var historicalDate = new DateOnly(2006, 1, 1);
			var periodForHelper = SkillDayFactory.GenerateMockedStatistics(historicalDate, workload);
			var historicalData = new TaskOwnerPeriod(historicalDate, periodForHelper.TaskOwnerDays, TaskOwnerPeriodType.Other);

			var indexVolumes = MockRepository.GenerateMock<IIndexVolumes>();
			var volumes = IndexVolumesFactory.CreateDayWeekMonthIndexVolumes();
			indexVolumes.Stub(x => x.Create(historicalData)).Return(volumes);

			var averageTasks = historicalData.TotalStatisticCalculatedTasks / historicalData.TaskOwnerDayCollection.Count;

			var target = new TeleoptiClassicBaseFake(indexVolumes);

			const double indexMonth = 1d;
			const double indexWeek = 1.1d;
			const double indexDay = 1.2d;

			const double totalIndex = indexMonth * indexWeek * indexDay;
			var tasks = totalIndex * averageTasks;

			var result = target.Forecast(historicalData, new DateOnlyPeriod(new DateOnly(2014, 1, 1), new DateOnly(2014, 1, 1)));
			result.ForecastingTargets.Single().Tasks.Should().Be.EqualTo(Math.Round(tasks, 4));
		}
	}
}