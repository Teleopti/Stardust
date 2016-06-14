using System;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Infrastructure.Events;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class IndexMaintenanceHandler : IHandle<IndexMaintenanceEvent>
	{
		private readonly IComponentContext _componentContext;

		public IndexMaintenanceHandler(IComponentContext componentContext)
		{
			_componentContext = componentContext;
		}

		public void Handle(IndexMaintenanceEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress)
		{
			var theRealOne = _componentContext.Resolve<IHandleEvent<IndexMaintenanceEvent>>();
			theRealOne.Handle(parameters);
		}
	}
}