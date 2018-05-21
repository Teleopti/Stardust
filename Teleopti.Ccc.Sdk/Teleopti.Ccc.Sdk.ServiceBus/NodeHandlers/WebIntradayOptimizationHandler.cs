using System;
using System.Collections.Generic;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class WebIntradayOptimizationHandler : IHandle<WebIntradayOptimizationStardustEvent>
	{
		private readonly IStardustJobFeedback _stardustJobFeedback;
		private readonly IComponentContext _componentContext;

		public WebIntradayOptimizationHandler(IStardustJobFeedback stardustJobFeedback, IComponentContext componentContext)
		{
			_stardustJobFeedback = stardustJobFeedback;
			_componentContext = componentContext;
		}

		[AsSystem]
		public void Handle(WebIntradayOptimizationStardustEvent parameters, 
			CancellationTokenSource cancellationTokenSource, 
			Action<string> sendProgress,
			ref IEnumerable<object> returnObjects)
		{
			_stardustJobFeedback.SendProgress = sendProgress;
			var theRealOne = _componentContext.Resolve<IHandleEvent<WebIntradayOptimizationStardustEvent>>();
			theRealOne.Handle(parameters);
			_stardustJobFeedback.SendProgress = null;
		}
	}
}