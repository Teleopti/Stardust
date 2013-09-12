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
		private readonly ITeamScheduleProvider _teamScheduleProvider;

		public TeamScheduleHub(ITeamScheduleProvider teamScheduleProvider)
		{
			_teamScheduleProvider = teamScheduleProvider;
		}

		[UnitOfWork]
		public virtual void SubscribeTeamSchedule(Guid teamId, DateTime date)
		{
			pushSchedule(Clients.Caller, teamId, date);
		}

		private void pushSchedule(dynamic target, Guid teamId, DateTime date)
		{
			target.incomingTeamSchedule(_teamScheduleProvider.TeamSchedule(teamId, date));
		}
	}
}