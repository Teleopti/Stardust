﻿using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface ISchedulingOptionsCreator
    {
        ISchedulingOptions CreateSchedulingOptions(
            IOptimizationPreferences optimizationPreferences);
    }

    public class SchedulingOptionsCreator : ISchedulingOptionsCreator
    {
        public ISchedulingOptions CreateSchedulingOptions(
            IOptimizationPreferences optimizationPreferences)
        {
            ISchedulingOptions schedulingOptions = new SchedulingOptions();

            schedulingOptions.TagToUseOnScheduling = optimizationPreferences.General.ScheduleTag;
	       
            schedulingOptions.UseBlockScheduling =
                optimizationPreferences.Extra.UseBlockScheduling 
                ? optimizationPreferences.Extra.BlockFinderTypeValue 
                : BlockFinderType.None;

            schedulingOptions.UseGroupScheduling = optimizationPreferences.Extra.UseTeams;
        	schedulingOptions.UseGroupSchedulingCommonCategory =
        		optimizationPreferences.Extra.UseGroupSchedulingCommonCategory;
        	schedulingOptions.UseGroupSchedulingCommonStart = optimizationPreferences.Extra.UseGroupSchedulingCommonStart;
        	schedulingOptions.UseGroupSchedulingCommonEnd = optimizationPreferences.Extra.UseGroupSchedulingCommonEnd;
        	schedulingOptions.UseSameDayOffs = optimizationPreferences.Extra.KeepSameDaysOffInTeam;
            schedulingOptions.UseCommonActivity = optimizationPreferences.Extra.UseCommonActivity;
            if(schedulingOptions.UseCommonActivity )
                schedulingOptions.CommonActivity = optimizationPreferences.Extra.CommonActivity;
            
            schedulingOptions.GroupOnGroupPage = optimizationPreferences.Extra.GroupPageOnTeam;
            

            setPreferencesInSchedulingOptions(optimizationPreferences, schedulingOptions);
            setRotationsInSchedulingOptions(optimizationPreferences, schedulingOptions);
            setAvailabilitiesInSchedulingOptions(optimizationPreferences, schedulingOptions);
            setStudentAvailabilitiesInSchedulingOptions(optimizationPreferences, schedulingOptions);

            schedulingOptions.UseShiftCategoryLimitations = optimizationPreferences.General.UseShiftCategoryLimitations;

            // schedulingOptions.ShiftCategory

            schedulingOptions.RefreshRate = optimizationPreferences.Advanced.RefreshScreenInterval;

            //schedulingOptions.OnlyShiftsWhenUnderstaffed = 

            schedulingOptions.Fairness = new Percent(optimizationPreferences.Extra.FairnessValue);
            schedulingOptions.GroupPageForShiftCategoryFairness = optimizationPreferences.Extra.GroupPageOnCompareWith;

			schedulingOptions.BlockFinderTypeForAdvanceScheduling =
			   optimizationPreferences.Extra.BlockFinderTypeForAdvanceOptimization;
	        schedulingOptions.GroupOnGroupPageForLevelingPer = optimizationPreferences.Extra.GroupPageOnTeamLevelingPer;
	        if (schedulingOptions.BlockFinderTypeForAdvanceScheduling != BlockFinderType.None)
		        schedulingOptions.UseSameDayOffs = true;

			//if (optimizationPreferences.Shifts.KeepShiftCategories)
			//    schedulingOptions.RescheduleOptions = OptimizationRestriction.KeepShiftCategory;
			//else if (optimizationPreferences.Shifts.KeepStartTimes || optimizationPreferences.Shifts.KeepEndTimes )
			//    schedulingOptions.RescheduleOptions = OptimizationRestriction.KeepStartAndEndTime ;
			//else
			//    schedulingOptions.RescheduleOptions = OptimizationRestriction.None;

            schedulingOptions.UseMinimumPersons = optimizationPreferences.Advanced.UseMinimumStaffing;
            schedulingOptions.UseMaximumPersons = optimizationPreferences.Advanced.UseMaximumStaffing;
            schedulingOptions.UseMaxSeats = optimizationPreferences.Advanced.UseMaximumSeats;
            schedulingOptions.DoNotBreakMaxSeats = optimizationPreferences.Advanced.DoNotBreakMaximumSeats;
        	schedulingOptions.UseAverageShiftLengths = optimizationPreferences.Advanced.UseAverageShiftLengths;

            // extra properties
            schedulingOptions.ConsiderShortBreaks = optimizationPreferences.Rescheduling.ConsiderShortBreaks;
            schedulingOptions.OnlyShiftsWhenUnderstaffed = optimizationPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed;

           

            return schedulingOptions;
        }

        private static void setPreferencesInSchedulingOptions(IOptimizationPreferences optimizationPreferences,
                                                              ISchedulingOptions schedulingOptions)
        {
            schedulingOptions.PreferencesDaysOnly = false; // always
			schedulingOptions.UsePreferencesMustHaveOnly = false; // always >>> bugfix 19947: Can't optimize days off with 100% must have-fulfillment 

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
    }
}