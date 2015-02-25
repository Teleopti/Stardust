using System;
using Autofac.Extras.DynamicProxy2;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Infrastructure.Aop;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	[HubName("personScheduleHub")]
	[CLSCompliant(false)]
	public class PersonScheduleHub : Hub
	{
		private readonly IPersonScheduleViewModelFactory _viewModelFactory;

		public PersonScheduleHub(IPersonScheduleViewModelFactory viewModelFactory)
		{
			_viewModelFactory = viewModelFactory;
		}

		[UnitOfWork]
		public virtual void PersonSchedule(Guid personId, DateTime date)
		{
			var data = _viewModelFactory.CreateViewModel(personId, date);
			Clients.Caller.incomingPersonSchedule(data);
		}

	}

}