using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NPOI.SS.Formula.Functions;
using Teleopti.Ccc.Domain.ApplicationLayer.ExportSchedule;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ViewModels
{
	public class HistoricalOverviewViewModelBuilder
	{
		private readonly IAgentStateReadModelReader _agentStateReader;
		private readonly ICommonAgentNameProvider _nameDisplaySetting;
		private readonly INow _now;
		private readonly IAgentAdherenceDayLoader _agentAdherenceDayLoader;
		private readonly IHistoricalOverviewReadModelReader _reader;
		private readonly IPersonRepository _persons;
		private readonly ITeamRepository _teams;

		public HistoricalOverviewViewModelBuilder(
			IAgentStateReadModelReader agentStateReader,
			ICommonAgentNameProvider nameDisplaySetting,
			INow now,
			IAgentAdherenceDayLoader agentAdherenceDayLoader,
			IHistoricalOverviewReadModelReader reader,
			IPersonRepository persons, ITeamRepository teams)
		{
			_agentStateReader = agentStateReader;
			_nameDisplaySetting = nameDisplaySetting;
			_now = now;
			_agentAdherenceDayLoader = agentAdherenceDayLoader;
			_reader = reader;
			_persons = persons;
			_teams = teams;
		}

		//	public IEnumerable<HistoricalOverviewTeamViewModel> Build(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds) => buildViewModelQuickAndDirty(siteIds, teamIds);
		public IEnumerable<HistoricalOverviewTeamViewModel> Build(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds) => buildViewModel(siteIds, teamIds);

		private IEnumerable<HistoricalOverviewTeamViewModel> buildViewModel(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds)
		{
			var teams = siteIds?.SelectMany(x => _teams.FindTeamsForSite(x));

			if (teams != null)
			{
				if (teamIds != null)
					teams = teams.Union(_teams.FindTeams(teamIds));
			}
			else
				teams = _teams.FindTeams(teamIds);


			var sevenDays = _now.UtcDateTime().Date.AddDays(-7).DateRange(7);
			var period = new DateOnlyPeriod(new DateOnly(sevenDays.First()), new DateOnly(sevenDays.Last()));

			var persons = teams.SelectMany(t => _persons.FindPeopleBelongTeam(t, period));
			var readModels = _reader.Read(persons.Select(p => p.Id.Value).ToArray());

			var dayStuff = from day in sevenDays
				from person in persons
				let pp = person.Period(day.ToDateOnly())
				select new
				{
					SiteTeamName = pp.Team.SiteAndTeam,
					PersonId = person.Id,
					Name = _nameDisplaySetting.CommonAgentNameSettings.BuildFor(person.Name.FirstName, person.Name.LastName, null),
					Day = day
				};

			return from rm in dayStuff
				group rm by rm.SiteTeamName
				into teamGroupedAgents
				select new HistoricalOverviewTeamViewModel
				{
					Name = teamGroupedAgents.First().SiteTeamName,
					Agents = (from agent in teamGroupedAgents
						group agent by agent.PersonId
						into agentGrouping
						let agentInGroup = agentGrouping.First()
						select new HistoricalOverviewAgentViewModel
						{
							Id = agentInGroup.PersonId.Value,
							Name = agentInGroup.Name,
							Days = from day in sevenDays
								select new HistoricalOverviewDayViewModel
								{
									Date = day.Date.ToString("yyyyMMdd"),
									DisplayDate = day.Date.ToString("MM") + "/" + day.Date.ToString("dd"),
									Adherence = getAdherence(readModels, agentInGroup.PersonId.Value),
									WasLateForWork = wasLateForWork(readModels, agentInGroup.PersonId.Value)
								},
							LateForWork = new HistoricalOverviewLateForWorkViewModel
							{
								Count = sevenDays.Count(day => getLateforWork(day, readModels, agentInGroup.PersonId.Value) > 0),
								TotalMinutes = sevenDays.Sum(day => getLateforWork(day, readModels, agentInGroup.PersonId.Value))
							}
						})
				};
		}

		private static int getLateforWork(DateTime day, IEnumerable<HistoricalOverviewReadModel> rm, Guid agentId)
		{
			if (rm.IsNullOrEmpty())
				return 0;
			return rm.SingleOrDefault(r => r.PersonId == agentId && r.Date == day.ToDateOnly())?.MinutesLateForWork ?? 0;
		}

		private static bool wasLateForWork(IEnumerable<HistoricalOverviewReadModel> rm, Guid agentId)
		{
			return calcHistoricalOverview(rm, agentId) != null && calcHistoricalOverview(rm, agentId).WasLateForWork;
		}

		private static int? getAdherence(IEnumerable<HistoricalOverviewReadModel> rm, Guid agentId)
		{
			return calcHistoricalOverview(rm, agentId)?.Adherence;
		}

		private static HistoricalOverviewReadModel calcHistoricalOverview(IEnumerable<HistoricalOverviewReadModel> rm, Guid agentId)
		{
			return rm.IsNullOrEmpty()
				? null
				: rm.Last(r => r.PersonId == agentId);
		}

		private IEnumerable<HistoricalOverviewTeamViewModel> buildViewModelQuickAndDirty(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds)
		{
			var filter = new AgentStateFilter() {TeamIds = teamIds, SiteIds = siteIds};

			var agents = from a in _agentStateReader.Read(filter)
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
							let loadedDay = _agentAdherenceDayLoader.LoadUntilNow(agent.Id, d.ToDateOnly())
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