using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Security.Principal;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class TeleoptiPrincipalCacheable : GenericPrincipal, ITeleoptiPrincipal, IUnsafePerson
	{
		private readonly IList<ClaimSet> _claimSets = new List<ClaimSet>();

		private TeleoptiPrincipalCacheable(IIdentity identity)
			: base(identity, new string[] { }) { }
		
		public static TeleoptiPrincipalCacheable Make(ITeleoptiIdentity identity, IPerson person)
		{
			var lightPerson = new Person();
			lightPerson.SetName(person.Name);
			lightPerson.SetId(person.Id);

			if (person.PermissionInformation != null)
			{
				lightPerson.PermissionInformation.SetDefaultTimeZone(person.PermissionInformation.DefaultTimeZone());
			}

			return new TeleoptiPrincipalCacheable(identity)
			       	{
			       		Person = lightPerson
			       	};
		}

		public IRegional Regional { get; set; }
		public IOrganisationMembership Organisation { get; set; }
		public IEnumerable<ClaimSet> ClaimSets => _claimSets;

		public void AddClaimSet(ClaimSet claimSet) { _claimSets.Add(claimSet); }
		
		public IPerson GetPerson(IPersonRepository personRepository)
		{
			var person = personRepository.Get(Person.Id.Value);
			if (person == null)
				throw new PersonNotFoundException("Person not found for this principal");
			return person;
		}

		public IPerson Person { get; set; }
	}
	
	public class PersonNotFoundException : Exception
	{
		public PersonNotFoundException(string message) : base(message) { }
		public PersonNotFoundException(string message, Exception innerException) : base(message, innerException) { }
	}

}