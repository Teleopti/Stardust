using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.IocCommon.Toggle
{
	public class FakeToggleManager : IToggleManager
	{
		private readonly IList<Toggles> _enabled = new List<Toggles>();

		public FakeToggleManager(Toggles toggle)
		{
			Enable(toggle);
		}

		public FakeToggleManager()
		{
		}

		public void Enable(Toggles toggle)
		{
			_enabled.Add(toggle);
		}

		public bool IsEnabled(Toggles toggle)
		{
			return _enabled.Any(e => e == toggle);
		}
	}
}