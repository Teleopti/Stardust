using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	[HubName("teamScheduleHub")]
	public class TeamScheduleHub : Hub
	{
		private readonly IPersonScheduleDayReadModelRepository _personScheduleDayReadModelRepository;
		
		public TeamScheduleHub(IPersonScheduleDayReadModelRepository personScheduleDayReadModelRepository)
		{
			_personScheduleDayReadModelRepository = personScheduleDayReadModelRepository;
		}

		[UnitOfWork]
		public void SubscribeTeamSchedule(Guid teamId, DateTime date)
		{
			var group = string.Format("{0}-{1}", teamId, date);
			Groups.Add(Context.ConnectionId, group);
			
			pushSchedule(Clients.Caller, teamId, date);
		}

		[UnitOfWork]
		public void ViewModelChanged(Guid teamId, DateTime date)
		{
			pushSchedule(Clients.Group(string.Format("{0}-{1}", teamId, date)), teamId, date);
		}

		private void pushSchedule(dynamic target, Guid teamId, DateTime date)
		{
			var dateTimePeriod = new DateTimePeriod(date, date.AddHours(25));
			var schedule = _personScheduleDayReadModelRepository.ForTeam(dateTimePeriod, teamId);
			target.incomingTeamSchedule(schedule.Select(s => JsonConvert.DeserializeObject(s.Shift))); ;
		}
	}
}