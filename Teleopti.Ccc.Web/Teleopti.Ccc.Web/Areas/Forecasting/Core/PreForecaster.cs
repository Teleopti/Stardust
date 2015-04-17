using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
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

		public PreForecaster(IPreForecastWorkload forecastWorkload, IQuickForecastWorkloadEvaluator forecastWorkloadEvaluator, IWorkloadRepository workloadRepository, IHistoricalPeriodProvider historicalPeriodProvider)
		{
			_forecastWorkload = forecastWorkload;
			_forecastWorkloadEvaluator = forecastWorkloadEvaluator;
			_workloadRepository = workloadRepository;
			_historicalPeriodProvider = historicalPeriodProvider;
		}

		public WorkloadForecastViewModel MeasureAndForecast(PreForecastInput input)
		{
			var workload = _workloadRepository.Get(input.WorkloadId);
			var evaluateResult = _forecastWorkloadEvaluator.Measure(workload, _historicalPeriodProvider.PeriodForEvaluate());
			var forecastResult = _forecastWorkload.PreForecast(workload, new DateOnlyPeriod(new DateOnly(input.ForecastStart), new DateOnly(input.ForecastEnd)));

			var data = new List<dynamic>();
			foreach (var dateKey in forecastResult.Keys)
			{
				data.Add(new
				{
					date = dateKey.Date,
					vh = 0,
					v0 = forecastResult[dateKey][ForecastMethodType.TeleoptiClassic],
					v1 = forecastResult[dateKey][ForecastMethodType.TeleoptiClassicWithTrend]
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
			return new WorkloadForecastViewModel
			{
				Name = workload.Name,
				WorkloadId = workload.Id.Value,
				SelectedForecastMethod = evaluateResult.Accuracies.Single(x => x.IsSelected).MethodId,
				ForecastMethods = methods.ToArray(),
				ForecastDays = data.ToArray()
			};
		}
	}
}