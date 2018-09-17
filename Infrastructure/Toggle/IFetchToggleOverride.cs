using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.Toggle
{
	public interface IFetchToggleOverride
	{
		bool? ToggleValue(Toggles toggle);
	}
}