using System;
using System.Linq;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class TeamIdForPerson : ITeamIdForPerson
	{
		private readonly IPersonOrganizationProvider _personOrganizationProvider;

		public TeamIdForPerson(IPersonOrganizationProvider personOrganizationProvider)
		{
			_personOrganizationProvider = personOrganizationProvider;
		}

		public Guid GetTeamId(Guid personId)
		{
			return _personOrganizationProvider.LoadAll()
				.Single(x => x.PersonId == personId).TeamId;
		}
	}
}