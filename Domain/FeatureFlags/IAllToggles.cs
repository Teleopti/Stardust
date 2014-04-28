using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.FeatureFlags
{
	public interface IAllToggles
	{
		ISet<Toggles> Toggles();
	}
}