using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Security.Principal;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class TeleoptiPrincipalCacheable : GenericPrincipal, ITeleoptiPrincipal
	{
		private readonly PrincipalPerson _principalPerson;
		private readonly IList<ClaimSet> _claimSets = new List<ClaimSet>();

		private TeleoptiPrincipalCacheable(IIdentity identity, PrincipalPerson principalPerson)
			: base(identity, new string[] { })
		{
			_principalPerson = principalPerson;
		}
		
		public static TeleoptiPrincipalCacheable Make(ITeleoptiIdentity identity, IPerson person)
		{
			return new TeleoptiPrincipalCacheable(identity, new PrincipalPerson(person));
		}

		public IRegional Regional { get; set; }
		public IOrganisationMembership Organisation { get; set; }
		public IEnumerable<ClaimSet> ClaimSets => _claimSets;

		public void AddClaimSet(ClaimSet claimSet) { _claimSets.Add(claimSet); }
		
		public IPerson GetPerson(IPersonRepository personRepository)
		{
			var person = personRepository.Get(_principalPerson.PersonId);
			if (person == null)
				throw new PersonNotFoundException("Person not found for this principal");
			return person;
		}

		public PrincipalPerson Person()
		{
			return _principalPerson;
		}
	}
}