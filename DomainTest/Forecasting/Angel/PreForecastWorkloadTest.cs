using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public class PreForecastWorkloadTest
	{
		[Test]
		public void ShouldReturnEmptyIfNoHistoricalData()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources();
			var workload = skill.WorkloadCollection.Single();
			var historicalPeriodProvider = new HistoricalPeriodProvider(new MutableNow(new DateTime(2014, 12, 31, 0, 0, 0, DateTimeKind.Utc)));
			var taskOwnerPeriod = new TaskOwnerPeriod(DateOnly.MinValue, new List<WorkloadDay>(), TaskOwnerPeriodType.Other);

			var dateOnly = new DateOnly(2015, 1, 1);
			var futurePeriod = new DateOnlyPeriod(dateOnly, dateOnly);
			var target = new PreForecastWorkload(null);
			var result = target.PreForecast(workload, futurePeriod, taskOwnerPeriod);

			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldPreForecastForWorkload()
		{
			var date1 = new DateOnly(2014, 12, 29);
			var date2 = new DateOnly(2014, 12, 30);
			var workloadDay1 = new WorkloadDay();
			workloadDay1.Create(date1, new Workload(SkillFactory.CreateSkill("Phone")), new List<TimePeriod>());
			workloadDay1.MakeOpen24Hours();
			workloadDay1.TotalStatisticCalculatedTasks = 8d;
			var workloadDay2 = new WorkloadDay();
			workloadDay2.Create(date2, new Workload(SkillFactory.CreateSkill("Phone")), new List<TimePeriod>());
			workloadDay2.MakeOpen24Hours();
			workloadDay2.TotalStatisticCalculatedTasks = 12d;

			var skill = SkillFactory.CreateSkillWithWorkloadAndSources();
			var workload = skill.WorkloadCollection.Single();
			var historicalPeriodProvider = new HistoricalPeriodProvider(new MutableNow(new DateTime(2014, 12, 31, 0, 0, 0, DateTimeKind.Utc)));
			var taskOwnerPeriod = new TaskOwnerPeriod(DateOnly.MinValue, new List<WorkloadDay>
			{
				workloadDay1,
				workloadDay2
			}, TaskOwnerPeriodType.Other);
			var dateOnly = new DateOnly(2015, 1, 1);
			var futurePeriod = new DateOnlyPeriod(dateOnly, dateOnly);
			var forecastMethodProvider = MockRepository.GenerateMock<IForecastMethodProvider>();
			const double tasks1 = 11.2;
			const double tasks2 = 11.3;

			var forecastMethod1 = StubForecastMethod(dateOnly, tasks1, taskOwnerPeriod, futurePeriod, ForecastMethodType.TeleoptiClassic);
			var forecastMethod2 = StubForecastMethod(dateOnly, tasks2, taskOwnerPeriod, futurePeriod, ForecastMethodType.TeleoptiClassicWithTrend);

			forecastMethodProvider.Stub(x => x.All()).Return(new[] {forecastMethod1, forecastMethod2});

			var target = new PreForecastWorkload(forecastMethodProvider);

			var result = target.PreForecast(workload, futurePeriod, taskOwnerPeriod);

			result.Count.Should().Be.EqualTo(1);
			result[dateOnly].Count.Should().Be.EqualTo(2);
			result[dateOnly][ForecastMethodType.TeleoptiClassic].Should().Be.EqualTo(tasks1);
			result[dateOnly][ForecastMethodType.TeleoptiClassicWithTrend].Should().Be.EqualTo(tasks2);
		}

		private IForecastMethod StubForecastMethod(DateOnly dateOnly, double numberOfTasks, TaskOwnerPeriod taskOwnerPeriod, DateOnlyPeriod futurePeriod, ForecastMethodType type)
		{
			var forecastMethod = MockRepository.GenerateMock<IForecastMethod>();
			var forecastingTarget = MockRepository.GenerateMock<IForecastingTarget>();
			forecastMethod.Stub(x => x.Id).Return(type);
			forecastingTarget.Stub(x => x.CurrentDate).Return(dateOnly);
			forecastingTarget.Stub(x => x.Tasks).Return(numberOfTasks);
			forecastMethod.Stub(x => x.Forecast(taskOwnerPeriod, futurePeriod)).Return(new[] {forecastingTarget});
			return forecastMethod;
		}
	}
}