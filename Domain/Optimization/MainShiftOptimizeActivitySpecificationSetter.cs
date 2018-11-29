using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IMainShiftOptimizeActivitySpecificationSetter
	{
		void SetMainShiftOptimizeActivitySpecification(SchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, IEditableShift mainShift, DateOnly viewDate);
	}

	public class MainShiftOptimizeActivitySpecificationSetter : IMainShiftOptimizeActivitySpecificationSetter
	{
		private readonly CorrectAlteredBetween _correctAlteredBetween;
		private readonly OptimizerActivitiesPreferencesFactory _optimizerActivitiesPreferencesFactory;

		public MainShiftOptimizeActivitySpecificationSetter(CorrectAlteredBetween correctAlteredBetween, OptimizerActivitiesPreferencesFactory optimizerActivitiesPreferencesFactory)
		{
			_correctAlteredBetween = correctAlteredBetween;
			_optimizerActivitiesPreferencesFactory = optimizerActivitiesPreferencesFactory;
		}

		public void SetMainShiftOptimizeActivitySpecification(SchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, IEditableShift mainShift, DateOnly viewDate)
		{
			if (schedulingOptions == null)
				return;

			var optimizerActivitiesPreferences = _optimizerActivitiesPreferencesFactory.Create(optimizationPreferences);

			if (optimizerActivitiesPreferences == null)
				return;

			schedulingOptions.MainShiftOptimizeActivitySpecification =
				new MainShiftOptimizeActivitiesSpecification(_correctAlteredBetween, optimizerActivitiesPreferences, mainShift, viewDate);
		}
	}
}