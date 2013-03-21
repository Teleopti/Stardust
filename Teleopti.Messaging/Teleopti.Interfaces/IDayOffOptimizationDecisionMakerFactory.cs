using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces
{
	/// <summary>
	/// Factory for creating the list of decision makers
	/// </summary>
	public interface IDayOffOptimizationDecisionMakerFactory
	{
		/// <summary>
		/// Create the decision makers
		/// </summary>
		/// <param name="scheduleMatrixArray"></param>
		/// <param name="optimizerPreferences"></param>
		/// <returns></returns>
		IEnumerable<IDayOffDecisionMaker> CreateDecisionMakers(
			ILockableBitArray scheduleMatrixArray,
			IOptimizationPreferences optimizerPreferences);
	}
}