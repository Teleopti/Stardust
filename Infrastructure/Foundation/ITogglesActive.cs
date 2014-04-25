using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public interface ITogglesActive
	{
		IDictionary<Toggles, bool> AllActiveToggles();
	}
}