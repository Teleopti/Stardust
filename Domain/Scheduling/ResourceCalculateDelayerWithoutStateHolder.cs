using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

public interface IResourceCalculateDelayerWithoutStateholder
{
	void CalculateIfNeeded(DateOnly scheduleDateOnly, DateTimePeriod? workShiftProjectionPeriod, ResourceCalculationData resourceCalculationData, Func<IDisposable> contextFunc);
}

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ResourceCalculateDelayerWithoutStateHolder : IResourceCalculateDelayerWithoutStateholder
	{
		private readonly IResourceCalculation _resourceCalculation;
		private readonly int _calculationFrequency;

		private readonly IUserTimeZone _userTimeZone;
		private DateOnly? _lastDate;
		private int _counter = 1;
		private bool _paused;

		public ResourceCalculateDelayerWithoutStateHolder(
			IResourceCalculation resourceCalculation, 
			int calculationFrequency, 
			IUserTimeZone userTimeZone)
		{
			_resourceCalculation = resourceCalculation;
			_calculationFrequency = calculationFrequency;
			_userTimeZone = userTimeZone;
		}

		public void CalculateIfNeeded(DateOnly scheduleDateOnly, DateTimePeriod? workShiftProjectionPeriod, ResourceCalculationData resourceCalculationData, Func<IDisposable> contextFunc)
		{
			if (_paused)
				return;

			if (!_lastDate.HasValue)
				_lastDate = scheduleDateOnly;

			if (_calculationFrequency == 1)
			{
				resourceCalculateDate(scheduleDateOnly, resourceCalculationData, contextFunc);
				DateTimePeriod? dateTimePeriod = workShiftProjectionPeriod;
				if (dateTimePeriod.HasValue)
				{
					DateTimePeriod period = dateTimePeriod.Value;
					if (isNightShift(period))
					{
						resourceCalculateDate(scheduleDateOnly.AddDays(1), resourceCalculationData, contextFunc);
					}
				}
				return;
			}

			if (_counter % _calculationFrequency == 0 || scheduleDateOnly != _lastDate.Value)
			{
				resourceCalculateDate(_lastDate.Value, resourceCalculationData, contextFunc);
				resourceCalculateDate(_lastDate.Value.AddDays(1), resourceCalculationData, contextFunc);
				_lastDate = scheduleDateOnly;
				_counter = 1;

				return;
			}

			_counter++;
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
			var tz = _userTimeZone.TimeZone();
			var viewerStartDate = new DateOnly(period.StartDateTimeLocal(tz));
			var viewerEndDate = new DateOnly(period.EndDateTimeLocal(tz).AddMinutes(-1));

			return viewerStartDate != viewerEndDate;
		}

		private void resourceCalculateDate(DateOnly date, ResourceCalculationData resourceCalculationData, Func<IDisposable> contextFunc)
		{
			_resourceCalculation.ResourceCalculate(date.ToDateOnlyPeriod(), resourceCalculationData, contextFunc);
		}
	}
}