using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	public class FakeToggleManager : IToggleManager
	{
		private readonly IDictionary<Toggles, bool> _toggles = new Dictionary<Toggles, bool>();

		public FakeToggleManager()
		{
		}

		public FakeToggleManager(Toggles toggle, bool value)
		{
			_toggles.Add(toggle, value);
		}

		public bool IsEnabled(Toggles toggle)
		{
			return !_toggles.ContainsKey(toggle) || _toggles[toggle];
		}
	}
}