using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    /// <summary>
    /// A scheduleRange object that supports adding with no permission checks.
    /// Shouldn't be used normally. Now used when read from database - needed for res calc stuff and others.
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-05-19
    /// </remarks>
    public class ScheduleRangeLowPermissionCheck : ScheduleRange
    {
        internal ScheduleRangeLowPermissionCheck(IScheduleDictionary owner, IScheduleParameters parameters, IAuthorizationService authorizationService)
            : base(owner, parameters, authorizationService)
        {

        }

        public void AddPersonAssignmentRangeUnsafe(IEnumerable<IPersonAssignment> personAssignmentCollection)
        {
            DoPermissionCheck = false;
            AddRange(personAssignmentCollection);
            DoPermissionCheck = true;
        }

        public void AddUnsafe(IScheduleData persistableScheduleData)
        {
            DoPermissionCheck = false;
            Add(persistableScheduleData);
            DoPermissionCheck = true;
        }
    }
}
