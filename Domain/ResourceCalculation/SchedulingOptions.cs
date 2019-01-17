using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class SchedulingOptions
	{
		private HashSet<IShiftCategory> _notAllowedShiftCategories = new HashSet<IShiftCategory>();
		private IList<ShiftProjectionCache> _notAllowedShiftProjectionCaches = new List<ShiftProjectionCache>(); 

		private bool _usePreferencesMustHaveOnly;
	    private bool _useRotations;
        private bool _rotationDaysOnly;
        private WorkShiftLengthHintOption _workShiftLengthHintOption = WorkShiftLengthHintOption.AverageWorkTime;
        private bool _useAvailability;
        private bool _availabilityDaysOnly;
        private bool _usePreferences;
        private bool _preferencesDaysOnly;

		private ISpecification<IEditableShift> _mainShiftOptimizeActivitySpecification;

	    public bool UseMinimumStaffing { get; set; }
        public bool UseMaximumStaffing { get; set; }
        public bool OnlyShiftsWhenUnderstaffed { get; set; }
        public ScheduleEmploymentType ScheduleEmploymentType { get; set; }
        public IShiftCategory ShiftCategory { get; set; }
        public IDayOffTemplate DayOffTemplate { get; set; }
        public bool UseShiftCategoryLimitations { get; set; }
        public bool UseStudentAvailability { get; set; }
		public bool UseTeam { get; set; }

	    public GroupPageLight GroupOnGroupPageForTeamBlockPer { get; set; } =new GroupPageLight("not set", GroupPageType.SingleAgent);
		private BlockFinderType _blockFinderTypeForAdvanceScheduling;
	    public bool BlockSameEndTime { get; set; }
        public bool BlockSameShiftCategory { get; set; }
        public bool BlockSameStartTime { get; set; }
		public bool BlockSameShift { get; set; }
        public bool UseBlock { get; set; }
        public int RefreshRate { get; set; }
        public bool UseSameDayOffs { get; set; }
        public IScheduleTag TagToUseOnScheduling { get; set; }
		//TODO maybe not needed?
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

			public bool IsDayScheduled(IScheduleDay scheduleDay)
			{
				var svc = ScheduleOnDayOffs ? (IIsDayScheduled) 
					new IsDayScheduledExcludeDayOff() : 
					new IsDayScheduled();
				return svc.Check(scheduleDay);
			}

	    public bool ShiftBagBackToLegal { get; set; }

	    public IBlockFinder BlockFinder()
		{
			switch (BlockFinderTypeForAdvanceScheduling)
			{
				case BlockFinderType.SingleDay:
					return ScheduleOnDayOffs ? (IBlockFinder) 
						new ReturnPeriodBasedOnDayBlockFinder() : 
						new SingleDayBlockFinder();
				case BlockFinderType.BetweenDayOff:
					return new BetweenDayOffBlockFinder();
				case BlockFinderType.SchedulePeriod:
					return new SchedulePeriodBlockFinder();
			}
			throw new NotSupportedException($"Cannot find block finder for {BlockFinderTypeForAdvanceScheduling}");
		}

		public ISpecification<IEditableShift> MainShiftOptimizeActivitySpecification
    	{
    		get
    		{
				if(_mainShiftOptimizeActivitySpecification == null)
					return new All<IEditableShift>();

    			return _mainShiftOptimizeActivitySpecification;
    		}
    		set { _mainShiftOptimizeActivitySpecification = value; }
    	}

	    public void AddNotAllowedShiftProjectionCache(ShiftProjectionCache shiftProjectionCache)
	    {
		    _notAllowedShiftProjectionCaches.Add(shiftProjectionCache);
	    }

	    public IList<ShiftProjectionCache> NotAllowedShiftProjectionCaches => _notAllowedShiftProjectionCaches;

	    public IRuleSetBag FixedShiftBag { get; set; }

	    public void ClearNotAllowedShiftProjectionCaches()
	    {
		    _notAllowedShiftProjectionCaches.Clear();
	    }

	    public IMultiplicatorDefinitionSet OvertimeType { get; set; }

	    public SchedulingOptions()
		{
			new SchedulingOptionsGeneralPersonalSetting().MapTo(this, new List<IScheduleTag>());
			new SchedulingOptionsAdvancedPersonalSetting().MapTo(this, new List<IShiftCategory>());
            new SchedulingOptionsExtraPersonalSetting().MapTo(this, new List<IScheduleTag>(), new List<GroupPageLight>(), new List<GroupPageLight>(), new List<IActivity>());
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
		
        public HashSet<IShiftCategory> NotAllowedShiftCategories => _notAllowedShiftCategories;

	    public bool ConsiderShortBreaks { get; set; } = true;

	    public bool ScheduleOnDayOffs { get; set; }

	    public bool AllowBreakContractTime { get; set; }

	    public bool SkipNegativeShiftValues { get; set; }

	    public bool IsClassic()
	    {
		    return !UseTeam && !UseBlock;
	    }

	    public SchedulingOptions Clone()
		{
			var ret = (SchedulingOptions) MemberwiseClone();
			ret._notAllowedShiftCategories = _notAllowedShiftCategories.ToHashSet();
			ret._notAllowedShiftProjectionCaches = _notAllowedShiftProjectionCaches.ToList();
            return ret;
        }
    }
}
