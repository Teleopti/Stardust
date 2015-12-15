using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModelBuilders;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public interface IGetAgents
	{
		IEnumerable<AgentViewModel> ForSites(Guid[] siteIds);
		IEnumerable<AgentViewModel> ForTeams(Guid[] teamIds);
	}

	public class GetAgents : IGetAgents
	{
		private readonly IPersonRepository _personRepository;
		private readonly ISiteRepository _siteRepository;
		private readonly ITeamRepository _teamRepository;
		private readonly INow _now;

		public GetAgents(IPersonRepository personRepository, ISiteRepository siteRepository, ITeamRepository teamRepository, INow now)
		{
			_personRepository = personRepository;
			_siteRepository = siteRepository;
			_teamRepository = teamRepository;
			_now = now;
		}

		public IEnumerable<AgentViewModel> ForSites(Guid[] siteIds)
		{
			var today = _now.LocalDateOnly();
			var sites = _siteRepository.LoadAll().Where(x => siteIds.Contains(x.Id.Value));
			var teams = new List<ITeam>();
			sites.ForEach(x => teams.AddRange(x.TeamCollection));
			var agents = new List<AgentViewModel>();
			teams.ForEach(t =>
				agents.AddRange(
					_personRepository.FindPeopleBelongTeam(t, new DateOnlyPeriod(today, today)).Select(p => new AgentViewModel
					{
						Name = p.Name.ToString(),
						PersonId = p.Id.Value,
						TeamName = t.Description.Name,
						TeamId = t.Id.Value.ToString(),
						SiteName = t.Site.Description.Name,
						SiteId = t.Site.Id.ToString()
					})));
			return agents;
		}

		public IEnumerable<AgentViewModel> ForTeams(Guid[] teamIds)
		{
			var today = _now.LocalDateOnly();
			var teams = _teamRepository.LoadAll().Where(x => teamIds.Contains(x.Id.Value));
			var agents = new List<AgentViewModel>();
			teams.ForEach(t =>
				agents.AddRange(
					_personRepository.FindPeopleBelongTeam(t, new DateOnlyPeriod(today, today)).Select(p => new AgentViewModel
					{
						Name = p.Name.ToString(),
						PersonId = p.Id.Value,
						TeamName = t.Description.Name,
						TeamId = t.Id.Value.ToString(),
						SiteName = t.Site.Description.Name,
						SiteId = t.Site.Id.ToString()
					})));
			return agents;
		}
	}
}