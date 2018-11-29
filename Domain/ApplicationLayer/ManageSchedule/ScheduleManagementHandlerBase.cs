using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ManageSchedule
{
	public abstract class ScheduleManagementHandlerBase
	{
		protected static bool HandleAbsenceSplits(DateTimePeriod period, PersonAbsence absence)
		{
			var absencePeriod = absence.Period;
			var shouldChange = false;
			// If the absence is outside the period, stop processing
			if (absencePeriod.StartDateTime > period.EndDateTime || absencePeriod.EndDateTime < period.StartDateTime)
				return true;
			// If the absence starts before the archive/import period, cut the start
			if (absencePeriod.StartDateTime < period.StartDateTime)
			{
				absencePeriod = absencePeriod.ChangeStartTime(period.StartDateTime - absencePeriod.StartDateTime);
				shouldChange = true;
			}
			// If the absence ends after the archive/import period, cut the end
			if (absencePeriod.EndDateTime > period.EndDateTime)
			{
				absencePeriod = absencePeriod.ChangeEndTime(period.EndDateTime - absencePeriod.EndDateTime);
				shouldChange = true;
			}

			if (shouldChange)
			{
				var old = absence.LastChange;
				absence.ModifyPersonAbsencePeriod(absencePeriod, null);
				absence.LastChange = old;
			}
				
			return false;
		}


		protected IList<IPersistableScheduleData> GetTargetSchedulesForDay(IScheduleDictionary targetScheduleDictionary, DateOnlyPeriod period, IPerson person, DateOnly dateOnly)
		{
			var targetSchedules = targetScheduleDictionary.SchedulesForPeriod(period, person);
			var targetDay = targetSchedules.First(x => x.DateOnlyAsPeriod.DateOnly.Equals(dateOnly));
			var targetStuff = targetDay.PersistableScheduleDataCollection();
			return targetStuff.ToList();
		}
	}
}