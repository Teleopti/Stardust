using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ToggleAdmin
{
	public interface IFetchAllToggleOverrides
	{
		IDictionary<string, bool> OverridenValues();
	}
}