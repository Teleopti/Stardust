using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Security.Principal;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class TeleoptiPrincipal : GenericPrincipal, ITeleoptiPrincipal
	{
		public static ITeleoptiPrincipal CurrentPrincipal => ServiceLocatorForLegacy.CurrentTeleoptiPrincipal.Current();

		private readonly IList<ClaimSet> _claimSets = new List<ClaimSet>();
		private readonly Func<Guid> _personId = () => Guid.Empty;

		public TeleoptiPrincipal(IIdentity identity, IPrincipalSource person)
			: base(identity, new string[] { })
		{
			if (person != null)
				_personId = person.PrincipalPersonId;
			Regional = new Regional(
				person?.PrincipalTimeZone(),
				person?.PrincipalCultureLCID() ?? 0,
				person?.PrincipalUICultureLCID() ?? 0);
		}
		
		public Guid PersonId => _personId.Invoke();
		public IRegional Regional { get; set; }
		public IOrganisationMembership Organisation { get; set; }
		public IEnumerable<ClaimSet> ClaimSets => _claimSets;
		public void AddClaimSet(ClaimSet claimSet) => _claimSets.Add(claimSet);
	}
}