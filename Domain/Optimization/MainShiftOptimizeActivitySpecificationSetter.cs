using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IMainShiftOptimizeActivitySpecificationSetter
	{
		void SetSpecification(ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, IMainShift mainShift, DateOnly viewDate);
	}

	public class MainShiftOptimizeActivitySpecificationSetter : IMainShiftOptimizeActivitySpecificationSetter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void SetSpecification(ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, IMainShift mainShift, DateOnly viewDate)
		{
			if (optimizationPreferences == null)
				return;

			if (schedulingOptions == null)
				return;

			schedulingOptions.MainShiftOptimizeActivitySpecification = null;

			var shiftPreferences = optimizationPreferences.Shifts;

			if (!shiftPreferences.AlterBetween && !shiftPreferences.KeepEndTimes &&
			    !shiftPreferences.KeepShiftCategories && !shiftPreferences.KeepStartTimes &&
			    shiftPreferences.SelectedActivities.Count == 0)
				return;

			IOptimizerActivitiesPreferences optimizerActivitiesPreferences = new OptimizerActivitiesPreferences();
			optimizerActivitiesPreferences.KeepShiftCategory = optimizationPreferences.Shifts.KeepShiftCategories;
			optimizerActivitiesPreferences.KeepStartTime = optimizationPreferences.Shifts.KeepStartTimes;
			optimizerActivitiesPreferences.KeepEndTime = optimizationPreferences.Shifts.KeepEndTimes;
			optimizerActivitiesPreferences.AllowAlterBetween = new TimePeriod(TimeSpan.FromHours(0), TimeSpan.FromHours(36));

			if (optimizationPreferences.Shifts.AlterBetween)
				optimizerActivitiesPreferences.AllowAlterBetween = optimizationPreferences.Shifts.SelectedTimePeriod;

			optimizerActivitiesPreferences.SetDoNotMoveActivities(optimizationPreferences.Shifts.SelectedActivities);
			optimizerActivitiesPreferences.DoNotAlterLengthOfActivity = null;

			schedulingOptions.MainShiftOptimizeActivitySpecification =
				new MainShiftOptimizeActivitiesSpecification(optimizerActivitiesPreferences, mainShift, viewDate,
				                                             StateHolderReader.Instance.StateReader.SessionScopeData.TimeZone);

		}
	}
}