using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class TogglesActive : ITogglesActive
	{
		private readonly IToggleManager _toggleManager;
		private readonly IAllToggles _allToggles;

		public TogglesActive(IToggleManager toggleManager, IAllToggles allToggles)
		{
			_toggleManager = toggleManager;
			_allToggles = allToggles;
		}

		public IDictionary<Toggles, bool> AllActiveToggles()
		{
			var ret =  new Dictionary<Toggles, bool>();
			foreach (var toggle in _allToggles.Toggles())
			{
				ret[toggle] = _toggleManager.IsEnabled(toggle);
			}
			return ret;
		}
	}
}