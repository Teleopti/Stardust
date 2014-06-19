using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.IocCommon.Toggle
{
	public class FalseToggleManager : IToggleManager
	{
		public bool IsEnabled(Toggles toggle)
		{
			return false;
		}
	}
}