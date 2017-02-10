using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class OptimizerActivitiesPreferencesFactory
	{
		public OptimizerActivitiesPreferences Create(IOptimizationPreferences optimizationPreferences)
		{
			if (optimizationPreferences == null)
				return null;

			var shiftPreferences = optimizationPreferences.Shifts;

			if (!shiftPreferences.KeepActivityLength && !shiftPreferences.AlterBetween && !shiftPreferences.KeepEndTimes &&
					!shiftPreferences.KeepShiftCategories && !shiftPreferences.KeepStartTimes &&
					shiftPreferences.SelectedActivities.Count == 0)
				return null;

			var optimizerActivitiesPreferences = new OptimizerActivitiesPreferences
			{
				KeepShiftCategory = optimizationPreferences.Shifts.KeepShiftCategories,
				KeepStartTime = optimizationPreferences.Shifts.KeepStartTimes,
				KeepEndTime = optimizationPreferences.Shifts.KeepEndTimes
			};
			if (optimizationPreferences.Shifts.AlterBetween)
			{
				optimizerActivitiesPreferences.AllowAlterBetween = optimizationPreferences.Shifts.SelectedTimePeriod;
			}

			optimizerActivitiesPreferences.SetDoNotMoveActivities(optimizationPreferences.Shifts.SelectedActivities);
			optimizerActivitiesPreferences.DoNotAlterLengthOfActivity = null;
			if (shiftPreferences.KeepActivityLength)
			{
				optimizerActivitiesPreferences.DoNotAlterLengthOfActivity = shiftPreferences.ActivityToKeepLengthOn;
			}

			return optimizerActivitiesPreferences;
		}
	}
}