using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.AuditHistory
{
    public interface IAuditHistoryScheduleDayCreator
    {
        void Apply(IScheduleDay currentScheduleDay,  IEnumerable<IPersistableScheduleData> newData);
    }

    public class AuditHistoryScheduleDayCreator : IAuditHistoryScheduleDayCreator
    {
        public void Apply(IScheduleDay currentScheduleDay,  IEnumerable<IPersistableScheduleData> newData)
        {
					currentScheduleDay.Clear<IPersonAbsence>();
					var resultingAss = currentScheduleDay.PersonAssignment(true);
					resultingAss.Clear();

	        foreach (var scheduleData in newData)
	        {
		        var newAss = scheduleData as IPersonAssignment;
						if (newAss != null)
						{
							resultingAss.FillWithDataFrom(newAss);
						}
						else
						{
							currentScheduleDay.Add(scheduleData.CreateTransient());
						}
	        }
        }
    }
}