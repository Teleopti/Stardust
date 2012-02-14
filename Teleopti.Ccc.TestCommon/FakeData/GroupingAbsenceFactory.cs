using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Creating test data for GroupingAbsence domain object
    /// </summary>
    public static class GroupingAbsenceFactory
    {
        /// <summary>
        /// Creates the grouping absence aggregate.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="absences">The absence types.</param>
        /// <returns></returns>
        public static GroupingAbsence CreateGroupingAbsenceAggregate(string name, IList<Absence> absences)
        {
            GroupingAbsence myGroupingAbsence = new GroupingAbsence(name);
            foreach (Absence absence in absences)
            {
                myGroupingAbsence.AddAbsence(absence);
            }
            return myGroupingAbsence;
        }

        /// <summary>
        /// Creates a grouping absence
        /// </summary>
        /// <param name="name">Name of grouping absence</param>
        /// <returns></returns>
        public static GroupingAbsence CreateSimpleGroupingAbsence(string name)
        {
            GroupingAbsence myGroupingAbsence = new GroupingAbsence(name);
            return myGroupingAbsence;
        }

        /// <summary>
        /// Creates A grouping absence without taking params
        /// </summary>
        public static GroupingAbsence CreateSimpleGroupingAbsence()
        {
            return CreateSimpleGroupingAbsence("Sick leave");
        }
    }
}