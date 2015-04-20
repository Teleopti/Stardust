using System;
using System.Collections.Generic;
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
		private readonly IPreForecastWorkload _forecastWorkload;
		private readonly IQuickForecastWorkloadEvaluator _forecastWorkloadEvaluator;
		private readonly IWorkloadRepository _workloadRepository;
		private readonly IHistoricalPeriodProvider _historicalPeriodProvider;
		private readonly IHistoricalData _historicalData;

		public PreForecaster(IPreForecastWorkload forecastWorkload, IQuickForecastWorkloadEvaluator forecastWorkloadEvaluator, IWorkloadRepository workloadRepository, IHistoricalPeriodProvider historicalPeriodProvider, IHistoricalData historicalData)
		{
			_forecastWorkload = forecastWorkload;
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
			var forecastResult = _forecastWorkload.PreForecast(workload, new DateOnlyPeriod(new DateOnly(input.ForecastStart), new DateOnly(input.ForecastEnd)), historicalDataForForecasting);

			var data = new List<dynamic>();

			foreach (var taskOwner in historicalDataForForecasting.TaskOwnerDayCollection)
			{
				data.Add(new
				{
					date = taskOwner.CurrentDate.Date,
					vh = taskOwner.TotalStatisticCalculatedTasks
				});
			}

			foreach (var dateKey in forecastResult.Keys)
			{
				data.Add(new
				{
					date = dateKey.Date,
					v0 = Math.Round(forecastResult[dateKey][ForecastMethodType.TeleoptiClassic], 1),
					v1 = Math.Round(forecastResult[dateKey][ForecastMethodType.TeleoptiClassicWithTrend], 1)
				});
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
			var bestAccuracy = evaluateResult.Accuracies.SingleOrDefault(x => x.IsSelected);

			var forecastMethodRecommended = bestAccuracy == null ? ForecastMethodType.None : bestAccuracy.MethodId;
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