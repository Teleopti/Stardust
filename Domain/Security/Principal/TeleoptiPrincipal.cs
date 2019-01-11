using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Security.Principal;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class TeleoptiPrincipal : GenericPrincipal, ITeleoptiPrincipal
	{
		public static ITeleoptiPrincipal CurrentPrincipal => ServiceLocatorForLegacy.CurrentTeleoptiPrincipal.Current();

		private readonly IList<ClaimSet> _claimSets = new List<ClaimSet>();

		private TeleoptiPrincipal(IIdentity identity)
			: base(identity, new string[] { }) { }
		
		public static TeleoptiPrincipal Make(ITeleoptiIdentity identity, Func<Guid> personId)
		{
			return new TeleoptiPrincipal(identity)
			{
				_personId = personId,
			};
		}

		private Func<Guid> _personId;

		public Guid PersonId => _personId.Invoke();

		public IRegional Regional { get; set; }
		public IOrganisationMembership Organisation { get; set; }
		public IEnumerable<ClaimSet> ClaimSets => _claimSets;

		public void AddClaimSet(ClaimSet claimSet)
		{
			_claimSets.Add(claimSet);
		}
	}
}