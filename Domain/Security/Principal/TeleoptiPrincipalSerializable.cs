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
		private readonly ITeleoptiIdentity _identity;

		[DataMember]
		private IList<ClaimSet> _claimSets = new List<ClaimSet>();

		private TeleoptiPrincipalSerializable(ITeleoptiIdentity identity)
			: base(identity, new string[] { }) { _identity = identity; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public static TeleoptiPrincipalSerializable Make(ITeleoptiIdentity identity, IPerson person)
		{
			return new TeleoptiPrincipalSerializable(identity)
			       	{
			       		PersonId = person.Id.Value,
			       		Person = person
			       	};
		}

		public ITeleoptiIdentity TeleoptiIdentity { get { return _identity; } }

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