using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Ccc.Web.Areas.Forecasting.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Core
{
	public class ForecastEvaluatorTest
	{
		[Test]
		public void ShouldGetHistoricalData()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources();
			var workloadRepository = MockRepository.GenerateMock<IWorkloadRepository>();
			var workload = skill.WorkloadCollection.Single();
			var preForecastInput = new EvaluateInput
			{
				WorkloadId = workload.Id.Value
			};
			workloadRepository.Stub(x => x.Get(preForecastInput.WorkloadId)).Return(workload);

			var date1 = new DateOnly(2014, 12, 29);
			var date2 = new DateOnly(2014, 12, 30);
			var date3 = new DateOnly(2014, 12, 31);
			var workloadDay1 = new WorkloadDay();
			workloadDay1.Create(date1, new Workload(SkillFactory.CreateSkill("Phone")), new List<TimePeriod>());
			workloadDay1.MakeOpen24Hours();
			workloadDay1.TotalStatisticCalculatedTasks = 8d;
			var workloadDay2 = new WorkloadDay();
			workloadDay2.Create(date2, new Workload(SkillFactory.CreateSkill("Phone")), new List<TimePeriod>());
			workloadDay2.MakeOpen24Hours();
			workloadDay2.TotalStatisticCalculatedTasks = 12d;

			var statisticRepository = MockRepository.GenerateMock<IStatisticRepository>();
			statisticRepository.Stub(x => x.QueueStatisticsUpUntilDate(workload.QueueSourceCollection)).Return(new DateOnlyPeriod(2012, 12, 20, 2014, 12, 20));
			var historicalPeriodProvider = new HistoricalPeriodProvider(new MutableNow(new DateTime(2014, 12, 20, 0, 0, 0, DateTimeKind.Utc)), statisticRepository);
			var historicalData = MockRepository.GenerateMock<IHistoricalData>();
			var taskOwnerPeriod = new TaskOwnerPeriod(DateOnly.MinValue, new List<WorkloadDay>{workloadDay1, workloadDay2}, TaskOwnerPeriodType.Other);
			historicalData.Stub(x => x.Fetch(workload, historicalPeriodProvider.PeriodForDisplay(workload))).Return(taskOwnerPeriod);

			var forecastWorkloadEvaluator = MockRepository.GenerateMock<IForecastWorkloadEvaluator>();
			forecastWorkloadEvaluator.Stub(x => x.Evaluate(workload))
				.Return(new WorkloadAccuracy
				{
					Accuracies =
						new[]
						{
							new MethodAccuracy
							{
								IsSelected = false,
								MeasureResult =
									new IForecastingTarget[]
									{
										new ForecastingTarget(date1, new OpenForWork(true, true)){Tasks = 23.1},
										new ForecastingTarget(date2, new OpenForWork(true, true)){Tasks = 23.2}
									}
							},
							new MethodAccuracy
							{
								IsSelected = true,
								MeasureResult =
									new IForecastingTarget[]
									{
										new ForecastingTarget(date1, new OpenForWork(true, true)){Tasks = 34.1},
										new ForecastingTarget(date2, new OpenForWork(true, true)){Tasks = 34.2},
										new ForecastingTarget(date3, new OpenForWork(true, true)){Tasks = 34.3}
									}
							}
						}
				});
			var target = new ForecastEvaluator(forecastWorkloadEvaluator, workloadRepository, historicalPeriodProvider, historicalData);

			var result = target.Evaluate(preForecastInput);

			dynamic firstDay = result.Days.First();
			((object)firstDay.date).Should().Be.EqualTo(date1.Date);
			((object)firstDay.vh).Should().Be.EqualTo(8d);
			((object)firstDay.vb).Should().Be.EqualTo(34.1);
			dynamic secondDay = result.Days.Second();
			((object)secondDay.date).Should().Be.EqualTo(date2.Date);
			((object)secondDay.vh).Should().Be.EqualTo(12d);
			((object)secondDay.vb).Should().Be.EqualTo(34.2);
			dynamic thirdDay = result.Days.Last();
			((object)thirdDay.date).Should().Be.EqualTo(date3.Date);
			((IDictionary<String, object>)thirdDay).ContainsKey("vh").Should().Be.False();
			((object)thirdDay.vb).Should().Be.EqualTo(34.3);
		}

		[Test]
		public void ShouldMeasureAndForecast()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources();
			var workloadRepository = MockRepository.GenerateMock<IWorkloadRepository>();
			var workload = skill.WorkloadCollection.Single();
			var preForecastInput = new EvaluateInput
			{
				WorkloadId = workload.Id.Value
			};
			workloadRepository.Stub(x => x.Get(preForecastInput.WorkloadId)).Return(workload);
			var forecastWorkloadEvaluator = MockRepository.GenerateMock<IForecastWorkloadEvaluator>();
			var statisticRepository = MockRepository.GenerateMock<IStatisticRepository>();
			statisticRepository.Stub(x => x.QueueStatisticsUpUntilDate(workload.QueueSourceCollection)).Return(new DateOnlyPeriod(2012, 12, 20, 2014, 12, 20));
			var historicalPeriodProvider = new HistoricalPeriodProvider(new MutableNow(new DateTime(2014, 12, 20, 0, 0, 0, DateTimeKind.Utc)), statisticRepository);
			forecastWorkloadEvaluator.Stub(x => x.Evaluate(workload))
				.Return(new WorkloadAccuracy
				{
					Accuracies =
						new[]
						{
							new MethodAccuracy {Number = 89, MethodId = ForecastMethodType.TeleoptiClassic, IsSelected = false},
							new MethodAccuracy {Number = 92, MethodId = ForecastMethodType.TeleoptiClassicWithTrend, IsSelected = true, MeasureResult = new IForecastingTarget[]{}}
						}
				});
			var dictionary = new Dictionary<DateOnly, IDictionary<ForecastMethodType, double>>();
			var oneDay = new Dictionary<ForecastMethodType, double> {{ForecastMethodType.TeleoptiClassic, 80d},{ForecastMethodType.TeleoptiClassicWithTrend, 90d}};
			var dateOnly = new DateOnly(2015,1,1);
			dictionary.Add(dateOnly, oneDay);
			var historicalData = MockRepository.GenerateMock<IHistoricalData>();
			var taskOwnerPeriod = new TaskOwnerPeriod(DateOnly.MinValue, new List<WorkloadDay>(), TaskOwnerPeriodType.Other);
			historicalData.Stub(x => x.Fetch(workload, historicalPeriodProvider.PeriodForDisplay(workload))).Return(taskOwnerPeriod);
			var target = new ForecastEvaluator(forecastWorkloadEvaluator, workloadRepository, historicalPeriodProvider, historicalData);

			var result = target.Evaluate(preForecastInput);

			result.WorkloadId.Should().Be.EqualTo(workload.Id.Value);
			result.Name.Should().Be.EqualTo(workload.Name);
			result.ForecastMethodRecommended.Should().Be.EqualTo(ForecastMethodType.TeleoptiClassicWithTrend);
			result.ForecastMethods.Any(x=>(int)x.AccuracyNumber==89&&x.ForecastMethodType==ForecastMethodType.TeleoptiClassic).Should().Be.True();
			result.ForecastMethods.Any(x => (int)x.AccuracyNumber == 92 && x.ForecastMethodType == ForecastMethodType.TeleoptiClassicWithTrend).Should().Be.True();
			result.Days.Any().Should().Be.False();
		}
	}
}