using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Security.Principal;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public interface ITeleoptiPrincipal
	{
		IPerson GetPerson(IPersonRepository personRepository);
		void AddClaimSet(ClaimSet claimSet);
		IIdentity Identity { get; }
		IEnumerable<ClaimSet> ClaimSets { get; }
		IPrincipalAuthorization PrincipalAuthorization { get; }
		IRegional Regional { get; }
		IOrganisationMembership Organisation { get; }
		bool IsInRole(string role);
	}
}