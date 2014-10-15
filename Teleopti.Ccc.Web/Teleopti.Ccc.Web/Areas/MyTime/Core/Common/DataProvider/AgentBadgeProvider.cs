using System;
using System.Collections.Generic;
using System.Linq;
using DotNetOpenAuth.Messaging;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class AgentBadgeProvider: IAgentBadgeProvider
	{
		private readonly IAgentBadgeRepository _repository;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IPersonRepository _personRepository;
		private readonly IBadgeSettingProvider _agentBadgeSettingsProvider;

		public AgentBadgeProvider(IAgentBadgeRepository repository, IPermissionProvider permissionProvider, IPersonRepository personRepository, IBadgeSettingProvider agentBadgeSettingsProvider)
		{
			_repository = repository;
			_permissionProvider = permissionProvider;
			_personRepository = personRepository;
			_agentBadgeSettingsProvider = agentBadgeSettingsProvider;
		}

		public IEnumerable<AgentBadgeOverview> GetPermittedAgents(DateOnly date, string functionPath)
		{
			var agentBadgeList = new List<AgentBadgeOverview>();
			var setting = _agentBadgeSettingsProvider.GetBadgeSettings();
			var agentBadges = _repository.GetAllAgentBadges() ?? new IAgentBadge[] { };
			var personGuidList = agentBadges.Select(item => item.Person).ToList();

			var personList = _personRepository.FindPeople(personGuidList);
			var permittedPersonList = (from t in personList
				where _permissionProvider.HasPersonPermission(functionPath, date, t)
				select t).ToArray();

			var permittedAgentBadgeList= (from a in agentBadges
				    join p in permittedPersonList on a.Person equals p.Id
					select a).ToArray();

			foreach(var p in permittedPersonList)
			{
				var agentOverviewItem = new AgentBadgeOverview();
				agentOverviewItem.AgentName = p.Name.ToString();
				foreach (var agent in permittedAgentBadgeList.Where(agent => p.Id == agent.Person))
				{
					agentOverviewItem.Gold += agent.GetGoldBadge(setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate);
					agentOverviewItem.Silver += agent.GetSilverBadge(setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate);
					agentOverviewItem.Bronze += agent.GetBronzeBadge(setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate);
				}
				agentBadgeList.Add(agentOverviewItem);
			}

			return agentBadgeList;		
		}
	}
}


