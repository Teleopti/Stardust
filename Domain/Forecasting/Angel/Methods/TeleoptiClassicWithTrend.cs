using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Trend;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public abstract class TeleoptiClassicWithTrend : TeleoptiClassicBase
	{
		private readonly ILinearTrendCalculator _linearTrendCalculator;

		protected TeleoptiClassicWithTrend(IIndexVolumes indexVolumes, ILinearTrendCalculator linearTrendCalculator)
			: base(indexVolumes)
		{
			_linearTrendCalculator = linearTrendCalculator;
		}

		protected override IEnumerable<DateAndTask> ForecastNumberOfTasks(ITaskOwnerPeriod historicalData, DateOnlyPeriod futurePeriod)
		{
			var trend = _linearTrendCalculator.CalculateTrend(historicalData);
			var dateAndTasks = base.ForecastNumberOfTasks(historicalData, futurePeriod).ToArray();

			var averageTasks = dateAndTasks.Average(x => x.Tasks);
			foreach (var dateAndTask in dateAndTasks)
			{
				dateAndTask.Tasks = Math.Max(0, dateAndTask.Tasks + dateAndTask.Date.Subtract(LinearTrend.StartDate).Days * trend.Slope + trend.Intercept - averageTasks);
			}
			return dateAndTasks;

		}
	}

	public class TeleoptiClassicLongTermWithTrend : TeleoptiClassicWithTrend
	{
		public TeleoptiClassicLongTermWithTrend(ILinearTrendCalculator linearTrendCalculator) : base(new IndexVolumesLongTerm(), linearTrendCalculator)
		{
		}

		public override ForecastMethodType Id
		{
			get { return ForecastMethodType.TeleoptiClassicLongTermWithTrend; }
		}
	}

	public class TeleoptiClassicMediumTermWithTrend : TeleoptiClassicWithTrend
	{
		public TeleoptiClassicMediumTermWithTrend(ILinearTrendCalculator linearTrendCalculator) : base(new IndexVolumesMediumTerm(), linearTrendCalculator)
		{
		}

		public override ForecastMethodType Id
		{
			get { return ForecastMethodType.TeleoptiClassicMediumTermWithTrend; }
		}
	}

	public class TeleoptiClassicMediumTermWithDayInMonthWithTrend : TeleoptiClassicWithTrend
	{
		public TeleoptiClassicMediumTermWithDayInMonthWithTrend(ILinearTrendCalculator linearTrendCalculator)
			: base(new IndexVolumesMediumTermWithDayInMonth(), linearTrendCalculator)
		{
		}

		public override ForecastMethodType Id
		{
			get { return ForecastMethodType.TeleoptiClassicMediumTermWithDayInMonthWithTrend; }
		}
	}

	public class TeleoptiClassicLongTermWithDayInMonthWithTrend : TeleoptiClassicWithTrend
	{
		public TeleoptiClassicLongTermWithDayInMonthWithTrend(ILinearTrendCalculator linearTrendCalculator)
			: base(new IndexVolumesLongTermWithDayInMonth(), linearTrendCalculator)
		{
		}

		public override ForecastMethodType Id
		{
			get { return ForecastMethodType.TeleoptiClassicLongTermWithDayInMonthWithTrend; }
		}
	}
}