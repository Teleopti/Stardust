using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class OptimizationPreferences : IOptimizationPreferences
	{
		public OptimizationPreferences()
		{
			General = new GeneralPreferences();
			Extra = new ExtraPreferences();
			Advanced = new AdvancedPreferences();
			Rescheduling = new ReschedulingPreferences();
            Shifts = new ShiftPreferences();
		}

		public GeneralPreferences General { get; set; }
		public ExtraPreferences Extra { get; set; }
		public IAdvancedPreferences Advanced { get; set; }
		public IReschedulingPreferences Rescheduling { get; set; }
        public ShiftPreferences Shifts { get; set; }
		public bool ShiftBagBackToLegal { get; set; }
	}

	public class GeneralPreferences
	{

		public IScheduleTag ScheduleTag { get; set; }

		public bool OptimizationStepDaysOff { get; set; }
		public bool OptimizationStepTimeBetweenDays { get; set; }
		public bool OptimizationStepShiftsForFlexibleWorkTime { get; set; }
		public bool OptimizationStepDaysOffForFlexibleWorkTime { get; set; }
		public bool OptimizationStepShiftsWithinDay { get; set; }
		public bool OptimizationStepFairness { get; set; }
		public bool OptimizationStepIntraInterval { get; set; }

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
		public IScheduleTagSetter CreateScheduleTagSetter()
		{
			return ScheduleTag == null
				? new ScheduleTagSetter(NullScheduleTag.Instance)
				: new ScheduleTagSetter(ScheduleTag);
		}
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

	public class ExtraPreferences
	{
		public bool UseTeams { get; set; }
		public bool UseTeamSameDaysOff { get; set; }
		public GroupPageLight TeamGroupPage { get; set; } = new GroupPageLight("not set", GroupPageType.SingleAgent);

		public bool UseTeamSameStartTime { get; set; }
		public bool UseTeamSameEndTime { get; set; }
		public bool UseTeamSameShiftCategory { get; set; }
        public bool UseTeamSameActivity { get; set; }
        public IActivity TeamActivityValue { get; set; }

        public BlockFinderType BlockTypeValue { get; set; }

	    public bool UseBlockSameEndTime { get; set; }
	    public bool UseBlockSameShiftCategory { get; set; }
	    public bool UseBlockSameStartTime { get; set; }
	    public bool UseBlockSameShift { get; set; }
	    public bool UseTeamBlockOption { get; set; }

		public IBlockFinder BlockFinder()
		{
			switch (BlockTypeValue)
			{
				case BlockFinderType.SingleDay:
					return new SingleDayBlockFinder();
				case BlockFinderType.BetweenDayOff:
					return new BetweenDayOffBlockFinder();
				case BlockFinderType.SchedulePeriod:
					return new SchedulePeriodBlockFinder();
			}
			throw new NotSupportedException($"Cannot find block finder for {BlockTypeValue}");
		}

		public bool IsClassic()
		{
			return !UseTeams && !UseTeamBlockOption;
		}
	}

    public class ShiftPreferences
    {

        public bool KeepShiftCategories { get; set; }
        public bool KeepEndTimes { get; set; }
        public bool KeepStartTimes { get; set; }
        public bool AlterBetween { get; set; }
        public IList<IActivity> SelectedActivities { get; set; }

	    public bool KeepActivityLength { get; set; }
		public IActivity ActivityToKeepLengthOn { get; set; }

	    public TimePeriod SelectedTimePeriod { get; set; }

		public ShiftPreferences()
		{
			SelectedActivities = new List<IActivity>();
		}
    }

	public class AdvancedPreferences : IAdvancedPreferences
	{
		public TargetValueOptions TargetValueCalculation { get; set; }
		public bool UseTweakedValues { get; set; }

		public bool UseMinimumStaffing { get; set; }
		public bool UseMaximumStaffing { get; set; }

		public MaxSeatsFeatureOptions UserOptionMaxSeatsFeature { get; set; }
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
