﻿using System;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Ccc.Web.Broker;
using Teleopti.Ccc.Web.Core.Aop.Aspects;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{

	[HubName("personScheduleHub")]
	public class PersonScheduleHub : TestableHub
	{
		private readonly IPersonScheduleViewModelFactory _viewModelFactory;

		public PersonScheduleHub(IPersonScheduleViewModelFactory viewModelFactory)
		{
			_viewModelFactory = viewModelFactory;
		}

		[UnitOfWork]
		public void PersonSchedule(Guid personId, DateTime date)
		{
			var data = _viewModelFactory.CreateViewModel(personId, date);
			Clients.Caller.incomingPersonSchedule(data);
		}

	}

}