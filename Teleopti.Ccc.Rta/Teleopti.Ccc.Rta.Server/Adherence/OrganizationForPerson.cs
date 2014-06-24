using System;
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
			PersonOrganizationData ret;
			return _personOrganizationProvider.LoadAll().TryGetValue(personId, out ret) ? 
				ret : 
				null;
		}
	}
}