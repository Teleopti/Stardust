using System;
using Autofac.Extras.DynamicProxy2;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Web.Core.Aop.Aspects;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	[HubName("groupScheduleHub")]
	[Intercept(typeof(AspectInterceptor))]
	[CLSCompliant(false)]
	public class GroupScheduleHub : Hub
	{
		private readonly IGroupScheduleViewModelFactory _groupScheduleViewModelFactory;

		public GroupScheduleHub(IGroupScheduleViewModelFactory groupScheduleViewModelFactory)
		{
			_groupScheduleViewModelFactory = groupScheduleViewModelFactory;
		}

		[UnitOfWork]
		public virtual void SubscribeGroupSchedule(Guid groupId, DateTime dateInUserTimeZone)
		{
			Clients.Caller.incomingGroupSchedule(_groupScheduleViewModelFactory.CreateViewModel(groupId, dateInUserTimeZone));
		}
	}
}