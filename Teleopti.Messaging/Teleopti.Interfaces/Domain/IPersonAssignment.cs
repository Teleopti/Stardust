using System;
using System.Collections.Generic;

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
                                            IRestrictionChecker<IPersonAssignment>, 
                                            IProjectionSource, 
                                            ICloneableEntity<IPersonAssignment>,
											IEquatable<IPersonAssignment>
    {
        /// <summary>
        /// Clears the personal shift.
        /// </summary>
        void ClearPersonalLayers();


        /// <summary>
        /// Clears the main shift.
        /// </summary>
        void ClearMainLayers();

	    void ClearOvertimeLayers();

	    void Clear();

			/// <summary>
			/// The date
			/// </summary>
	    DateOnly Date { get; }

	    IShiftCategory ShiftCategory { get; }
	    IEnumerable<IMainShiftLayer> MainLayers();
	    IEnumerable<IPersonalShiftLayer> PersonalLayers();
	    IEnumerable<IOvertimeShiftLayer> OvertimeLayers();
	    IEnumerable<IShiftLayer> ShiftLayers { get; }

	    /// <summary>
		/// Publish the ScheduleChangedEvent
		/// </summary>
		void ScheduleChanged();

		bool RemoveLayer(IShiftLayer layer);
	    void AddPersonalLayer(IActivity activity, DateTimePeriod period);
	    void AddOvertimeLayer(IActivity activity, DateTimePeriod period, IMultiplicatorDefinitionSet multiplicatorDefinitionSet);
	    IDayOff DayOff();
	    void SetDayOff(IDayOffTemplate template);
	    void SetThisAssignmentsDayOffOn(IPersonAssignment dayOffDestination);
	    bool AssignedWithDayOff(IDayOffTemplate template);
	    void FillWithDataFrom(IPersonAssignment newAss);
	    void AddMainLayer(IActivity activity, DateTimePeriod period);
	    void SetShiftCategory(IShiftCategory shiftCategory);
	    void SetMainLayersAndShiftCategoryFrom(IPersonAssignment assignment);
	    void InsertMainLayer(IActivity activity, DateTimePeriod period, int index);
    }
}