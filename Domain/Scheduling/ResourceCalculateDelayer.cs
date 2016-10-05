using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ResourceCalculateDelayer : IResourceCalculateDelayer
	{
		private readonly IResourceOptimization _resourceOptimizationHelper;
		private readonly int _calculationFrequency;
		private readonly bool _considerShortBreaks;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private DateOnly? _lastDate;
		private int _counter = 1;
		private bool _paused;

		public ResourceCalculateDelayer(
			IResourceOptimization resourceOptimizationHelper, 
			int calculationFrequency, 
			bool considerShortBreaks,
			ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_calculationFrequency = calculationFrequency;
			_considerShortBreaks = considerShortBreaks;
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		public bool CalculateIfNeeded(DateOnly scheduleDateOnly, DateTimePeriod? workShiftProjectionPeriod, bool doIntraIntervalCalculation)
		{
			if (_paused)
				return false;

			if (!_lastDate.HasValue)
				_lastDate = scheduleDateOnly;

			if (_calculationFrequency == 1)
			{
				resourceCalculateDate(scheduleDateOnly, _considerShortBreaks, doIntraIntervalCalculation);
				DateTimePeriod? dateTimePeriod = workShiftProjectionPeriod;
				if (dateTimePeriod.HasValue)
				{
					DateTimePeriod period = dateTimePeriod.Value;
					if (isNightShift(period))
					{
						resourceCalculateDate(scheduleDateOnly.AddDays(1), _considerShortBreaks, doIntraIntervalCalculation);
					}
				}
				return true;
			}

			if (_counter % _calculationFrequency == 0 || scheduleDateOnly != _lastDate.Value)
			{
				resourceCalculateDate(_lastDate.Value, _considerShortBreaks, doIntraIntervalCalculation);
				resourceCalculateDate(_lastDate.Value.AddDays(1), _considerShortBreaks, doIntraIntervalCalculation);
				_lastDate = scheduleDateOnly;
				_counter = 1;

				return true;
			}

			_counter++;
			return false;
		}

		public void Pause()
		{
			_paused = true;
		}

		public void Resume()
		{
			_paused = false;
		}

		private bool isNightShift(DateTimePeriod period)
		{
			var tz = TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone;
			var viewerStartDate = new DateOnly(period.StartDateTimeLocal(tz));
			var viewerEndDate = new DateOnly(period.EndDateTimeLocal(tz).AddMinutes(-1));

			return viewerStartDate != viewerEndDate;
		}

		private void resourceCalculateDate(DateOnly date, bool considerShortBreaks, bool doIntraIntervalCalculation)
		{
			_resourceOptimizationHelper.ResourceCalculate(date, _schedulingResultStateHolder.ToResourceOptimizationData(considerShortBreaks, doIntraIntervalCalculation));
		}
	}
}