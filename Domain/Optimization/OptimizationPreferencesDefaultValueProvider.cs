﻿using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class OptimizationPreferencesDefaultValueProvider : IOptimizationPreferencesProvider
	{
		private IOptimizationPreferences _setFromTests;

		public IOptimizationPreferences Fetch()
		{
			return _setFromTests ?? new OptimizationPreferences
			{
				General = new GeneralPreferences {ScheduleTag = NullScheduleTag.Instance, OptimizationStepDaysOff = true}
			};
		}

		public void SetFromTestsOnly(IOptimizationPreferences optimizationPreferences)
		{
			_setFromTests = optimizationPreferences;
		}
	}
}