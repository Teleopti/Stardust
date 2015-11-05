using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces
{
	/// <summary>
	/// Factory for creating the list of decision makers
	/// </summary>
	public interface IDayOffOptimizationDecisionMakerFactory
	{
		IEnumerable<IDayOffDecisionMaker> CreateDecisionMakers(
			ILockableBitArray scheduleMatrixArray,
			IOptimizationPreferences optimizerPreferences,
			IDaysOffPreferences daysOffPreferences);
	}
}