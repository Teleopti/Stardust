using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Security.Principal;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Security.Principal
{	
	public interface ITeleoptiPrincipal : IPrincipal
	{
		IRegional Regional { get; }
		IOrganisationMembership Organisation { get; }
		IEnumerable<ClaimSet> ClaimSets { get; }
		// required by RoleToPrincipalCommand which is used everywhere a principal is created.
		// refactor??
		void AddClaimSet(ClaimSet claimSet);

		IPerson GetPerson(IPersonRepository personRepository);
	}
}