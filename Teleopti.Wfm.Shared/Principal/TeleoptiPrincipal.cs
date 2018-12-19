using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Security.Principal;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class TeleoptiPrincipal : GenericPrincipal, ITeleoptiPrincipal
	{
		private readonly IList<ClaimSet> _claimSets = new List<ClaimSet>();
		private readonly Func<Guid> _personId = () => Guid.Empty;

		public TeleoptiPrincipal(IIdentity identity, IPrincipalSource source)
			: base(identity, new string[] { })
		{
			if (source != null)
				_personId = source.PrincipalPersonId;
			Regional = new Regional(
				source?.PrincipalTimeZone(),
				source?.PrincipalCultureLCID() ?? 0,
				source?.PrincipalUICultureLCID() ?? 0);
		}
		
		public Guid PersonId => _personId.Invoke();
		public IRegional Regional { get; set; }
		public IOrganisationMembership Organisation { get; set; }
		public IEnumerable<ClaimSet> ClaimSets => _claimSets;
		public void AddClaimSet(ClaimSet claimSet) => _claimSets.Add(claimSet);
	}
}