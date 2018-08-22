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
			var filter = new AgentStateFilter() {TeamIds = teamIds, SiteIds = siteIds};
			var persons = from p in _reader.Read(filter)
				select new
				{
					p.PersonId,
					p.TeamId,
					p.TeamName,
					p.SiteName,
					p.FirstName,
					p.LastName
				};
			var days = DateOnly.Today.AddDays(-8).DateRange(7);


			var things =
				from p in persons
				group p by p.TeamId
				into t
				select new HistoricalOverviewTeamViewModel
				{
					Name = t.First().SiteName + "/" + t.First().TeamName,
					Agents = (from p2 in t
						let ds = from d in days
							let x = _agentAdherenceDayLoader.Load(p2.PersonId, d)
							let change = x.Changes().FirstOrDefault(change => change.LateForWork != null)
							let lateForWorkText = change != null ? change.LateForWork : "0"
							let lateForWorkInMin = Int32.Parse(Regex.Replace(lateForWorkText, "[^0-9.]", ""))
							select new
							{
								d = d,
								Date = d.Date,
								percent = x.Percentage().GetValueOrDefault(),
								LateForWork = x.Changes().Where(xx => xx.LateForWork != null),
								LateForWorkInMin = lateForWorkInMin
							}
						select new HistoricalOverviewAgentViewModel
						{
							Id = p2.PersonId,
							Name = p2.LastName + " " + p2.FirstName,
							IntervalAdherence = 0,
							Days = (from d in ds
								select new HistoricalOverviewDayViewModel
								{
									Date = d.Date.ToString("yyyyMMdd"),
									DisplayDate = d.Date.ToString("MM/dd"),
									Adherence = _agentAdherenceDayLoader.Load(p2.PersonId, d.d).Percentage().GetValueOrDefault(),
									WasLateForWork = d.LateForWork.Any()
								}).ToArray(),
							LateForWork = new HistoricalOverviewLateForWorkViewModel
							{
								Count = ds.Count(x => x.LateForWork.Any()),
								TotalMinutes = ds.Sum(x => x.LateForWorkInMin)
							}
						}).ToArray()
				};

			return Ok(things.ToArray());
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