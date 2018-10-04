using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.Toggle.InApp
{
	public class NoFetchingOfToggleOverride : IFetchToggleOverride
	{
		public bool? OverridenValue(Toggles toggle)
		{
			return null;
		}
	}
}