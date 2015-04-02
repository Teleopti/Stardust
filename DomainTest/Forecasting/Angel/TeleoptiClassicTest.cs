using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	[TestFixture]
	public class TeleoptiClassicTest
	{
        private TeleoptiClassic target;
        private TaskOwnerPeriod historicalData;
		private double _averageTasks;

        [SetUp]
        public void Setup()
        {
			var skill = SkillFactory.CreateSkill("testSkill");
			skill.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
			var workload = WorkloadFactory.CreateWorkload(skill);

			var historicalDate = new DateOnly(2006, 1, 1);
			var periodForHelper = SkillDayFactory.GenerateMockedStatistics(historicalDate, workload);
			historicalData = new TaskOwnerPeriod(historicalDate, periodForHelper.TaskOwnerDays, TaskOwnerPeriodType.Other);

	        var indexVolumes = MockRepository.GenerateMock<IIndexVolumes>();
			var volumes = IndexVolumesFactory.Create();
			indexVolumes.Stub(x => x.Create(historicalData)).Return(volumes);

			_averageTasks = historicalData.TotalStatisticCalculatedTasks / historicalData.TaskOwnerDayCollection.Count;

			target = new TeleoptiClassic(indexVolumes);
		}

		[Test]
		public void ShouldForecastTasksUsingIndexesCorrectly()
		{
			const double indexMonth = 1d;
			const double indexWeek = 1.1d;
			const double indexDay = 1.2d;

			const double totalIndex = indexMonth * indexWeek * indexDay;
			var tasks = totalIndex * _averageTasks;

			var result = target.Forecast(historicalData, new DateOnlyPeriod(new DateOnly(2014, 1, 1), new DateOnly(2014, 1, 1)));
			result.Single().Tasks.Should().Be.EqualTo(Math.Round(tasks, 4));
		}
	}
}