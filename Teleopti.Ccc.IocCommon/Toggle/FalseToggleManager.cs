using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;

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