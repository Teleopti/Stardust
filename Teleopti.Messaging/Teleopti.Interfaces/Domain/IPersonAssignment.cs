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
																							IVersioned
    {
        /// <summary>
        /// Clears the personal shift.
        /// </summary>
        void ClearPersonalActivities(bool muteEvent = true, TrackedCommandInfo trackedCommandInfo = null);


        /// <summary>
        /// Clears the main shift.
        /// </summary>
        void ClearMainActivities(bool muteEvent = true, TrackedCommandInfo trackedCommandInfo = null);

	    void ClearOvertimeActivities(bool muteEvent = true, TrackedCommandInfo trackedCommandInfo = null);

	    void Clear();

			/// <summary>
			/// The date
			/// </summary>
	    DateOnly Date { get; }

	    IShiftCategory ShiftCategory { get; }
	    IEnumerable<IMainShiftLayer> MainActivities();
	    IEnumerable<IPersonalShiftLayer> PersonalActivities();
	    IEnumerable<IOvertimeShiftLayer> OvertimeActivities();
	    IEnumerable<IShiftLayer> ShiftLayers { get; }

	    bool RemoveActivity(IShiftLayer layer, bool muteEvent = true, TrackedCommandInfo trackedCommandInfo = null);
	    void AddPersonalActivity(IActivity activity, DateTimePeriod period);
	    void AddOvertimeActivity(IActivity activity, DateTimePeriod period, IMultiplicatorDefinitionSet multiplicatorDefinitionSet);
	    IDayOff DayOff();
	    void SetDayOff(IDayOffTemplate template);
	    void SetThisAssignmentsDayOffOn(IPersonAssignment dayOffDestination);
	    bool AssignedWithDayOff(IDayOffTemplate template);
	    void FillWithDataFrom(IPersonAssignment newAss);
	    void AddActivity(IActivity activity, DateTimePeriod period);
		void AddActivity(IActivity activity, DateTimePeriod period, TrackedCommandInfo trackedCommandInfo);
	    void SetShiftCategory(IShiftCategory shiftCategory);
	    void SetActivitiesAndShiftCategoryFrom(IPersonAssignment assignment);
	    void InsertActivity(IActivity activity, DateTimePeriod period, int index);
	    void InsertOvertimeLayer(IActivity activity, DateTimePeriod period, int index, IMultiplicatorDefinitionSet multiplicatorDefinitionSet);
	    void InsertPersonalLayer(IActivity activity, DateTimePeriod period, int index);
	    void MoveLayerVertical(IMoveLayerVertical target, IShiftLayer layer);
		void MoveActivityAndSetHighestPriority(IActivity activity, DateTime currentStartTime, DateTime newStartTime, TimeSpan length, TrackedCommandInfo trackedCommandInfo);
	    void AddActivity(IActivity activity, TimePeriod period);
    }
}