using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings;
using Teleopti.Ccc.UserTexts;

using HandleOptionViewDictionary = System.Collections.Generic.Dictionary<Teleopti.Ccc.Domain.WorkflowControl.OvertimeValidationHandleType, Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings.OvertimeRequestValidationHandleOptionView>;
using StaffingCheckMethodOptionViewDictionary = System.Collections.Generic.Dictionary<Teleopti.Ccc.Domain.InterfaceLegacy.Domain.OvertimeRequestStaffingCheckMethod, Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings.OvertimeRequestStaffingCheckMethodOptionView>;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration
{
	public class WorkflowControlSetModel : IWorkflowControlSetModel
	{
		private static IList<AbsenceRequestPeriodTypeModel> _defaultAbsenceRequestPeriodAdapters;
		private static IList<OvertimeRequestPeriodTypeModel> _defaultOvertimeRequestPeriodAdapters;
		private static IList<OvertimeRequestPeriodSkillTypeModel> _defaultOvertimeRequestSkillTypeAdapters;
		private readonly List<AbsenceRequestPeriodModel> _absenceRequestPeriodModels = new List<AbsenceRequestPeriodModel>();
		private readonly List<OvertimeRequestPeriodModel> _overtimeRequestPeriodModels = new List<OvertimeRequestPeriodModel>();
		private static IList<ISkillType> _supportedSkillTypes = new List<ISkillType>();

		internal static readonly HandleOptionViewDictionary
			OvertimeRequestWorkRuleValidationHandleOptionViews
				= new HandleOptionViewDictionary
				{
					{
						OvertimeValidationHandleType.Pending,
						new OvertimeRequestValidationHandleOptionView(OvertimeValidationHandleType.Pending,
							Resources.SendToAdministrator)
					},
					{
						OvertimeValidationHandleType.Deny,
						new OvertimeRequestValidationHandleOptionView(OvertimeValidationHandleType.Deny, Resources.Deny)
					}
				};

		internal static readonly StaffingCheckMethodOptionViewDictionary
			OvertimeRequestStaffingCheckMethodOptionViews
				= new StaffingCheckMethodOptionViewDictionary
				{
					{
						OvertimeRequestStaffingCheckMethod.Intraday,
						new OvertimeRequestStaffingCheckMethodOptionView(OvertimeRequestStaffingCheckMethod.Intraday,
							Resources.Intraday)
					},
					{
						OvertimeRequestStaffingCheckMethod.IntradayWithShrinkage,
						new OvertimeRequestStaffingCheckMethodOptionView(OvertimeRequestStaffingCheckMethod.IntradayWithShrinkage, Resources.IntradayWithShrinkage)
					}
				};

		public WorkflowControlSetModel(IWorkflowControlSet domainEntity)
			: this(domainEntity, domainEntity.EntityClone()) { }

		public WorkflowControlSetModel(IWorkflowControlSet originalDomainEntity, IWorkflowControlSet clonedDomainEntity)
		{
			OriginalDomainEntity = originalDomainEntity;
			DomainEntity = clonedDomainEntity;
		}

		public Guid? Id => DomainEntity.Id;

		public string Name
		{
			get => DomainEntity.Name;
			set
			{
				if (DomainEntity.Name == value) return;
				DomainEntity.Name = value;
				IsDirty = true;
			}
		}

		public TimeSpan ShiftTradeTargetTimeFlexibility
		{
			get => DomainEntity.ShiftTradeTargetTimeFlexibility;
			set
			{
				if (DomainEntity.ShiftTradeTargetTimeFlexibility == value) return;
				DomainEntity.ShiftTradeTargetTimeFlexibility = value;
				IsDirty = true;
			}
		}

		public int? WriteProtection
		{
			get => DomainEntity.WriteProtection;
			set
			{
				if (DomainEntity.WriteProtection == value) return;
				DomainEntity.WriteProtection = value;
				IsDirty = true;
			}
		}

		public IActivity AllowedPreferenceActivity
		{
			get => DomainEntity.AllowedPreferenceActivity;
			set
			{
				if (DomainEntity.AllowedPreferenceActivity == value) return;
				DomainEntity.AllowedPreferenceActivity = value;
				IsDirty = true;
			}
		}

		public string UpdatedInfo
		{
			get
			{
				LocalizedUpdateInfo localizer = new LocalizedUpdateInfo();
				string changed = localizer.UpdatedByText(DomainEntity, Resources.UpdatedByColon);

				return changed;
			}
		}

		public IWorkflowControlSet DomainEntity { get; private set; }

		public bool ToBeDeleted { get; set; }

		public bool IsNew => !Id.HasValue;

		public IWorkflowControlSet OriginalDomainEntity { get; private set; }

		public static IList<AbsenceRequestPeriodTypeModel> DefaultAbsenceRequestPeriodAdapters
		{
			get
			{
				setDefaultAbsencePeriodIfMissing();
				return _defaultAbsenceRequestPeriodAdapters;
			}
		}

		public static IList<OvertimeRequestPeriodTypeModel> DefaultOvertimeRequestPeriodAdapters
		{
			get
			{
				setDefaultOvertimePeriodIfMissing();
				return _defaultOvertimeRequestPeriodAdapters;
			}
		}

		public static IList<OvertimeRequestPeriodSkillTypeModel> DefaultOvertimeRequestSkillTypeAdapters
		{
			get
			{
				setDefaultOvertimePeriodIfMissing();
				initialOvertimeRequestSkillTypeAdapters();
				return _defaultOvertimeRequestSkillTypeAdapters;
			}
		}

		public static void SetSupportedSkillTypes(IList<ISkillType> skillTypes)
		{
			_supportedSkillTypes = skillTypes;
		}

		public static IList<ISkillType> GetSupportedSkillTypes()
		{
			return _supportedSkillTypes;
		}

		public IList<AbsenceRequestPeriodModel> AbsenceRequestPeriodModels
		{
			get
			{
				_absenceRequestPeriodModels.Clear();
				_absenceRequestPeriodModels.AddRange(DomainEntity.AbsenceRequestOpenPeriods.Select(a => new AbsenceRequestPeriodModel(a, this)));
				return _absenceRequestPeriodModels;
			}
		}

		public IList<OvertimeRequestPeriodModel> OvertimeRequestPeriodModels
		{
			get
			{
				_overtimeRequestPeriodModels.Clear();
				_overtimeRequestPeriodModels.AddRange(DomainEntity.OvertimeRequestOpenPeriods.Select(a => new OvertimeRequestPeriodModel(a, this)));
				return _overtimeRequestPeriodModels;
			}
		}

		public DateTime? SchedulePublishedToDate
		{
			get => DomainEntity.SchedulePublishedToDate;
			set
			{
				if (DomainEntity.SchedulePublishedToDate == value) return;
				DomainEntity.SchedulePublishedToDate = value;
				IsDirty = true;
			}
		}

		public DateOnlyPeriod PreferenceInputPeriod
		{
			get => DomainEntity.PreferenceInputPeriod;
			set
			{
				if (DomainEntity.PreferenceInputPeriod == value) return;
				DomainEntity.PreferenceInputPeriod = value;
				IsDirty = true;
			}
		}

		public DateOnlyPeriod PreferencePeriod
		{
			get => DomainEntity.PreferencePeriod;
			set
			{
				if (DomainEntity.PreferencePeriod == value) return;
				DomainEntity.PreferencePeriod = value;
				IsDirty = true;
			}
		}

		public DateOnlyPeriod StudentAvailabilityInputPeriod
		{
			get => DomainEntity.StudentAvailabilityInputPeriod;
			set
			{
				if (DomainEntity.StudentAvailabilityInputPeriod == value) return;
				DomainEntity.StudentAvailabilityInputPeriod = value;
				IsDirty = true;
			}
		}

		public DateOnlyPeriod StudentAvailabilityPeriod
		{
			get => DomainEntity.StudentAvailabilityPeriod;
			set
			{
				if (DomainEntity.StudentAvailabilityPeriod == value) return;
				DomainEntity.StudentAvailabilityPeriod = value;
				IsDirty = true;
			}
		}

		public MinMax<int> ShiftTradeOpenPeriodDays
		{
			get => DomainEntity.ShiftTradeOpenPeriodDaysForward;
			set
			{
				if (DomainEntity.ShiftTradeOpenPeriodDaysForward == value) return;
				DomainEntity.ShiftTradeOpenPeriodDaysForward = value;
				IsDirty = true;
			}
		}

		public IEnumerable<IDayOffTemplate> AllowedPreferenceDayOffs => DomainEntity.AllowedPreferenceDayOffs;

		public IEnumerable<IShiftCategory> AllowedPreferenceShiftCategories => DomainEntity.AllowedPreferenceShiftCategories;

		public IEnumerable<IAbsence> AllowedPreferenceAbsences => DomainEntity.AllowedPreferenceAbsences;

		public IEnumerable<IAbsence> AllowedAbsencesForReport => DomainEntity.AllowedAbsencesForReport;

		public IEnumerable<ISkill> MustMatchSkills => DomainEntity.MustMatchSkills;

		public OvertimeRequestValidationHandleOptionView OvertimeRequestMaximumOvertimeValidationHandleOptionView
		{
			get => DomainEntity.OvertimeRequestMaximumTimeHandleType != null ? OvertimeRequestWorkRuleValidationHandleOptionViews[DomainEntity.OvertimeRequestMaximumTimeHandleType.Value] : null;
			set
			{
				if (DomainEntity.OvertimeRequestMaximumTimeHandleType == value?.WorkRuleValidationHandleType) return;
				DomainEntity.OvertimeRequestMaximumTimeHandleType = value?.WorkRuleValidationHandleType;
				IsDirty = true;
			}
		}

		public virtual void AddAllowedPreferenceDayOff(IDayOffTemplate dayOff)
		{
			DomainEntity.AddAllowedPreferenceDayOff(dayOff);
			IsDirty = true;
		}

		public virtual void RemoveAllowedPreferenceDayOff(IDayOffTemplate dayOff)
		{
			DomainEntity.RemoveAllowedPreferenceDayOff(dayOff);
			IsDirty = true;
		}

		public virtual void AddAllowedPreferenceShiftCategory(IShiftCategory shiftCategory)
		{
			DomainEntity.AddAllowedPreferenceShiftCategory(shiftCategory);
			IsDirty = true;
		}

		public virtual void AddAllowedPreferenceAbsence(IAbsence absence)
		{
			DomainEntity.AddAllowedPreferenceAbsence(absence);
			IsDirty = true;
		}

		public virtual void RemoveAllowedPreferenceAbsence(IAbsence absence)
		{
			DomainEntity.RemoveAllowedPreferenceAbsence(absence);
			IsDirty = true;
		}

		public virtual void AddAllowedAbsenceForReport(IAbsence absence)
		{
			DomainEntity.AddAllowedAbsenceForReport(absence);
			IsDirty = true;
		}

		public virtual void RemoveAllowedAbsenceForReport(IAbsence absence)
		{
			DomainEntity.RemoveAllowedAbsenceForReport(absence);
			IsDirty = true;
		}

		public virtual void RemoveAllowedPreferenceShiftCategory(IShiftCategory shiftCategory)
		{
			DomainEntity.RemoveAllowedPreferenceShiftCategory(shiftCategory);
			IsDirty = true;
		}

		private static void setDefaultAbsencePeriodIfMissing()
		{
			if (_defaultAbsenceRequestPeriodAdapters != null) return;

			var period = getCurrentMonthPeriod(new DateOnly(DateOnly.Today.Date.AddMonths(1)));
			var openPeriod = getCurrentMonthPeriod(DateOnly.Today);

			var openDatePeriod = new AbsenceRequestOpenDatePeriod
			{
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				AbsenceRequestProcess = new PendingAbsenceRequest(),
				Period = period,
				OpenForRequestsPeriod = openPeriod
			};

			var openRollingPeriod = new AbsenceRequestOpenRollingPeriod
			{
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				AbsenceRequestProcess = new PendingAbsenceRequest(),
				BetweenDays = new MinMax<int>(2, 15),
				OpenForRequestsPeriod = openPeriod
			};

			_defaultAbsenceRequestPeriodAdapters = new List<AbsenceRequestPeriodTypeModel>
			{
				new AbsenceRequestPeriodTypeModel(openDatePeriod, Resources.FromTo),
				new AbsenceRequestPeriodTypeModel(openRollingPeriod, Resources.Rolling)
			};
		}

		private static void setDefaultOvertimePeriodIfMissing()
		{
			if (_defaultOvertimeRequestPeriodAdapters != null) return;

			IOvertimeRequestOpenPeriod overtimeOpenDatePeriod = new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				Period = getCurrentMonthPeriod(new DateOnly(DateOnly.Today.Date.AddMonths(1)))
			};

			IOvertimeRequestOpenPeriod overtimeOpenRollingPeriod = new OvertimeRequestOpenRollingPeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				BetweenDays = new MinMax<int>(2, 15)
			};

			_defaultOvertimeRequestPeriodAdapters = new List<OvertimeRequestPeriodTypeModel>
			{
				new OvertimeRequestPeriodTypeModel(overtimeOpenDatePeriod, Resources.FromTo),
				new OvertimeRequestPeriodTypeModel(overtimeOpenRollingPeriod, Resources.Rolling)
			};
		}

		private static void initialOvertimeRequestSkillTypeAdapters()
		{
			if (_defaultOvertimeRequestSkillTypeAdapters != null && _defaultOvertimeRequestSkillTypeAdapters.Count > 0) return;
			_defaultOvertimeRequestSkillTypeAdapters = _supportedSkillTypes
				.Select(a => new OvertimeRequestPeriodSkillTypeModel(a, Resources.ResourceManager.GetString(a.Description.Name)))
				.ToList();
		}

		private static DateOnlyPeriod getCurrentMonthPeriod(DateOnly dateViewPoint)
		{
			var startDateOnly = new DateOnly(dateViewPoint.Year, dateViewPoint.Month, 1);
			var endDateOnly = new DateOnly(dateViewPoint.Year, dateViewPoint.Month, startDateOnly.Date.AddMonths(1).AddDays(-1).Day);

			return new DateOnlyPeriod(startDateOnly, endDateOnly);
		}

		public void UpdateAfterMerge(IWorkflowControlSet updatedWorkflowControlSet)
		{
			OriginalDomainEntity = updatedWorkflowControlSet;
			DomainEntity = OriginalDomainEntity.EntityClone();
			IsDirty = false;
		}

		public void AddSkillToMatchList(ISkill skill)
		{
			DomainEntity.AddSkillToMatchList(skill);
			IsDirty = true;
		}

		public void RemoveSkillFromMatchList(ISkill skill)
		{
			DomainEntity.RemoveSkillFromMatchList(skill);
			IsDirty = true;
		}

		public bool AutoGrantShiftTradeRequest
		{
			get => DomainEntity.AutoGrantShiftTradeRequest;
			set
			{
				if (DomainEntity.AutoGrantShiftTradeRequest == value) return;
				DomainEntity.AutoGrantShiftTradeRequest = value;
				IsDirty = true;
			}
		}

		public TimeSpan? OvertimeRequestMaximumTime
		{
			get => DomainEntity.OvertimeRequestMaximumTime;
			set
			{
				if (DomainEntity.OvertimeRequestMaximumTime == value) return;
				DomainEntity.OvertimeRequestMaximumTime = value;
				IsDirty = true;
			}
		}

		public FairnessType GetFairnessType()
		{
			return DomainEntity.GetFairnessType();
		}

		public void SetFairnessType(FairnessType fairnessType)
		{
			if (DomainEntity.GetFairnessType() == fairnessType) return;
			DomainEntity.SetFairnessType(fairnessType);
			IsDirty = true;
		}

		public bool IsDirty { get; set; }

		public bool AnonymousTrading
		{
			get => DomainEntity.AnonymousTrading;
			set
			{
				if (DomainEntity.AnonymousTrading == value) return;
				DomainEntity.AnonymousTrading = value;
				IsDirty = true;
			}
		}

		public bool LockTrading
		{
			get => DomainEntity.LockTrading;
			set
			{
				if (DomainEntity.LockTrading == value) return;
				DomainEntity.LockTrading = value;
				IsDirty = true;
			}
		}

		public bool AbsenceRequestWaitlistingEnabled
		{
			get => DomainEntity.AbsenceRequestWaitlistEnabled;
			set
			{
				if (DomainEntity.AbsenceRequestWaitlistEnabled == value) return;
				DomainEntity.AbsenceRequestWaitlistEnabled = value;
				IsDirty = true;
			}
		}

		public WaitlistProcessOrder AbsenceRequestWaitlistingProcessOrder
		{
			get => DomainEntity.AbsenceRequestWaitlistProcessOrder;
			set
			{
				if (DomainEntity.AbsenceRequestWaitlistProcessOrder == value) return;
				DomainEntity.AbsenceRequestWaitlistProcessOrder = value;
				IsDirty = true;
			}
		}

		public int? AbsenceRequestCancellationThreshold
		{
			get => DomainEntity.AbsenceRequestCancellationThreshold;
			set
			{
				if (DomainEntity.AbsenceRequestCancellationThreshold == value) return;
				DomainEntity.AbsenceRequestCancellationThreshold = value;
				IsDirty = true;
			}
		}

		public int? AbsenceRequestExpiredThreshold
		{
			get => DomainEntity.AbsenceRequestExpiredThreshold;
			set
			{
				if (DomainEntity.AbsenceRequestExpiredThreshold == value) return;
				DomainEntity.AbsenceRequestExpiredThreshold = value;
				IsDirty = true;
			}
		}

		public bool IsOvertimeProbabilityEnabled
		{
			get => DomainEntity.OvertimeProbabilityEnabled;
			set
			{
				if (DomainEntity.OvertimeProbabilityEnabled == value) return;
				DomainEntity.OvertimeProbabilityEnabled = value;
				IsDirty = true;
			}
		}

		public bool AbsenceProbabilityEnabled
		{
			get => DomainEntity.AbsenceProbabilityEnabled;
			set
			{
				if (DomainEntity.AbsenceProbabilityEnabled == value) return;
				DomainEntity.AbsenceProbabilityEnabled = value;
				IsDirty = true;
			}
		}

		public bool OvertimeRequestMaximumTimeEnabled
		{
			get => DomainEntity.OvertimeRequestMaximumTimeEnabled;
			set
			{
				if (DomainEntity.OvertimeRequestMaximumTimeEnabled == value) return;
				DomainEntity.OvertimeRequestMaximumTimeEnabled = value;
				IsDirty = true;
			}
		}

		public bool OvertimeRequestMaximumContinuousWorkTimeEnabled
		{
			get => DomainEntity.OvertimeRequestMaximumContinuousWorkTimeEnabled;
			set
			{
				if (DomainEntity.OvertimeRequestMaximumContinuousWorkTimeEnabled == value) return;
				DomainEntity.OvertimeRequestMaximumContinuousWorkTimeEnabled = value;
				IsDirty = true;
			}
		}

		public TimeSpan? OvertimeRequestMinimumRestTimeThreshold
		{
			get => DomainEntity.OvertimeRequestMinimumRestTimeThreshold;
			set
			{
				if (DomainEntity.OvertimeRequestMinimumRestTimeThreshold == value) return;
				DomainEntity.OvertimeRequestMinimumRestTimeThreshold = value;
				IsDirty = true;
			}
		}

		public TimeSpan? OvertimeRequestMaximumContinuousWorkTime
		{
			get => DomainEntity.OvertimeRequestMaximumContinuousWorkTime;
			set
			{
				if (DomainEntity.OvertimeRequestMaximumContinuousWorkTime == value) return;
				DomainEntity.OvertimeRequestMaximumContinuousWorkTime = value;
				IsDirty = true;
			}
		}

		public OvertimeRequestValidationHandleOptionView OvertimeRequestMaximumContinuousWorkTimeValidationHandleOptionView
		{
			get => DomainEntity.OvertimeRequestMaximumContinuousWorkTimeHandleType != null ? OvertimeRequestWorkRuleValidationHandleOptionViews[DomainEntity.OvertimeRequestMaximumContinuousWorkTimeHandleType.Value] : null;
			set
			{
				if (DomainEntity.OvertimeRequestMaximumContinuousWorkTimeHandleType == value?.WorkRuleValidationHandleType) return;
				DomainEntity.OvertimeRequestMaximumContinuousWorkTimeHandleType = value?.WorkRuleValidationHandleType;
				IsDirty = true;
			}
		}

		public OvertimeRequestStaffingCheckMethodOptionView OvertimeRequestStaffingCheckMethodOptionView
		{
			get => OvertimeRequestStaffingCheckMethodOptionViews[DomainEntity.OvertimeRequestStaffingCheckMethod];
			set
			{
				if (DomainEntity.OvertimeRequestStaffingCheckMethod == value.OvertimeRequestStaffingCheckMethod) return;
				DomainEntity.OvertimeRequestStaffingCheckMethod = value.OvertimeRequestStaffingCheckMethod;
				IsDirty = true;
			}
		}

		public bool OvertimeRequestUsePrimarySkill
		{
			get => DomainEntity.OvertimeRequestUsePrimarySkill;
			set
			{
				if (DomainEntity.OvertimeRequestUsePrimarySkill == value) return;
				DomainEntity.OvertimeRequestUsePrimarySkill = value;
				IsDirty = true;
			}
		}

		public int MaxConsecutiveWorkingDays
		{
			get => DomainEntity.MaximumConsecutiveWorkingDays;
			set
			{
				if (DomainEntity.MaximumConsecutiveWorkingDays == value) return;
				DomainEntity.MaximumConsecutiveWorkingDays = value;
				IsDirty = true;
			}
		}
	}
}