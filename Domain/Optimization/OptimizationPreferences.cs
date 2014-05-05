using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class OptimizationPreferences : IOptimizationPreferences
	{
		public OptimizationPreferences()
		{
			General = new GeneralPreferences();
			DaysOff = new DaysOffPreferences();
			Extra = new ExtraPreferences();
			Advanced = new AdvancedPreferences();
			Rescheduling = new ReschedulingPreferences();
            Shifts = new ShiftPreferences();
		}

		public IGeneralPreferences General { get; set; }
		public IDaysOffPreferences DaysOff { get; set; }
		public IExtraPreferences Extra { get; set; }
		public IAdvancedPreferences Advanced { get; set; }
		public IReschedulingPreferences Rescheduling { get; set; }
        public IShiftPreferences Shifts { get; set; }

	}

	public class GeneralPreferences : IGeneralPreferences
	{

		public IScheduleTag ScheduleTag { get; set; }

		public bool OptimizationStepDaysOff { get; set; }
		public bool OptimizationStepTimeBetweenDays { get; set; }
		public bool OptimizationStepShiftsForFlexibleWorkTime { get; set; }
		public bool OptimizationStepDaysOffForFlexibleWorkTime { get; set; }
		public bool OptimizationStepShiftsWithinDay { get; set; }
		public bool OptimizationStepFairness { get; set; }

		public bool UsePreferences { get; set; }
		public bool UseMustHaves { get; set; }
		public bool UseRotations { get; set; }
		public bool UseAvailabilities { get; set; }
		public bool UseStudentAvailabilities { get; set; }
		public bool UseShiftCategoryLimitations { get; set; }

		public double PreferencesValue { get; set; }
		public double MustHavesValue { get; set; }
		public double RotationsValue { get; set; }
		public double AvailabilitiesValue { get; set; }
		public double StudentAvailabilitiesValue { get; set; }

	}

	public class DaysOffPreferences : IDaysOffPreferences
	{

		public bool UseKeepExistingDaysOff { get; set; }
		public double KeepExistingDaysOffValue { get; set; }

		public bool UseDaysOffPerWeek { get; set; }
		public bool UseConsecutiveDaysOff { get; set; }
		public bool UseConsecutiveWorkdays { get; set; }
		public bool UseFullWeekendsOff { get; set; }
		public bool UseWeekEndDaysOff { get; set; }

		public MinMax<int> DaysOffPerWeekValue { get; set; }
		public MinMax<int> ConsecutiveDaysOffValue { get; set; }
		public MinMax<int> ConsecutiveWorkdaysValue { get; set; }
		public MinMax<int> FullWeekendsOffValue { get; set; }
		public MinMax<int> WeekEndDaysOffValue { get; set; }

		public bool ConsiderWeekBefore { get; set; }
		public bool ConsiderWeekAfter { get; set; }

		public bool KeepFreeWeekends { get; set; }
		public bool KeepFreeWeekendDays { get; set; }

	}

	public class ExtraPreferences : IExtraPreferences
	{
		public BlockFinderType BlockFinderTypeValue { get; set; }

		public bool UseTeams { get; set; }
		public bool KeepSameDaysOffInTeam { get; set; }
		public IGroupPageLight GroupPageOnTeam { get; set; }

		public IGroupPageLight GroupPageOnCompareWith { get; set; }
		public double FairnessValue { get; set; }

		
		public bool UseGroupSchedulingCommonStart { get; set; }
		public bool UseGroupSchedulingCommonEnd { get; set; }
		public bool UseGroupSchedulingCommonCategory { get; set; }
        public bool UseCommonActivity { get; set; }
        public IActivity CommonActivity { get; set; }

        public BlockFinderType BlockFinderTypeForAdvanceOptimization { get; set; }

	    public IGroupPageLight GroupPageOnTeamBlockPer{get ; set; }
	    public bool UseTeamBlockSameEndTime { get; set; }
	    public bool UseTeamBlockSameShiftCategory { get; set; }
	    public bool UseTeamBlockSameStartTime { get; set; }
	    public bool UseTeamBlockSameShift { get; set; }
	    public bool UseTeamBlockOption { get; set; }
	}

    public class ShiftPreferences : IShiftPreferences
    {

        public bool KeepShiftCategories { get; set; }
        public bool KeepEndTimes { get; set; }
        public bool KeepStartTimes { get; set; }
        public bool KeepShifts { get; set; }
        public bool AlterBetween { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<IActivity> SelectedActivities { get; set; }

	    public bool KeepActivityLength { get; set; }
		public IActivity ActivityToKeepLengthOn { get; set; }

	    public TimePeriod SelectedTimePeriod { get; set; }
        public double KeepShiftsValue { get; set; }

		public ShiftPreferences()
		{
			SelectedActivities = new List<IActivity>();
		}
    }

	public class AdvancedPreferences : IAdvancedPreferences
	{

		public TargetValueOptions TargetValueCalculation { get; set; }
		public bool UseIntraIntervalDeviation { get; set; }
		public bool UseTweakedValues { get; set; }

		public bool UseMinimumStaffing { get; set; }
		public bool UseMaximumStaffing { get; set; }
		public bool UseMaximumSeats { get; set; }
		public bool DoNotBreakMaximumSeats { get; set; }
		public bool UseAverageShiftLengths { get; set; }

		public int RefreshScreenInterval { get; set; }

		public AdvancedPreferences()
		{
			UseAverageShiftLengths = true;
		}
	}

	public class ReschedulingPreferences : IReschedulingPreferences
	{
		public ReschedulingPreferences()
		{
			ConsiderShortBreaks = true;
		}

		public bool ConsiderShortBreaks { get; set; }
		public bool OnlyShiftsWhenUnderstaffed { get; set; }
		public IDictionary<IPerson, IScheduleRange> AllSelectedScheduleRangeClones { get; set; }
	}
}
