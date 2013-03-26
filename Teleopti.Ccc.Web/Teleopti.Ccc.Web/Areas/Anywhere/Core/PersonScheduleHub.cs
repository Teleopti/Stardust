﻿using System;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Ccc.Web.Core.Aop.Aspects;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{

	[HubName("personScheduleHub")]
	public class PersonScheduleHub : Hub
	{
		private readonly IPersonScheduleViewModelFactory _viewModelFactory;

		public PersonScheduleHub(IPersonScheduleViewModelFactory viewModelFactory)
		{
			_viewModelFactory = viewModelFactory;
		}

		[UnitOfWork]
		public void SubscribePersonSchedule(Guid personId, DateTime date)
		{
			var data = _viewModelFactory.CreateViewModel(personId, date);
			Clients.Caller.incomingPersonSchedule(data);
		}

	}

}