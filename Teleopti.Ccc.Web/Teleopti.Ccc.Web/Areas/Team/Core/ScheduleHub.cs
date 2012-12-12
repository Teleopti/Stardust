using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Team.Core
{
	[HubName("scheduleHub")]
	public class ScheduleHub : Hub
	{
		private readonly IPersonScheduleDayReadModelRepository _personScheduleDayReadModelRepository;

		public ScheduleHub(IPersonScheduleDayReadModelRepository personScheduleDayReadModelRepository)
		{
			_personScheduleDayReadModelRepository = personScheduleDayReadModelRepository;
		}

		[UnitOfWork]
		public IEnumerable<object> SubscribeTeamSchedule(Guid teamId, DateTime date)
		{
			var schedule = _personScheduleDayReadModelRepository.ForTeam(new DateOnly(date), teamId);
			return schedule.Select(s => Newtonsoft.Json.JsonConvert.DeserializeObject(s.Shift));
		}
	}
}