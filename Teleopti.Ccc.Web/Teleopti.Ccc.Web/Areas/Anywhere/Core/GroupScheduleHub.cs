using System;
using System.Linq;
using Autofac.Extras.DynamicProxy2;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Ccc.Domain.Collection;
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
			var dateTimeInUtc = TimeZoneHelper.ConvertToUtc(dateInUserTimeZone, userTimeZone);
			
			var schedules = _groupScheduleViewModelFactory.CreateViewModel(groupId, dateInUserTimeZone).ToArray();
			var results = schedules.Batch(30).Select(s => new
			{
				BaseDate = dateTimeInUtc,
				Schedules = s.ToArray(),
				TotalCount = schedules.Count()
			});

			results.ForEach(r => Clients.Caller.incomingGroupSchedule(r));
		}
	}
}