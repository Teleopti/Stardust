using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration
{
	public interface IWorkflowControlSetModel
	{
		Guid? Id { get; }
		string Name { get; set; }
		TimeSpan ShiftTradeTargetTimeFlexibility { get; set; }
		int? WriteProtection { get; set; }
		IActivity AllowedPreferenceActivity { get; set; }
		string UpdatedInfo { get; }
		IWorkflowControlSet DomainEntity { get; }
		bool ToBeDeleted { get; set; }
		bool IsNew { get; }
		int MaxConsecutiveWorkingDays { get; set; }
		IWorkflowControlSet OriginalDomainEntity { get; }
		IList<AbsenceRequestPeriodModel> AbsenceRequestPeriodModels { get; }
		IList<OvertimeRequestPeriodModel> OvertimeRequestPeriodModels { get; }
		DateTime? SchedulePublishedToDate { get; set; }
		DateOnlyPeriod PreferenceInputPeriod { get; set; }
		DateOnlyPeriod PreferencePeriod { get; set; }
		DateOnlyPeriod StudentAvailabilityInputPeriod { get; set; }
		DateOnlyPeriod StudentAvailabilityPeriod { get; set; }
		MinMax<int> ShiftTradeOpenPeriodDays { get; set; }
		IEnumerable<IDayOffTemplate> AllowedPreferenceDayOffs { get; }
		IEnumerable<IShiftCategory> AllowedPreferenceShiftCategories { get; }
		IEnumerable<IAbsence> AllowedPreferenceAbsences { get; }
		IEnumerable<IAbsence> AllowedAbsencesForReport { get; }
		IEnumerable<ISkill> MustMatchSkills { get; }
		bool AutoGrantShiftTradeRequest { get; set; }
		TimeSpan? OvertimeRequestMaximumTime { get; set; }
		OvertimeRequestValidationHandleOptionView OvertimeRequestMaximumOvertimeValidationHandleOptionView { get; set; }

		void AddAllowedPreferenceDayOff(IDayOffTemplate dayOff);
		void RemoveAllowedPreferenceDayOff(IDayOffTemplate dayOff);
		void AddAllowedPreferenceShiftCategory(IShiftCategory shiftCategory);
		void RemoveAllowedPreferenceShiftCategory(IShiftCategory shiftCategory);
		void AddAllowedPreferenceAbsence(IAbsence absence);
		void RemoveAllowedPreferenceAbsence(IAbsence absence);
		void AddAllowedAbsenceForReport(IAbsence absence);
		void RemoveAllowedAbsenceForReport(IAbsence absence);
		void UpdateAfterMerge(IWorkflowControlSet updatedWorkflowControlSet);
		void AddSkillToMatchList(ISkill skill);
		void RemoveSkillFromMatchList(ISkill skill);
		FairnessType GetFairnessType();
		void SetFairnessType(FairnessType fairnessType);

		bool IsDirty { get; set; }
		bool AnonymousTrading { get; set; }
		bool LockTrading { get; set; }
		bool AbsenceRequestWaitlistingEnabled { get; set; }
		WaitlistProcessOrder AbsenceRequestWaitlistingProcessOrder { get; set; }
		int? AbsenceRequestCancellationThreshold { get; set; }
		int? AbsenceRequestExpiredThreshold { get; set; }

		bool AbsenceProbabilityEnabled { get; set; }

		bool IsOvertimeProbabilityEnabled { get; set; }

		bool OvertimeRequestMaximumTimeEnabled { get; set; }
		bool OvertimeRequestMaximumContinuousWorkTimeEnabled { get; set; }
		TimeSpan? OvertimeRequestMinimumRestTimeThreshold { get; set; }
		TimeSpan? OvertimeRequestMaximumContinuousWorkTime { get; set; }
		OvertimeRequestValidationHandleOptionView OvertimeRequestMaximumContinuousWorkTimeValidationHandleOptionView { get; set; }
		OvertimeRequestStaffingCheckMethodOptionView OvertimeRequestStaffingCheckMethodOptionView { get; set; }
		bool OvertimeRequestUsePrimarySkill { get; set; }
	}
}