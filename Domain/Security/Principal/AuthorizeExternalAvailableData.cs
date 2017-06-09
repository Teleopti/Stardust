using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class AuthorizeExternalAvailableData : IAuthorizeAvailableData
    {
        private readonly IEnumerable<Guid> _availableTeams;
		private readonly IEnumerable<Guid> _availableSites;
		private readonly IEnumerable<Guid> _availableBusinessUnits;

        private AuthorizeExternalAvailableData(IAvailableData availableData)
        {
            _availableTeams = availableData.AvailableTeams.Select(t => t.Id.GetValueOrDefault()).ToList();
            _availableSites = availableData.AvailableSites.Select(s => s.Id.GetValueOrDefault()).ToList();
            _availableBusinessUnits = availableData.AvailableBusinessUnits.Select(b => b.Id.GetValueOrDefault()).ToList();
        }

	    public static IAuthorizeAvailableData Create(IAvailableData availableData)
	    {
		    return availableData.AvailableTeams.Any() || availableData.AvailableSites.Any() ||
							 availableData.AvailableBusinessUnits.Any()
			    ? (IAuthorizeAvailableData) new AuthorizeExternalAvailableData(availableData)
			    : new AuthorizeNone();
	    }

        public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IPerson person)
        {
            var period = person.Period(dateOnly);
            if (period==null)
            {
                return false;
            }

            ITeam team = period.Team;
            return Check(queryingPerson, dateOnly, team);
        }

        public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ITeam team)
        {
            var result = CheckTeam(team);
            if (result.HasValue)
            {
                return result.Value;
            }

            return Check(queryingPerson, dateOnly, team.Site);
        }

        public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ISite site)
        {
            var result = CheckSite(site);
            if (result.HasValue)
            {
                return result.Value;
            }

            IBusinessUnit businessUnit = site.BusinessUnit;
            return Check(queryingPerson, dateOnly, businessUnit);
        }

        public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IBusinessUnit businessUnit)
        {
            var result = CheckBusinessUnit(businessUnit);

            return result.GetValueOrDefault(false);
        }

    	public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IPersonAuthorization authorization)
    	{
    		return _availableTeams.Contains(authorization.TeamId.GetValueOrDefault()) ||
    		       _availableSites.Contains(authorization.SiteId.GetValueOrDefault()) ||
    		       _availableBusinessUnits.Contains(authorization.BusinessUnitId);
    	}

		public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ITeamAuthorization authorization)
		{
			return _availableTeams.Contains(authorization.TeamId) ||
				   _availableSites.Contains(authorization.SiteId) ||
				   _availableBusinessUnits.Contains(authorization.BusinessUnitId);
		}

	    public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ISiteAuthorization authorization)
	    {
			return _availableSites.Contains(authorization.SiteId) ||
				   _availableBusinessUnits.Contains(authorization.BusinessUnitId);
		}

		private bool? CheckTeam(ITeam team)
        {
            if (team == null)
            {
                return false;
            }

            if (_availableTeams.Contains(team.Id.GetValueOrDefault()))
            {
                return true;
            }
            return null;
        }

        private bool? CheckSite(ISite site)
        {
            if (site == null)
            {
                return false;
            }

            if (_availableSites.Contains(site.Id.GetValueOrDefault()))
            {
                return true;
            }
            return null;
        }

        private bool? CheckBusinessUnit(IBusinessUnit businessUnit)
        {
            if (businessUnit == null)
            {
                return false;
            }

            if (_availableBusinessUnits.Contains(businessUnit.Id.GetValueOrDefault()))
            {
                return true;
            }
            return null;
        }
    }
}
