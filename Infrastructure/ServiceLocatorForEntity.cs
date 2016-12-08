using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.Infrastructure
{
	// these properties are injected by reflection in ServiceLocatorModule
	// always prefer dependency injection over service locator!

	public static class ServiceLocatorForLegacy
	{
		private static IToggleManager _toggleManager;

		public static IToggleManager ToggleManager
		{
			get { return _toggleManager ?? new NoToggleManager(); }
			set { _toggleManager = value; }
		}
		
	}

	public class NoToggleManager : IToggleManager
	{
		public bool IsEnabled(Toggles toggle)
		{
			return false;
		}
	}
}
