using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class AuthorizeMySite : IAuthorizeAvailableData
	{
		public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IPerson person)
		{
			var targetPeriod = person.Period(dateOnly);
			
			if (targetPeriod == null || targetPeriod.Team == null || targetPeriod.Team.Site == null)
			{
				return false;
			}

			return queryingPerson.BelongsToSite(targetPeriod.Team.Site,dateOnly);
		}

		public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ITeam team)
		{
			if (team == null || team.Site == null)
			{
				return false;
			}

			return queryingPerson.BelongsToSite(team.Site,dateOnly);
		}

		public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ISite site)
		{
			if (site == null)
			{
				return false;
			}

			return queryingPerson.BelongsToSite(site,dateOnly);
		}

		public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, Guid businessUnitId)
		{
			return false;
		}

		public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IPersonAuthorization authorization)
		{
			return queryingPerson.BelongsToSite(authorization.SiteId.GetValueOrDefault(), dateOnly);
		}

		public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ITeamAuthorization authorization)
		{
			return queryingPerson.BelongsToSite(authorization.SiteId, dateOnly);
		}

		public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ISiteAuthorization authorization)
		{
			return queryingPerson.BelongsToSite(authorization.SiteId, dateOnly);
		}
	}
}