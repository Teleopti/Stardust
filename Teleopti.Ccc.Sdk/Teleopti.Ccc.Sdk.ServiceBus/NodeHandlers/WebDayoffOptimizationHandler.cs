using System;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class WebDayOffOptimizationHandler : IHandle<WebDayoffOptimizationStardustEvent>
	{
		private readonly IComponentContext _componentContext;
		private readonly IStardustJobFeedback _stardustJobFeedback;

		public WebDayOffOptimizationHandler(IStardustJobFeedback stardustJobFeedback, IComponentContext componentContext)
		{
			_stardustJobFeedback = stardustJobFeedback;
			_componentContext = componentContext;
		}

		[AsSystem]
		public virtual void Handle(WebDayoffOptimizationStardustEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress)
		{
			_stardustJobFeedback.SendProgress = sendProgress;
			var theRealOne = _componentContext.Resolve<IHandleEvent<WebDayoffOptimizationStardustEvent>>();
			theRealOne.Handle(parameters);
			_stardustJobFeedback.SendProgress = null;
		}
	}
}