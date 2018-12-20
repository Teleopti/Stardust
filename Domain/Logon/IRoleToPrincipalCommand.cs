using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Logon
{
	public interface IRoleToPrincipalCommand
	{
		void Execute(ITeleoptiPrincipal principal, IPersonRepository personRepository, string tenantName);
		void Execute(IPersonOwner personOwner, IClaimsOwner claimsOwner, IPersonRepository personRepository, string tenantName);
	}
}