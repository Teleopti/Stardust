using System;
using System.Threading;
using Autofac;
using log4net;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class RunRequestWaitlistStardustHandler : IHandle<RunRequestWaitlistEvent>, IRunOnStardust
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(RunRequestWaitlistStardustHandler));
		private readonly IComponentContext _componentContext;

		public RunRequestWaitlistStardustHandler(IComponentContext componentContext)
		{
			_componentContext = componentContext;
		}

		[AsSystem]
		public virtual void Handle(RunRequestWaitlistEvent @event,
			CancellationTokenSource cancellationTokenSource,
			Action<string> sendProgress)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.Debug(
					$"Consuming event for running request waitlist with Id=\"{@event.Id}\", "
					+ $"Period=\"{@event.Period}\" (Message timestamp=\"{@event.Timestamp}\")");
			}
			
			var theRealOne = _componentContext.Resolve<IHandleEvent<RunRequestWaitlistEvent>>();
			theRealOne.Handle(@event);
		}
	}
}