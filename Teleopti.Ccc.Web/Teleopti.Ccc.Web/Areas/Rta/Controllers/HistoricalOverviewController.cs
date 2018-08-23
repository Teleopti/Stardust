using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ViewModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	public class HistoricalOverviewController : ApiController
	{
		private readonly IAgentStateReadModelReader _reader;
		private readonly IAgentAdherenceDayLoader _agentAdherenceDayLoader;

		public HistoricalOverviewController(IAgentStateReadModelReader reader,
			IAgentAdherenceDayLoader agentAdherenceDayLoader)
		{
			_reader = reader;
			_agentAdherenceDayLoader = agentAdherenceDayLoader;
		}

		[UnitOfWork, ReadModelUnitOfWork, HttpGet, Route("api/HistoricalOverview/Load")]
		public virtual IHttpActionResult Load([FromUri] IEnumerable<Guid> siteIds = null, [FromUri] IEnumerable<Guid> teamIds = null)
		{
			var vm = BuildViewModelQuickAndDirty(siteIds, teamIds);

			return Ok(vm);
		}

		private IEnumerable<HistoricalOverviewTeamViewModel> BuildViewModelQuickAndDirty(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds)
		{
			var agents = LoadAgents(siteIds, teamIds);
			var teams = BuildTeams(agents);
			return teams;
		}

		private IEnumerable<HistoricalOverviewAgent> LoadAgents(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds)
		{
			var filter = new AgentStateFilter() { TeamIds = teamIds, SiteIds = siteIds };

			var persons = from p in _reader.Read(filter)
				select new HistoricalOverviewAgent
				{
					Id = p.PersonId,
					SiteAndTeam = p.SiteName + "/" + p.TeamName,
					Name = p.LastName + " " + p.FirstName
				};
			return persons;
		}

		private IEnumerable<HistoricalOverviewTeamViewModel> BuildTeams(IEnumerable<HistoricalOverviewAgent> agents)
		{
			var teams =
				from agent in agents
				group agent by agent.SiteAndTeam
				into agentsWithTeamGrouping
				select new HistoricalOverviewTeamViewModel
				{
					Name = agentsWithTeamGrouping.First().SiteAndTeam,
					Agents = BuildAgents(agentsWithTeamGrouping)
				};

			return teams.ToArray();
		}

		private IEnumerable<HistoricalOverviewAgentViewModel> BuildAgents(IGrouping<string, HistoricalOverviewAgent> agentsWithTeamGrouping)
		{
			var agents = from agent in agentsWithTeamGrouping
				let daySpan = BuildDaySpanAndDayAdherence(agent)
				select new HistoricalOverviewAgentViewModel
				{
					Id = agent.Id,
					Name = agent.Name,
					IntervalAdherence = 0,
					Days = BuildDays(daySpan, agent),
					LateForWork = BuildLateForWork(daySpan)
				};

			return agents.ToArray();
		}

		private IEnumerable<HistoricalOverviewDay> BuildDaySpanAndDayAdherence( HistoricalOverviewAgent agent)
		{
			var daySpan = DateOnly.Today.AddDays(-8).DateRange(7);

			var days = from day in daySpan
				let adherenceDay = _agentAdherenceDayLoader.Load(agent.Id, day)
				let change = adherenceDay.Changes().FirstOrDefault(change => change.LateForWork != null)
				let lateForWorkText = change != null ? change.LateForWork : "0"
				let minutesLateForWork = Int32.Parse(Regex.Replace(lateForWorkText, "[^0-9.]", ""))
				select new HistoricalOverviewDay
				{
					Date = day.Date,
					AdherencePercent = adherenceDay.Percentage().GetValueOrDefault(),
					MinutesLateForWork = minutesLateForWork
				};

			return days.ToArray();
		}

		private IEnumerable<HistoricalOverviewDayViewModel> BuildDays(IEnumerable<HistoricalOverviewDay> days, HistoricalOverviewAgent agent)
		{
			return (from day in days
				select new HistoricalOverviewDayViewModel
				{
					Date = day.Date.ToString("yyyyMMdd"),
					DisplayDate = day.Date.ToString("MM/dd"),
					Adherence = day.AdherencePercent, 
					WasLateForWork = day.MinutesLateForWork > 0
				}).ToArray();
		}

		private static HistoricalOverviewLateForWorkViewModel BuildLateForWork(IEnumerable<HistoricalOverviewDay> days)
		{
			return new HistoricalOverviewLateForWorkViewModel
			{
				Count = days.Count(day => day.MinutesLateForWork > 0),
				TotalMinutes = days.Sum(day => day.MinutesLateForWork)
			};
		}

		class HistoricalOverviewAgent
		{
			public Guid Id { get; set; }
			public string SiteAndTeam { get; set; }
			public string Name { get; set; }
		}

		class HistoricalOverviewDay
		{
			public DateTime Date { get; set; }
			public int AdherencePercent { get; set; }
			public int MinutesLateForWork { get; set; }
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
		public int IntervalAdherence { get; set; }
		public HistoricalOverviewLateForWorkViewModel LateForWork { get; set; }
		public IEnumerable<HistoricalOverviewDayViewModel> Days { get; set; }
	}

	public class HistoricalOverviewDayViewModel
	{
		public string Date { get; set; }
		public string DisplayDate { get; set; }
		public int Adherence { get; set; }
		public bool WasLateForWork { get; set; }
	}

	public class HistoricalOverviewLateForWorkViewModel
	{
		public int Count { get; set; }
		public int TotalMinutes { get; set; }
	}
}