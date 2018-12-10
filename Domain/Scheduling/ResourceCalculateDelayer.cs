using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling
{
	//don't use this one. don't check night shift in correct way. Use ScheduleChangesAffectedDates instead
	public class ResourceCalculateDelayer : IResourceCalculateDelayer
	{
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly bool _considerShortBreaks;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IUserTimeZone _userTimeZone;

		public ResourceCalculateDelayer(IResourceCalculation resourceOptimizationHelper, 
			bool considerShortBreaks,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IUserTimeZone userTimeZone)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_considerShortBreaks = considerShortBreaks;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_userTimeZone = userTimeZone;
		}

		public void CalculateIfNeeded(DateOnly scheduleDateOnly, DateTimePeriod? workShiftProjectionPeriod, bool doIntraIntervalCalculation)
		{
			resourceCalculateDate(scheduleDateOnly, _considerShortBreaks, doIntraIntervalCalculation);
			var dateTimePeriod = workShiftProjectionPeriod;
			if (dateTimePeriod.HasValue)
			{
				var period = dateTimePeriod.Value;
				if (isNightShift(period))
				{
					resourceCalculateDate(scheduleDateOnly.AddDays(1), _considerShortBreaks, doIntraIntervalCalculation);
				}
			}
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