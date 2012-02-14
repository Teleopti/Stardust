using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using SdkTestClientWin.Sdk;

namespace SdkTestClientWin.Domain
{
    public class Team
    {
        private readonly TeamDto _dto;
        private readonly Site _site;
        private IList<Agent> _agentColl;

        private Team(){}

        public Team(TeamDto dto, Site site)
        {
            _dto = dto;
            _site = site;
            _agentColl = new List<Agent>();
        }

        public TeamDto Dto
        {
            get { return _dto; }
        }

        public Site Site
        {
            get { return _site; }
        }

        public ReadOnlyCollection<Agent> AgentColl
        {
            get { return new ReadOnlyCollection<Agent>(_agentColl); }
        }

        public Agent CreateAndAddAgent(PersonDto personDto)
        {
            Agent agent = new Agent(personDto, this);
            _agentColl.Add(agent);
            return agent;
        }
    }
}