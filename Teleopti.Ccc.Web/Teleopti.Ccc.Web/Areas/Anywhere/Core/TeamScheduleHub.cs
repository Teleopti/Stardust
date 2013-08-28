using System;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Ccc.Web.Broker;
using Teleopti.Ccc.Web.Core.Aop.Aspects;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	[HubName("teamScheduleHub")]
	public class TeamScheduleHub : TestableHub
	{
		private readonly ITeamScheduleProvider _teamScheduleProvider;

		public TeamScheduleHub(ITeamScheduleProvider teamScheduleProvider)
		{
			_teamScheduleProvider = teamScheduleProvider;
		}

		[UnitOfWork]
		public void SubscribeTeamSchedule(Guid teamId, DateTime date)
		{
			pushSchedule(Clients.Caller, teamId, date);
		}

		private void pushSchedule(dynamic target, Guid teamId, DateTime date)
		{
			target.incomingTeamSchedule(_teamScheduleProvider.TeamSchedule(teamId, date));
		}
	}
}