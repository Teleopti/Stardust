using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public class CalculateBestOvertimeBeforeOrAfter : ICalculateBestOvertime
	{
		public List<DateTimePeriod> GetBestOvertime(MinMax<TimeSpan> overtimeDurantion, IList<OvertimePeriodValue> overtimePeriodValueMappedDat, IScheduleDay scheduleDay, int minimumResolution)
		{
			return new List<DateTimePeriod>();
		}
	}
}
