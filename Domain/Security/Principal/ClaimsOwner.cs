using System.Collections.Generic;
using System.IdentityModel.Claims;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class ClaimsOwner : IClaimsOwner
	{
		private readonly IList<ClaimSet> _claimSets = new List<ClaimSet>();

		public ClaimsOwner(IPerson person)
		{
			Organisation = person == null ? new OrganisationMembership() : OrganisationMembership.FromPerson(person);
		}

		public IOrganisationMembership Organisation { get; }
		public IEnumerable<ClaimSet> ClaimSets => _claimSets;
		public void AddClaimSet(ClaimSet claimSet)
		{
			_claimSets.Add(claimSet);
		}
	}
}