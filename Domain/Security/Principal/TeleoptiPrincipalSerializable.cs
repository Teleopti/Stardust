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
	public class TeleoptiPrincipalSerializable : GenericPrincipal, ITeleoptiPrincipal
	{
		[DataMember]
		private IList<ClaimSet> _claimSets = new List<ClaimSet>();

		public TeleoptiPrincipalSerializable(IIdentity identity, IPerson person) : base(identity, new string[] { })
		{
			PersonId = person.Id.Value;
			Regional = Principal.Regional.FromPerson(person);
			Organisation = OrganisationMembership.FromPerson(person);
		}

		[DataMember]
		public Guid PersonId { get; private set; }
		public IPerson GetPerson(IPersonRepository personRepository) { return personRepository.Get(PersonId); }

		public void AddClaimSet(ClaimSet claimSet) { _claimSets.Add(claimSet); }
		public IEnumerable<ClaimSet> ClaimSets { get { return _claimSets; } }

		[DataMember]
		public IRegional Regional { get; private set; }

		[DataMember]
		public IOrganisationMembership Organisation { get; private set; }
	}
}