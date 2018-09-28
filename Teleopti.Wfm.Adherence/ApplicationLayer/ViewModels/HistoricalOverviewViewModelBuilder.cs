﻿using System;
using System.Collections.Generic;
using System.Linq;
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
		private readonly ICommonAgentNameProvider _nameDisplaySetting;
		private readonly INow _now;
		private readonly IHistoricalOverviewReadModelReader _reader;
		private readonly IPersonRepository _persons;
		private readonly ITeamRepository _teams;

		public HistoricalOverviewViewModelBuilder(
			ICommonAgentNameProvider nameDisplaySetting,
			INow now,
			IHistoricalOverviewReadModelReader reader,
			IPersonRepository persons, ITeamRepository teams)
		{
			_nameDisplaySetting = nameDisplaySetting;
			_now = now;
			_reader = reader;
			_persons = persons;
			_teams = teams;
		}
	
		public IEnumerable<HistoricalOverviewTeamViewModel> Build(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds) 
		{
			var teams = getTeams(siteIds, teamIds);
			var sevenDays = _now.UtcDateTime().Date.AddDays(-7).DateRange(7).ToArray();
			var period = new DateOnlyPeriod(new DateOnly(sevenDays.First()), new DateOnly(sevenDays.Last()));
			var persons = teams.SelectMany(t => _persons.FindPeopleBelongTeam(t, period)).ToArray();
			var firstDay = sevenDays.First().ToDateOnly();
			var lastDay = sevenDays.Last().ToDateOnly();			
			var readModel = _reader.Read(persons.Select(p => p.Id.Value))
										.Where(x => x.Date >= firstDay && 
													x.Date <= lastDay)
										.ToArray();

			var agentsPerDayGroupedOnTeam = (from agentDay in readModel
				let person = persons.FirstOrDefault(p => p.Id == agentDay.PersonId)
				let pp = person?.Period(agentDay.Date)
				where pp != null
				select new 
				{
					TeamId = pp.Team.Id,
					SiteTeamName = pp.Team.SiteAndTeam,
					agentDay.PersonId,
					Name = _nameDisplaySetting.CommonAgentNameSettings.BuildFor(person.Name.FirstName, person.Name.LastName, null),
					Day = agentDay.Date,
					Adherence = calculateAdherence(agentDay.SecondsInAdherence, agentDay.SecondsOutOfAdherence),
					agentDay.WasLateForWork,
					agentDay.MinutesLateForWork,
					agentDay.SecondsInAdherence,
					agentDay.SecondsOutOfAdherence
				}).OrderBy(ai => ai.SiteTeamName).ThenBy(ai => ai.Name).ToLookup(ai => ai.TeamId);

			
			return (from agentsOnTeam in agentsPerDayGroupedOnTeam
					select new HistoricalOverviewTeamViewModel
					{
						Name = agentsOnTeam.First().SiteTeamName,
						Agents = (from agent in agentsOnTeam
							group agent by agent.PersonId
							into groupedAgent
							select new HistoricalOverviewAgentViewModel
							{
								Id = groupedAgent.First().PersonId,
								Name = groupedAgent.First().Name,
								Days = (from day in sevenDays
									let agentDay = groupedAgent.FirstOrDefault(a => a.Day == day.ToDateOnly())
									select new HistoricalOverviewDayViewModel
									{
										Date = day.ToString("yyyyMMdd"),
										DisplayDate = day.ToString("MM") + "/" + day.ToString("dd"),
										Adherence = agentDay?.Adherence,
										WasLateForWork = agentDay?.WasLateForWork ?? false
									}).ToArray(),
								LateForWork = new HistoricalOverviewLateForWorkViewModel()
								{
									Count = groupedAgent.Count(a => a.WasLateForWork),
									TotalMinutes = groupedAgent.Sum(a => a.MinutesLateForWork)
								},
								IntervalAdherence = calculateAdherence(
									groupedAgent.All(a => a.SecondsInAdherence == null) ? null : groupedAgent.Sum(a => a.SecondsInAdherence),
									groupedAgent.All(a => a.SecondsOutOfAdherence == null) ? null : groupedAgent.Sum(a => a.SecondsOutOfAdherence))
							})
					}
				).ToArray();
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

		private static int? calculateAdherence(int? secondsInAdherence, int? secondsOutOfAdherence)
		{
			if (secondsInAdherence == null)
				return null;

			var inAdherence = Convert.ToDouble(secondsInAdherence);
			var outAdherence = Convert.ToDouble(secondsOutOfAdherence);
			var expectedWorkTime = inAdherence + outAdherence;

			if (expectedWorkTime.Equals(0.0))
				return null;
			
			//Financial Rounding
			return  Convert.ToInt32((inAdherence / expectedWorkTime) * 100);		
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