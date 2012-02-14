using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using SdkTestClientWin.Sdk;

namespace SdkTestClientWin.Domain
{
    public class Site
    {
        private readonly SiteDto _dto;
        private IList<Team> teamColl;

        private Site(){}

        public Site(SiteDto dto)
        {
            _dto = dto; 
            teamColl = new List<Team>();
        }

        public SiteDto Dto
        {
            get { return _dto; }
        }

        public ReadOnlyCollection<Team> TeamColl
        {
            get { return new ReadOnlyCollection<Team>(teamColl); }
        }

        public Team CreateAndAddTeam(TeamDto teamDto)
        {
            Team team = new Team(teamDto, this);
            teamColl.Add(team);
            return team;
        }
    }
}