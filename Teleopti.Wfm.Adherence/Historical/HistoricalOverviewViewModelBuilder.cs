using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Wfm.Adherence.Historical.AgentAdherenceDay;
using Teleopti.Wfm.Adherence.Historical.Infrastructure;

namespace Teleopti.Wfm.Adherence.Historical
{
	public class HistoricalOverviewViewModelBuilder
	{
		private readonly ICommonAgentNameProvider _nameDisplaySetting;
		private readonly IHistoricalOverviewReadModelReader _reader;
		private readonly IPersonRepository _persons;
		private readonly ITeamRepository _teams;
		private readonly IUserNow _userNow;

		public HistoricalOverviewViewModelBuilder(
			ICommonAgentNameProvider nameDisplaySetting,
			IHistoricalOverviewReadModelReader reader,
			IPersonRepository persons, ITeamRepository teams, IUserNow userNow)
		{
			_nameDisplaySetting = nameDisplaySetting;
			_reader = reader;
			_persons = persons;
			_teams = teams;
			_userNow = userNow;
		}

		public IEnumerable<HistoricalOverviewTeamViewModel> Build(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds)
		{
			var teams = getTeams(siteIds, teamIds);
			// users in different timezones will see different 7 days...
			var userSevenDays = _userNow.Date().AddDays(-7)
				.DateRange(7)
				.Select(x => x.Date)
				.ToArray();
			var displayDays = from day in userSevenDays
				let displayDay = day.ToString("MM") + "/" + day.ToString("dd")
				select displayDay;
			var period = new DateOnlyPeriod(userSevenDays.First(), userSevenDays.Last());
			var persons = teams.SelectMany(t => _persons.FindPeopleBelongTeam(t, period)).ToArray();
			var firstDay = new DateOnly(userSevenDays.First());
			var lastDay = new DateOnly(userSevenDays.Last());
			var readModel = _reader.Read(persons.Select(p => p.Id.Value))
				.Where(x => x.Date >= firstDay &&
							x.Date <= lastDay)
				.ToArray();

			var agentsPerDayGroupedOnTeam = (from agentDay in readModel
				let person = persons.FirstOrDefault(p => p.Id == agentDay.PersonId)
				let pp = person?.Period(new Ccc.Domain.InterfaceLegacy.Domain.DateOnly(agentDay.Date.Date))
				where pp != null
				select new
				{
					TeamId = pp.Team.Id,
					SiteTeamName = pp.Team.SiteAndTeam,
					agentDay.PersonId,
					Name = _nameDisplaySetting.CommonAgentNameSettings.BuildFor(person.Name.FirstName, person.Name.LastName, null),
					Day = agentDay.Date.Date,
					Adherence = AdherencePercentageCalculation.Calculate(agentDay.SecondsInAdherence, agentDay.SecondsOutOfAdherence),
					agentDay.WasLateForWork,
					agentDay.MinutesLateForWork,
					agentDay.SecondsInAdherence,
					agentDay.SecondsOutOfAdherence
				}).OrderBy(ai => ai.SiteTeamName).ThenBy(ai => ai.Name).ToLookup(ai => ai.TeamId);

			return (from agentsOnTeam in agentsPerDayGroupedOnTeam
					select new HistoricalOverviewTeamViewModel
					{
						Name = agentsOnTeam.First().SiteTeamName,
						DisplayDays = displayDays,
						Agents = (from agent in agentsOnTeam
							group agent by agent.PersonId
							into groupedAgent
							select new HistoricalOverviewAgentViewModel
							{
								Id = groupedAgent.First().PersonId,
								Name = groupedAgent.First().Name,
								Days = (from day in userSevenDays
									let agentDay = groupedAgent.FirstOrDefault(a => a.Day == day)
									select new HistoricalOverviewDayViewModel
									{
										Date = day.ToString("yyyyMMdd"),
										Adherence = agentDay?.Adherence,
										WasLateForWork = agentDay?.WasLateForWork ?? false
									}).ToArray(),
								LateForWork = new HistoricalOverviewLateForWorkViewModel()
								{
									Count = groupedAgent.Count(a => a.WasLateForWork),
									TotalMinutes = groupedAgent.Sum(a => a.MinutesLateForWork)
								},
								IntervalAdherence = AdherencePercentageCalculation.Calculate(
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
	}

	public class HistoricalOverviewTeamViewModel
	{
		public string Name { get; set; }
		public IEnumerable<HistoricalOverviewAgentViewModel> Agents { get; set; }
		public IEnumerable<string> DisplayDays { get; set; }
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
		public int? Adherence { get; set; }
		public bool WasLateForWork { get; set; }
	}
}