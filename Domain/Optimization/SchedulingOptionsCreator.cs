﻿using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface ISchedulingOptionsCreator
    {
        ISchedulingOptions CreateSchedulingOptions(
            IOptimizationPreferences optimizationPreferences);
    }

	public class SchedulingOptionsCreator : ISchedulingOptionsCreator
	{
		public ISchedulingOptions CreateSchedulingOptions(IOptimizationPreferences optimizationPreferences)
		{
			ISchedulingOptions schedulingOptions = new SchedulingOptions();

			schedulingOptions.ShiftBagBackToLegal = optimizationPreferences.ShiftBagBackToLegal;
			schedulingOptions.TagToUseOnScheduling = optimizationPreferences.General.ScheduleTag;
			schedulingOptions.UseTeam = optimizationPreferences.Extra.UseTeams;
			schedulingOptions.TeamSameShiftCategory =
				optimizationPreferences.Extra.UseTeamSameShiftCategory;
			schedulingOptions.TeamSameStartTime = optimizationPreferences.Extra.UseTeamSameStartTime;
			schedulingOptions.TeamSameEndTime = optimizationPreferences.Extra.UseTeamSameEndTime;
			schedulingOptions.UseSameDayOffs = optimizationPreferences.Extra.UseTeamSameDaysOff;
			schedulingOptions.TeamSameActivity = optimizationPreferences.Extra.UseTeamSameActivity;
			if (schedulingOptions.TeamSameActivity)
				schedulingOptions.CommonActivity = optimizationPreferences.Extra.TeamActivityValue;

			schedulingOptions.GroupOnGroupPageForTeamBlockPer = optimizationPreferences.Extra.TeamGroupPage;


			setPreferencesInSchedulingOptions(optimizationPreferences, schedulingOptions);
			setRotationsInSchedulingOptions(optimizationPreferences, schedulingOptions);
			setAvailabilitiesInSchedulingOptions(optimizationPreferences, schedulingOptions);
			setStudentAvailabilitiesInSchedulingOptions(optimizationPreferences, schedulingOptions);

			schedulingOptions.UseShiftCategoryLimitations = optimizationPreferences.General.UseShiftCategoryLimitations;

			schedulingOptions.RefreshRate = optimizationPreferences.Advanced.RefreshScreenInterval;

			schedulingOptions.BlockFinderTypeForAdvanceScheduling =
				optimizationPreferences.Extra.BlockTypeValue;
			if (optimizationPreferences.Extra.UseTeamBlockOption)
			{
				schedulingOptions.UseSameDayOffs = true;
			}
			else
			{
				schedulingOptions.UseSameDayOffs = optimizationPreferences.Extra.UseTeamSameDaysOff;
			}

			schedulingOptions.UseMinimumPersons = optimizationPreferences.Advanced.UseMinimumStaffing;
			schedulingOptions.UseMaximumPersons = optimizationPreferences.Advanced.UseMaximumStaffing;
			schedulingOptions.UserOptionMaxSeatsFeature = optimizationPreferences.Advanced.UserOptionMaxSeatsFeature;
			schedulingOptions.UseAverageShiftLengths = optimizationPreferences.Advanced.UseAverageShiftLengths;

			// extra properties
			schedulingOptions.ConsiderShortBreaks = optimizationPreferences.Rescheduling.ConsiderShortBreaks;
			schedulingOptions.OnlyShiftsWhenUnderstaffed = optimizationPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed;

			setTeamBlockOptions(optimizationPreferences, schedulingOptions);

			return schedulingOptions;
		}

		private static void setPreferencesInSchedulingOptions(IOptimizationPreferences optimizationPreferences,
			ISchedulingOptions schedulingOptions)
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
			ISchedulingOptions schedulingOptions)
		{
			bool use = optimizationPreferences.General.UseRotations;
			double value = optimizationPreferences.General.RotationsValue;

			schedulingOptions.RotationDaysOnly = false; // always
			schedulingOptions.UseRotations = use && value == 1d;
		}

		private static void setAvailabilitiesInSchedulingOptions(IOptimizationPreferences optimizationPreferences,
			ISchedulingOptions schedulingOptions)
		{
			bool use = optimizationPreferences.General.UseAvailabilities;
			double value = optimizationPreferences.General.AvailabilitiesValue;

			schedulingOptions.AvailabilityDaysOnly = false; // always
			schedulingOptions.UseAvailability = use && value == 1d;
		}

		private static void setStudentAvailabilitiesInSchedulingOptions(IOptimizationPreferences optimizationPreferences,
			ISchedulingOptions schedulingOptions)
		{
			bool use = optimizationPreferences.General.UseStudentAvailabilities;
			double value = optimizationPreferences.General.StudentAvailabilitiesValue;

			schedulingOptions.UseStudentAvailability = use && value == 1d;
		}

		private static void setTeamBlockOptions(IOptimizationPreferences optimizationPreferences,
			ISchedulingOptions schedulingOptions)
		{
			schedulingOptions.UseBlock = optimizationPreferences.Extra.UseTeamBlockOption;
			schedulingOptions.BlockSameEndTime = optimizationPreferences.Extra.UseBlockSameEndTime;
			schedulingOptions.BlockSameStartTime = optimizationPreferences.Extra.UseBlockSameStartTime;
			schedulingOptions.BlockSameShift = optimizationPreferences.Extra.UseBlockSameShift;
			schedulingOptions.BlockSameShiftCategory = optimizationPreferences.Extra.UseBlockSameShiftCategory;

			if (!optimizationPreferences.Extra.UseTeams)
				schedulingOptions.GroupOnGroupPageForTeamBlockPer = GroupPageLight.SingleAgentGroup(Resources.SingleAgentTeam);

			if (!optimizationPreferences.Extra.UseTeamBlockOption)
				schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.SingleDay;

		}
	}
}