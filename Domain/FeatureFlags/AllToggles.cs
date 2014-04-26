using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.FeatureFlags
{
	public class AllToggles : IAllToggles
	{
		public ISet<Toggles> Toggles()
		{
			return new HashSet<Toggles>
			(
				(Toggles[])Enum.GetValues(typeof(Toggles))
			);
		}
	}
}