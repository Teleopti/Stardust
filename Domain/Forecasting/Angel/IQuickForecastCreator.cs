using System;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class ForecastWorkloadInput
	{
		public Guid WorkloadId { get; set; }
		public ForecastMethodType ForecastMethodId { get; set; }
	}

	public interface IQuickForecastCreator
	{
		void CreateForecastForWorkloads(DateOnlyPeriod futurePeriod, ForecastWorkloadInput[] workloads);
		void CreateForecastForAll(DateOnlyPeriod futurePeriod);
	}

	public interface IPreForecaster
	{
		WorkloadForecastingViewModel MeasureAndForecast(PreForecastInputModel model);
	}

	public class PreForecastInputModel
	{
		public DateTime ForecastStart { get; set; }
		public DateTime ForecastEnd { get; set; }
		public Guid WorkloadId { get; set; }
	}

	public class WorkloadForecastingViewModel
	{
		public Guid WorkloadId { get; set; }
		public string Name { get; set; }
		public ForecastMethodViewModel[] ForecastMethods { get; set; }
		public dynamic[] ForecastDayViewModels { get; set; }
		public ForecastMethodType SelectedForecastMethod { get; set; }
	}

	public class ForecastMethodViewModel
	{
		public double AccuracyNumber { get; set; }
		public ForecastMethodType ForecastMethodType { get; set; }
	}

	public class ForecastDayViewModel
	{
		public DateOnly Date { get; set; }
		public ForecastDayResult[] ForecastDayResults { get; set; }
	}

	public class ForecastDayResult
	{
		public double Tasks { get; set; }
	}

	public interface IPreForecastWorkload
	{
		ForecastDayViewModel[] PreForecast(IWorkload workload, DateOnlyPeriod futurePeriod);
	}

	public class PreForecastWorkload : IPreForecastWorkload
	{
		private readonly IHistoricalData _historicalData;
		private readonly IForecastMethodProvider _forecastMethodProvider;
		private readonly IHistoricalPeriodProvider _historicalPeriodProvider;

		public PreForecastWorkload(IHistoricalData historicalData, IForecastMethodProvider forecastMethodProvider, IHistoricalPeriodProvider historicalPeriodProvider)
		{
			_historicalData = historicalData;
			_forecastMethodProvider = forecastMethodProvider;
			_historicalPeriodProvider = historicalPeriodProvider;
		}

		public ForecastDayViewModel[] PreForecast(IWorkload workload, DateOnlyPeriod futurePeriod)
		{
			var historicalData = _historicalData.Fetch(workload, _historicalPeriodProvider.PeriodForForecast());
			if (!historicalData.TaskOwnerDayCollection.Any())
				return new ForecastDayViewModel[] { };
			var forecastMethods = _forecastMethodProvider.All();
			foreach (var forecastMethod in forecastMethods)
			{
				var forecastingTargets = forecastMethod.Forecast(historicalData, futurePeriod);
				foreach (var forecastingTarget in forecastingTargets)
				{
					//forecastingTarget.CurrentDate
				}
			}
			return null;
		}
	}

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

		public WorkloadForecastingViewModel MeasureAndForecast(PreForecastInputModel model)
		{
			var workload = _workloadRepository.Get(model.WorkloadId);
			var evaluateResult = _forecastWorkloadEvaluator.Measure(workload, _historicalPeriodProvider.PeriodForEvaluate());
			//var forecastResult = _forecastWorkload.PreForecast(workload, new DateOnlyPeriod(new DateOnly(model.ForecastStart), new DateOnly(model.ForecastEnd)));
			return new WorkloadForecastingViewModel()
			{
				Name = workload.Name,
				WorkloadId = workload.Id.Value,
				SelectedForecastMethod = evaluateResult.Accuracies.Single(x=>x.IsSelected).MethodId,
				ForecastMethods = new[]
				{
					new ForecastMethodViewModel {AccuracyNumber = 10.2,ForecastMethodType = ForecastMethodType.TeleoptiClassic},
					new ForecastMethodViewModel {AccuracyNumber = 30.2, ForecastMethodType = ForecastMethodType.TeleoptiClassicWithTrend}
				},
				ForecastDayViewModels = new dynamic[]
				{
					new
					{
						date = new DateTime(2012,1,1),
						vh = 20,
						v0 = 34,
						v1 = 35
					},
					new
					{
						date = new DateTime(2012,1,2),
						vh = 23,
						v0 = 35,
						v1 = 25
					}
					//new ForecastDayViewModel(){Date = new DateOnly(2014,1,1), ForecastDayResults = new []
					//{
					//	new ForecastDayResult()
					//	{
					//		Tasks = 20
					//	},
					//	new ForecastDayResult()
					//	{
					//		Tasks = 34
					//	}
					//}},
					//new ForecastDayViewModel(){Date = new DateOnly(2014,1,2), ForecastDayResults = new []
					//{
					//	new ForecastDayResult()
					//	{
					//		Tasks = 23
					//	},
					//	new ForecastDayResult()
					//	{
					//		Tasks = 35
					//	}
					//}}
				}


			};
		}
	}
}