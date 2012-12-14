using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Team.Core
{
	[HubName("scheduleHub")]
	public class ScheduleHub : Hub
	{
		private readonly IPersonScheduleDayReadModelRepository _personScheduleDayReadModelRepository;
		private readonly ITeamProvider _teamProvider;
		
		public ScheduleHub(IPersonScheduleDayReadModelRepository personScheduleDayReadModelRepository, ITeamProvider teamProvider)
		{
			_personScheduleDayReadModelRepository = personScheduleDayReadModelRepository;
			_teamProvider = teamProvider;
		}

		[UnitOfWork]
		public object AvailableTeams(DateTime date)
		{
			return new
			       	{
			       		Teams = _teamProvider.GetPermittedTeams(new DateOnly(date)).Select(t => new {t.Id, t.SiteAndTeam}).ToList()
			       	};
		}

		[UnitOfWork]
		public IEnumerable<object> SubscribeTeamSchedule(Guid teamId, DateTime date)
		{
			var schedule = _personScheduleDayReadModelRepository.ForTeam(new DateOnly(date), teamId);
			return schedule.Select(s => Newtonsoft.Json.JsonConvert.DeserializeObject(s.Shift));
		}
	}
}