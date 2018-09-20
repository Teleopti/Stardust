using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.ToggleAdmin;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeFetchAllToggleOverrides : IFetchAllToggleOverrides
	{
		private readonly ICollection<string> _overridenToggles = new List<string>();
		
		
		public IDictionary<string, bool> OverridenValues()
		{
			return _overridenToggles.ToDictionary(key => key, value => true);
		}

		public void Add(Toggles toggle)
		{
			_overridenToggles.Add(toggle.ToString());
		}
	}
}