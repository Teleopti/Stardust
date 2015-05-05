using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	public class PreForecaster : IPreForecaster
	{
		private readonly IQuickForecastWorkloadEvaluator _forecastWorkloadEvaluator;
		private readonly IWorkloadRepository _workloadRepository;
		private readonly IHistoricalPeriodProvider _historicalPeriodProvider;
		private readonly IHistoricalData _historicalData;

		public PreForecaster(IQuickForecastWorkloadEvaluator forecastWorkloadEvaluator, IWorkloadRepository workloadRepository, IHistoricalPeriodProvider historicalPeriodProvider, IHistoricalData historicalData)
		{
			_forecastWorkloadEvaluator = forecastWorkloadEvaluator;
			_workloadRepository = workloadRepository;
			_historicalPeriodProvider = historicalPeriodProvider;
			_historicalData = historicalData;
		}

		public WorkloadForecastViewModel MeasureAndForecast(PreForecastInput input)
		{
			var workload = _workloadRepository.Get(input.WorkloadId);
			var evaluateResult = _forecastWorkloadEvaluator.Measure(workload, _historicalPeriodProvider.PeriodForEvaluate());
			var bestAccuracy = evaluateResult.Accuracies.SingleOrDefault(x => x.IsSelected);

			var days = createDayViewModels(workload, bestAccuracy);
			var methods = createMethodViewModels(evaluateResult);

			return new WorkloadForecastViewModel
			{
				Name = workload.Name,
				WorkloadId = workload.Id.Value,
				ForecastMethodRecommended = (bestAccuracy == null ? ForecastMethodType.None : bestAccuracy.MethodId),
				ForecastMethods = methods,
				ForecastDays = days
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

		private dynamic[] createDayViewModels(IWorkload workload, MethodAccuracy bestAccuracy)
		{
			var historicalDataForForecasting = _historicalData.Fetch(workload, _historicalPeriodProvider.PeriodForForecast());
			var data = new Dictionary<DateOnly, dynamic>();
			foreach (var taskOwner in historicalDataForForecasting.TaskOwnerDayCollection)
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

			var days = data.Values.ToArray();
			return days;
		}
	}
}