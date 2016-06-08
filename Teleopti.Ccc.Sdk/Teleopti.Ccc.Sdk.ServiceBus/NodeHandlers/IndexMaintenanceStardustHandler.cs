using System;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Infrastructure.Events;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class IndexMaintenanceStardustHandler : IHandle<IndexMaintenanceStardustEvent>
	{
		private readonly IComponentContext _componentContext;

		public IndexMaintenanceStardustHandler(IComponentContext componentContext)
		{
			_componentContext = componentContext;
		}

		public void Handle(IndexMaintenanceStardustEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress)
		{
			var theRealOne = _componentContext.Resolve<IHandleEvent<IndexMaintenanceStardustEvent>>();
			theRealOne.Handle(parameters);
		}
	}
}