using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Security.Principal;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public interface ITeleoptiPrincipalForLegacy
	{
		IPerson UnsafePerson { get; }
	}

	public class TeleoptiPrincipalForLegacy : GenericPrincipal, ITeleoptiPrincipal, ITeleoptiPrincipalForLegacy
	{
		private IClaimsOwner _claimsOwner;
		private IPerson _person;
		private IIdentity _identity;

		public TeleoptiPrincipalForLegacy(IIdentity identity, IPerson person) : base(identity, new string[] { })
		{
			_person = person;
			_claimsOwner = new ClaimsOwner(person);
			initializeFromPerson();
		}

		public void ChangePrincipal(TeleoptiPrincipalForLegacy principal)
		{
			_person = principal._person;
			_claimsOwner = principal._claimsOwner;
			_identity = principal.Identity;
			initializeFromPerson();
		}

		private void initializeFromPerson()
		{
			if (_person == null) return;
			var info = _person.PermissionInformation;
			Regional = new Regional(
				info.DefaultTimeZone(),
				info.CultureLCID() ?? System.Threading.Thread.CurrentThread.CurrentCulture.LCID,
				info.UICultureLCID() ?? System.Threading.Thread.CurrentThread.CurrentUICulture.LCID);
		}

		public override IIdentity Identity => _identity ?? base.Identity;

		public Guid PersonId => _person.Id.GetValueOrDefault();
		public IRegional Regional { get; set; }
		public IOrganisationMembership Organisation => _claimsOwner.Organisation;

		public IEnumerable<ClaimSet> ClaimSets => _claimsOwner.ClaimSets;
		public void AddClaimSet(ClaimSet claimSet) => _claimsOwner.AddClaimSet(claimSet);

		IPerson ITeleoptiPrincipalForLegacy.UnsafePerson => _person;
	}
}