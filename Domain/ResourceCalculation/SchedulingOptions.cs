using System;
using System.Collections.Generic;
using System.ComponentModel;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class SchedulingOptions : ISchedulingOptions
    {
		private readonly IList<IShiftCategory> _notAllowedShiftCategories = new List<IShiftCategory>();
		private bool _usePreferencesMustHaveOnly;
		private bool _considerShortBreaks = true;
        private bool _useRotations;
        private bool _rotationDaysOnly;
        private WorkShiftLengthHintOption _workShiftLengthHintOption = WorkShiftLengthHintOption.AverageWorkTime;
        private bool _useAvailability;
        private bool _availabilityDaysOnly;
        private bool _usePreferences;
        private bool _preferencesDaysOnly;

		private ISpecification<IEditableShift> _mainShiftOptimizeActivitySpecification;
        public bool UseMinimumPersons { get; set; }
        public bool UseMaximumPersons { get; set; }
        public bool OnlyShiftsWhenUnderstaffed { get; set; }
        public ScheduleEmploymentType ScheduleEmploymentType { get; set; }
        public IShiftCategory ShiftCategory { get; set; }
        public IDayOffTemplate DayOffTemplate { get; set; }
        public bool UseShiftCategoryLimitations { get; set; }
        public Percent Fairness { get; set; }
        public bool UseStudentAvailability { get; set; }
		public bool UseTeam { get; set; }

        public IGroupPageLight GroupOnGroupPageForTeamBlockPer { get; set; }
		private BlockFinderType _blockFinderTypeForAdvanceScheduling;
        public bool BlockSameEndTime { get; set; }
        public bool BlockSameShiftCategory { get; set; }
        public bool BlockSameStartTime { get; set; }
		public bool BlockSameShift { get; set; }
        public bool UseBlock { get; set; }

        public bool DoNotBreakMaxStaffing { get; set; }
        public IGroupPageLight GroupPageForShiftCategoryFairness { get; set; }
        public int RefreshRate { get; set; }
		public bool UseMaxSeats { get; set; }
		public bool DoNotBreakMaxSeats { get; set; }
        public bool UseSameDayOffs { get; set; }
        public IScheduleTag TagToUseOnScheduling { get; set; }
    	public int ResourceCalculateFrequency { get; set; }
    	public TimeSpan? UseCustomTargetTime { get; set; }
    	public bool ShowTroubleshot { get; set; }
		public bool TeamSameStartTime { get; set; }
		public bool TeamSameEndTime { get; set; }
		public bool TeamSameShiftCategory { get; set; }

        public IActivity CommonActivity { get; set; }
        public bool TeamSameActivity { get; set; }
		public bool UseAverageShiftLengths { get; set; }

        
        public BlockFinderType BlockFinderTypeForAdvanceScheduling
        {
	        get
	        {
				if(!UseBlock)
					return BlockFinderType.SingleDay;

		        return _blockFinderTypeForAdvanceScheduling;
	        }
            set
            {
                _blockFinderTypeForAdvanceScheduling = value;
            }
        }

        public ISpecification<IEditableShift> MainShiftOptimizeActivitySpecification
    	{
    		get
    		{
				if(_mainShiftOptimizeActivitySpecification == null)
					return  new All<IEditableShift>();

    			return _mainShiftOptimizeActivitySpecification;
    		}
    		set { _mainShiftOptimizeActivitySpecification = value; }
    	}

    	public SchedulingOptions()
		{
			new SchedulingOptionsGeneralPersonalSetting().MapTo(this, new List<IScheduleTag>());
			new SchedulingOptionsAdvancedPersonalSetting().MapTo(this, new List<IShiftCategory>());
            new SchedulingOptionsExtraPersonalSetting().MapTo(this, new List<IScheduleTag>(), new List<IGroupPageLight>(), new List<IGroupPageLight>(), new List<IActivity>());
		}


        public bool UsePreferences
        {
            get { return _usePreferences; }
            set
            {
                _usePreferences = value;
                if(!_usePreferences)
                {
                    _preferencesDaysOnly = false;
                    _usePreferencesMustHaveOnly = false;
                }
            }
        }

        public bool PreferencesDaysOnly
        {
            get
            {
                if (!_usePreferences)
                {
                    return false;
                }
                return _preferencesDaysOnly;
            }
            set { _preferencesDaysOnly = value; }
        }

        public bool UsePreferencesMustHaveOnly
        {
            get
            {
                if (!_usePreferences)
                {
                    return false;
                }
                return _usePreferencesMustHaveOnly;
            }
            set
            {
                _usePreferencesMustHaveOnly = value;
            }
        }

        public bool UseRotations
        {
            get { return _useRotations; }
            set
            {
                if (value == false)
                    _rotationDaysOnly = false;

                _useRotations = value;
            }
        }

        public bool RotationDaysOnly
        {
            get
            {
                if (!_useRotations)
                {
                    return false;
                }
                return _rotationDaysOnly;
            }
            set { _rotationDaysOnly = value; }
        }

        public bool UseAvailability
        {
            get { return _useAvailability; }
            set
            {
                if (value == false)
                    _availabilityDaysOnly = false;

                _useAvailability = value;
            }
        }

        public bool AvailabilityDaysOnly
        {
            get
            {
                if (!_useAvailability)
                {
                    return false;
                }
                return _availabilityDaysOnly;
            }
            set { _availabilityDaysOnly = value; }
        }

        /// <summary>
        /// Gets or sets the work shift length hint, whether the service should find a longer or a shorter workshift if possible.
        /// </summary>
        /// <value>The work shift length hint option.</value>
        [DefaultValue(typeof(WorkShiftLengthHintOption), "WorkShiftLengthHintOption.AverageWorktime")]
        public WorkShiftLengthHintOption WorkShiftLengthHintOption
        {
            get { return _workShiftLengthHintOption; }
            set { _workShiftLengthHintOption = value; }
        }

        
        public IList<IShiftCategory> NotAllowedShiftCategories
        {
            get { return _notAllowedShiftCategories; }
        }

        public bool ConsiderShortBreaks
        {
            get { return _considerShortBreaks; }
            set { _considerShortBreaks = value; }
        }

        #region ICloneable Members

        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }
}
