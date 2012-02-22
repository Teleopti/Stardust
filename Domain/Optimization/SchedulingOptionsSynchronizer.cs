using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface ISchedulingOptionsSynchronizer
    {
        void SynchronizeSchedulingOption(
            IOptimizationPreferences optimizationPreferences,
            ISchedulingOptions schedulingOptions);
    }

    public class SchedulingOptionsSynchronizer : ISchedulingOptionsSynchronizer
    {
        public void SynchronizeSchedulingOption(
            IOptimizationPreferences optimizationPreferences, 
            ISchedulingOptions schedulingOptions)
        {
            schedulingOptions.TagToUseOnScheduling = optimizationPreferences.General.ScheduleTag;
            schedulingOptions.UseBlockScheduling =
                optimizationPreferences.Extra.UseBlockScheduling 
                ? optimizationPreferences.Extra.BlockFinderTypeValue 
                : BlockFinderType.None;

            schedulingOptions.UseGroupOptimizing = optimizationPreferences.Extra.UseTeams;
            schedulingOptions.GroupOnGroupPage = optimizationPreferences.Extra.GroupPageOnTeam;

            schedulingOptions.UseRotations = optimizationPreferences.General.UseRotations;
            schedulingOptions.RotationDaysOnly = false; //???
            schedulingOptions.UseAvailability = optimizationPreferences.General.UseAvailabilities;
            schedulingOptions.AvailabilityDaysOnly = false;  //???
            schedulingOptions.UseStudentAvailability = optimizationPreferences.General.UseStudentAvailabilities;
            schedulingOptions.UsePreferences = optimizationPreferences.General.UsePreferences;
            schedulingOptions.PreferencesDaysOnly = false;  //???
            schedulingOptions.UsePreferencesMustHaveOnly = false;  //???
            schedulingOptions.UseShiftCategoryLimitations = optimizationPreferences.General.UseShiftCategoryLimitations;

            // schedulingOptions.ShiftCategory

            schedulingOptions.RefreshRate = optimizationPreferences.Advanced.RefreshScreenInterval;

            //schedulingOptions.OnlyShiftsWhenUnderstaffed = 

            schedulingOptions.Fairness = new Percent(optimizationPreferences.Extra.FairnessValue);
            schedulingOptions.GroupPageForShiftCategoryFairness = optimizationPreferences.Extra.GroupPageOnCompareWith;

            if (optimizationPreferences.Extra.KeepShiftCategories)
                schedulingOptions.RescheduleOptions = OptimizationRestriction.KeepShiftCategory;
            else if (optimizationPreferences.Extra.KeepStartAndEndTimes)
                schedulingOptions.RescheduleOptions = OptimizationRestriction.KeepStartAndEndTime;
            else
                schedulingOptions.RescheduleOptions = OptimizationRestriction.None;

            schedulingOptions.UseMinimumPersons = optimizationPreferences.Advanced.UseMinimumStaffing;
            schedulingOptions.UseMaximumPersons = optimizationPreferences.Advanced.UseMaximumStaffing;
            schedulingOptions.UseMaxSeats = optimizationPreferences.Advanced.UseMaximumSeats;
            schedulingOptions.DoNotBreakMaxSeats = optimizationPreferences.Advanced.DoNotBreakMaximumSeats;
        }
    }
}