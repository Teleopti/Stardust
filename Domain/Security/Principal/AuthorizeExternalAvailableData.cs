using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class AuthorizeExternalAvailableData : IAuthorizeAvailableData
    {
        private readonly Guid[] _availableTeams;
		private readonly Guid[] _availableSites;
		private readonly Guid[] _availableBusinessUnits;

        private AuthorizeExternalAvailableData(IAvailableData availableData)
        {
            _availableTeams = availableData.AvailableTeams.Select(t => t.Id.GetValueOrDefault()).ToArray();
            _availableSites = availableData.AvailableSites.Select(s => s.Id.GetValueOrDefault()).ToArray();
            _availableBusinessUnits = availableData.AvailableBusinessUnits.Select(b => b.Id.GetValueOrDefault()).ToArray();
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
            var result = checkTeam(team);
            if (result.HasValue)
            {
                return result.Value;
            }

            return Check(queryingPerson, dateOnly, team.Site);
        }

        public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, ISite site)
        {
            var result = checkSite(site);
            if (result.HasValue)
            {
                return result.Value;
            }

            IBusinessUnit businessUnit = site.GetOrFillWithBusinessUnit_DONTUSE();
            return Check(queryingPerson, dateOnly, businessUnit.Id.GetValueOrDefault());
        }

		public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, Guid businessUnitId)
		{
            var result = checkBusinessUnit(businessUnitId);

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

		private bool? checkTeam(ITeam team)
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

        private bool? checkSite(ISite site)
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

        private bool? checkBusinessUnit(Guid businessUnitId)
        {
            if (businessUnitId == Guid.Empty)
            {
                return false;
            }

            if (_availableBusinessUnits.Contains(businessUnitId))
            {
                return true;
            }
            return null;
        }
    }
}
