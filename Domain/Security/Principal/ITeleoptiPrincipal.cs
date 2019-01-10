using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Security.Principal;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Security.Principal
{	
	public interface ITeleoptiPrincipal : IPrincipal, IPersonOwner, IClaimsOwner
	{
		IRegional Regional { get; }
	}

	public interface IPersonOwner
	{
		IPerson GetPerson(IPersonRepository personRepository);
	}

	public interface IClaimsOwner
	{
		IOrganisationMembership Organisation { get; }
		IEnumerable<ClaimSet> ClaimSets { get; }
		void AddClaimSet(ClaimSet claimSet);
	}
}