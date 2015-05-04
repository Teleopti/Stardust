using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;

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
			var historicalDataForForecasting = _historicalData.Fetch(workload, _historicalPeriodProvider.PeriodForForecast());

			var bestAccuracy = evaluateResult.Accuracies.SingleOrDefault(x => x.IsSelected);
			var forecastMethodRecommended = bestAccuracy == null ? ForecastMethodType.None : bestAccuracy.MethodId;

			var data = new List<dynamic>();
			foreach (var taskOwner in historicalDataForForecasting.TaskOwnerDayCollection)
			{
				if (bestAccuracy == null)
				{
					data.Add(new
					{
						date = taskOwner.CurrentDate.Date,
						vh = taskOwner.TotalStatisticCalculatedTasks
					});
				}
				else
				{
					data.Add(new
					{
						date = taskOwner.CurrentDate.Date,
						vh = taskOwner.TotalStatisticCalculatedTasks,
						vb = Math.Round(bestAccuracy.MeasureResult.Single(x => x.CurrentDate == taskOwner.CurrentDate).Tasks, 1)
					});
				}
			}

			var methods = new List<dynamic>();
			foreach (var accuracy in evaluateResult.Accuracies)
			{
				methods.Add(new
				{
					AccuracyNumber = accuracy.Number,
					ForecastMethodType = accuracy.MethodId
				});
			}
			
			return new WorkloadForecastViewModel
			{
				Name = workload.Name,
				WorkloadId = workload.Id.Value,
				ForecastMethodRecommended = forecastMethodRecommended,
				ForecastMethods = methods.ToArray(),
				ForecastDays = data.ToArray()
			};

		}
	}
}