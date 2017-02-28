using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IOptimizationPreferencesProvider
	{
		IOptimizationPreferences Fetch();
	}
}