﻿using System;
using Autofac.Extras.DynamicProxy2;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	[HubName("groupScheduleHub")]
	[Intercept(typeof(AspectInterceptor))]
	[CLSCompliant(false)]
	public class GroupScheduleHub : Hub
	{
		private readonly IGroupScheduleViewModelFactory _groupScheduleViewModelFactory;
		private readonly ILoggedOnUser _user;

		public GroupScheduleHub(IGroupScheduleViewModelFactory groupScheduleViewModelFactory, ILoggedOnUser user)
		{
			_groupScheduleViewModelFactory = groupScheduleViewModelFactory;
			_user = user;
		}

		[UnitOfWork]
		public virtual void SubscribeGroupSchedule(Guid groupId, DateTime dateInUserTimeZone)
		{
			var userTimeZone = _user.CurrentUser().PermissionInformation.DefaultTimeZone();
			var dateTimeInUtc = TimeZoneInfo.ConvertTime(dateInUserTimeZone, userTimeZone, TimeZoneInfo.Utc);

			var schedules = _groupScheduleViewModelFactory.CreateViewModel(groupId, dateInUserTimeZone);
			var result = new
			{
				BaseDate = dateTimeInUtc,
				Schedules = schedules
			};

			Clients.Caller.incomingGroupSchedule(result);
		}
	}
}