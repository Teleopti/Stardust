using System.Drawing;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfig.Creators
{
    public class AbsenceCreator
    {
        private readonly IGroupingAbsence _groupingAbsence;

        public AbsenceCreator(IGroupingAbsence groupingAbsence)
        {
            _groupingAbsence = groupingAbsence;
        }

        /// <summary>
        /// Creates the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="color">The color.</param>
        /// <param name="priority">The priority.</param>
        /// <param name="requestable">if set to <c>true</c> [requestable].</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-11-11
        /// </remarks>
        public IAbsence Create(Description description, Color color, byte priority, bool requestable)
        {
            IAbsence absence = new Absence
                                   {
                                       Description = description,
                                       DisplayColor = color,
                                       GroupingAbsence = _groupingAbsence,
                                       Priority = priority,
                                       Requestable = requestable
                                   };
            return absence;
        }
    }
}
