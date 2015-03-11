using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Aggregator
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
			PersonOrganizationData ret;
			return _personOrganizationProvider.PersonOrganizationData().TryGetValue(personId, out ret) ? 
				ret : 
				null;
		}
	}
}