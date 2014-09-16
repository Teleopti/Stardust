using System;
using Autofac.Extras.DynamicProxy2;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Ccc.Web.Broker;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Ccc.Web.Core.Aop.Core;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	[HubName("groupScheduleHub")]
	[Intercept(typeof(AspectInterceptor))]
	public class GroupScheduleHub : TestableHub
	{
		private readonly IGroupScheduleViewModelFactory _groupScheduleViewModelFactory;

		public GroupScheduleHub(IGroupScheduleViewModelFactory groupScheduleViewModelFactory)
		{
			_groupScheduleViewModelFactory = groupScheduleViewModelFactory;
		}

		[UnitOfWork(Order = 1), MultipleBusinessUnits(Order = 2)]
		public virtual void SubscribeGroupSchedule(Guid groupId, DateTime dateInUserTimeZone)
		{
			Clients.Caller.incomingGroupSchedule(_groupScheduleViewModelFactory.CreateViewModel(groupId, dateInUserTimeZone));
		}
	}
}