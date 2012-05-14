using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Runtime.Serialization;
using System.Security.Principal;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2012/05/")]
	public class TeleoptiPrincipalSerializable : GenericPrincipal, ITeleoptiPrincipal, IUnsafePerson
	{
		[DataMember]
		private IList<ClaimSet> _claimSets = new List<ClaimSet>();

		private TeleoptiPrincipalSerializable(IIdentity identity)
			: base(identity, new string[] { }) { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public static TeleoptiPrincipalSerializable Make(ITeleoptiIdentity identity, IPerson person)
		{
			return new TeleoptiPrincipalSerializable(identity)
			       	{
			       		PersonId = person.Id.Value,
			       		Person = person
			       	};
		}

		[DataMember]
		public Guid PersonId { get; private set; }
		public IPerson Person { get; set; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IPerson GetPerson(IPersonRepository personRepository) { return personRepository.Get(PersonId); }

		public void AddClaimSet(ClaimSet claimSet) { _claimSets.Add(claimSet); }
		public IEnumerable<ClaimSet> ClaimSets { get { return _claimSets; } }

		[DataMember]
		public IRegional Regional { get; set; }

		[DataMember]
		public IOrganisationMembership Organisation { get; set; }

	}
}