using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public interface IUpdatedByScope
	{
		void OnThisThreadUse(IPerson person);
	}
}