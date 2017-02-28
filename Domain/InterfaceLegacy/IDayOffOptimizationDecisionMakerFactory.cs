using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy
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