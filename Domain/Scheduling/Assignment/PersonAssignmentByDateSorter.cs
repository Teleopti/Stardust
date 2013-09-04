using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    /// <summary>
    /// Sorts assignments by date.
    /// Tested from scheduleTest.
    /// 
    /// Will keep state of startdate due to Period
    /// is a quite expansive operation
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-05-26
    /// </remarks>
    public class PersonAssignmentByDateSorter : IComparer<IPersonAssignment>
    {
        private readonly IDictionary<IPersonAssignment, DateTime> personAssignmentStartDateState;

        public PersonAssignmentByDateSorter()
        {
            personAssignmentStartDateState = new Dictionary<IPersonAssignment, DateTime>();
        }

        public int Compare(IPersonAssignment x, IPersonAssignment y)
        {
            DateTime xStartDate = getSetValue(x);
            DateTime yStartDate = getSetValue(y);

			if (xStartDate == yStartDate && x.CreatedOn.HasValue && y.CreatedOn.HasValue)
				return x.CreatedOn.Value.CompareTo(y.CreatedOn.Value);

            return xStartDate.CompareTo(yStartDate);
        }

        private DateTime getSetValue(IPersonAssignment assignment)
        {
            DateTime time;
            if (!personAssignmentStartDateState.TryGetValue(assignment,out time))
            {
                time = assignment.Period.StartDateTime;
                personAssignmentStartDateState[assignment] = time;
            }
            return time;
        }
    }
}
