using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Configuration
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
        IWorkflowControlSet OriginalDomainEntity { get; }
        IList<AbsenceRequestPeriodModel> AbsenceRequestPeriodModels { get; }
        DateTime? SchedulePublishedToDate { get; set; }
        DateOnlyPeriod PreferenceInputPeriod { get; set; }
        DateOnlyPeriod PreferencePeriod { get; set; }
        DateOnlyPeriod StudentAvailabilityInputPeriod { get; set; }
        DateOnlyPeriod StudentAvailabilityPeriod { get; set; }
        MinMax<int> ShiftTradeOpenPeriodDays { get; set; }
        IEnumerable<IDayOffTemplate> AllowedPreferenceDayOffs { get; }
        IEnumerable<IShiftCategory> AllowedPreferenceShiftCategories { get; }
        IEnumerable<IAbsence> AllowedPreferenceAbsences { get; }
        IEnumerable<ISkill> MustMatchSkills { get; }
        bool AutoGrantShiftTradeRequest { get; set; }
        void AddAllowedPreferenceDayOff(IDayOffTemplate dayOff);
        void RemoveAllowedPreferenceDayOff(IDayOffTemplate dayOff);
        void AddAllowedPreferenceShiftCategory(IShiftCategory shiftCategory);
        void RemoveAllowedPreferenceShiftCategory(IShiftCategory shiftCategory);
        void AddAllowedPreferenceAbsence(IAbsence absence);
        void RemoveAllowedPreferenceAbsence(IAbsence absence);
        void UpdateAfterMerge(IWorkflowControlSet updatedWorkflowControlSet);
        void AddSkillToMatchList(ISkill skill);
        void RemoveSkillFromMatchList(ISkill skill);
        bool UseShiftCategoryFairness { get; set; }
        bool IsDirty { get; set; }
	    TimeSpan MinTimePerWeek { get; set; }
    }

    public class WorkflowControlSetModel : IWorkflowControlSetModel
    {
        private static IList<AbsenceRequestPeriodTypeModel> _defaultAbsenceRequestPeriodAdapters;
        private readonly List<AbsenceRequestPeriodModel> _absenceRequestPeriodModels = new List<AbsenceRequestPeriodModel>();

        public WorkflowControlSetModel(IWorkflowControlSet domainEntity)
            : this(domainEntity, domainEntity.EntityClone())
        {
        }

        public WorkflowControlSetModel(IWorkflowControlSet originalDomainEntity, IWorkflowControlSet clonedDomainEntity)
        {
            OriginalDomainEntity = originalDomainEntity;
            DomainEntity = clonedDomainEntity;
        }

        public Guid? Id
        {
            get { return DomainEntity.Id; }
        }

        public string Name
        {
            get { return DomainEntity.Name; }
            set
            {
                if (DomainEntity.Name == value) return;
                DomainEntity.Name = value;
                IsDirty = true;
            }
        }
        public TimeSpan ShiftTradeTargetTimeFlexibility
        {
            get { return DomainEntity.ShiftTradeTargetTimeFlexibility; }
            set
            {
                if (DomainEntity.ShiftTradeTargetTimeFlexibility == value) return;
                DomainEntity.ShiftTradeTargetTimeFlexibility = value;
                IsDirty = true;
            }
        }

        public int? WriteProtection
        {
            get { return DomainEntity.WriteProtection; }
            set
            {
                if (DomainEntity.WriteProtection == value) return;
                DomainEntity.WriteProtection = value;
                IsDirty = true;
            }
        }

        public IActivity AllowedPreferenceActivity
        {
            get { return DomainEntity.AllowedPreferenceActivity; }
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

        public bool IsNew { get { return !Id.HasValue; } }

        public IWorkflowControlSet OriginalDomainEntity { get; private set; }

        public static IList<AbsenceRequestPeriodTypeModel> DefaultAbsenceRequestPeriodAdapters
        {
            get
            {
                setDefaultPeriodIfMissing();
                return _defaultAbsenceRequestPeriodAdapters;
            }
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

        public DateTime? SchedulePublishedToDate
        {
            get { return DomainEntity.SchedulePublishedToDate; }
            set
            {
                if (DomainEntity.SchedulePublishedToDate == value) return;
                DomainEntity.SchedulePublishedToDate = value;
                IsDirty = true;
            }
        }

        public DateOnlyPeriod PreferenceInputPeriod
        {
            get { return DomainEntity.PreferenceInputPeriod; }
            set
            {
                if (DomainEntity.PreferenceInputPeriod == value) return;
                DomainEntity.PreferenceInputPeriod = value;
                IsDirty = true;
            }
        }

        public DateOnlyPeriod PreferencePeriod
        {
            get { return DomainEntity.PreferencePeriod; }
            set
            {
                if (DomainEntity.PreferencePeriod == value) return;
                DomainEntity.PreferencePeriod = value;
                IsDirty = true;
            }
        }

        public DateOnlyPeriod StudentAvailabilityInputPeriod
        {
            get { return DomainEntity.StudentAvailabilityInputPeriod; }
            set
            {
                if (DomainEntity.StudentAvailabilityInputPeriod == value) return;
                DomainEntity.StudentAvailabilityInputPeriod = value;
                IsDirty = true;
            }
        }

        public DateOnlyPeriod StudentAvailabilityPeriod
        {
            get { return DomainEntity.StudentAvailabilityPeriod; }
            set
            {
                if (DomainEntity.StudentAvailabilityPeriod == value) return;
                DomainEntity.StudentAvailabilityPeriod = value;
                IsDirty = true;
            }
        }

        public MinMax<int> ShiftTradeOpenPeriodDays
        {
            get { return DomainEntity.ShiftTradeOpenPeriodDaysForward; }
            set
            {
                if (DomainEntity.ShiftTradeOpenPeriodDaysForward == value) return;
                DomainEntity.ShiftTradeOpenPeriodDaysForward = value;
                IsDirty = true;
            }
        }

        public IEnumerable<IDayOffTemplate> AllowedPreferenceDayOffs
        {
            get
            {
                return DomainEntity.AllowedPreferenceDayOffs;
            }
        }

        public IEnumerable<IShiftCategory> AllowedPreferenceShiftCategories
        {
            get
            {
                return DomainEntity.AllowedPreferenceShiftCategories;
            }
        }

        public IEnumerable<IAbsence> AllowedPreferenceAbsences
        {
            get
            {
                return DomainEntity.AllowedPreferenceAbsences;
            }
        }

        public IEnumerable<ISkill> MustMatchSkills
        {
            get { return DomainEntity.MustMatchSkills; }
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

        public virtual void RemoveAllowedPreferenceShiftCategory(IShiftCategory shiftCategory)
        {
            DomainEntity.RemoveAllowedPreferenceShiftCategory(shiftCategory);
            IsDirty = true;
        }

        private static void setDefaultPeriodIfMissing()
        {
            if (_defaultAbsenceRequestPeriodAdapters == null)
            {
                DateOnlyPeriod period = getCurrentMonthPeriod(new DateOnly(DateOnly.Today.Date.AddMonths(1)));
                DateOnlyPeriod openPeriod = getCurrentMonthPeriod(DateOnly.Today);
                AbsenceRequestOpenDatePeriod openDatePeriod = new AbsenceRequestOpenDatePeriod
                {
                    StaffingThresholdValidator = new StaffingThresholdValidator(),
                    AbsenceRequestProcess = new PendingAbsenceRequest(),
                    Period = period,
                    OpenForRequestsPeriod = openPeriod
                };

                AbsenceRequestOpenRollingPeriod openRollingPeriod = new AbsenceRequestOpenRollingPeriod
                {
                    StaffingThresholdValidator = new StaffingThresholdValidator(),
                    AbsenceRequestProcess = new PendingAbsenceRequest(),
                    BetweenDays = new MinMax<int>(2, 15),
                    OpenForRequestsPeriod = openPeriod
                };

                _defaultAbsenceRequestPeriodAdapters = new List<AbsenceRequestPeriodTypeModel>
                                                           {
                                                               new AbsenceRequestPeriodTypeModel(openDatePeriod,Resources.FromTo),
                                                               new AbsenceRequestPeriodTypeModel(openRollingPeriod, Resources.Rolling)
                                                           };
            }
        }

        private static DateOnlyPeriod getCurrentMonthPeriod(DateOnly dateViewPoint)
        {
            DateOnly startDateOnly = new DateOnly(dateViewPoint.Year, dateViewPoint.Month, 1);
            DateOnly endDateOnly = new DateOnly(dateViewPoint.Year, dateViewPoint.Month,
                                                startDateOnly.Date.AddMonths(1).AddDays(-1).Day);

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
            get { return DomainEntity.AutoGrantShiftTradeRequest; }
            set
            {
                if (DomainEntity.AutoGrantShiftTradeRequest == value) return;
                DomainEntity.AutoGrantShiftTradeRequest = value;
                IsDirty = true;
            }
        }

        public bool UseShiftCategoryFairness
        {
            get { return DomainEntity.UseShiftCategoryFairness; }
            set
            {
                if (DomainEntity.UseShiftCategoryFairness == value) return;
                DomainEntity.UseShiftCategoryFairness = value;
                IsDirty = true;
            }
        }

        public bool IsDirty
        {
            get;
            set;
        }

		public TimeSpan MinTimePerWeek
		{
			get { return DomainEntity.MinTimePerWeek; }
			set
			{
				if (DomainEntity.MinTimePerWeek == value) return;
				DomainEntity.MinTimePerWeek = value;
				IsDirty = true;
			}
		}
    }
}
