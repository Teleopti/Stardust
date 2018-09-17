using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.IocCommonTest.Toggle
{
	public class FakeFetchToggleOverride : IFetchToggleOverride
	{
		private readonly IDictionary<Toggles, bool> _overridenToggles = new Dictionary<Toggles, bool>();
		
		public bool? OverridenValue(Toggles toggle)
		{
			if (_overridenToggles.TryGetValue(toggle, out var ret))
				return ret;
			return null;
		}

		public void SetValue(Toggles toggle, bool value)
		{
			_overridenToggles[toggle] = value;
		}
	}
}