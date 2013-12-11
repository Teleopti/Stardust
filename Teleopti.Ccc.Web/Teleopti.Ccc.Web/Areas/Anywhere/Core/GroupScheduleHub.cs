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
		private readonly ITeamScheduleViewModelFactory _teamScheduleViewModelFactory;

		public GroupScheduleHub(ITeamScheduleViewModelFactory teamScheduleViewModelFactory)
		{
			_teamScheduleViewModelFactory = teamScheduleViewModelFactory;
		}

		[UnitOfWork]
		public virtual void SubscribeGroupSchedule(Guid groupId, DateTime date)
		{
			pushSchedule(Clients.Caller, groupId, date);
		}

		private void pushSchedule(dynamic target, Guid groupId, DateTime date)
		{
			target.incomingGroupSchedule(_teamScheduleViewModelFactory.CreateViewModel(groupId, date));
		}
	}
}