using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupPersonSchedulePeriodChecker
    {
        bool AllInSameGroupHasSameSchedulePeriod(IGroupPerson groupPerson, IList<DateOnly> dateOnlyList);
    }

    public class GroupPersonSchedulePeriodChecker : IGroupPersonSchedulePeriodChecker
    {

        public bool AllInSameGroupHasSameSchedulePeriod(IGroupPerson groupPerson, IList<DateOnly> dateOnlyList)
        {
            if(groupPerson == null)
                throw new ArgumentNullException("groupPerson");
            return groupPerson.GroupMembers.All(person => personHasSameSchedulePeriod(person, dateOnlyList));
        }

        private static bool personHasSameSchedulePeriod(IPerson person, IList<DateOnly> dateOnlyList)
        {
            if (person == null) return false;
            if (dateOnlyList.Count < 2) return false;
            ISchedulePeriod schedulePeriod = person.SchedulePeriod(dateOnlyList[0]);
            if (schedulePeriod == null) return false;
            for (int i = 1; i < dateOnlyList.Count; i++)
            {
                if (!person.SchedulePeriod(dateOnlyList[i]).Equals(schedulePeriod))
                    return false;
            }
            return true;
        }
    }
}