using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class AuthorizeExternalAvailableData : IAuthorizeAvailableData
    {
        private readonly IEnumerable<Guid> _availableTeams;
		private readonly IEnumerable<Guid> _availableSites;
		private readonly IEnumerable<Guid> _availableBusinessUnits;
		private readonly IEnumerable<Guid> _availablePersons;

        public AuthorizeExternalAvailableData(IAvailableData availableData)
        {
            _availablePersons = availableData.AvailablePersons.Select(p => p.Id.GetValueOrDefault()).ToList();
            _availableTeams = availableData.AvailableTeams.Select(t => t.Id.GetValueOrDefault()).ToList();
            _availableSites = availableData.AvailableSites.Select(s => s.Id.GetValueOrDefault()).ToList();
            _availableBusinessUnits = availableData.AvailableBusinessUnits.Select(b => b.Id.GetValueOrDefault()).ToList();
        }

        public bool Check(IOrganisationMembership queryingPerson, DateOnly dateOnly, IPerson person)
        {
            if (_availablePersons.Contains(person.Id.GetValueOrDefault()))
            {
                return true;
            }

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

    	public bool Check(IOrganisationMembershipWithId queryingPerson, DateOnly dateOnly, IAuthorizeOrganisationDetail authorizeOrganisationDetail)
    	{
    		return _availablePersons.Contains(authorizeOrganisationDetail.PersonId) ||
    		       _availableTeams.Contains(authorizeOrganisationDetail.TeamId.GetValueOrDefault()) ||
    		       _availableSites.Contains(authorizeOrganisationDetail.SiteId.GetValueOrDefault()) ||
    		       _availableBusinessUnits.Contains(authorizeOrganisationDetail.BusinessUnitId);
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
