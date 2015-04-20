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
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Ccc.Web.Areas.Forecasting.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Core
{
	public class PreForecasterTest
	{
		[Test]
		public void ShouldGetHistoricalData()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources();
			var workloadRepository = MockRepository.GenerateMock<IWorkloadRepository>();
			var workload = skill.WorkloadCollection.Single();
			var preForecastInput = new PreForecastInput
			{
				WorkloadId = workload.Id.Value,
				ForecastStart = new DateTime(2015, 1, 1),
				ForecastEnd = new DateTime(2015, 1, 31)
			};
			workloadRepository.Stub(x => x.Get(preForecastInput.WorkloadId)).Return(workload);

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

			var historicalPeriodProvider = new HistoricalPeriodProvider(new MutableNow(new DateTime(2014, 12, 20, 0, 0, 0, DateTimeKind.Utc)));
			var historicalData = MockRepository.GenerateMock<IHistoricalData>();
			var taskOwnerPeriod = new TaskOwnerPeriod(DateOnly.MinValue, new List<WorkloadDay>{workloadDay1, workloadDay2}, TaskOwnerPeriodType.Other);
			historicalData.Stub(x => x.Fetch(workload, historicalPeriodProvider.PeriodForForecast())).Return(taskOwnerPeriod);
			var preForecastWorkload = MockRepository.GenerateMock<IPreForecastWorkload>();
			preForecastWorkload.Stub(
				x =>
					x.PreForecast(workload,
						new DateOnlyPeriod(new DateOnly(preForecastInput.ForecastStart), new DateOnly(preForecastInput.ForecastEnd)),
						taskOwnerPeriod)).Return(new Dictionary<DateOnly, IDictionary<ForecastMethodType, double>>());
			var quickForecastWorkloadEvaluator = MockRepository.GenerateMock<IQuickForecastWorkloadEvaluator>();
			quickForecastWorkloadEvaluator.Stub(x=>x.Measure(workload, historicalPeriodProvider.PeriodForEvaluate())).Return(new WorkloadAccuracy{Accuracies = new[]{new MethodAccuracy{IsSelected = true}}});
			var target = new PreForecaster(preForecastWorkload, quickForecastWorkloadEvaluator, workloadRepository, historicalPeriodProvider, historicalData);

			var result = target.MeasureAndForecast(preForecastInput);

			dynamic firstDay = result.ForecastDays.First();
			((object)firstDay.date).Should().Be.EqualTo(date1.Date);
			((object)firstDay.vh).Should().Be.EqualTo(8d);
			dynamic secondDay = result.ForecastDays.Last();
			((object)secondDay.date).Should().Be.EqualTo(date2.Date);
			((object)secondDay.vh).Should().Be.EqualTo(12d);
		}

		[Test]
		public void ShouldMeasureAndForecast()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources();
			var workloadRepository = MockRepository.GenerateMock<IWorkloadRepository>();
			var workload = skill.WorkloadCollection.Single();
			var preForecastInput = new PreForecastInput
			{
				WorkloadId = workload.Id.Value,
				ForecastStart = new DateTime(2015, 1, 1),
				ForecastEnd = new DateTime(2015, 1, 31)
			};
			workloadRepository.Stub(x => x.Get(preForecastInput.WorkloadId)).Return(workload);
			var quickForecastWorkloadEvaluator = MockRepository.GenerateMock<IQuickForecastWorkloadEvaluator>();
			var historicalPeriodProvider = new HistoricalPeriodProvider(new MutableNow(new DateTime(2014, 12, 20, 0, 0, 0, DateTimeKind.Utc)));
			quickForecastWorkloadEvaluator.Stub(x => x.Measure(workload, historicalPeriodProvider.PeriodForEvaluate()))
				.Return(new WorkloadAccuracy
				{
					Accuracies =
						new[]
						{
							new MethodAccuracy {Number = 89, MethodId = ForecastMethodType.TeleoptiClassic, IsSelected = false},
							new MethodAccuracy {Number = 92, MethodId = ForecastMethodType.TeleoptiClassicWithTrend, IsSelected = true}
						}
				});
			var preForecastWorkload = MockRepository.GenerateMock<IPreForecastWorkload>();
			var dictionary = new Dictionary<DateOnly, IDictionary<ForecastMethodType, double>>();
			var oneDay = new Dictionary<ForecastMethodType, double> {{ForecastMethodType.TeleoptiClassic, 80d},{ForecastMethodType.TeleoptiClassicWithTrend, 90d}};
			var dateOnly = new DateOnly(2015,1,1);
			dictionary.Add(dateOnly, oneDay);
			var historicalData = MockRepository.GenerateMock<IHistoricalData>();
			var taskOwnerPeriod = new TaskOwnerPeriod(DateOnly.MinValue, new List<WorkloadDay>(), TaskOwnerPeriodType.Other);
			historicalData.Stub(x => x.Fetch(workload, historicalPeriodProvider.PeriodForForecast())).Return(taskOwnerPeriod);
			preForecastWorkload.Stub(x => x.PreForecast(workload, new DateOnlyPeriod(new DateOnly(preForecastInput.ForecastStart), new DateOnly(preForecastInput.ForecastEnd)), taskOwnerPeriod)).Return(dictionary);
			var target = new PreForecaster(preForecastWorkload, quickForecastWorkloadEvaluator, workloadRepository, historicalPeriodProvider, historicalData);

			
			var result = target.MeasureAndForecast(preForecastInput);

			result.WorkloadId.Should().Be.EqualTo(workload.Id.Value);
			result.Name.Should().Be.EqualTo(workload.Name);
			result.ForecastMethodRecommended.Should().Be.EqualTo(ForecastMethodType.TeleoptiClassicWithTrend);
			result.ForecastMethods.Any(x=>(int)x.AccuracyNumber==89&&x.ForecastMethodType==ForecastMethodType.TeleoptiClassic).Should().Be.True();
			result.ForecastMethods.Any(x => (int)x.AccuracyNumber == 92 && x.ForecastMethodType == ForecastMethodType.TeleoptiClassicWithTrend).Should().Be.True();
			dynamic forecastDayViewModel = result.ForecastDays[0];
			((object)forecastDayViewModel.date).Should().Be.EqualTo(dateOnly.Date);
			((object)forecastDayViewModel.v0).Should().Be.EqualTo(80d);
			((object)forecastDayViewModel.v1).Should().Be.EqualTo(90d);
		}
	}
}