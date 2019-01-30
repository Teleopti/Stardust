using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Security.Principal;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public static class TeleoptiPrincipalWithUnsafePersonExtensions
	{
		public static IPerson UnsafePerson(this ITeleoptiPrincipalWithUnsafePerson instance)
		{
			return instance.UnsafePersonObject() as IPerson;
		}
	}

	public class TeleoptiPrincipalWithUnsafePerson : GenericPrincipal, ITeleoptiPrincipal, ITeleoptiPrincipalWithUnsafePerson
	{
		private IClaimsOwner _claimsOwner;
		private IPerson _person;
		private IIdentity _identity;

		public TeleoptiPrincipalWithUnsafePerson(IIdentity identity, IPerson person) : this(identity, new PersonAndBusinessUnit(person, null))
		{
		}
		
		public TeleoptiPrincipalWithUnsafePerson(IIdentity identity, IPrincipalSource source) : base(identity, new string[] { })
		{
			_person = source.UnsafePerson() as IPerson;
			_claimsOwner = new ClaimsOwner(source);
			initializeFromPerson();
		}

		public void ChangePrincipal(TeleoptiPrincipalWithUnsafePerson principal)
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

		object ITeleoptiPrincipalWithUnsafePerson.UnsafePersonObject() => _person;
	}
}