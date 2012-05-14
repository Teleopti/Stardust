using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Runtime.Serialization;
using System.Security.Principal;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class TeleoptiPrincipalCachable : GenericPrincipal, ITeleoptiPrincipal, IUnsafePerson
	{
		private readonly IList<ClaimSet> _claimSets = new List<ClaimSet>();

		private TeleoptiPrincipalCachable(IIdentity identity)
			: base(identity, new string[] { }) { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public static TeleoptiPrincipalCachable Make(ITeleoptiIdentity identity, IPerson person)
		{
			return new TeleoptiPrincipalCachable(identity)
			       	{
			       		Person = person
			       	};
		}

		public IRegional Regional { get; set; }
		public IOrganisationMembership Organisation { get; set; }
		public IEnumerable<ClaimSet> ClaimSets { get { return _claimSets; } }

		public void AddClaimSet(ClaimSet claimSet) { _claimSets.Add(claimSet); }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IPerson GetPerson(IPersonRepository personRepository) { return personRepository.Get(Person.Id.Value); }
		public IPerson Person { get; set; }

	}
}