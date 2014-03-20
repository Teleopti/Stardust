using System;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Rta;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class OrganizationForPerson : IOrganizationForPerson
	{
		private readonly IPersonOrganizationProvider _personOrganizationProvider;

		public OrganizationForPerson(IPersonOrganizationProvider personOrganizationProvider)
		{
			_personOrganizationProvider = personOrganizationProvider;
		}

		public PersonOrganizationData GetOrganization(Guid personId)
		{
			return _personOrganizationProvider.LoadAll()
				.Single(x => x.PersonId == personId);
		}
	}
}