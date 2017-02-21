using System;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class WebOptimizationHandler : IHandle<WebOptimizationStardustEvent>
	{
		private readonly IComponentContext _componentContext;
		private readonly IStardustJobFeedback _stardustJobFeedback;

		public WebOptimizationHandler(IStardustJobFeedback stardustJobFeedback, IComponentContext componentContext)
		{
			_stardustJobFeedback = stardustJobFeedback;
			_componentContext = componentContext;
		}

		[AsSystem]
		public virtual void Handle(WebOptimizationStardustEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress)
		{
			_stardustJobFeedback.SendProgress = sendProgress;
			var theRealOne = _componentContext.Resolve<IHandleEvent<WebOptimizationStardustEvent>>();
			theRealOne.Handle(parameters);
			_stardustJobFeedback.SendProgress = null;
		}
	}
}