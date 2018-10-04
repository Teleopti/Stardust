using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.Toggle.InApp
{
	public interface IFetchToggleOverride
	{
		bool? OverridenValue(Toggles toggle);
	}
}