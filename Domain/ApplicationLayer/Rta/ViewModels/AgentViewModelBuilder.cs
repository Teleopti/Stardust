using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class AgentViewModelBuilder
	{
		private readonly ICurrentAuthorization _permissionProvider;
		private readonly ICommonAgentNameProvider _commonAgentNameProvider;
		private readonly IPersonRepository _personRepository;
		private readonly ISiteRepository _siteRepository;
		private readonly ITeamRepository _teamRepository;
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private readonly INow _now;

		public AgentViewModelBuilder(
			ICurrentAuthorization permissionProvider,
			ICommonAgentNameProvider commonAgentNameProvider,
			IPersonRepository personRepository,
			ISiteRepository siteRepository,
			ITeamRepository teamRepository,
			IGroupingReadOnlyRepository groupingReadOnlyRepository,
			INow now)
		{
			_permissionProvider = permissionProvider;
			_commonAgentNameProvider = commonAgentNameProvider;
			_personRepository = personRepository;
			_siteRepository = siteRepository;
			_teamRepository = teamRepository;
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
			_now = now;
		}
		
		public IEnumerable<AgentViewModel> ForTeam(Guid teamId)
		{
			var team = _teamRepository.Get(teamId);
			var isPermitted = _permissionProvider.Current().IsPermitted(
				DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview,
				_now.LocalDateOnly(),
				team);
			if (!isPermitted)
			{
				throw new PermissionException();
			}

			var commonAgentNameSettings = _commonAgentNameProvider.CommonAgentNameSettings;

			var today = _now.LocalDateOnly();
			return _personRepository.FindPeopleBelongTeam(team, new DateOnlyPeriod(today, today))
					.Select(
						x =>
							new AgentViewModel
							{
								PersonId = x.Id.GetValueOrDefault(),
								Name = commonAgentNameSettings.BuildCommonNameDescription(_personRepository.Get(x.Id.GetValueOrDefault())),
								SiteId = team.Site.Id.ToString(),
								SiteName = team.Site.Description.Name,
								TeamId = team.Id.ToString(),
								TeamName = team.Description.Name
							}).ToArray();
		}




		public IEnumerable<AgentViewModel> For(ViewModelFilter filter)
		{
			if (filter.SiteIds != null && filter.SkillIds != null)
				return forSkillAndSite(filter.SkillIds.ToArray(), filter.SiteIds.ToArray());
			if (filter.SiteIds != null)
				return forSites(filter.SiteIds.ToArray());
			if (filter.TeamIds != null && filter.SkillIds != null)
				return forSkillAndTeam(filter.SkillIds.ToArray(), filter.TeamIds.ToArray());
			if (filter.TeamIds != null)
				return forTeams(filter.TeamIds.ToArray());
			return forSkill(filter.SkillIds.ToArray());
		}

		private IEnumerable<AgentViewModel> forSites(IEnumerable<Guid> siteIds)
		{
			var today = new DateOnly(_now.UtcDateTime());
			var sites = _siteRepository.LoadAll().Where(x => siteIds.Contains(x.Id.Value));
			var teams = new List<ITeam>();
			sites.ForEach(x => teams.AddRange(x.TeamCollection));
			var agents = new List<AgentViewModel>();
			var commonAgentNameSettings = _commonAgentNameProvider.CommonAgentNameSettings;
			teams.ForEach(t =>
				agents.AddRange(
					_personRepository.FindPeopleBelongTeam(t, new DateOnlyPeriod(today, today))
						.Select(p => new AgentViewModel
						{
							Name = commonAgentNameSettings.BuildCommonNameDescription(_personRepository.Get(p.Id.GetValueOrDefault())),
							PersonId = p.Id.Value,
							TeamName = t.Description.Name,
							TeamId = t.Id.Value.ToString(),
							SiteName = t.Site.Description.Name,
							SiteId = t.Site.Id.ToString()
						})));
			return agents;
		}

		private IEnumerable<AgentViewModel> forTeams(IEnumerable<Guid> teamIds)
		{
			var today = new DateOnly(_now.UtcDateTime());
			var teams = _teamRepository.LoadAll().Where(x => teamIds.Contains(x.Id.Value));
			var agents = new List<AgentViewModel>();
			var commonAgentNameSettings = _commonAgentNameProvider.CommonAgentNameSettings;
			teams.ForEach(t =>
				agents.AddRange(
					_personRepository.FindPeopleBelongTeam(t, new DateOnlyPeriod(today, today)).Select(p => new AgentViewModel
					{
						Name = commonAgentNameSettings.BuildCommonNameDescription(_personRepository.Get(p.Id.GetValueOrDefault())),
						PersonId = p.Id.Value,
						TeamName = t.Description.Name,
						TeamId = t.Id.Value.ToString(),
						SiteName = t.Site.Description.Name,
						SiteId = t.Site.Id.ToString()
					})));
			return agents;
		}


		private IEnumerable<AgentViewModel> forSkill(IEnumerable<Guid> skill)
		{
			var commonAgentNameSettings = _commonAgentNameProvider.CommonAgentNameSettings;
			return skill.SelectMany(s =>
			{
				var readOnlyGroupDetails = _groupingReadOnlyRepository.DetailsForGroup(s, _now.LocalDateOnly());
				return readOnlyGroupDetails
					.Select(x => new AgentViewModel
					{
						PersonId = x.PersonId,
						Name = commonAgentNameSettings.BuildCommonNameDescription(x.FirstName, x.LastName, x.EmploymentNumber),
						SiteId = x.SiteId.ToString(),
						SiteName = _siteRepository.Load(x.SiteId.Value).Description.Name,
						TeamId = x.TeamId.ToString(),
						TeamName = _teamRepository.Load(x.TeamId.Value).Description.Name
					});
			})
			.ToArray();
		}

		private IEnumerable<AgentViewModel> forSkillAndTeam(IEnumerable<Guid> skill, IEnumerable<Guid> team)
		{
			var commonAgentNameSettings = _commonAgentNameProvider.CommonAgentNameSettings;
			return skill.SelectMany(s =>
			{
				return _groupingReadOnlyRepository.DetailsForGroup(s, _now.LocalDateOnly())
					.Where(x=> team.Any(t=>t==x.TeamId.Value))
					.Select(x => new AgentViewModel
					{
						PersonId = x.PersonId,
						Name = commonAgentNameSettings.BuildCommonNameDescription(x.FirstName, x.LastName, x.EmploymentNumber),
						SiteId = x.SiteId.ToString(),
						SiteName = _siteRepository.Load(x.SiteId.Value).Description.Name,
						TeamId = x.TeamId.ToString(),
						TeamName = _teamRepository.Load(x.TeamId.Value).Description.Name
					});
			}).ToArray();
		}

		private IEnumerable<AgentViewModel> forSkillAndSite(IEnumerable<Guid> skill, IEnumerable<Guid> site)
		{
			var commonAgentNameSettings = _commonAgentNameProvider.CommonAgentNameSettings;
			return skill.SelectMany(s =>
			{
				return _groupingReadOnlyRepository.DetailsForGroup(s, _now.LocalDateOnly())
					.Where(x => site.Any(si => si == x.SiteId.Value))
					.Select(x => new AgentViewModel
					{
						PersonId = x.PersonId,
						Name = commonAgentNameSettings.BuildCommonNameDescription(x.FirstName, x.LastName, x.EmploymentNumber),
						SiteId = x.SiteId.ToString(),
						SiteName = _siteRepository.Load(x.SiteId.Value).Description.Name,
						TeamId = x.TeamId.ToString(),
						TeamName = _teamRepository.Load(x.TeamId.Value).Description.Name
					});
			}).ToArray();
		}

	}
}