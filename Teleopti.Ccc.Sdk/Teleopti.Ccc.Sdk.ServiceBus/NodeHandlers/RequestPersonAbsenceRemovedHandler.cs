using System;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class RequestPersonAbsenceRemovedHandler : IHandle<RequestPersonAbsenceRemovedEvent>
	{
		private readonly IComponentContext _componentContext;

		public RequestPersonAbsenceRemovedHandler(IComponentContext componentContext)
		{
			_componentContext = componentContext;
		}

		public void Handle(RequestPersonAbsenceRemovedEvent parameters, CancellationTokenSource cancellationTokenSource,
			Action<string> sendProgress)
		{
			var theRealOne = _componentContext.Resolve<IHandleEvent<RequestPersonAbsenceRemovedEvent>>();
			theRealOne.Handle(parameters);
		}
	}
}
