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
        }

        public IGeneralPreferences General { get; set; }
        public IDaysOffPreferences DaysOff { get; set; }
        public IExtraPreferences Extra { get; set; }
        public IAdvancedPreferences Advanced { get; set; }
    }

    public class GeneralPreferences : IGeneralPreferences
    {
        public GeneralPreferences()
        {
            OptimizationForDaysOff = true;
            OptimizationForShiftsWithinDay = true;
            AllowMoveShiftsWithinDay = true;

            UsePreferences = true;
            UseMustHaves = true;
            UseRotations = true;
            UseAvailabilities = true;
            UseStudentAvailabilities = true;
            UseShiftCategoryLimitations = true;

            PreferencesValue = 0.8d;
            MustHavesValue = 1d;
            RotationsValue = 1d;
            AvailabilitiesValue = 1d;
            StudentAvailabilitiesValue = 1d;
        }

        public IScheduleTag ScheduleTag { get; set; }

        public bool OptimizationForDaysOff { get; set; }
        public bool OptimizationForShiftsWithinDay { get; set; }
        public bool ShiftsForFlexibleWorkTime { get; set; }
        public bool DaysOffForFlexibleWorkTime { get; set; }
        public bool AllowMoveShiftsWithinDay { get; set; }

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
        public DaysOffPreferences()
        {
            KeepExistingDaysOffValue = 0d;

            UseDaysOffPerWeek = true;
            UseConsecutiveDaysOff = true;
            UseConsecutiveWorkdays = true;

            DaysOffPerWeekValue = new MinMax<int>(1, 3);
            ConsecutiveDaysOffValue = new MinMax<int>(1, 3);
            ConsecutiveWorkdaysValue = new MinMax<int>(2, 6);

            ConsiderWeekBefore = true;

            KeepFreeWeekends = true;
            KeepFreeWeekendDays = true;
        }

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
        public ExtraPreferences()
        {
            BlockFinderOptionsValue = BlockFinderType.BetweenDayOff;
            KeepShiftsValue = 0.8d;
        }

        public bool UseBlockScheduling { get; set; }

        public BlockFinderType BlockFinderOptionsValue { get; set; }

        public bool UseTeams { get; set; }
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
        public AdvancedPreferences()
        {
            TargetValue = TargetValueOptions.StandardDeviation;
            UseTweakedValues = true;
            UseMinimumStaffing = true;
            UseMaximumStaffing = true;
            UseMaximumSeats = true;
            RefreshScreenInterval = 10;
        }

        public TargetValueOptions TargetValue { get; set; }
        public bool UseIntraIntervalDeviation { get; set; }
        public bool UseTweakedValues { get; set; }

        public bool UseMinimumStaffing { get; set; }
        public bool UseMaximumStaffing { get; set; }
        public bool UseMaximumSeats { get; set; }
        public bool DoNotBreakMaximumSeats { get; set; }

        public int RefreshScreenInterval { get; set; }
    }
}
