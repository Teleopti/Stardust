using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class OptimizationPreferencesDefaultValueProvider : IOptimizationPreferencesProvider
	{
		private IOptimizationPreferences _setFromTests;

		public IOptimizationPreferences Fetch()
		{
			return _setFromTests ?? new OptimizationPreferences
			{
				General = new GeneralPreferences 
				{
					UseShiftCategoryLimitations	= true,
					ScheduleTag = NullScheduleTag.Instance, 
					OptimizationStepDaysOff = true,
					UseRotations = true,
					RotationsValue = 1,
					UseAvailabilities = true,
					AvailabilitiesValue = 1
				},
				Advanced = new AdvancedPreferences{UseMinimumStaffing = true}
			};
		}

		public void SetFromTestsOnly_LegacyDONOTUSE(IOptimizationPreferences optimizationPreferences)
		{
			_setFromTests = optimizationPreferences;
		}
	}
}