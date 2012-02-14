using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupPersonSameForPersonOnDateChecker
    {
        IGroupPerson FindCommonGroupPersonForPersonOnDates(IPerson person, IList<DateOnly> dateOnlyList, IList<IPerson> allSelectedPersons);
        IGroupPersonsBuilder GroupPersonsBuilder { get; }
    }

    public class GroupPersonSameForPersonOnDateChecker : IGroupPersonSameForPersonOnDateChecker
    {
        private readonly IGroupPersonsBuilder _groupPersonsBuilder;

        public GroupPersonSameForPersonOnDateChecker(IGroupPersonsBuilder groupPersonsBuilder)
        {
            _groupPersonsBuilder = groupPersonsBuilder;
        }

        public IGroupPerson FindCommonGroupPersonForPersonOnDates(IPerson person, IList<DateOnly> dateOnlyList,
            IList<IPerson> allSelectedPersons)
        {
            if (person == null || dateOnlyList == null || allSelectedPersons == null) return null;
           
            if (dateOnlyList.Count < 2) return null;

            //all groups on selectedpersons on a date
            var groupPersons = _groupPersonsBuilder.BuildListOfGroupPersons(dateOnlyList[0], allSelectedPersons, false);

            var dateOneGroupPerson = findGroupPersonForPerson(person, groupPersons);
            if (dateOneGroupPerson == null)
                return null;
            for (int i = 1; i < dateOnlyList.Count; i++)
            {
                groupPersons = _groupPersonsBuilder.BuildListOfGroupPersons(dateOnlyList[i], allSelectedPersons, false);
                var nextDateGroupPerson = findGroupPersonForPerson(person, groupPersons);
                if (nextDateGroupPerson == null)
                    return null;
                if (!dateOneGroupPerson.GroupMembers.Count.Equals(nextDateGroupPerson.GroupMembers.Count))
                    return null;
                if (!groupsHasSameMembers(dateOneGroupPerson, nextDateGroupPerson))
                    return null;
            }
            return dateOneGroupPerson;
        }

        public IGroupPersonsBuilder GroupPersonsBuilder
        {
            get { return _groupPersonsBuilder; }
        }

        private static IGroupPerson findGroupPersonForPerson(IPerson person, IEnumerable<IGroupPerson> groupPersons)
        {
            if (groupPersons.Count() == 0) return null;
            return groupPersons.FirstOrDefault(groupPerson => groupPerson.GroupMembers.Contains(person));
        }

        private static bool groupsHasSameMembers(IGroupPerson groupPersonOne, IGroupPerson groupPersonTwo)
        {
            return groupPersonOne.GroupMembers.All(groupMember => groupPersonTwo.GroupMembers.Contains(groupMember));
        }
    }
}