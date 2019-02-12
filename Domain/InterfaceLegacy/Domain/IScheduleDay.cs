using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{

	/// <summary>
	/// A mutable part of a schedule range
	/// </summary>
	/// <remarks>
	/// Created by: rogerkr
	/// Created date: 2008-08-25
	/// </remarks>
	public interface IScheduleDay : IProjectionSource, IOriginator<IScheduleDay>, ISchedule
	{
		/// <summary>
		/// Gets the date only as period.
		/// </summary>
		/// <value>The date only as period.</value>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2010-05-19
		/// </remarks>
		IDateOnlyAsDateTimePeriod DateOnlyAsPeriod { get; }


		/// <summary>
		/// What part of this schedule is most significant.
		/// Will only return proper value if working with schedule days, 
		/// otherwise Unknown is returned.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-09-03
		/// </remarks>
		SchedulePartView SignificantPart();

		/// <summary>
		/// Determines whether this instance is scheduled. IE if SignificantPart is a DayOff, ContractDayOff, MainShift or FullDayAbsence
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if this instance is scheduled; otherwise, <c>false</c>.
		/// </returns>
		bool IsScheduled();

		/// <summary>
		/// Gets a list of cloned person absences.
		/// </summary>
		/// <param name="includeOutsideActualDay"></param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2010-06-21
		/// </remarks>
		IPersonMeeting[] PersonMeetingCollection(bool includeOutsideActualDay);

		/// <summary>
		/// Gets a list of cloned person absences.
		/// </summary>
		/// <param name="includeOutsideActualDay"></param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2010-06-21
		/// </remarks>
		IPersonAbsence[] PersonAbsenceCollection(bool includeOutsideActualDay);

		/// <summary>
		/// Gets a list of cloned person absences.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-05-23
		/// </remarks>
		IPersonAbsence[] PersonAbsenceCollection();

		///<summary>
		/// What part of this schedule is most significant for display.
		/// Will only return proper value if working with schedule days, 
		/// otherwise Unknown is returned.
		///</summary>
		///<returns></returns>
		SchedulePartView SignificantPartForDisplay();

		IEditableShift GetEditorShift();

		#region Shouldn't be here!
		/// <summary>
		/// Delete full day absence
		/// </summary>
		/// <param name="source"></param>
		void DeleteFullDayAbsence(IScheduleDay source);


		/// <summary>
		/// Merge absences
		/// </summary>
		/// <param name="source"></param>
		/// <param name="all"></param>
		void MergeAbsences(IScheduleDay source, bool all);


		/// <summary>
		/// Delete absence
		/// </summary>
		void DeleteAbsence(bool all);

		/// <summary>
		/// Updates the SchedulePart from the source
		/// </summary>
		/// <param name="source"></param>
		/// <param name="isDelete"></param>
		void Merge(IScheduleDay source, bool isDelete, ITimeZoneGuard timeZoneGuard);

		/// <summary>
		/// Updates the SchedulePart from the source
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="isDelete">if set to <c>true</c> [delete].</param>
		/// <param name="ignoreTimeZoneChanges">if set to <c>true</c> calculate with time changes.</param>
		/// <param name="ignoreAssignmentPermission">if set to <c>true</c> when team leader has approve shift trade permission, ignore this modify assignment permission.</param>
		void Merge(IScheduleDay source, bool isDelete, bool ignoreTimeZoneChanges, ITimeZoneGuard timeZoneGuard, bool ignoreAssignmentPermission = false);

		#endregion

		/// <summary>
		/// Gets a value indicating whether [full access].
		/// </summary>
		/// <value><c>true</c> if [full access]; otherwise, <c>false</c>.</value>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-05-20
		/// </remarks>
		bool FullAccess { get; }

		/// <summary>
		/// Get a list of cloned personMeetings
		/// </summary>
		IPersonMeeting[] PersonMeetingCollection();

		/// <summary>
		/// Get a list of cloned overtime availabilities
		/// </summary>
		IOvertimeAvailability[] OvertimeAvailablityCollection();

		/// <summary>
		/// Gets a list of cloned person restrictions.
		/// 
		/// DONT USE THIS ANYMORE. WILL BE DELETED! Use RestrictionCollection instead!
		/// 
		/// Please give an example how to use RestrictionCollection when trying to 
		/// extract all IPreferenceDay from this collection please. 
		/// 
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-12-10
		/// </remarks>
		IScheduleData[] PersonRestrictionCollection();

		/// <summary>
		/// Gets a list of restrictions
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2009-10-01
		/// </remarks>
		IEnumerable<IRestrictionBase> RestrictionCollection();

		/// <summary>
		/// Gets a list of notes.
		/// </summary>
		/// <returns></returns>
		INote[] NoteCollection();


		/// <summary>
		/// Gets a list of public notes.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: HenryG and JonasN
		/// Created date: 2010-12-02
		/// </remarks>
		IPublicNote[] PublicNoteCollection();

		/// <summary>
		/// Gets the business rule response collection.
		/// </summary>
		/// <value>The business rule response collection.</value>
		/// /// 
		/// <remarks>
		///  Created by: Ola
		///  Created date: 2008-08-22    
		/// /// </remarks>
		IList<IBusinessRuleResponse> BusinessRuleResponseCollection { get; }

		/// <summary>
		/// Gets a value indicating whether this schedule is published.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is published; otherwise, <c>false</c>.
		/// </value>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-10-08
		/// </remarks>
		bool IsFullyPublished { get; }

		/// <summary>
		/// Gets the schedule data collection.
		/// </summary>
		/// <value>The schedule data collection.</value>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2009-03-10
		/// </remarks>
		IEnumerable<IPersistableScheduleData> PersistableScheduleDataCollection();


		/// <summary>
		/// Clears this instance from a specific type of schedule data.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-09-15
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")] //rk fixar snart
		void Clear<T>() where T : IScheduleData;

		/// <summary>
		/// Adds the specified schedule data.
		/// </summary>
		/// <param name="scheduleData">The schedule data.</param>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-09-15
		/// </remarks>
		void Add(IScheduleData scheduleData);

		/// <summary>
		/// Removes the specified schedule data.
		/// </summary>
		/// <param name="scheduleData">The schedule data.</param>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-09-15
		/// </remarks>
		void Remove(IScheduleData scheduleData);

		/// <summary>
		/// Gets the time zone.
		/// </summary>
		/// <value>The time zone.</value>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2009-03-19
		/// </remarks>
		TimeZoneInfo TimeZone { get; }

		/// <summary>
		/// Returns the ScheduleTag or its null representation NullScheduleTag.
		/// </summary>
		/// <returns></returns>
		IScheduleTag ScheduleTag();


		/// <summary>
		/// Creates and adds a dayoff
		/// </summary>
		/// <param name="dayOff"></param>
		void CreateAndAddDayOff(IDayOffTemplate dayOff);

		IPersonAbsence CreateAndAddAbsence(IAbsenceLayer layer);

		void CreateAndAddActivity(IActivity activity, DateTimePeriod period, IShiftCategory shiftCategory);
		IScheduleDay CreateAndAddActivity(IActivity activity, DateTimePeriod period);

		/// <summary>
		/// Creates and adds personal activity
		/// </summary>
		void CreateAndAddPersonalActivity(IActivity activity, DateTimePeriod period, bool muteEvent = true, TrackedCommandInfo trackedCommandInfo = null);

		/// <summary>
		/// Creates the and add note.
		/// </summary>
		/// <param name="text">The text.</param>
		void CreateAndAddNote(string text);

		/// <summary>
		/// Creates the and add public note.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <remarks>
		/// Created by: HenryG and JonasN
		/// Created date: 2010-12-02
		/// </remarks>
		void CreateAndAddPublicNote(string text);

		/// <summary>
		/// Creates and adds overtime.
		/// </summary>
		void CreateAndAddOvertime(IActivity activity, DateTimePeriod period, IMultiplicatorDefinitionSet definitionSet, bool muteEvent = false, TrackedCommandInfo trackedCommandInfo = null);

		/// <summary>
		/// Adds or replaces the mainShift in the PersonAssignment returned by AssignmenHighZOrder.
		/// If no PersonAssignment exists a new will be created.
		/// </summary>
		/// <param name="mainShift">The main shift.</param>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-10-28
		/// </remarks>
		void AddMainShift(IEditableShift mainShift);

		void DeleteMainShift();
		void DeleteMainShiftSpecial();

		/// <summary>
		/// Delete personal shift
		/// </summary>
		void DeletePersonalStuff();
		/// <summary>
		/// Delete day off
		/// </summary>
		void DeleteDayOff(TrackedCommandInfo trackedCommandInfo = null);

		/// <summary>
		/// Delete overtime
		/// </summary>
		void DeleteOvertime();

		/// <summary>
		/// Delete preference restriction
		/// </summary>
		void DeletePreferenceRestriction();

		/// <summary>
		/// Delete student availability restriction
		/// </summary>
		void DeleteStudentAvailabilityRestriction();

		/// <summary>
		/// Delete overtime availability
		/// </summary>
		void DeleteOvertimeAvailability();

		/// <summary>
		/// Deletes a schedulenote.
		/// </summary>
		void DeleteNote();

		/// <summary>
		/// Deletes the public note.
		/// </summary>
		/// <remarks>
		/// Created by: HenryG and JonasN
		/// Created date: 2010-12-02
		/// </remarks>
		void DeletePublicNote();

		bool HasDayOff();

		///<summary>
		/// Returns the person assignment to use for view if more than one assignment 
		/// can not be viewed at the time.
		///</summary>
		IPersonAssignment PersonAssignment(bool createIfNotExists = false);

		IScheduleDay ReFetch();
		void AddMainShift(IPersonAssignment mainShiftSource);
		bool HasProjection();

		IPreferenceDay PreferenceDay();

		DateTimePeriod AbsenceSplitPeriod(IScheduleDay scheduleDay);
		void AdjustFullDayAbsenceNextDay(IScheduleDay destination);
	}

}
