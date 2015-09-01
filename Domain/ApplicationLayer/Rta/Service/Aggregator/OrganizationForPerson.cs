using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service.Aggregator
{
	public class OrganizationForPerson : IOrganizationForPerson
	{
		private readonly IDatabaseLoader _personOrganizationProvider;

		public OrganizationForPerson(IDatabaseLoader personOrganizationProvider)
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