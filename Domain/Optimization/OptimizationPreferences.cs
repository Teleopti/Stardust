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
		}

		public IGeneralPreferences General { get; set; }
		public IDaysOffPreferences DaysOff { get; set; }
		public IExtraPreferences Extra { get; set; }
		public IAdvancedPreferences Advanced { get; set; }
		public IReschedulingPreferences Rescheduling { get; set; }

	}

	public class GeneralPreferences : IGeneralPreferences
	{
		public IScheduleTag ScheduleTag { get; set; }

		public bool OptimizationStepDaysOff { get; set; }
		public bool OptimizationStepTimeBetweenDays { get; set; }
		public bool OptimizationStepShiftsForFlexibleWorkTime { get; set; }
		public bool OptimizationStepDaysOffForFlexibleWorkTime { get; set; }
		public bool OptimizationStepShiftsWithinDay { get; set; }

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

		public bool UseBlockScheduling { get; set; }

		public BlockFinderType BlockFinderTypeValue { get; set; }

		public bool UseTeams { get; set; }
		public bool KeepSameDaysOffInTeam { get; set; }
		public IGroupPage GroupPageOnTeam { get; set; }

		public double FairnessValue { get; set; }

		public IGroupPage GroupPageOnCompareWith { get; set; }

		public bool KeepShiftCategories { get; set; }
		public bool KeepStartAndEndTimes { get; set; }
		public bool KeepShifts { get; set; }

		public double KeepShiftsValue { get; set; }
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

		public int RefreshScreenInterval { get; set; }
	}

	public class ReschedulingPreferences : IReschedulingPreferences
	{
		public ReschedulingPreferences()
		{
			ConsiderShortBreaks = true;
		}

		public bool ConsiderShortBreaks { get; set; }
		public bool OnlyShiftsWhenUnderstaffed { get; set; }
	}
}
