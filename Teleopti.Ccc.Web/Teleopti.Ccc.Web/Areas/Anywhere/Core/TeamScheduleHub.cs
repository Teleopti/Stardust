using System;
using Autofac.Extras.DynamicProxy2;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Ccc.Web.Broker;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Ccc.Web.Core.Aop.Core;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	[HubName("teamScheduleHub")]
	[Intercept(typeof(AspectInterceptor))]
	public class TeamScheduleHub : TestableHub
	{
		private readonly ITeamScheduleViewModelFactory _teamScheduleViewModelFactory;

		public TeamScheduleHub(ITeamScheduleViewModelFactory teamScheduleViewModelFactory)
		{
			_teamScheduleViewModelFactory = teamScheduleViewModelFactory;
		}

		[UnitOfWork]
		public virtual void SubscribeTeamSchedule(Guid groupId, DateTime date)
		{
			pushSchedule(Clients.Caller, groupId, date);
		}

		private void pushSchedule(dynamic target, Guid groupId, DateTime date)
		{
			target.incomingTeamSchedule(_teamScheduleViewModelFactory.CreateViewModel(groupId, date));
		}
	}
}