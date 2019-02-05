using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.WorkflowControl;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public enum WaitlistProcessOrder
	{
		FirstComeFirstServed = 0,
		BySeniority = 1
	}

	/// <summary>
	/// Holder of Workflow Control Set information.
	/// </summary>
	/// <remarks>
	/// Created by: HenryG
	/// Created date: 2010-04-15
	/// </remarks>
	public interface IWorkflowControlSet : IAggregateRoot,
		IChangeInfo,
		IFilterOnBusinessUnit,
		ICloneableEntity<IWorkflowControlSet>
	{
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2010-04-22
		/// </remarks>
		string Name { get; set; }

		/// <summary>
		/// Gets or sets the allowed preference activity.
		/// This activity can be used for extended preferences
		/// </summary>
		/// <value>The allowed preference activity.</value>
		IActivity AllowedPreferenceActivity { get; set; }

		/// <summary>
		/// Gets the open absence request periods.
		/// </summary>
		/// <value>The open absence request periods.</value>
		/// <remarks>
		/// Created by: HenryG
		/// Created date: 2010-04-15
		/// </remarks>
		ReadOnlyCollection<IAbsenceRequestOpenPeriod> AbsenceRequestOpenPeriods { get; }

		/// <summary>
		/// Adds the open absence request period.
		/// </summary>
		/// <param name="absenceRequestOpenPeriod">The open absence request period.</param>
		/// <remarks>
		/// Created by: HenryG
		/// Created date: 2010-04-15
		/// </remarks>
		void AddOpenAbsenceRequestPeriod(IAbsenceRequestOpenPeriod absenceRequestOpenPeriod);
		void AddOpenOvertimeRequestPeriod(IOvertimeRequestOpenPeriod overtimeRequestOpenPeriod);

		/// <summary>
		/// Gets the extractor for absence.
		/// </summary>
		/// <param name="absence">The absence.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: henryg
		/// Created date: 2010-04-22
		/// </remarks>
		IOpenAbsenceRequestPeriodExtractor GetExtractorForAbsence(IAbsence absence);

		/// <summary>
		/// Moves the period down.
		/// </summary>
		/// <param name="absenceRequestOpenPeriod">The absence request open period.</param>
		/// <remarks>
		/// Created by: henryg
		/// Created date: 2010-04-22
		/// </remarks>
		void MovePeriodDown(IAbsenceRequestOpenPeriod absenceRequestOpenPeriod);

		/// <summary>
		/// Moves the period up.
		/// </summary>
		/// <param name="absenceRequestOpenPeriod">The absence request open period.</param>
		/// <remarks>
		/// Created by: henryg
		/// Created date: 2010-04-22
		/// </remarks>
		void MovePeriodUp(IAbsenceRequestOpenPeriod absenceRequestOpenPeriod);

		void MovePeriodDown(IOvertimeRequestOpenPeriod overtimeRequestOpenPeriod);
		void MovePeriodUp(IOvertimeRequestOpenPeriod overtimeRequestOpenPeriod);

		/// <summary>
		/// Removes the open absence request period.
		/// </summary>
		/// <param name="absenceRequestOpenPeriod">The absence request open period.</param>
		/// <returns>Order index of the period before removal.</returns>
		/// <remarks>
		/// Created by: henryg
		/// Created date: 2010-04-22
		/// </remarks>
		int RemoveOpenAbsenceRequestPeriod(IAbsenceRequestOpenPeriod absenceRequestOpenPeriod);
		int RemoveOpenOvertimeRequestPeriod(IOvertimeRequestOpenPeriod overtimeRequestOpenPeriod);

		/// <summary>
		/// Inserts the period.
		/// </summary>
		/// <param name="absenceRequestOpenPeriod">The absence request open period.</param>
		/// <param name="orderIndex">Index of the order.</param>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2010-04-27
		/// </remarks>
		void InsertPeriod(IAbsenceRequestOpenPeriod absenceRequestOpenPeriod, int orderIndex);

		/// <summary>
		/// Gets or sets the schedule published to date.
		/// </summary>
		/// <value>The schedule published to date.</value>
		/// /// 
		/// <remarks>
		///  Created by: Ola
		///  Created date: 2008-09-29    
		/// /// </remarks>
		DateTime? SchedulePublishedToDate { get; set; }


		///<summary>
		/// Gets or sets the preference period.
		/// It is a period THAT the agent may edit to input, modify its preferences for the a later scheduling process.
		///</summary>
		DateOnlyPeriod PreferencePeriod { get; set; }

		///<summary>
		/// Gets or sets the preference input period.
		/// It is the calendar period WHEN the user is allowed to set its preferences for the PreferencePeriod.
		/// This period is then compared to today's date to determine if the agent is allowed to provide student
		/// availability data.
		///</summary>
		DateOnlyPeriod PreferenceInputPeriod { get; set; }

		///<summary>
		/// Gets or sets the student availability period.
		/// This is the period THAT the agent may edit to input its availability data for the a later scheduling process.
		///</summary>
		DateOnlyPeriod StudentAvailabilityPeriod { get; set; }

		///<summary>
		/// Gets or sets the student availability input period.
		/// It is the calendar period WHEN the user is allowed to set its availabilities for the StudentAvailabilityPeriod.
		/// This period is then compared to today's date to determine if the agent is allowed to provide student
		/// availability data.
		/// Also refered to as the "open" period, or "Is Open".
		///</summary>
		DateOnlyPeriod StudentAvailabilityInputPeriod { get; set; }

		/// <summary>
		/// Gets or sets the write protection.
		/// </summary>
		/// <value>The write protection.</value>
		/// /// 
		/// <remarks>
		///  Created by: Ola
		///  Created date: 2009-03-02    
		/// /// </remarks>
		int? WriteProtection { get; set; }

		/// <summary>
		/// Gets or sets the shift trade target time flexibilty.
		/// </summary>
		/// <value>The shift trade target time flexibilty.</value>
		TimeSpan ShiftTradeTargetTimeFlexibility { get; set; }

		/// <summary>
		/// Gets the skills that must match when swaping shift in shift trade.
		/// </summary>
		/// <value>Thes kills.</value>
		IEnumerable<ISkill> MustMatchSkills { get; }

		/// <summary>
		/// Adds the skill to match list.
		/// </summary>
		/// <param name="skill">The skill.</param>
		void AddSkillToMatchList(ISkill skill);

		/// <summary>
		/// Removes the skill from match list.
		/// </summary>
		/// <param name="skill">The skill.</param>
		void RemoveSkillFromMatchList(ISkill skill);

		/// <summary>
		/// Gets or sets the days forward where the Shift Trade Period is open.
		/// </summary>
		/// <value>The shift trade open period days forward.</value>
		MinMax<int> ShiftTradeOpenPeriodDaysForward { get; set; }

		/// <summary>
		/// Add allowed shiftcategory for preferences
		/// </summary>
		/// <param name="shiftCategory"></param>
		void AddAllowedPreferenceShiftCategory(IShiftCategory shiftCategory);

		/// <summary>
		/// Remove shiftcategory from allowed for preferences
		/// </summary>
		/// <param name="shiftCategory"></param>
		void RemoveAllowedPreferenceShiftCategory(IShiftCategory shiftCategory);

		/// <summary>
		/// Get shiftcategories allowed for preferences
		/// </summary>
		IEnumerable<IShiftCategory> AllowedPreferenceShiftCategories { get; }

		/// <summary>
		/// Add allowed dayoff for preferences
		/// </summary>
		/// <param name="dayOff"></param>
		void AddAllowedPreferenceDayOff(IDayOffTemplate dayOff);

		/// <summary>
		/// Remove dayoff from allowed for preferences
		/// </summary>
		/// <param name="dayOff"></param>
		void RemoveAllowedPreferenceDayOff(IDayOffTemplate dayOff);

		/// <summary>
		/// Get dayoffs allowed for preferences
		/// </summary>
		IEnumerable<IDayOffTemplate> AllowedPreferenceDayOffs { get; }

		/// <summary>
		/// Gets or sets a value indicating whether [auto grant] is enabled.
		/// </summary>
		/// <value><c>true</c> if [auto grant]; otherwise, <c>false</c>.</value>
		bool AutoGrantShiftTradeRequest { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether [Anonymous Trading] is enabled.
		/// </summary>
		/// <value><c>true</c> if [Anonymous Trading]; otherwise, <c>false</c>.</value>
		bool AnonymousTrading { get; set; }

		/// <summary>
		/// Get absences allowed for preferences
		/// </summary>
		IEnumerable<IAbsence> AllowedPreferenceAbsences { get; }

		/// <summary>
		/// Adds an absence.
		/// </summary>
		/// <param name="absence">The absence.</param>
		void AddAllowedPreferenceAbsence(IAbsence absence);

		/// <summary>
		/// Removes the allowed absence.
		/// </summary>
		/// <param name="absence">The absence.</param>
		void RemoveAllowedPreferenceAbsence(IAbsence absence);

		/// <summary>
		/// Get absences allowed for report
		/// </summary>
		IEnumerable<IAbsence> AllowedAbsencesForReport { get; }

		/// <summary>
		/// Gets or sets a value indicating whether [Lock Trading] is enabled.
		/// </summary>
		/// <value><c>true</c> if [Lock Trading]; otherwise, <c>false</c>.</value>
		bool LockTrading { get; set; }

		bool AbsenceRequestWaitlistEnabled { get; set; }

		WaitlistProcessOrder AbsenceRequestWaitlistProcessOrder { get; set; }

		int? AbsenceRequestCancellationThreshold { get; set; }

		int? AbsenceRequestExpiredThreshold { get; set; }

		bool AbsenceProbabilityEnabled { get; set; }

		/// <summary>
		/// Adds an absence allowed for report.
		/// </summary>
		/// <param name="absence">The absence.</param>
		void AddAllowedAbsenceForReport(IAbsence absence);

		/// <summary>
		/// Removes the allowed absence for report.
		/// </summary>
		/// <param name="absence">The absence.</param>
		void RemoveAllowedAbsenceForReport(IAbsence absence);

		FairnessType GetFairnessType();

		void SetFairnessType(FairnessType fairnessType);

		bool WaitlistingIsEnabled(IAbsenceRequest absenceRequest);

		IAbsenceRequestOpenPeriod GetMergedAbsenceRequestOpenPeriod(IAbsenceRequest absenceRequest);

		bool IsAbsenceRequestValidatorEnabled<T>(TimeZoneInfo timeZone) where T : IAbsenceRequestValidator;

		bool IsAbsenceRequestCheckStaffingByIntradayWithShrinkage(DateOnly today,DateOnly date);

		bool IsAbsenceRequestCheckStaffingByIntraday(DateOnly today, DateOnly date);

		bool OvertimeProbabilityEnabled { get; set; }

		TimeSpan? OvertimeRequestMaximumTime { get; set; }

		OvertimeValidationHandleType? OvertimeRequestMaximumTimeHandleType { get; set; }

		ReadOnlyCollection<IOvertimeRequestOpenPeriod> OvertimeRequestOpenPeriods { get; }
		bool OvertimeRequestMaximumTimeEnabled { get; set; }
		bool OvertimeRequestMaximumContinuousWorkTimeEnabled { get; set; }
		OvertimeValidationHandleType? OvertimeRequestMaximumContinuousWorkTimeHandleType { get; set; }
		TimeSpan? OvertimeRequestMaximumContinuousWorkTime { get; set; }
		TimeSpan? OvertimeRequestMinimumRestTimeThreshold { get; set; }
		OvertimeRequestStaffingCheckMethod OvertimeRequestStaffingCheckMethod { get; set; }
		bool OvertimeRequestUsePrimarySkill { get; set; }
		int MaximumConsecutiveWorkingDays { get; set; }
		void InsertOvertimePeriod(IOvertimeRequestOpenPeriod newOvertimeRequestOpenPeriod, int currentIndex);
	}
}