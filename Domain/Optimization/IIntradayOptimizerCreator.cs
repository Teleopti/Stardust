using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IIntradayOptimizerCreator
	{
		IEnumerable<IIntradayOptimizer2> Create(DateOnlyPeriod period, IEnumerable<IScheduleDay> scheduleDays,
			IOptimizationPreferences optimizerPreferences,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);
	}
}