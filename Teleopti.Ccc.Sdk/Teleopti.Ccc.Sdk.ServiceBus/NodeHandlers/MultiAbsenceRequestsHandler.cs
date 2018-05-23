using System;
using System.Collections.Generic;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class MultiAbsenceRequestsHandler : IHandle<NewMultiAbsenceRequestsCreatedEvent>
	{
		private readonly IComponentContext _componentContext;
		private readonly IStardustJobFeedback _stardustJobFeedback;

		public MultiAbsenceRequestsHandler(IComponentContext componentContext, IStardustJobFeedback stardustJobFeedback)
		{
			_componentContext = componentContext;
			_stardustJobFeedback = stardustJobFeedback;
		}

		public void Handle(NewMultiAbsenceRequestsCreatedEvent parameters, 
			CancellationTokenSource cancellationTokenSource,
			Action<string> sendProgress,
			ref IEnumerable<object> returnObjects)
		{
			_stardustJobFeedback.SendProgress = sendProgress;
			var theRealOne = _componentContext.Resolve<IHandleEvent<NewMultiAbsenceRequestsCreatedEvent>>();
			theRealOne.Handle(parameters);
			_stardustJobFeedback.SendProgress = null;
		}
	}
}
