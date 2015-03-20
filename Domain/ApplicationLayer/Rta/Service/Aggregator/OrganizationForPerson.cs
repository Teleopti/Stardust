using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service.Aggregator
{
	public class OrganizationForPerson : IOrganizationForPerson
	{
		private readonly IPersonOrganizationProvider _personOrganizationProvider;

		public OrganizationForPerson(IPersonOrganizationProvider personOrganizationProvider)
		{
			_personOrganizationProvider = personOrganizationProvider;
		}
		//not used ???
		public PersonOrganizationData GetOrganization(Guid personId, string tenant)
		{
			PersonOrganizationData ret;
			return _personOrganizationProvider.PersonOrganizationData(tenant).TryGetValue(personId, out ret) ? 
				ret : 
				null;
		}
	}
}