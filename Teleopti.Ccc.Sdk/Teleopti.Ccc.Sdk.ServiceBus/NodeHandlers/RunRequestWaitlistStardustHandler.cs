using System;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class RunRequestWaitlistStardustHandler : IHandle<RunRequestWaitlistEvent>, IRunOnStardust
	{
		private readonly IComponentContext _componentContext;
		private readonly IStardustJobFeedback _stardustJobFeedback;

		public RunRequestWaitlistStardustHandler(IComponentContext componentContext, IStardustJobFeedback stardustJobFeedback)
		{
			_componentContext = componentContext;
			_stardustJobFeedback = stardustJobFeedback;
		}

		[AsSystem]
		public virtual void Handle(RunRequestWaitlistEvent @event,
			CancellationTokenSource cancellationTokenSource,
			Action<string> sendProgress)
		{
			_stardustJobFeedback.SendProgress = sendProgress;

			var theRealOne = _componentContext.Resolve<IHandleEvent<RunRequestWaitlistEvent>>();
			theRealOne.Handle(@event);

			_stardustJobFeedback.SendProgress = null;
		}
	}
}