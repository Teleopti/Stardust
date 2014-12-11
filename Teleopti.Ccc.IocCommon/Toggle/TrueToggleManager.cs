using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.IocCommon.Toggle
{
	public class TrueToggleManager : IToggleManager
	{
		public bool IsEnabled(Toggles toggle)
		{
			return true;
		}
	}
}