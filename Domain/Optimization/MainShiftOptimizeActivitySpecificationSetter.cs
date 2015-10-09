using System;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IMainShiftOptimizeActivitySpecificationSetter
	{
		void SetMainShiftOptimizeActivitySpecification(ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, IEditableShift mainShift, DateOnly viewDate);
	}

	public class MainShiftOptimizeActivitySpecificationSetter : IMainShiftOptimizeActivitySpecificationSetter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void SetMainShiftOptimizeActivitySpecification(ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, IEditableShift mainShift, DateOnly viewDate)
		{
			if (schedulingOptions == null)
				return;

			IOptimizerActivitiesPreferences optimizerActivitiesPreferences = createOptimizerActivitiesPreferences(optimizationPreferences);

			if (optimizerActivitiesPreferences == null)
				return;

			schedulingOptions.MainShiftOptimizeActivitySpecification =
				new MainShiftOptimizeActivitiesSpecification(optimizerActivitiesPreferences, mainShift, viewDate,
															 StateHolderReader.Instance.StateReader.SessionScopeData.TimeZone);

		}

		private IOptimizerActivitiesPreferences createOptimizerActivitiesPreferences(IOptimizationPreferences optimizationPreferences)
		{
			if (optimizationPreferences == null)
				return null;

			var shiftPreferences = optimizationPreferences.Shifts;

			if (!shiftPreferences.KeepActivityLength && !shiftPreferences.AlterBetween && !shiftPreferences.KeepEndTimes &&
				!shiftPreferences.KeepShiftCategories && !shiftPreferences.KeepStartTimes &&
				shiftPreferences.SelectedActivities.Count == 0)
				return null;

			IOptimizerActivitiesPreferences optimizerActivitiesPreferences = new OptimizerActivitiesPreferences();
			optimizerActivitiesPreferences.KeepShiftCategory = optimizationPreferences.Shifts.KeepShiftCategories;
			optimizerActivitiesPreferences.KeepStartTime = optimizationPreferences.Shifts.KeepStartTimes;
			optimizerActivitiesPreferences.KeepEndTime = optimizationPreferences.Shifts.KeepEndTimes;
			optimizerActivitiesPreferences.AllowAlterBetween = new TimePeriod(TimeSpan.FromHours(0), TimeSpan.FromHours(36));

			if (optimizationPreferences.Shifts.AlterBetween)
				optimizerActivitiesPreferences.AllowAlterBetween = optimizationPreferences.Shifts.SelectedTimePeriod;

			optimizerActivitiesPreferences.SetDoNotMoveActivities(optimizationPreferences.Shifts.SelectedActivities);
			optimizerActivitiesPreferences.DoNotAlterLengthOfActivity = null;
			if (shiftPreferences.KeepActivityLength)
			{
				optimizerActivitiesPreferences.DoNotAlterLengthOfActivity = shiftPreferences.ActivityToKeepLengthOn;
			}

			return optimizerActivitiesPreferences;

		}
	}

	public class MainShiftOptimizeActivitySpecificationSetterOff : IMainShiftOptimizeActivitySpecificationSetter
	{
		public void SetMainShiftOptimizeActivitySpecification(ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, IEditableShift mainShift, DateOnly viewDate){}	
	}
}