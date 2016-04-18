using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public interface IUpdatedByScope
	{
		void OnThisThreadUse(IPerson person);
	}
}