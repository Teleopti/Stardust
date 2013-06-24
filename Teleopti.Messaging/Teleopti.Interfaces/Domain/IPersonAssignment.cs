using System;
using System.Collections.Generic;
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
											IAggregateRootWithEvents,
                                            IChangeInfo,
                                            IBelongsToBusinessUnit,
                                            IRestrictionChecker<IPersonAssignment>, 
                                            IProjectionSource, 
                                            ICloneableEntity<IPersonAssignment>
    {
			void SetMainShiftLayers(IEnumerable<IMainShiftLayer> activityLayers, IShiftCategory shiftCategory);

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
        void ClearMainLayers();

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
			/// The date
			/// </summary>
	    DateOnly Date { get; }

	    IShiftCategory ShiftCategory { get; }
	    IEnumerable<IMainShiftLayer> MainLayers { get; }
			IEnumerable<IPersonalShiftLayer> PersonalLayers { get; }

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

	    bool RemoveLayer(IMainShiftLayer layer);
	    bool RemoveLayer(IPersonalShiftLayer layer);
	    void AddPersonalLayer(IActivity activity, DateTimePeriod period);
    }
}