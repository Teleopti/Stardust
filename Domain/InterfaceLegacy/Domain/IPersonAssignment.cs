using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
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

		void Clear(bool muteEvent = false);

		/// <summary>
		/// The date
		/// </summary>
		DateOnly Date { get; }

		string Source { get; set; }

		IShiftCategory ShiftCategory { get; }
		IEnumerable<MainShiftLayer> MainActivities();
		IEnumerable<PersonalShiftLayer> PersonalActivities();
		IEnumerable<OvertimeShiftLayer> OvertimeActivities();
		IEnumerable<ShiftLayer> ShiftLayers { get; }
		DateTimePeriod PeriodExcludingPersonalActivity();
		bool RemoveActivity(ShiftLayer layer, bool muteEvent = true, TrackedCommandInfo trackedCommandInfo = null);
		void AddPersonalActivity(IActivity activity, DateTimePeriod period, bool muteEvent = true, TrackedCommandInfo trackedCommandInfo = null);

		void AddOvertimeActivity(IActivity activity, DateTimePeriod period,
			IMultiplicatorDefinitionSet multiplicatorDefinitionSet, bool muteEvent = true, TrackedCommandInfo trackedCommandInfo = null);

		DayOff DayOff();
		IDayOffTemplate DayOffTemplate { get; }
		void SetDayOff(IDayOffTemplate template, bool muteEvent = false, TrackedCommandInfo trackedCommandInfo = null);
		void SetThisAssignmentsDayOffOn(IPersonAssignment dayOffDestination, bool muteEvent = false, TrackedCommandInfo trackedCommandInfo = null);
		bool AssignedWithDayOff(IDayOffTemplate template);
		void FillWithDataFrom(IPersonAssignment personAssignmentSource, bool muteEvent = false);
		void AddActivityFromTo(IActivity activity, DateTime start, DateTime end);
		void AddActivity(IActivity activity, DateTime start, DateTime end);
		void AddActivity(IActivity activity, DateTimePeriod period, bool muteEvent = false);
		void AddActivity(IActivity activity, DateTimePeriod period, TrackedCommandInfo trackedCommandInfo, bool muteEvent = false);
		void SetShiftCategory(IShiftCategory shiftCategory, bool muteEvent = true, TrackedCommandInfo trackedCommandInfo = null);
		void SetActivitiesAndShiftCategoryFrom(IPersonAssignment assignment);
		void InsertActivity(IActivity activity, DateTimePeriod period, int index);

		void InsertOvertimeLayer(IActivity activity, DateTimePeriod period, int index,
			IMultiplicatorDefinitionSet multiplicatorDefinitionSet);

		void InsertPersonalLayer(IActivity activity, DateTimePeriod period, int index);

		void MoveAllActivitiesAndKeepOriginalPriority(DateTime newStartTimeInUtc,
			TrackedCommandInfo trackedCommandInfo, bool muteEvent = false);

		void MoveActivityAndSetHighestPriority(IActivity activity, DateTime currentStartTime, DateTime newStartTime,
			TimeSpan length, TrackedCommandInfo trackedCommandInfo);

		void MoveActivityAndKeepOriginalPriority(ShiftLayer shiftLayer, DateTime newStartTimeInUtc,
			TrackedCommandInfo trackedCommandInfo, bool muteEvent = false);

		void AddActivity(IActivity activity, TimePeriod period);
		void SetActivitiesAndShiftCategoryFromWithOffset(IPersonAssignment sourceAssignment, TimeSpan periodOffset);
		void MoveLayerDown(ShiftLayer shiftLayer);
		void MoveLayerUp(ShiftLayer shiftLayer);
		bool HasDayOffOrMainShiftLayer();
	}
}