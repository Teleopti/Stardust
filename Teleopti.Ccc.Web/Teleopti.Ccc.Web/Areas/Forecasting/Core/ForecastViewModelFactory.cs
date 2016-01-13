using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Angel.Outlier;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	public class ForecastViewModelFactory : IForecastViewModelFactory
	{
		private readonly IForecastWorkloadEvaluator _forecastWorkloadEvaluator;
		private readonly IWorkloadRepository _workloadRepository;
		private readonly IHistoricalPeriodProvider _historicalPeriodProvider;
		private readonly IHistoricalData _historicalData;
		private readonly IOutlierRemover _outlierRemover;
		private readonly IForecastMethodProvider _forecastMethodProvider;
		private readonly IForecastMisc _forecastMisc;

		public ForecastViewModelFactory(IForecastWorkloadEvaluator forecastWorkloadEvaluator, IWorkloadRepository workloadRepository, IHistoricalPeriodProvider historicalPeriodProvider, IHistoricalData historicalData, IOutlierRemover outlierRemover, IForecastMethodProvider forecastMethodProvider, IForecastMisc forecastMisc)
		{
			_forecastWorkloadEvaluator = forecastWorkloadEvaluator;
			_workloadRepository = workloadRepository;
			_historicalPeriodProvider = historicalPeriodProvider;
			_historicalData = historicalData;
			_outlierRemover = outlierRemover;
			_forecastMethodProvider = forecastMethodProvider;
			_forecastMisc = forecastMisc;
		}

		public WorkloadEvaluateViewModel Evaluate(EvaluateInput input)
		{
			var workload = _workloadRepository.Get(input.WorkloadId);
			var evaluateResult = _forecastWorkloadEvaluator.Evaluate(workload);
			var bestAccuracy = evaluateResult.Accuracies.SingleOrDefault(x => x.IsSelected);

			var availablePeriod = _historicalPeriodProvider.AvailablePeriod(workload);
			var evaluationDayViewModels = availablePeriod.HasValue
				? createEvaluationDayViewModels(workload, bestAccuracy,
					HistoricalPeriodProvider.DivideIntoTwoPeriods(availablePeriod.Value).Item2)
				: new dynamic[] {};
			return new WorkloadEvaluateViewModel
			{
				Name = _forecastMisc.WorkloadName(workload.Skill.Name, workload.Name),
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
				QueueStatisticsDays = availablePeriod.HasValue
					? createQueueStatisticsDayViewModels(workload, input.MethodId, availablePeriod.Value)
					: new dynamic[] {}
			};
		}

		public WorkloadEvaluateMethodsViewModel EvaluateMethods(EvaluateMethodsInput input)
		{
			var workload = _workloadRepository.Get(input.WorkloadId);
			var evaluateResult = _forecastWorkloadEvaluator.Evaluate(workload);
			var availablePeriod = _historicalPeriodProvider.AvailablePeriod(workload);
			var result = new WorkloadEvaluateMethodsViewModel
			{
				WorkloadId = workload.Id.Value,
				WorkloadName = workload.Name
			};

			if (!availablePeriod.HasValue)
			{
				result.Methods = new WorkloadEvaluateMethodViewModel[] {};
				return result;
			}
			var historicalData = _historicalData.Fetch(workload, availablePeriod.Value);

			var methods = new List<WorkloadEvaluateMethodViewModel>();
			foreach (var methodAccuracy in evaluateResult.Accuracies)
			{
				var data = new Dictionary<DateOnly, dynamic>();
				foreach (var taskOwner in historicalData.TaskOwnerDayCollection)
				{
					dynamic item = new ExpandoObject();
					item.date = taskOwner.CurrentDate.Date;
					item.vh = taskOwner.TotalStatisticCalculatedTasks;
					item.vacw = taskOwner.TotalStatisticAverageAfterTaskTime.TotalSeconds;
					item.vtt = taskOwner.TotalStatisticAverageTaskTime.TotalSeconds;
					data.Add(taskOwner.CurrentDate, item);
				}
				foreach (var dayResult in methodAccuracy.MeasureResult)
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
				methods.Add(new WorkloadEvaluateMethodViewModel
				{
					MethodId = methodAccuracy.MethodId,
					AccuracyNumber = methodAccuracy.Number,
					PeriodEvaluateOnStart = methodAccuracy.PeriodEvaluateOn.StartDate.Date,
					PeriodEvaluateOnEnd = methodAccuracy.PeriodEvaluateOn.EndDate.Date,
					PeriodUsedToEvaluateStart = methodAccuracy.PeriodUsedToEvaluate.StartDate.Date,
					PeriodUsedToEvaluateEnd = methodAccuracy.PeriodUsedToEvaluate.EndDate.Date,
					IsSelected = methodAccuracy.IsSelected,
					Days = data.Values.ToArray()
				});
			}
			result.Methods = methods.ToArray();
			return result;
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

		private dynamic[] createEvaluationDayViewModels(IWorkload workload, MethodAccuracy bestAccuracy, DateOnlyPeriod period)
		{
			var historicalData = _historicalData.Fetch(workload, period);
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

		private dynamic[] createQueueStatisticsDayViewModels(IWorkload workload, ForecastMethodType method, DateOnlyPeriod period)
		{
			var historicalData = _historicalData.Fetch(workload, period);
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