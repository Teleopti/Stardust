using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.Toggle
{
	public interface IToggleManager
	{
		bool IsEnabled(Toggles toggle);
	}
}