using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Forecasting.Angel;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Ccc.Web.Areas.Forecasting.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Core
{
	public class ForecastViewModelFactoryTest
	{
		[Test]
		public void ShouldGetQueueStatisticsAndQueueStatisticsNoOutliersForQueueStatisticsViewModel()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources();
			var workloadRepository = MockRepository.GenerateMock<IWorkloadRepository>();
			var workload = skill.WorkloadCollection.Single();

			var date1 = new DateOnly(2014, 12, 19);
			var date2 = new DateOnly(2014, 12, 20);
			var workloadDay1 = new WorkloadDay();
			workloadDay1.Create(date1, new Workload(SkillFactory.CreateSkill("Phone")), new List<TimePeriod>());
			workloadDay1.MakeOpen24Hours();
			workloadDay1.TotalStatisticCalculatedTasks = 8d;
			var workloadDay2 = new WorkloadDay();
			workloadDay2.Create(date2, new Workload(SkillFactory.CreateSkill("Phone")), new List<TimePeriod>());
			workloadDay2.MakeOpen24Hours();
			workloadDay2.TotalStatisticCalculatedTasks = 12d;

			var availablePeriod = new DateOnlyPeriod(2012, 12, 20, 2014, 12, 20);
			var historicalPeriodProvider = MockRepository.GenerateMock<IHistoricalPeriodProvider>();
			historicalPeriodProvider.Stub(x => x.AvailablePeriod(workload)).Return(availablePeriod);
			var historicalData = MockRepository.GenerateMock<IHistoricalData>();
			var taskOwnerPeriod = new TaskOwnerPeriod(DateOnly.MinValue, new List<WorkloadDay> { workloadDay1, workloadDay2 }, TaskOwnerPeriodType.Other);
			historicalData.Stub(x => x.Fetch(workload, availablePeriod)).Return(taskOwnerPeriod);
			
			var queueStatisticsInput = new QueueStatisticsInput
			{
				WorkloadId = workload.Id.Value,
				MethodId = ForecastMethodType.TeleoptiClassicLongTerm
			};
			workloadRepository.Stub(x => x.Get(queueStatisticsInput.WorkloadId)).Return(workload);
			var target = new ForecastViewModelFactory(null, workloadRepository, historicalPeriodProvider, historicalData, new OutlierRemover(), new ForecastMethodProvider(new LinearRegressionTrendCalculator()), null);
			
			var result = target.QueueStatistics(queueStatisticsInput);

			result.WorkloadId.Should().Be.EqualTo(workload.Id.Value);
			dynamic firstDay = result.QueueStatisticsDays.First();
			((object)firstDay.date).Should().Be.EqualTo(date1.Date);
			((object)firstDay.vh).Should().Be.EqualTo(8d);
			((object)firstDay.vh2).Should().Be.EqualTo(8d);
		}

		[Test]
		public void ShouldGetCorrectPeriodForEvaluationViewModel()
		{
			var historicalPeriodProvider = MockRepository.GenerateMock<IHistoricalPeriodProvider>();
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources();
			var workloadRepository = MockRepository.GenerateMock<IWorkloadRepository>();
			var workload = skill.WorkloadCollection.Single();
			var evaluateInput = new EvaluateInput
			{
				WorkloadId = workload.Id.Value
			};
			workloadRepository.Stub(x => x.Get(evaluateInput.WorkloadId)).Return(workload);
			var historicalData = MockRepository.GenerateMock<IHistoricalData>();
			var forecastWorkloadEvaluator = MockRepository.GenerateMock<IForecastWorkloadEvaluator>();
			forecastWorkloadEvaluator.Stub(x => x.Evaluate(workload)).Return(new WorkloadAccuracy() { Accuracies = new MethodAccuracy[] { } });
			var target = new ForecastViewModelFactory(forecastWorkloadEvaluator, workloadRepository, historicalPeriodProvider, historicalData, null, null, MockRepository.GenerateMock<IForecastMisc>());
			var availablePeriod = new DateOnlyPeriod(2012, 3, 16, 2015, 3, 15);
			historicalPeriodProvider.Stub(x => x.AvailablePeriod(workload)).Return(availablePeriod);
			historicalData.Stub(x => x.Fetch(Arg<IWorkload>.Is.Equal(workload), Arg<DateOnlyPeriod>.Is.Anything)).Return(new TaskOwnerPeriod(DateOnly.Today, new ITaskOwner[] { }, TaskOwnerPeriodType.Other));

			target.Evaluate(evaluateInput);

			historicalData.AssertWasCalled(x => x.Fetch(workload, new DateOnlyPeriod(2014, 3, 16, 2015, 3, 15)));
		}

		[Test]
		public void ShouldGetHistoricalDataForEvaluationViewModel()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources();
			var workloadRepository = MockRepository.GenerateMock<IWorkloadRepository>();
			var workload = skill.WorkloadCollection.Single();
			var evaluateInput = new EvaluateInput
			{
				WorkloadId = workload.Id.Value
			};
			workloadRepository.Stub(x => x.Get(evaluateInput.WorkloadId)).Return(workload);

			var date1 = new DateOnly(2014, 12, 9);
			var date2 = new DateOnly(2014, 12, 10);
			var date3 = new DateOnly(2014, 12, 11);
			var workloadDay1 = new WorkloadDay();
			workloadDay1.Create(date1, new Workload(SkillFactory.CreateSkill("Phone")), new List<TimePeriod>());
			workloadDay1.MakeOpen24Hours();
			workloadDay1.TotalStatisticCalculatedTasks = 8d;
			var workloadDay2 = new WorkloadDay();
			workloadDay2.Create(date2, new Workload(SkillFactory.CreateSkill("Phone")), new List<TimePeriod>());
			workloadDay2.MakeOpen24Hours();
			workloadDay2.TotalStatisticCalculatedTasks = 12d;

			var availablePeriod = new DateOnlyPeriod(2012, 12, 20, 2014, 12, 20);
			var historicalPeriodProvider = MockRepository.GenerateMock<IHistoricalPeriodProvider>();
			historicalPeriodProvider.Stub(x => x.AvailablePeriod(workload)).Return(availablePeriod);
			var historicalData = MockRepository.GenerateMock<IHistoricalData>();
			var taskOwnerPeriod = new TaskOwnerPeriod(DateOnly.MinValue, new List<WorkloadDay>{workloadDay1, workloadDay2}, TaskOwnerPeriodType.Other);
			historicalData.Stub(x => x.Fetch(workload, new DateOnlyPeriod(2013, 12, 21, 2014, 12, 20))).Return(taskOwnerPeriod);

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
			var target = new ForecastViewModelFactory(forecastWorkloadEvaluator, workloadRepository, historicalPeriodProvider, historicalData, null, null, MockRepository.GenerateMock<IForecastMisc>());

			var result = target.Evaluate(evaluateInput);

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
		public void ShouldGetEvaluationResultForEvaluationViewModel()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources();
			var workloadRepository = MockRepository.GenerateMock<IWorkloadRepository>();
			var workload = skill.WorkloadCollection.Single();
			var evaluateInput = new EvaluateInput
			{
				WorkloadId = workload.Id.Value
			};
			workloadRepository.Stub(x => x.Get(evaluateInput.WorkloadId)).Return(workload);
			var forecastWorkloadEvaluator = MockRepository.GenerateMock<IForecastWorkloadEvaluator>();
			var availablePeriod = new DateOnlyPeriod(2012, 12, 20, 2014, 12, 20);
			var historicalPeriodProvider = MockRepository.GenerateMock<IHistoricalPeriodProvider>();
			historicalPeriodProvider.Stub(x => x.AvailablePeriod(workload)).Return(availablePeriod);
			forecastWorkloadEvaluator.Stub(x => x.Evaluate(workload))
				.Return(new WorkloadAccuracy
				{
					Accuracies =
						new[]
						{
							new MethodAccuracy {Number = 89, MethodId = ForecastMethodType.TeleoptiClassicLongTerm, IsSelected = false},
							new MethodAccuracy {Number = 92, MethodId = ForecastMethodType.TeleoptiClassicLongTermWithTrend, IsSelected = true, MeasureResult = new IForecastingTarget[]{}}
						}
				});
			var dictionary = new Dictionary<DateOnly, IDictionary<ForecastMethodType, double>>();
			var oneDay = new Dictionary<ForecastMethodType, double> { { ForecastMethodType.TeleoptiClassicLongTerm, 80d }, { ForecastMethodType.TeleoptiClassicLongTermWithTrend, 90d } };
			var dateOnly = new DateOnly(2015,1,1);
			dictionary.Add(dateOnly, oneDay);
			var historicalData = MockRepository.GenerateMock<IHistoricalData>();
			var taskOwnerPeriod = new TaskOwnerPeriod(DateOnly.MinValue, new List<WorkloadDay>(), TaskOwnerPeriodType.Other);
			historicalData.Stub(x => x.Fetch(workload, HistoricalPeriodProvider.DivideIntoTwoPeriods(availablePeriod).Item2)).Return(taskOwnerPeriod);
			var forecastMisc = MockRepository.GenerateMock<IForecastMisc>();
			var workloadName = workload.Skill.Name + " - " + workload.Name;
			forecastMisc.Stub(x => x.WorkloadName(workload.Skill.Name, workload.Name)).Return(workloadName);
			var target = new ForecastViewModelFactory(forecastWorkloadEvaluator, workloadRepository, historicalPeriodProvider, historicalData, null, null, forecastMisc);

			var result = target.Evaluate(evaluateInput);

			result.WorkloadId.Should().Be.EqualTo(workload.Id.Value);
			result.Name.Should().Be.EqualTo(workloadName);
			((ForecastMethodType)result.ForecastMethodRecommended.Id).Should().Be.EqualTo(ForecastMethodType.TeleoptiClassicLongTermWithTrend);
			result.ForecastMethods.Any(x => (int)x.AccuracyNumber == 89 && x.ForecastMethodType == ForecastMethodType.TeleoptiClassicLongTerm).Should().Be.True();
			result.ForecastMethods.Any(x => (int)x.AccuracyNumber == 92 && x.ForecastMethodType == ForecastMethodType.TeleoptiClassicLongTermWithTrend).Should().Be.True();
			result.Days.Any().Should().Be.False();
		}
	}
}