using System;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;


namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class AddOverTimeHandler : IHandle<AddOverTimeEvent>
	{
		private readonly IComponentContext _componentContext;

		public AddOverTimeHandler(IComponentContext componentContext)
		{
			_componentContext = componentContext;
		}

		public void Handle(AddOverTimeEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress)
		{
			var theRealOne = _componentContext.Resolve<IHandleEvent<AddOverTimeEvent>>();
			theRealOne.Handle(parameters);
		}
	}
}