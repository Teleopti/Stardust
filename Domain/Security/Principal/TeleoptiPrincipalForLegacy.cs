﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Security.Principal;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class TeleoptiPrincipalForLegacy : GenericPrincipal, ITeleoptiPrincipal, ITeleoptiPrincipalForLegacy
	{
		private static readonly CurrentTeleoptiPrincipal currentTeleoptiPrincipal = new CurrentTeleoptiPrincipal(new ThreadPrincipalContext());

		public static ITeleoptiPrincipal CurrentPrincipal => currentTeleoptiPrincipal.Current();

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
			if (_person != null)
				Regional = Principal.Regional.FromPersonWithThreadCultureFallback(_person);
		}

		public override IIdentity Identity => _identity ?? base.Identity;

		public Guid PersonId => _person.Id.GetValueOrDefault();
		public Name PersonName => _person.Name;
		public IRegional Regional { get; set; }
		public IOrganisationMembership Organisation => _claimsOwner.Organisation;

		public IEnumerable<ClaimSet> ClaimSets => _claimsOwner.ClaimSets;
		public void AddClaimSet(ClaimSet claimSet) => _claimsOwner.AddClaimSet(claimSet);

		IPerson ITeleoptiPrincipalForLegacy.UnsafePerson => _person;
	}

	public interface ITeleoptiPrincipalForLegacy
	{
		IPerson UnsafePerson { get; }
	}
}