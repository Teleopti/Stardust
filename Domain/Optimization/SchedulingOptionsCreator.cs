using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface ISchedulingOptionsCreator
    {
        SchedulingOptions CreateSchedulingOptions(
            IOptimizationPreferences optimizationPreferences);
    }

	public class SchedulingOptionsCreator : ISchedulingOptionsCreator
	{
		public SchedulingOptions CreateSchedulingOptions(IOptimizationPreferences optimizationPreferences)
		{
			var schedulingOptions = new SchedulingOptions
			{
				ShiftBagBackToLegal = optimizationPreferences.ShiftBagBackToLegal,
				TagToUseOnScheduling = optimizationPreferences.General.ScheduleTag,
				UseTeam = optimizationPreferences.Extra.UseTeams,
				TeamSameShiftCategory = optimizationPreferences.Extra.UseTeamSameShiftCategory,
				TeamSameStartTime = optimizationPreferences.Extra.UseTeamSameStartTime,
				TeamSameEndTime = optimizationPreferences.Extra.UseTeamSameEndTime,
				UseSameDayOffs = optimizationPreferences.Extra.UseTeamSameDaysOff,
				TeamSameActivity = optimizationPreferences.Extra.UseTeamSameActivity
			};

			if (schedulingOptions.TeamSameActivity)
				schedulingOptions.CommonActivity = optimizationPreferences.Extra.TeamActivityValue;

			schedulingOptions.GroupOnGroupPageForTeamBlockPer = optimizationPreferences.Extra.TeamGroupPage;


			setPreferencesInSchedulingOptions(optimizationPreferences, schedulingOptions);
			setRotationsInSchedulingOptions(optimizationPreferences, schedulingOptions);
			setAvailabilitiesInSchedulingOptions(optimizationPreferences, schedulingOptions);
			setStudentAvailabilitiesInSchedulingOptions(optimizationPreferences, schedulingOptions);

			schedulingOptions.UseShiftCategoryLimitations = optimizationPreferences.General.UseShiftCategoryLimitations;

			schedulingOptions.RefreshRate = optimizationPreferences.Advanced.RefreshScreenInterval;

			

			schedulingOptions.UseMinimumStaffing = optimizationPreferences.Advanced.UseMinimumStaffing;
			schedulingOptions.UseMaximumStaffing = optimizationPreferences.Advanced.UseMaximumStaffing;
			schedulingOptions.UseAverageShiftLengths = optimizationPreferences.Advanced.UseAverageShiftLengths;

			// extra properties
			schedulingOptions.ConsiderShortBreaks = optimizationPreferences.Rescheduling.ConsiderShortBreaks;
			schedulingOptions.OnlyShiftsWhenUnderstaffed = optimizationPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed;

			new BlockPreferencesMapper().UpdateSchedulingOptionsFromOptimizationPreferences(schedulingOptions, optimizationPreferences);

			return schedulingOptions;
		}

		private static void setPreferencesInSchedulingOptions(IOptimizationPreferences optimizationPreferences,
			SchedulingOptions schedulingOptions)
		{
			schedulingOptions.PreferencesDaysOnly = false; // always
			schedulingOptions.UsePreferencesMustHaveOnly = false;
				// always >>> bugfix 19947: Can't optimize days off with 100% must have-fulfillment 

			bool usePreferences = optimizationPreferences.General.UsePreferences;
			double preferencesValue = optimizationPreferences.General.PreferencesValue;
			bool usePreferencesAndValueIs1 = usePreferences && preferencesValue == 1d;
			schedulingOptions.UsePreferences = usePreferencesAndValueIs1;
		}

		private static void setRotationsInSchedulingOptions(IOptimizationPreferences optimizationPreferences,
			SchedulingOptions schedulingOptions)
		{
			bool use = optimizationPreferences.General.UseRotations;
			double value = optimizationPreferences.General.RotationsValue;

			schedulingOptions.RotationDaysOnly = false; // always
			schedulingOptions.UseRotations = use && value == 1d;
		}

		private static void setAvailabilitiesInSchedulingOptions(IOptimizationPreferences optimizationPreferences,
			SchedulingOptions schedulingOptions)
		{
			bool use = optimizationPreferences.General.UseAvailabilities;
			double value = optimizationPreferences.General.AvailabilitiesValue;

			schedulingOptions.AvailabilityDaysOnly = false; // always
			schedulingOptions.UseAvailability = use && value == 1d;
		}

		private static void setStudentAvailabilitiesInSchedulingOptions(IOptimizationPreferences optimizationPreferences,
			SchedulingOptions schedulingOptions)
		{
			bool use = optimizationPreferences.General.UseStudentAvailabilities;
			double value = optimizationPreferences.General.StudentAvailabilitiesValue;

			schedulingOptions.UseStudentAvailability = use && value == 1d;
		}
	}
}