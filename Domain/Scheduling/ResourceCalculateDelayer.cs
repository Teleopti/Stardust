using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ResourceCalculateDelayer : IResourceCalculateDelayer
	{
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly int _calculationFrequency;
		private readonly bool _considerShortBreaks;
		private DateOnly? _lastDate;
		private int _counter = 1;
		private bool _paused;

		public ResourceCalculateDelayer(
			IResourceOptimizationHelper resourceOptimizationHelper, 
			int calculationFrequency, 
			bool considerShortBreaks)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_calculationFrequency = calculationFrequency;
			_considerShortBreaks = considerShortBreaks;
		}

		public bool CalculateIfNeeded(DateOnly scheduleDateOnly, DateTimePeriod? workShiftProjectionPeriod, bool doIntraIntervalCalculation)
		{
			if (_paused)
				return false;

			if (!_lastDate.HasValue)
				_lastDate = scheduleDateOnly;

			if (_calculationFrequency == 1)
			{
				_resourceOptimizationHelper.ResourceCalculateDate(scheduleDateOnly, _considerShortBreaks, doIntraIntervalCalculation);
				DateTimePeriod? dateTimePeriod = workShiftProjectionPeriod;
				if (dateTimePeriod.HasValue)
				{
					DateTimePeriod period = dateTimePeriod.Value;
					if (period.StartDateTime.Date != period.EndDateTime.Date)
						_resourceOptimizationHelper.ResourceCalculateDate(scheduleDateOnly.AddDays(1), _considerShortBreaks, doIntraIntervalCalculation);
				}
				return true;
			}

			if (_counter % _calculationFrequency == 0 || scheduleDateOnly != _lastDate.Value)
			{
				_resourceOptimizationHelper.ResourceCalculateDate(_lastDate.Value,  _considerShortBreaks, doIntraIntervalCalculation);
				_resourceOptimizationHelper.ResourceCalculateDate(_lastDate.Value.AddDays(1),  _considerShortBreaks, doIntraIntervalCalculation);
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
	}
}