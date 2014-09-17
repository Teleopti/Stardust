using System;
using Autofac.Extras.DynamicProxy2;
using Microsoft.AspNet.SignalR.Hubs;
using Teleopti.Ccc.Web.Broker;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Ccc.Web.Core.Aop.Core;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{

	[HubName("personScheduleHub")]
	[Intercept(typeof(AspectInterceptor))]
	public class PersonScheduleHub : TestableHub
	{
		private readonly IPersonScheduleViewModelFactory _viewModelFactory;

		public PersonScheduleHub(IPersonScheduleViewModelFactory viewModelFactory)
		{
			_viewModelFactory = viewModelFactory;
		}

		[UnitOfWork(Order = 1), MultipleBusinessUnits(Order = 2)]
		public virtual void PersonSchedule(Guid personId, DateTime date)
		{
			var data = _viewModelFactory.CreateViewModel(personId, date);
			Clients.Caller.incomingPersonSchedule(data);
		}

	}

}