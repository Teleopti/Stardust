using Teleopti.Ccc.Domain.ResourceCalculation;
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

            schedulingOptions.UseGroupOptimizing = optimizationPreferences.Extra.UseTeams;
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

            // extra properties
            schedulingOptions.ConsiderShortBreaks = optimizationPreferences.Rescheduling.ConsiderShortBreaks;
            schedulingOptions.OnlyShiftsWhenUnderstaffed = optimizationPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed;

            return schedulingOptions;
        }

        private static void setPreferencesInSchedulingOptions(IOptimizationPreferences optimizationPreferences,
                                                              ISchedulingOptions schedulingOptions)
        {
            bool use = optimizationPreferences.General.UsePreferences;
            double value = optimizationPreferences.General.PreferencesValue;

            schedulingOptions.UsePreferencesMustHaveOnly = false; // always
            schedulingOptions.PreferencesDaysOnly = false; // always
            schedulingOptions.UsePreferences = use && value == 1d;            
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