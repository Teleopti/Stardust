﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels;
using Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Wfm.Adherence.ApplicationLayer.ViewModels
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
			var teams = getTeams(siteIds, teamIds);
			var sevenDays = _now.UtcDateTime().Date.AddDays(-7).DateRange(7).ToArray();
			var period = new DateOnlyPeriod(new DateOnly(sevenDays.First()), new DateOnly(sevenDays.Last()));
			var persons = teams.SelectMany(t => _persons.FindPeopleBelongTeam(t, period)).ToArray();
			var readModel = _reader.Read(persons.Select(p => p.Id.Value).ToArray()).Where(x => x.Date != _now.UtcDateTime().ToDateOnly());
			var agentsOnTeamForAllDays = groupAgentsOnTeamForAllDays(sevenDays, persons.Where(p => readModel.Any(r => r.PersonId == p.Id.Value)).ToArray());

			return (from agentsOnTeam in agentsOnTeamForAllDays
				select new HistoricalOverviewTeamViewModel
				{
					Name = agentsOnTeam.First().SiteTeamName,
					Agents = (from agentForAllDays in agentsOnTeam
						group agentForAllDays by agentForAllDays.PersonId
						into agentGrouping
						let agentId = agentGrouping.First().PersonId.Value
						let agentName = agentGrouping.First().Name
						let readModelForPerson = readModel.Where(r => r.PersonId == agentId).ToArray()
						select new HistoricalOverviewAgentViewModel
						{
							Id = agentId,
							Name = agentName,
							Days = from day in sevenDays
								let readModelForPersonAndDay = readModelForPerson.FirstOrDefault(r => r.Date == day.ToDateOnly())
								select new HistoricalOverviewDayViewModel
								{
									Date = day.Date.ToString("yyyyMMdd"),
									DisplayDate = day.Date.ToString("MM") + "/" + day.Date.ToString("dd"),
									Adherence = calculateAdherence(readModelForPersonAndDay?.SecondsInAdherence, readModelForPersonAndDay?.SecondsOutOfAdherence),
									WasLateForWork = readModelForPersonAndDay != null && readModelForPersonAndDay.WasLateForWork
								},
							LateForWork = buildLateForWork(agentId, sevenDays, readModelForPerson),
							IntervalAdherence = calculateIntervalAdherence(agentId, sevenDays, readModelForPerson)
						}).ToArray()
				}).ToArray();
		}

		private IEnumerable<ITeam> getTeams(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds)
		{
			var teams = siteIds?.SelectMany(x => _teams.FindTeamsForSite(x));

			if (teams != null)
			{
				if (teamIds != null)
					teams = teams.Union(_teams.FindTeams(teamIds));
			}
			else
				teams = _teams.FindTeams(teamIds);

			return teams;
		}

		private IEnumerable<IGrouping<string, agentInfo>> groupAgentsOnTeamForAllDays(IEnumerable<DateTime> days, IPerson[] persons)
		{
			var agentsOnDayAndTeam = from day in days
				from person in persons
				let pp = person.Period(day.ToDateOnly())
				select new agentInfo
				{
					SiteTeamName = pp.Team.SiteAndTeam,
					PersonId = person.Id,
					Name = _nameDisplaySetting.CommonAgentNameSettings.BuildFor(person.Name.FirstName, person.Name.LastName, null),
					Day = day
				};

			var agentsOnTeamForAllDays = from agentByDay in agentsOnDayAndTeam
				group agentByDay by agentByDay.SiteTeamName
				into agentsGroupedOnTeam
				select agentsGroupedOnTeam;
			return agentsOnTeamForAllDays;
		}

		private HistoricalOverviewLateForWorkViewModel buildLateForWork(Guid agentId, IEnumerable<DateTime> days, IEnumerable<HistoricalOverviewReadModel> readModel)
		{
			var minutesForDaysWithLateForWork = calculateLateMinutesForDay(readModel, agentId, days).ToArray();
			return new HistoricalOverviewLateForWorkViewModel
			{
				Count = minutesForDaysWithLateForWork.Count(),
				TotalMinutes = minutesForDaysWithLateForWork.Sum()
			};
		}

		private IEnumerable<int> calculateLateMinutesForDay(IEnumerable<HistoricalOverviewReadModel> readModel, Guid agentId,  IEnumerable<DateTime> days)
		{
			return from day in days
				let lateMinutesForDay = getReadModelForPersonAndDay(readModel, agentId, day)?.MinutesLateForWork ?? 0
				where lateMinutesForDay > 0
				select lateMinutesForDay;
		}

		private int? calculateIntervalAdherence(Guid personId, IEnumerable<DateTime> days, IEnumerable<HistoricalOverviewReadModel> readModel)
		{
			var modelForPersonEachDay = days.Select(d => getReadModelForPersonAndDay(readModel, personId, d)).Where(rm => rm != null).ToList();
			
			if (modelForPersonEachDay.All(x => x.SecondsInAdherence == null))
				return null;

			var inAdherenceSum = modelForPersonEachDay.Where(model => model.SecondsInAdherence != null).Sum(model => model.SecondsInAdherence);
			var outOfAdherenceSum = modelForPersonEachDay.Where(model => model.SecondsOutOfAdherence != null).Sum(model => model.SecondsOutOfAdherence);

			return calculateAdherence(inAdherenceSum, outOfAdherenceSum);
		}

		private HistoricalOverviewReadModel getReadModelForPersonAndDay(IEnumerable<HistoricalOverviewReadModel> readModel, Guid agentId, DateTime day)
		{
			return readModel.IsNullOrEmpty()
				? null
				: readModel.SingleOrDefault(m => m.PersonId == agentId && m.Date == day.Date.ToDateOnly());
		}
		
		private static int? calculateAdherence(int? secondsInAdherence, int? secondsOutOfAdherence)
		{
			if (secondsInAdherence == null)
				return null;
			
			var inAdherence = Convert.ToDouble(secondsInAdherence);
			var outAdherence = Convert.ToDouble(secondsOutOfAdherence);
			
			var percent = (inAdherence / (inAdherence + outAdherence)) * 100;
			return Convert.ToInt32(percent);
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

		class agentInfo
		{
			public string SiteTeamName { get; set; }
			public Guid? PersonId { get; set; }
			public string Name { get; set; }
			public DateTime Day { get; set; }
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
		public int? IntervalAdherence { get; set; }
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