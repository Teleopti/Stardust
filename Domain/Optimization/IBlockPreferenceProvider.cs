using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IBlockPreferenceProvider
	{
		ExtraPreferences ForAgent(IPerson person, DateOnly dateOnly);
	}
}