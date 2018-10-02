using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ToggleAdmin
{
	public interface IPersistToggleOverride
	{
		void Save(Toggles toggle, bool value);
		void Delete(string toggle);
	}
}