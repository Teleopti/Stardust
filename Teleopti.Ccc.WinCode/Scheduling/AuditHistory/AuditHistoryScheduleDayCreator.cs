using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
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
					var resultingDay = (IScheduleDay)currentScheduleDay.Clone();
					resultingDay.Clear<IPersonAssignment>();
					resultingDay.Clear<IPersonAbsence>();
					newData.ForEach(resultingDay.Add);
	        return resultingDay;
        }
    }
}