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
		public object SubscribePersonScheduleViewModel(Guid personId, DateTime date)
		{
			return new
				{
					Id = personId, 
					Name = "Mathias Stenbvom", 
					Site = "Maramö", 
					Team = "Alliansen"
				};
		}
	}

	public class PersonScheduleViewModel
	{
	}
}