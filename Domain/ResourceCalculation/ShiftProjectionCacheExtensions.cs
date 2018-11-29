using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public static class ShiftProjectionCacheExtensions
	{
		public static DateTimePeriod WorkShiftProjectionPeriod(this ShiftProjectionCache shiftProjectionCache) => 
			shiftProjectionCache.TheWorkShift.Projection.Period().Value;
		public static TimeSpan WorkShiftStartTime(this ShiftProjectionCache shiftProjectionCache) => 
			shiftProjectionCache.WorkShiftProjectionPeriod().StartDateTime.TimeOfDay;
		public static TimeSpan WorkShiftEndTime(this ShiftProjectionCache shiftProjectionCache) => 
			shiftProjectionCache.WorkShiftProjectionPeriod().EndDateTime.Subtract(shiftProjectionCache.WorkShiftProjectionPeriod().StartDateTime.Date);
		public static TimeSpan WorkShiftProjectionContractTime(this ShiftProjectionCache shiftProjectionCache) => 
			shiftProjectionCache.TheWorkShift.Projection.ContractTime();
	}
}