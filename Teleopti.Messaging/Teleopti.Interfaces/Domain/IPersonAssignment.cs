using System;
using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Represents a person's work assignment. Roughly speaking it contains 
    /// all the information about the person's work assignment for a concrete period: what, when,
    /// how long to work with etc. However it only contains activities, does not contain absences.
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-02-25
    /// </remarks>
    public interface IPersonAssignment : IPersistableScheduleData, 
                                            IChangeInfo,
                                            IBelongsToBusinessUnit,
                                            IRestrictionChecker<IPersonAssignment>, 
                                            IProjectionSource, 
                                            ICloneableEntity<IPersonAssignment>
    {
        /// <summary>
        /// Gets the main shift.
        /// </summary>
        /// <value>The main shift.</value>
        IMainShift MainShift { get; }

        /// <summary>
        /// Gets the personal shift collection.
        /// </summary>
        /// <value>The personal shift collection.</value>
        ReadOnlyCollection<IPersonalShift> PersonalShiftCollection { get; }

        /// <summary>
        /// Adds a personal shift.
        /// </summary>
        /// <param name="personalShift">The personal shift.</param>
        void AddPersonalShift(IPersonalShift personalShift);

        /// <summary>
        /// Inserts  a personal shift.
        /// </summary>
        /// <param name="personalShift"></param>
        /// <param name="index"></param>
        void InsertPersonalShift(IPersonalShift personalShift, int index);

        /// <summary>
        /// Clears the personal shift.
        /// </summary>
        void ClearPersonalShift();

        /// <summary>
        /// Removes the personal shift.
        /// </summary>
        /// <param name="personalShift">The personal shift.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-02-28
        /// </remarks>
        void RemovePersonalShift(IPersonalShift personalShift);

        /// <summary>
        /// Clears the main shift.
        /// </summary>
        /// <param name="personAssignmentRepository">The uow.</param>
        /// <remarks>
        /// Repository must be injected to let nhibernate delete
        /// unreferenced mainshift from db.
        /// NHibernate doesn't support "all-delete-orphan" on
        /// one-to-one references.
        /// Remove this injection later when/if this is supported
        /// out-of-the-box.
        /// </remarks>
        void ClearMainShift(IPersonAssignmentRepository personAssignmentRepository);

        /// <summary>
        /// Sets the main shift.
        /// </summary>
        /// <param name="mainShift">The main shift.</param>
        void SetMainShift(IMainShift mainShift);

        /// <summary>
        /// Gets or sets the ZOrder for PersonAssignment, used in gui.
        /// </summary>
        DateTime ZOrder{get; set;}

        /// <summary>
        /// Gets the over time shift collection.
        /// </summary>
        /// <value>The over time shift collection.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-02-05
        /// </remarks>
        ReadOnlyCollection<IOvertimeShift> OvertimeShiftCollection { get; }

        /// <summary>
        /// Adds the over time shift.
        /// </summary>
        /// <param name="overtimeShift">The over time shift.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-02-05
        /// </remarks>
        void AddOvertimeShift(IOvertimeShift overtimeShift);

        /// <summary>
        /// Removes the over time shift.
        /// </summary>
        /// <param name="overtimeShift">The over time shift.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-02-05
        /// </remarks>
        void RemoveOvertimeShift(IOvertimeShift overtimeShift);

		/// <summary>
		/// Publish the ScheduleChangedEvent
		/// </summary>
		void ScheduleChanged(string dataSource);
    }
}