using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class AuthorizeMyTeam : IAuthorizeAvailableData
	{
		public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IPerson person)
		{
			var targetPeriod = person.Period(dateOnly);
			
			if (targetPeriod==null || targetPeriod.Team ==null)
			{
				return false;
			}

			return queryingPerson.BelongsToTeam(targetPeriod.Team,dateOnly);
		}

		public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ITeam team)
		{
			if (team == null)
			{
				return false;
			}

			return queryingPerson.BelongsToTeam(team, dateOnly);
		}

		public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ISite site)
		{
			return false;
		}

		public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, Guid businessUnitId)
		{
			return false;
		}

		public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IPersonAuthorization authorization)
		{
			return queryingPerson.BelongsToTeam(authorization.TeamId.GetValueOrDefault(), dateOnly);
		}

		public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ITeamAuthorization authorization)
		{
			return queryingPerson.BelongsToTeam(authorization.TeamId, dateOnly);
		}

		public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ISiteAuthorization authorization)
		{
			return false;
		}
	}
}