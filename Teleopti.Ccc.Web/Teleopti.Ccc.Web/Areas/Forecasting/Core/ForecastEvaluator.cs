using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Angel.Outlier;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	public interface IForecastResultViewModelFactory
	{
		WorkloadForecastResultViewModel Create(Guid workloadId, DateOnlyPeriod dateOnlyPeriod);
	}

	public class ForecastResultViewModelFactory : IForecastResultViewModelFactory
	{
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly IFutureData _futureData;
		private readonly IWorkloadRepository _workloadRepository;

		public ForecastResultViewModelFactory(IWorkloadRepository workloadRepository, ISkillDayRepository skillDayRepository, ICurrentScenario currentScenario, IFutureData futureData)
		{
			_workloadRepository = workloadRepository;
			_skillDayRepository = skillDayRepository;
			_currentScenario = currentScenario;
			_futureData = futureData;
		}

		public WorkloadForecastResultViewModel Create(Guid workloadId, DateOnlyPeriod futurePeriod)
		{
			var workload = _workloadRepository.Get(workloadId);
			var currentScenario = _currentScenario.Current();
			var skillDays = _skillDayRepository.FindRange(futurePeriod, workload.Skill, currentScenario);
			var futureWorkloadDays = _futureData.Fetch(workload, skillDays, futurePeriod);
			return new WorkloadForecastResultViewModel
			{
				WorkloadId = workloadId,
				Days = futureWorkloadDays.Select(x => new { date = x.CurrentDate.Date, vc = x.Tasks, vaht = x.AverageTaskTime, vacw = x.AverageAfterTaskTime }).ToArray<dynamic>()
			};
		}
	}

	public class ForecastEvaluator : IForecastEvaluator
	{
		private readonly IForecastWorkloadEvaluator _forecastWorkloadEvaluator;
		private readonly IWorkloadRepository _workloadRepository;
		private readonly IHistoricalPeriodProvider _historicalPeriodProvider;
		private readonly IHistoricalData _historicalData;
		private readonly IOutlierRemover _outlierRemover;
		private readonly IForecastMethodProvider _forecastMethodProvider;

		public ForecastEvaluator(IForecastWorkloadEvaluator forecastWorkloadEvaluator, IWorkloadRepository workloadRepository, IHistoricalPeriodProvider historicalPeriodProvider, IHistoricalData historicalData, IOutlierRemover outlierRemover, IForecastMethodProvider forecastMethodProvider)
		{
			_forecastWorkloadEvaluator = forecastWorkloadEvaluator;
			_workloadRepository = workloadRepository;
			_historicalPeriodProvider = historicalPeriodProvider;
			_historicalData = historicalData;
			_outlierRemover = outlierRemover;
			_forecastMethodProvider = forecastMethodProvider;
		}

		public WorkloadForecastViewModel Evaluate(EvaluateInput input)
		{
			var workload = _workloadRepository.Get(input.WorkloadId);
			var evaluateResult = _forecastWorkloadEvaluator.Evaluate(workload);
			var bestAccuracy = evaluateResult.Accuracies.SingleOrDefault(x => x.IsSelected);

			var availablePeriod = _historicalPeriodProvider.AvailablePeriod(workload);
			var evaluationDayViewModels = availablePeriod.HasValue
				? createEvaluationDayViewModels(workload, bestAccuracy,
					HistoricalPeriodProvider.DivideIntoTwoPeriods(availablePeriod.Value).Item2)
				: new dynamic[] {};
			return new WorkloadForecastViewModel
			{
				Name = workload.Name,
				WorkloadId = workload.Id.Value,
				ForecastMethods = createMethodViewModels(evaluateResult),
				Days = evaluationDayViewModels,
				ForecastMethodRecommended = bestAccuracy == null
					? (dynamic) new {Id = ForecastMethodType.None}
					: new
					{
						Id = bestAccuracy.MethodId,
						PeriodEvaluateOnStart = bestAccuracy.PeriodEvaluateOn.StartDate.Date,
						PeriodEvaluateOnEnd = bestAccuracy.PeriodEvaluateOn.EndDate.Date,
						PeriodUsedToEvaluateStart = bestAccuracy.PeriodUsedToEvaluate.StartDate.Date,
						PeriodUsedToEvaluateEnd = bestAccuracy.PeriodUsedToEvaluate.EndDate.Date
					}
			};
		}

		public WorkloadQueueStatisticsViewModel QueueStatistics(QueueStatisticsInput input)
		{
			var workload = _workloadRepository.Get(input.WorkloadId);
			var availablePeriod = _historicalPeriodProvider.AvailablePeriod(workload);
			return new WorkloadQueueStatisticsViewModel
			{
				WorkloadId = workload.Id.Value,
				QueueStatisticsDays = createQueueStatisticsDayViewModels(workload, input.ForecastMethodType, availablePeriod)
			};
		}

		private dynamic[] createMethodViewModels(WorkloadAccuracy evaluateResult)
		{
			var methods = new List<dynamic>();
			foreach (var accuracy in evaluateResult.Accuracies)
			{
				methods.Add(new
				{
					AccuracyNumber = accuracy.Number,
					ForecastMethodType = accuracy.MethodId
				});
			}
			return methods.ToArray();
		}

		private dynamic[] createEvaluationDayViewModels(IWorkload workload, MethodAccuracy bestAccuracy, DateOnlyPeriod? period)
		{
			if(!period.HasValue)
				return new dynamic[]{};
			var historicalData = _historicalData.Fetch(workload, period.Value);
			var data = new Dictionary<DateOnly, dynamic>();
			foreach (var taskOwner in historicalData.TaskOwnerDayCollection)
			{
				dynamic item = new ExpandoObject();
				item.date = taskOwner.CurrentDate.Date;
				item.vh = taskOwner.TotalStatisticCalculatedTasks;
				data.Add(taskOwner.CurrentDate, item);
			}
			if (bestAccuracy != null)
			{
				foreach (var dayResult in bestAccuracy.MeasureResult)
				{
					if (data.ContainsKey(dayResult.CurrentDate))
					{
						data[dayResult.CurrentDate].vb = Math.Round(dayResult.Tasks, 1);
					}
					else
					{
						dynamic item = new ExpandoObject();
						item.date = dayResult.CurrentDate.Date;
						item.vb = Math.Round(dayResult.Tasks, 1);
						data.Add(dayResult.CurrentDate, item);
					}
				}
			}
			return data.Values.ToArray();
		}

		private dynamic[] createQueueStatisticsDayViewModels(IWorkload workload, ForecastMethodType method, DateOnlyPeriod? period)
		{
			if(!period.HasValue)
				return new dynamic[]{};
			var historicalData = _historicalData.Fetch(workload, period.Value);
			var forecastMethod = _forecastMethodProvider.Get(method);
			var data = new Dictionary<DateOnly, dynamic>();
			foreach (var taskOwner in historicalData.TaskOwnerDayCollection)
			{
				dynamic item = new ExpandoObject();
				item.date = taskOwner.CurrentDate.Date;
				item.vh = taskOwner.TotalStatisticCalculatedTasks;
				data.Add(taskOwner.CurrentDate, item);
			}

			var historicalDataNoOutliers = _outlierRemover.RemoveOutliers(historicalData, forecastMethod);
			foreach (var day in historicalDataNoOutliers.TaskOwnerDayCollection)
			{
				data[day.CurrentDate].vh2 = Math.Round(day.TotalStatisticCalculatedTasks, 1);
			}
			return data.Values.ToArray();
		}
	}
}