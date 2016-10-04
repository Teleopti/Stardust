using System;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class ResourceCalculateReadModelUpdater : IHandle<UpdateResourceCalculateReadModelEvent>
	{
		private readonly IComponentContext _componentContext;
		private readonly IStardustJobFeedback _stardustJobFeedback;

		public ResourceCalculateReadModelUpdater(IComponentContext componentContext, IStardustJobFeedback stardustJobFeedback)
		{
			_componentContext = componentContext;
			_stardustJobFeedback = stardustJobFeedback;
		}

		public void Handle(UpdateResourceCalculateReadModelEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress)
		{
			_stardustJobFeedback.SendProgress = sendProgress;
			var theRealOne = _componentContext.Resolve<IHandleEvent<UpdateResourceCalculateReadModelEvent>>();
			theRealOne.Handle(parameters);
			_stardustJobFeedback.SendProgress = null;
		}
	}
}