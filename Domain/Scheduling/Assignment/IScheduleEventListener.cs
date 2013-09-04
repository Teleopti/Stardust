using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    /// <summary>
    /// Listener for Schedule events
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-05-08
    /// </remarks>
    public interface IScheduleEventListener
    {
        /// <summary>
        /// Called when [person assignment add].
        /// </summary>
        /// <param name="ass">The ass.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-08
        /// </remarks>
        void OnPersonAssignmentAdd(IPersonAssignment ass);

        /// <summary>
        /// Called when [person day off add].
        /// </summary>
        /// <param name="off">The off.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-08
        /// </remarks>
        void OnPersonDayOffAdd(PersonDayOff off);

        /// <summary>
        /// Called when [person absence add].
        /// </summary>
        /// <param name="abs">The abs.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-08
        /// </remarks>
        void OnPersonAbsenceAdd(PersonAbsence abs);

        /// <summary>
        /// Called when [person assignment remove].
        /// </summary>
        /// <param name="ass">The ass.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-08
        /// </remarks>
        void OnPersonAssignmentRemove(IPersonAssignment ass);

        /// <summary>
        /// Called when [person day off remove].
        /// </summary>
        /// <param name="off">The off.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-08
        /// </remarks>
        void OnPersonDayOffRemove(PersonDayOff off);

        /// <summary>
        /// Called when [person absence remove].
        /// </summary>
        /// <param name="abs">The abs.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-05-08
        /// </remarks>
        void OnPersonAbsenceRemove(PersonAbsence abs);
    }
}
