using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Security.Principal;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{	
	public interface ITeleoptiPrincipal : IPrincipal
	{
		ITeleoptiIdentity TeleoptiIdentity { get; }
		IRegional Regional { get; }
		IOrganisationMembership Organisation { get; }
		IEnumerable<ClaimSet> ClaimSets { get; }

		// required by RoleToPrincipalCommand which is used everywhere a principal is created.
		// refactor??
		void AddClaimSet(ClaimSet claimSet);

		IPerson GetPerson(IPersonRepository personRepository);

	}
}