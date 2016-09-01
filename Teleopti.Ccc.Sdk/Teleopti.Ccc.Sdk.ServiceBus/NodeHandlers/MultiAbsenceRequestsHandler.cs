using System;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class MultiAbsenceRequestsHandler : IHandle<NewMultiAbsenceRequestsCreatedEvent>
	{
		private readonly IComponentContext _componentContext;

		public MultiAbsenceRequestsHandler(IComponentContext componentContext)
		{
			_componentContext = componentContext;
		}

		public void Handle(NewMultiAbsenceRequestsCreatedEvent parameters, CancellationTokenSource cancellationTokenSource,
			Action<string> sendProgress)
		{
			var theRealOne = _componentContext.Resolve<IHandleEvent<NewMultiAbsenceRequestsCreatedEvent>>();
			theRealOne.Handle(parameters);
		}
	}
}
