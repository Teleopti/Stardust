using System;
using Autofac.Extras.DynamicProxy2;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Ccc.Web.Broker;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Ccc.Web.Core.Aop.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	[HubName("groupScheduleHub")]
	[Intercept(typeof(AspectInterceptor))]
	public class GroupScheduleHub : TestableHub
	{
		private readonly IGroupScheduleViewModelFactory _groupScheduleViewModelFactory;
		private readonly IUserTimeZone _userTimeZone;

		public GroupScheduleHub(IGroupScheduleViewModelFactory groupScheduleViewModelFactory, IUserTimeZone userTimeZone)
		{
			_groupScheduleViewModelFactory = groupScheduleViewModelFactory;
			_userTimeZone = userTimeZone;
		}

		[UnitOfWork]
		public virtual void SubscribeGroupSchedule(Guid groupId, DateTime dateInUserTimeZone)
		{
			pushSchedule(Clients.Caller, groupId, TimeZoneInfo.ConvertTime(dateInUserTimeZone, _userTimeZone.TimeZone(), TimeZoneInfo.Utc));
		}

		private void pushSchedule(dynamic target, Guid groupId, DateTime dateTimeInUtc)
		{
			target.incomingGroupSchedule(_groupScheduleViewModelFactory.CreateViewModel(groupId, dateTimeInUtc));
		}
	}
}