using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ViewModels
{
	public class HistoricalOverviewViewModelBuilder
	{
		private readonly IAgentStateReadModelReader _reader;
		private readonly ICommonAgentNameProvider _nameDisplaySetting;
		private readonly INow _now;
		private readonly IAgentAdherenceDayLoader _agentAdherenceDayLoader; 

		public HistoricalOverviewViewModelBuilder(
			IAgentStateReadModelReader reader,
			ICommonAgentNameProvider nameDisplaySetting,
			INow now,
			IAgentAdherenceDayLoader agentAdherenceDayLoader)
		{
			_reader = reader;
			_nameDisplaySetting = nameDisplaySetting;
			_now = now;
			_agentAdherenceDayLoader = agentAdherenceDayLoader;
		}

		public IEnumerable<HistoricalOverviewTeamViewModel> Build(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds) => buildViewModelQuickAndDirty(siteIds, teamIds);

		private IEnumerable<HistoricalOverviewTeamViewModel> buildViewModelQuickAndDirty(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds)
		{
			var filter = new AgentStateFilter() {TeamIds = teamIds, SiteIds = siteIds};

			var agents = from a in _reader.Read(filter)
				select new
				{
					Id = a.PersonId,
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
					Agents = (from agent in teamGroupedAgents
						let adherenceDays =
							from d in sevenDays
							let loadedDay = _agentAdherenceDayLoader.Load(agent.Id, d.ToDateOnly())
							let change = loadedDay.Changes().FirstOrDefault(change => change.LateForWork != null)
							let lateForWorkText = change != null ? change.LateForWork : "0"
							let minutesLateForWork = int.Parse(Regex.Replace(lateForWorkText, "[^0-9.]", ""))
							select new
							{
								LoadedDay = loadedDay,
								MinutesLateForWork = minutesLateForWork,
								d.Date
							}
						select new HistoricalOverviewAgentViewModel()
						{
							Id = agent.Id,
							Name = agent.Name,
							Days = (from day in adherenceDays
								select new HistoricalOverviewDayViewModel
								{
									Date = day.Date.ToString("yyyyMMdd"),
									DisplayDate = day.Date.ToString("MM") + "/" + day.Date.ToString("dd"),
									Adherence = day.LoadedDay.Percentage(),
									WasLateForWork = day.LoadedDay.Changes().Any(x => x.LateForWork != null)
								}).ToArray(),
							LateForWork = new HistoricalOverviewLateForWorkViewModel
							{
								Count = adherenceDays.Count(ad => ad.MinutesLateForWork > 0),
								TotalMinutes = adherenceDays.Sum(ad => ad.MinutesLateForWork)
							}
						}).ToArray(),
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
		public Guid Id { get; set; }
		public string Name { get; set; }
		public IEnumerable<HistoricalOverviewDayViewModel> Days { get; set; }
		public HistoricalOverviewLateForWorkViewModel LateForWork { get; set; }
		public int IntervalAdherence { get; set; }
		
	}

	public class HistoricalOverviewLateForWorkViewModel
	{
		public int Count { get; set; }
		public int TotalMinutes { get; set; }		
	}

	public class HistoricalOverviewDayViewModel
	{
		public string Date { get; set; }
		public string DisplayDate { get; set; }
		public int? Adherence { get; set; }
		public bool WasLateForWork { get; set; }
	}
}