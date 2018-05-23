using Autofac;
using Stardust.Node.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class ImportAgentHandler : IHandle<ImportAgentEvent>
	{
		private readonly IStardustJobFeedback _stardustJobFeedback;
		private readonly IComponentContext _componentContext;

		public ImportAgentHandler(IStardustJobFeedback stardustJobFeedback, IComponentContext componentContext)
		{
			_stardustJobFeedback = stardustJobFeedback;
			_componentContext = componentContext;
		}

		public void Handle(ImportAgentEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress, ref IEnumerable<object> returnObjects)
		{
			_stardustJobFeedback.SendProgress = sendProgress;
			var theRealOne = _componentContext.Resolve<IHandleEvent<ImportAgentEvent>>();
			theRealOne.Handle(parameters);
			_stardustJobFeedback.SendProgress = null;
		}
	}
}
