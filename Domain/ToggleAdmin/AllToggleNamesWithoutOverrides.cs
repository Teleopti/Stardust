using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ToggleAdmin
{
	public class AllToggleNamesWithoutOverrides
	{
		private readonly IAllToggles _allToggles;
		private readonly IFetchAllToggleOverrides _fetchAllToggleOverrides;

		public AllToggleNamesWithoutOverrides(IAllToggles allToggles, IFetchAllToggleOverrides fetchAllToggleOverrides)
		{
			_allToggles = allToggles;
			_fetchAllToggleOverrides = fetchAllToggleOverrides;
		}
		public IEnumerable<string> Execute()
		{
			var allToggles = _allToggles.Toggles().Select(x => x.ToString());
			return allToggles.Except(_fetchAllToggleOverrides.OverridenValues().Select(x => x.Key)).OrderBy(x=>x);
		}
	}
}