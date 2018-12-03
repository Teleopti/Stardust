using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration
{
	/// <summary>
	/// Represents the Workflow Control Set GUI.
	/// </summary>
	public interface IWorkflowControlSetView
	{
		/// <summary>
		/// Fills the workload control set combo.
		/// </summary>
		/// <param name="workflowControlSetModelCollection">The workflow control set model collection.</param>
		/// <param name="displayMember">The display member.</param>
		void FillWorkloadControlSetCombo(IEnumerable<IWorkflowControlSetModel> workflowControlSetModelCollection, string displayMember);

		/// <summary>
		/// Fills the allowed preference activity combo.
		/// </summary>
		/// <param name="activityCollection">The activity collection.</param>
		/// <param name="displayMember">The display member.</param>
		void FillAllowedPreferenceActivityCombo(IEnumerable<IActivity> activityCollection, string displayMember);

		/// <summary>
		/// Sets the name.
		/// </summary>
		/// <param name="name">The name.</param>
		void SetName(string name);

		/// <summary>
		/// Sets the updated info.
		/// </summary>
		/// <param name="text">The text.</param>
		void SetUpdatedInfo(string text);

		/// <summary>
		/// Selects the workflow control set in the drop down list.
		/// </summary>
		/// <param name="model">The model.</param>
		void SelectWorkflowControlSet(IWorkflowControlSetModel model);

		/// <summary>
		/// Initializes the view with common data from presenter (absences etc.)
		/// </summary>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2010-04-27
		/// </remarks>
		void InitializeView();

		/// <summary>
		/// Refreshes the open periods grid.
		/// </summary>
		/// <remarks>
		/// Created by: JonasN
		/// Created date: 2010-04-28
		/// </remarks>
		void RefreshOpenPeriodsGrid();

		/// <summary>
		/// Sets the open periods grid row count.
		/// </summary>
		/// <param name="rowCount">The row count.</param>
		void SetOpenPeriodsGridRowCount(int rowCount);

		/// <summary>
		/// Confirms the delete of request period.
		/// </summary>
		/// <returns></returns>
		bool ConfirmDeleteOfRequestPeriod();

		/// <summary>
		/// Gets the absence request period selected.
		/// </summary>
		/// <returns></returns>
		IList<AbsenceRequestPeriodModel> AbsenceRequestPeriodSelected { get; }

		/// <summary>
		/// Enables/disables the handling of absence request periods.
		/// </summary>
		/// <param name="enable">if set to <c>true</c> [enable].</param>
		void EnableHandlingOfAbsenceRequestPeriods(bool enable);

		/// <summary>
		/// Sets number of write protected days.
		/// </summary>
		/// <param name="writeProtection">The write protection.</param>
		void SetWriteProtectedDays(int? writeProtection);

		/// <summary>
		/// Sets the calendar culture info.
		/// </summary>
		/// <param name="cultureInfo">The culture info.</param>
		void SetCalendarCultureInfo(CultureInfo cultureInfo);

		/// <summary>
		/// Sets the preference periods.
		/// </summary>
		/// <param name="insertPeriod">The insert period.</param>
		/// <param name="preferencePeriod">The preference period.</param>
		void SetPreferencePeriods(DateOnlyPeriod insertPeriod, DateOnlyPeriod preferencePeriod);

		/// <summary>
		/// Sets the shift trade period days.
		/// </summary>
		/// <param name="periodDays">The period days.</param>
		void SetShiftTradePeriodDays(MinMax<int> periodDays);

		/// <summary>
		/// Loads the date only visualizer with new data.
		/// </summary>
		void LoadDateOnlyVisualizer();

		/// <summary>
		/// Sets the available matching skills
		/// </summary>
		/// <param name="selectedModel">The selected model.</param>
		void SetMatchingSkills(IWorkflowControlSetModel selectedModel);

		/// <summary>
		/// Sets the allowed day offs.
		/// </summary>
		/// <param name="selectedModel">The selected model.</param>
		void SetAllowedDayOffs(IWorkflowControlSetModel selectedModel);

		/// <summary>
		/// Sets the allowed shift categories.
		/// </summary>
		/// <param name="selectedModel">The selected model.</param>
		void SetAllowedShiftCategories(IWorkflowControlSetModel selectedModel);

		/// <summary>
		/// Sets the allowed absences.
		/// </summary>
		/// <param name="selectedModel">The selected model.</param>
		void SetAllowedAbsences(IWorkflowControlSetModel selectedModel);

		/// <summary>
		/// Sets the allowed absences for report.
		/// </summary>
		/// <param name="selectedModel">The selected model.</param>
		void SetAllowedAbsencesForReport(IWorkflowControlSetModel selectedModel);

		/// <summary>
		/// Sets the shift trade target time flexibility.
		/// </summary>
		/// <param name="flexibility">The flexibility.</param>
		void SetShiftTradeTargetTimeFlexibility(TimeSpan flexibility);

		/// <summary>
		/// Sets the auto grant.
		/// </summary>
		/// <param name="autoGrant">if set to <c>true</c> [auto grant].</param>
		void SetAutoGrant(bool autoGrant);

		/// <summary>
		/// Sets the anonymous trading.
		/// </summary>
		/// <param name="anonymousTrading">if set to <c>true</c> [anonymous trading].</param>
		void SetAnonymousTrading(bool anonymousTrading);

		void SetAbsenceRequestWaitlisting(bool absenceRequestWaitlistingEnabled, WaitlistProcessOrder processOrder);

		void SetAbsenceRequestCancellation(IWorkflowControlSetModel selectedModel);

		void SetAbsenceRequestExpiration(IWorkflowControlSetModel selectedModel);

		void SetAbsenceProbability(bool absenceProbabilityEnabled);

		/// <summary>
		/// Sets the lock trading.
		/// </summary>
		/// <param name="lockTrading">if set to <c>true</c> [lock trading].</param>
		void SetLockTrading(bool lockTrading);

		/// <summary>
		/// Disables all but add.
		/// </summary>
		void DisableAllButAdd();

		/// <summary>
		/// Enables all authorized functionality - this is the opposite of DisableAllButAdd.
		/// </summary>
		void EnableAllAuthorized();

		/// <summary>
		/// Set what fairness system should be used for scheduling
		/// </summary>
		/// <param name="value"></param>
		void SetFairnessType(FairnessType value);

		/// <summary>
		/// Sets the student availability periods.
		/// </summary>
		/// <param name="insertPeriod">The insert period.</param>
		/// <param name="studentAvailabilityPeriod">The student availability period.</param>
		void SetStudentAvailabilityPeriods(DateOnlyPeriod insertPeriod, DateOnlyPeriod studentAvailabilityPeriod);

		void SetOvertimeProbability(bool overtimeProbability);

		void RefreshOvertimeOpenPeriodsGrid();

		void SetOvertimeOpenPeriodsGridRowCount(int rowCount);
		void SetOverTimeRequestMaximumTimeHandleType(OvertimeRequestValidationHandleOptionView overtimeRequestValidationHandleOptionView);
		void SetOverTimeRequestStaffingCheckMethod(OvertimeRequestStaffingCheckMethodOptionView overtimeRequestStaffingCheckMethodOptionView);
		void SetOverTimeRequestMaximumTime(TimeSpan? selectedModelOvertimeRequestMaximumTime);
		void SetOvertimeRequestMaximumTimeEnabled(bool selectedModelOvertimeRequestMaximumTimeEnabled);
		void SetOverTimeRequestMaximumContinuousWorkTimeHandleType(OvertimeRequestValidationHandleOptionView selectedModelOvertimeRequestMaximumContinuousWorkTimeValidationHandleOptionView);
		void SetOverTimeRequestMaximumContinuousWorkTime(TimeSpan? selectedModelOvertimeRequestMaximumContinuousWorkTime);
		void SetOverTimeRequestMinimumRestTimeThreshold(TimeSpan? selectedModelOvertimeRequestMinimumRestTimeThreshold);
		void SetOvertimeRequestMaximumContinuousWorkTimeEnabled(bool selectedModelOvertimeRequestMaximumContinuousWorkTimeEnabled);
		void SetOvertimeRequestUsePrimarySkill(bool usePrimarySkill);
		void SetMaxConsecutiveWorkingDays(int maxConsecutiveWorkingDays);
	}
}