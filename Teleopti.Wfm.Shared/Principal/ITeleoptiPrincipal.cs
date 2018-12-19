using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Security.Principal;

namespace Teleopti.Ccc.Domain.Security.Principal
{	
	public interface ITeleoptiPrincipal : IPrincipal, IClaimsOwner
	{
		Guid PersonId { get; }
		IRegional Regional { get; }
	}

	public interface IClaimsOwner
	{
		IOrganisationMembership Organisation { get; }
		IEnumerable<ClaimSet> ClaimSets { get; }
		void AddClaimSet(ClaimSet claimSet);
	}
}