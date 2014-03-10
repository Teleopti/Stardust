using System;
using System.Linq;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class TeamIdForPerson : ITeamIdForPerson
	{
		private readonly IPersonOrganizationReader _personOrganizationReader;

		public TeamIdForPerson(IPersonOrganizationReader personOrganizationReader)
		{
			_personOrganizationReader = personOrganizationReader;
		}

		public Guid GetTeamId(Guid personId)
		{
			var personData = _personOrganizationReader.LoadAll();
			return personData.Single(x => x.PersonId == personId).TeamId;
		}
	}
}