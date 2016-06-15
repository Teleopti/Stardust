using System;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class ResourceCalculateReadModelUpdater : IHandle<UpdateResourceCalculateReadModelEvent>
	{
		private readonly IComponentContext _componentContext;

		public ResourceCalculateReadModelUpdater(IComponentContext componentContext)
		{
			_componentContext = componentContext;
		}

		public void Handle(UpdateResourceCalculateReadModelEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress)
		{
			var theRealOne = _componentContext.Resolve<IHandleEvent<UpdateResourceCalculateReadModelEvent>>();
			theRealOne.Handle(parameters);
		}
	}
}