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
		public void SetSpecification(ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, IMainShift mainShift, DateOnly viewDate)
		{
			IOptimizerActivitiesPreferences optimizerActivitiesPreferences = new OptimizerActivitiesPreferences();
			optimizerActivitiesPreferences.KeepShiftCategory = optimizationPreferences.Shifts.KeepShiftCategories;
			optimizerActivitiesPreferences.KeepStartTime = optimizationPreferences.Shifts.KeepStartTimes;
			optimizerActivitiesPreferences.KeepEndTime = optimizationPreferences.Shifts.KeepEndTimes;
			//throw new NotImplementedException();
			optimizerActivitiesPreferences.AllowAlterBetween = new TimePeriod(TimeSpan.FromHours(0), TimeSpan.FromHours(36));
			if (optimizationPreferences.Shifts.AlterBetween)
				optimizerActivitiesPreferences.AllowAlterBetween = optimizationPreferences.Shifts.SelectedTimePeriod;
			optimizerActivitiesPreferences.SetDoNotMoveActivities(new List<IActivity>());

			schedulingOptions.MainShiftOptimizeActivitySpecification = new MainShiftOptimizeActivitiesSpecification(optimizerActivitiesPreferences, mainShift, viewDate, StateHolderReader.Instance.StateReader.SessionScopeData.TimeZone);
		}
	}
}