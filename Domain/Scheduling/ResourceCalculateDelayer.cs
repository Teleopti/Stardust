using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ResourceCalculateDelayer : IResourceCalculateDelayer
	{
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly int _calculationFrequency;
		private readonly bool _considerShortBreaks;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IUserTimeZone _userTimeZone;
		private DateOnly? _lastDate;
		private int _counter = 1;

		public ResourceCalculateDelayer(
			IResourceCalculation resourceOptimizationHelper, 
			int calculationFrequency, 
			bool considerShortBreaks,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IUserTimeZone userTimeZone)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_calculationFrequency = calculationFrequency;
			_considerShortBreaks = considerShortBreaks;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_userTimeZone = userTimeZone;
		}

		public void CalculateIfNeeded(DateOnly scheduleDateOnly, DateTimePeriod? workShiftProjectionPeriod, bool doIntraIntervalCalculation)
		{
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
				return;
			}

			if (_counter % _calculationFrequency == 0 || scheduleDateOnly != _lastDate.Value)
			{
				resourceCalculateDate(_lastDate.Value, _considerShortBreaks, doIntraIntervalCalculation);
				resourceCalculateDate(_lastDate.Value.AddDays(1), _considerShortBreaks, doIntraIntervalCalculation);
				_lastDate = scheduleDateOnly;
				_counter = 1;

				return;
			}

			_counter++;
		}

		private bool isNightShift(DateTimePeriod period)
		{
			var tz = _userTimeZone.TimeZone();
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