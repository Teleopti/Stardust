using System;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Infrastructure.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class IndexMaintenanceHandler : IHandle<IndexMaintenanceEvent>
	{
		private readonly IComponentContext _componentContext;
		private readonly IStardustJobFeedback _stardustJobFeedback;

		public IndexMaintenanceHandler(IComponentContext componentContext, IStardustJobFeedback stardustJobFeedback)
		{
			_componentContext = componentContext;
			_stardustJobFeedback = stardustJobFeedback;
		}

		public void Handle(IndexMaintenanceEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress)
		{
			_stardustJobFeedback.SendProgress = sendProgress;
			var theRealOne = _componentContext.Resolve<IHandleEvent<IndexMaintenanceEvent>>();
			theRealOne.Handle(parameters);
			_stardustJobFeedback.SendProgress = null;
		}
	}
}