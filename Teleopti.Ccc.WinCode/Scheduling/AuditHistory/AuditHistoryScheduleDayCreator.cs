using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.AuditHistory
{
    public interface IAuditHistoryScheduleDayCreator
    {
        IScheduleDay Create(IScheduleDay currentScheduleDay,  IEnumerable<IPersistableScheduleData> newData);
    }

    public class AuditHistoryScheduleDayCreator : IAuditHistoryScheduleDayCreator
    {
        public IScheduleDay Create(IScheduleDay currentScheduleDay,  IEnumerable<IPersistableScheduleData> newData)
        {
            var resultingDay = (ExtractedSchedule)currentScheduleDay.Clone();
            foreach (var personAssignment in currentScheduleDay.PersonAssignmentCollectionDoNotUse())
            {
                resultingDay.RemovePersonAssignment(personAssignment);
            }
            foreach (var persistableScheduleData in currentScheduleDay.PersistableScheduleDataCollection())
            {
                if(persistableScheduleData is IPersonAbsence || persistableScheduleData is IPersonDayOff)
                    resultingDay.Remove(persistableScheduleData);
            }

            foreach (var persistableScheduleData in newData)
            {
				if (persistableScheduleData is IPersonAbsence)
				{
					if (persistableScheduleData.Period.ToDateOnlyPeriod(currentScheduleDay.TimeZone).Contains(currentScheduleDay.DateOnlyAsPeriod.DateOnly))
						resultingDay.Add(persistableScheduleData);
					continue;
				}
				if(persistableScheduleData.Period.ToDateOnlyPeriod(currentScheduleDay.TimeZone).StartDate != currentScheduleDay.DateOnlyAsPeriod.DateOnly)
					continue;
                if(persistableScheduleData is IPersonDayOff || persistableScheduleData is IPersonAssignment)
                    resultingDay.Add(persistableScheduleData);
            }

            return resultingDay;
        }
    }
}