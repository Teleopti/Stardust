using System;
using System.Collections.Generic;
using System.Dynamic;
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
			Groups.Add(Context.ConnectionId, string.Format("{0}-{1}", teamId, date));
			pushSchedule(Clients.Caller, teamId, date);
		}

		//[UnitOfWork]
		//public void PushTeamSchedule(Guid teamId, DateTime date)
		//{
		//    pushSchedule(Clients.Group(string.Format("{0}-{1}", teamId, date)), teamId, date);
		//}

		private void pushSchedule(dynamic target, Guid teamId, DateTime date)
		{
			var dateTimePeriod = new DateTimePeriod(date, date.AddHours(25));
			var schedule = _personScheduleDayReadModelRepository.ForTeam(dateTimePeriod, teamId);
			if (schedule != null)
				target.incomingTeamSchedule(schedule.Select(s => JsonConvert.DeserializeObject<ExpandoObject>(s.Shift)));
		}

	}
}