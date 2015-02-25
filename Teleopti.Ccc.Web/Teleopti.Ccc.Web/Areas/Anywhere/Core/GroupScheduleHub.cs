using System;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	[HubName("groupScheduleHub")]
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
			
			var schedules = _groupScheduleViewModelFactory.CreateViewModel(groupId, dateInUserTimeZone).ToArray();
			if (schedules.IsEmpty())
			{
				Clients.Caller.incomingGroupSchedule(new
				{
					BaseDate = dateTimeInUtc,
					Schedules = new GroupScheduleShiftViewModel[] {},
					TotalCount = 0
				});
			}
			else
			{
				foreach (var s in schedules.Batch(30))
				{
					Clients.Caller.incomingGroupSchedule(new
					{
						BaseDate = dateTimeInUtc,
						Schedules = s.ToArray(),
						TotalCount = schedules.Count()
					});
				}
			}
		}
	}
}