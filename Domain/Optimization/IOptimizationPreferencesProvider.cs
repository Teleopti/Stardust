using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IOptimizationPreferencesProvider
	{
		IOptimizationPreferences Fetch();
	}
}