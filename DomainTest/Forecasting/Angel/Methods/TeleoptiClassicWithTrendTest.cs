using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Angel.Trend;
using Teleopti.Ccc.DomainTest.Forecasting.Angel.Outlier;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Methods
{
	[TestFixture]
	public class TeleoptiClassicWithTrendTest
	{
		private TeleoptiClassicWithTrend target;
		private TaskOwnerPeriod historicalData;
		private double _averageTasks;
		private LinearTrend linearTrend;
		private ILinearTrendCalculator _linearTrendCalculator;

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
			var volumes = IndexVolumesFactory.CreateDayWeekMonthIndexVolumes();
			indexVolumes.Stub(x => x.Create(historicalData)).Return(volumes);

			_averageTasks = historicalData.TotalStatisticCalculatedTasks / historicalData.TaskOwnerDayCollection.Count;

			_linearTrendCalculator = MockRepository.GenerateMock<ILinearTrendCalculator>();
			
			target = new FakeTeleoptiClassicWithTrend(indexVolumes, _linearTrendCalculator);
		}

		[Test]
		public void ShouldForecastTasksWithTrend()
		{

			linearTrend = new LinearTrend
			{
				Slope = 1,
				Intercept = 2
			};
			_linearTrendCalculator.Stub(x => x.CalculateTrend(historicalData)).Return(linearTrend);

			const double indexMonth = 1d;
			const double indexWeek = 1.1d;
			const double indexDay = 1.2d;

			const double totalIndex = indexMonth * indexWeek * indexDay;
			var tasks = totalIndex * _averageTasks;

			var result = target.Forecast(historicalData, new DateOnlyPeriod(new DateOnly(2014, 1, 1), new DateOnly(2014, 1, 1)));
			var expected = tasks + linearTrend.Slope*new DateOnly(2014, 1, 1).Subtract(LinearTrend.StartDate).Days +
						   linearTrend.Intercept - tasks;
			result.Single().Tasks.Should().Be.EqualTo(Math.Round(expected, 4));
		}

		[Test]
		public void ShouldGetZeroIfForecastTasksIsNegative()
		{
			linearTrend = new LinearTrend
			{
				Slope = -1,
				Intercept = 2
			};
			_linearTrendCalculator.Stub(x => x.CalculateTrend(historicalData)).Return(linearTrend);

			const double indexMonth = 1d;
			const double indexWeek = 1.1d;
			const double indexDay = 1.2d;

			const double totalIndex = indexMonth * indexWeek * indexDay;
			var tasks = totalIndex * _averageTasks;

			var result = target.Forecast(historicalData, new DateOnlyPeriod(new DateOnly(2014, 1, 1), new DateOnly(2014, 1, 1)));
			var expected = tasks + linearTrend.Slope * new DateOnly(2014, 1, 1).Subtract(LinearTrend.StartDate).Days +
						   linearTrend.Intercept - tasks;
			expected = Math.Max(0, expected);
			result.Single().Tasks.Should().Be.EqualTo(Math.Round(expected, 4));
		}
	}
}