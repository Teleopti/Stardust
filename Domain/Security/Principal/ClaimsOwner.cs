using System.Collections.Generic;
using System.IdentityModel.Claims;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class ClaimsOwner : IClaimsOwner
	{
		private readonly IList<ClaimSet> _claimSets = new List<ClaimSet>();

		public ClaimsOwner(IPerson person) : this(new PersonAndBusinessUnit(person, null))
		{
		}

		public ClaimsOwner(IPrincipalSource source)
		{
			Organisation = new OrganisationMembership().Initialize(source);
		}

		public IOrganisationMembership Organisation { get; }
		public IEnumerable<ClaimSet> ClaimSets => _claimSets;

		public void AddClaimSet(ClaimSet claimSet)
		{
			_claimSets.Add(claimSet);
		}
	}
}