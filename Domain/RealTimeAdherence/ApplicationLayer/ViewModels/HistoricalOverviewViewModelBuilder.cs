using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ViewModels
{
	public class HistoricalOverviewViewModelBuilder
	{
		private readonly IAgentStateReadModelReader _reader;
		private readonly ICommonAgentNameProvider _nameDisplaySetting;
		private readonly INow _now;

		public HistoricalOverviewViewModelBuilder(
			IAgentStateReadModelReader reader,
			ICommonAgentNameProvider nameDisplaySetting,
			INow now)
		{
			_reader = reader;
			_nameDisplaySetting = nameDisplaySetting;
			_now = now;
		}

		public IEnumerable<HistoricalOverviewTeamViewModel> Build(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds) => buildViewModelQuickAndDirty(siteIds, teamIds);

		private IEnumerable<HistoricalOverviewTeamViewModel> buildViewModelQuickAndDirty(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds)
		{
			var filter = new AgentStateFilter() {TeamIds = teamIds, SiteIds = siteIds};

			var agents = from a in _reader.Read(filter)
				select new
				{
					SiteAndTeam = a.SiteName + "/" + a.TeamName,
					Name = _nameDisplaySetting.CommonAgentNameSettings.BuildFor(a.FirstName, a.LastName, null)
				};

			var sevenDays = _now.UtcDateTime().Date.AddDays(-7).DateRange(7);

			var teams =
				from agent in agents
				group agent by agent.SiteAndTeam
				into teamGroupedAgents
				select new HistoricalOverviewTeamViewModel
				{
					Name = teamGroupedAgents.First().SiteAndTeam,
					Agents = from agent in teamGroupedAgents
						select new HistoricalOverviewAgentViewModel()
						{
							Name = agent.Name,
							Days = sevenDays.Select(d => new HistoricalOverviewDayViewModel()
							{
								Date = d.Date.ToString("yyyyMMdd"),
								DisplayDate = d.Date.ToString("MM") + "/"  + d.Date.ToString("dd")
							})
						},
				};

			return teams.ToArray();
		}
	}

	public class HistoricalOverviewTeamViewModel
	{
		public string Name { get; set; }
		public IEnumerable<HistoricalOverviewAgentViewModel> Agents { get; set; }
	}

	public class HistoricalOverviewAgentViewModel
	{
		public string Name { get; set; }
		public IEnumerable<HistoricalOverviewDayViewModel> Days { get; set; }
	}

	public class HistoricalOverviewDayViewModel
	{
		public string Date { get; set; }
		public string DisplayDate { get; set; }
	}
}