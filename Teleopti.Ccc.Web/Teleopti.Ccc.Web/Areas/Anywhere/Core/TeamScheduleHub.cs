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
			pushSchedule(Clients.Caller, teamId, date);
		}

		private void pushSchedule(dynamic target, Guid teamId, DateTime date)
		{
			date = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
			var dateTimePeriod = new DateTimePeriod(date, date.AddHours(25));
			var schedule = _personScheduleDayReadModelRepository.ForTeam(dateTimePeriod, teamId);
			if (schedule != null)
				target.incomingTeamSchedule(schedule.Select(s => JsonConvert.DeserializeObject<ExpandoObject>(s.Shift)));
		}

	}
}