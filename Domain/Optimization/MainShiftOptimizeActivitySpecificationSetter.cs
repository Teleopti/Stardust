using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IMainShiftOptimizeActivitySpecificationSetter
	{
		void SetMainShiftOptimizeActivitySpecification(ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, IEditableShift mainShift, DateOnly viewDate);
	}

	public class MainShiftOptimizeActivitySpecificationSetter : IMainShiftOptimizeActivitySpecificationSetter
	{
		private readonly OptimizerActivitiesPreferencesFactory _optimizerActivitiesPreferencesFactory;

		public MainShiftOptimizeActivitySpecificationSetter(OptimizerActivitiesPreferencesFactory optimizerActivitiesPreferencesFactory)
		{
			_optimizerActivitiesPreferencesFactory = optimizerActivitiesPreferencesFactory;
		}

		public void SetMainShiftOptimizeActivitySpecification(ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, IEditableShift mainShift, DateOnly viewDate)
		{
			if (schedulingOptions == null)
				return;

			var optimizerActivitiesPreferences = _optimizerActivitiesPreferencesFactory.Create(optimizationPreferences);

			if (optimizerActivitiesPreferences == null)
				return;

			schedulingOptions.MainShiftOptimizeActivitySpecification =
				new MainShiftOptimizeActivitiesSpecification(optimizerActivitiesPreferences, mainShift, viewDate,
															 StateHolderReader.Instance.StateReader.UserTimeZone);
		}
	}
}