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
	[HubName("personScheduleHub")]
	public class PersonScheduleHub : Hub
	{
		[UnitOfWork]
		public void SubscribePersonSchedule(Guid personId, DateTime date)
		{
			Groups.Add(Context.ConnectionId, string.Format("{0}-{1}", personId, date));
			pushData(Clients.Caller, personId, date);

		}

		//[UnitOfWork]
		//public void PushPersonSchedule(Guid personId, DateTime date)
		//{
		//    pushData(Clients.Group(string.Format("{0}-{1}", personId, date)), personId, date);
		//}

		private void pushData(dynamic target, Guid personId, DateTime date)
		{
			//var dateTimePeriod = new DateTimePeriod(date, date.AddHours(25));
			//var schedule = _personScheduleDayReadModelRepository.ForTeam(dateTimePeriod, teamId);
			var data = new
				{
					Id = personId,
					Name = "Mathias Stenbvom",
					Site = "Maramö",
					Team = "Alliansen"
				};
			target.incomingPersonSchedule(data);
		}

	}
}