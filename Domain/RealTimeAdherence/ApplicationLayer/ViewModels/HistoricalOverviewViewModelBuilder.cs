﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using NPOI.HSSF.Util;
using NPOI.OpenXmlFormats.Dml;
using NPOI.SS.Formula.Functions;
using Teleopti.Ccc.Domain.ApplicationLayer.ExportSchedule;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
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


			var sevenDays = _now.UtcDateTime().Date.AddDays(-7).DateRange(7).ToArray();
			var period = new DateOnlyPeriod(new DateOnly(sevenDays.First()), new DateOnly(sevenDays.Last()));

			var persons = teams.SelectMany(t => _persons.FindPeopleBelongTeam(t, period)).ToArray();
			var readModels = _reader.Read(persons.Select(p => p.Id.Value).ToArray());

			var daysWithAgents = from day in sevenDays
				from person in persons
				let pp = person.Period(day.ToDateOnly())
				select new
				{
					SiteTeamName = pp.Team.SiteAndTeam,
					PersonId = person.Id,
					Name = _nameDisplaySetting.CommonAgentNameSettings.BuildFor(person.Name.FirstName, person.Name.LastName, null),
					Day = day
				};

			return (from dayWithAgent in daysWithAgents
				group dayWithAgent by dayWithAgent.SiteTeamName
				into agentsGroupedOnTeam
				select new HistoricalOverviewTeamViewModel
				{
					Name = agentsGroupedOnTeam.First().SiteTeamName,
					Agents = (from agent in agentsGroupedOnTeam
						group agent by agent.PersonId
						into agentGrouping
						let agentInGroup = agentGrouping.First()
						let agentId = agentInGroup.PersonId.Value
						let minutesForDaysWithLateForWork = from d in sevenDays
							let lateMinutesForDay = getLateforWork(readModels, agentId, d)
							where lateMinutesForDay > 0
							select lateMinutesForDay
						select new HistoricalOverviewAgentViewModel
						{
							Id = agentId,
							Name = agentInGroup.Name,
							Days = from day in sevenDays
								let readModelForPersonAndDay = getHistoricalOverview(readModels, agentId, day)
								select new HistoricalOverviewDayViewModel
								{
									Date = day.Date.ToString("yyyyMMdd"),
									DisplayDate = day.Date.ToString("MM") + "/" + day.Date.ToString("dd"),
									Adherence = readModelForPersonAndDay?.Adherence,
									WasLateForWork = readModelForPersonAndDay != null && readModelForPersonAndDay.WasLateForWork
								},
							LateForWork = new HistoricalOverviewLateForWorkViewModel
							{
								Count = minutesForDaysWithLateForWork.Count(),
								TotalMinutes = minutesForDaysWithLateForWork.Sum()
							},
							IntervalAdherence = calculateIntervalAdherence(sevenDays, readModels, agentId)
						}).ToArray()
				}).ToArray();
		}

		private static int? calculateIntervalAdherence(IEnumerable<DateTime> sevenDays, IEnumerable<HistoricalOverviewReadModel> readModels, Guid personId)
		{
			if (sevenDays.All(x => getHistoricalOverview(readModels, personId, x)?.Adherence == null))
				return null;
			
			var sumAdherences = sevenDays.Sum(d =>
			{
				var model = getHistoricalOverview(readModels, personId, d); 
				if ( model?.ShiftLength == null || model.Adherence == null)
					return 0;
				return model.Adherence * model.ShiftLength;
			});
		
			var sumPeriods = sevenDays.Sum(d =>
			{
				var model = getHistoricalOverview(readModels, personId, d); 
				if ( model == null || model.ShiftLength == null)
					return 0;
				
				return model.ShiftLength;
			});
			
			return sumAdherences / sumPeriods;
		}

		private static int getLateforWork(IEnumerable<HistoricalOverviewReadModel> rm, Guid agentId, DateTime day)
		{
			return getHistoricalOverview(rm, agentId, day)?.MinutesLateForWork ?? 0;
		}

		private static HistoricalOverviewReadModel getHistoricalOverview(IEnumerable<HistoricalOverviewReadModel> model, Guid agentId, DateTime day)
		{
			return model.IsNullOrEmpty()
				? null
				: model.SingleOrDefault(m => m.PersonId == agentId && m.Date == day.Date.ToDateOnly());
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