using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Security.Principal;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class TeleoptiPrincipalCacheable : GenericPrincipal, ITeleoptiPrincipal, IUnsafePerson
	{
		private readonly IList<ClaimSet> _claimSets = new List<ClaimSet>();

		private TeleoptiPrincipalCacheable(IIdentity identity)
			: base(identity, new string[] { }) { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public static TeleoptiPrincipalCacheable Make(ITeleoptiIdentity identity, IPerson person)
		{
			return new TeleoptiPrincipalCacheable(identity)
			       	{
			       		Person = person
			       	};
		}

		public IRegional Regional { get; set; }
		public IOrganisationMembership Organisation { get; set; }
		public IEnumerable<ClaimSet> ClaimSets { get { return _claimSets; } }

		public void AddClaimSet(ClaimSet claimSet) { _claimSets.Add(claimSet); }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IPerson GetPerson(IPersonRepository personRepository)
		{
			var person = personRepository.Get(Person.Id.Value);
			if (person == null)
				throw new PersonNotFoundException("Person not found for this principal");
			return person;
		}

		public IPerson Person { get; set; }

	}


	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors"),
	System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
	public class PersonNotFoundException : Exception
	{
		public PersonNotFoundException(string message) : base(message) { }
		public PersonNotFoundException(string message, Exception innerException) : base(message, innerException) { }
	}

}