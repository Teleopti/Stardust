using System;
using System.Collections.Generic;
using System.Linq;
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
				return forSkillAndSite(filter.SiteIds.ToArray(), filter.SkillIds.ToArray());
			if (filter.SiteIds != null)
				return forSites(filter.SiteIds.ToArray());
			if (filter.TeamIds != null && filter.SkillIds != null)
				return forSkillAndTeam(filter.TeamIds.ToArray(), filter.SkillIds.ToArray());
			if (filter.TeamIds != null)
				return forTeams(filter.TeamIds.ToArray());
			return forSkill(filter.SkillIds.ToArray());
		}

		private IEnumerable<AgentViewModel> forSites(IEnumerable<Guid> siteIds)
		{
			var today = new DateOnly(_now.UtcDateTime());
			var lookup = _siteRepository.LoadAll().ToLookup(s => s.Id.GetValueOrDefault());
			var commonAgentNameSettings = _commonAgentNameProvider.CommonAgentNameSettings;
			return
				(
					from siteId in siteIds
					let site = lookup[siteId].First()
					from team in site.TeamCollection
					from person in _personRepository.FindPeopleBelongTeam(team, new DateOnlyPeriod(today, today))
					select new AgentViewModel
					{
						Name = commonAgentNameSettings.BuildCommonNameDescription(person),
						PersonId = person.Id.Value,
						TeamName = team.Description.Name,
						TeamId = team.Id.Value.ToString(),
						SiteName = site.Description.Name,
						SiteId = site.Id.ToString()
					}).ToArray();
		}

		private IEnumerable<AgentViewModel> forTeams(IEnumerable<Guid> teamIds)
		{
			var today = new DateOnly(_now.UtcDateTime());
			var commonAgentNameSettings = _commonAgentNameProvider.CommonAgentNameSettings;
			return
				(
					from team in _teamRepository.FindTeams(teamIds)
					from person in _personRepository.FindPeopleBelongTeam(team, new DateOnlyPeriod(today, today))
					select new AgentViewModel
					{
						Name = commonAgentNameSettings.BuildCommonNameDescription(person),
						PersonId = person.Id.Value,
						TeamName = team.Description.Name,
						TeamId = team.Id.Value.ToString(),
						SiteName = team.Site.Description.Name,
						SiteId = team.Site.Id.ToString()
					}).ToArray();
		}

		private IEnumerable<AgentViewModel> forSkill(IEnumerable<Guid> skills)
		{
			var commonAgentNameSettings = _commonAgentNameProvider.CommonAgentNameSettings;
			return
				(
					from skill in skills
					from grouping in _groupingReadOnlyRepository.DetailsForGroup(skill, _now.LocalDateOnly())
					select new AgentViewModel
					{
						Name =
							commonAgentNameSettings.BuildCommonNameDescription(grouping.FirstName, grouping.LastName,
								grouping.EmploymentNumber),
						PersonId = grouping.PersonId,
						SiteId = grouping.SiteId.ToString(),
						SiteName = _siteRepository.Load(grouping.SiteId.Value).Description.Name,
						TeamId = grouping.TeamId.ToString(),
						TeamName = _teamRepository.Load(grouping.TeamId.Value).Description.Name
					}).ToArray();
		}

		private IEnumerable<AgentViewModel> forSkillAndSite(IEnumerable<Guid> sites, IEnumerable<Guid> skills)
		{
			var commonAgentNameSettings = _commonAgentNameProvider.CommonAgentNameSettings;
			return
				(
					from site in sites
					from skill in skills
					from grouping in _groupingReadOnlyRepository.DetailsForGroup(skill, _now.LocalDateOnly())
					where site == grouping.SiteId.Value
					select new AgentViewModel
					{
						PersonId = grouping.PersonId,
						Name =
							commonAgentNameSettings.BuildCommonNameDescription(grouping.FirstName, grouping.LastName,
								grouping.EmploymentNumber),
						SiteId = grouping.SiteId.ToString(),
						SiteName = _siteRepository.Load(grouping.SiteId.Value).Description.Name,
						TeamId = grouping.TeamId.ToString(),
						TeamName = _teamRepository.Load(grouping.TeamId.Value).Description.Name
					}
					).ToArray();
		}

		private IEnumerable<AgentViewModel> forSkillAndTeam(IEnumerable<Guid> teams, IEnumerable<Guid> skills)
		{
			var commonAgentNameSettings = _commonAgentNameProvider.CommonAgentNameSettings;
			return
				(
					from skill in skills
					from team in teams
					from grouping in _groupingReadOnlyRepository.DetailsForGroup(skill, _now.LocalDateOnly())
					where team == grouping.TeamId.Value
					select new AgentViewModel
					{
						Name =
							commonAgentNameSettings.BuildCommonNameDescription(grouping.FirstName, grouping.LastName,
								grouping.EmploymentNumber),
						PersonId = grouping.PersonId,
						SiteId = grouping.SiteId.ToString(),
						SiteName = _siteRepository.Load(grouping.SiteId.Value).Description.Name,
						TeamId = grouping.TeamId.ToString(),
						TeamName = _teamRepository.Load(grouping.TeamId.Value).Description.Name
					}).ToArray();
		}

	}
}