using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class OptimizationPreferencesProvider
	{
		private OptimizationPreferences _setFromTests;

		public OptimizationPreferences Fetch()
		{
			return _setFromTests ?? new OptimizationPreferences
			{
				General = new GeneralPreferences {ScheduleTag = NullScheduleTag.Instance, OptimizationStepDaysOff = true}
			};
		}

		public void SetFromTestsOnly(OptimizationPreferences optimizationPreferences)
		{
			_setFromTests = optimizationPreferences;
		}
	}
}