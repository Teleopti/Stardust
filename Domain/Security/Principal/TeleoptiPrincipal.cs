using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Security.Principal;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class TeleoptiPrincipal : GenericPrincipal, ITeleoptiPrincipal
	{
		public static ITeleoptiPrincipal CurrentPrincipal => ServiceLocatorForLegacy.CurrentTeleoptiPrincipal.Current();

		private readonly IList<ClaimSet> _claimSets = new List<ClaimSet>();

		private TeleoptiPrincipal(IIdentity identity)
			: base(identity, new string[] { }) { }
		
		public static TeleoptiPrincipal Make(ITeleoptiIdentity identity, IPerson person)
		{
			return new TeleoptiPrincipal(identity)
			{
				_person = person,
				_personId = person.Id.GetValueOrDefault(),
				PersonName = person.Name
			};
		}

		private IPerson _person;
		private Guid _personId;

		public Guid PersonId
		{
			get
			{
				if (_personId != Guid.Empty)
					return _personId;
				_personId = _person.Id.GetValueOrDefault();
				return _personId;
			}
		}

		public Name PersonName { get; set; }

		public IRegional Regional { get; set; }
		public IOrganisationMembership Organisation { get; set; }
		public IEnumerable<ClaimSet> ClaimSets => _claimSets;

		public void AddClaimSet(ClaimSet claimSet)
		{
			_claimSets.Add(claimSet);
		}
	}
}