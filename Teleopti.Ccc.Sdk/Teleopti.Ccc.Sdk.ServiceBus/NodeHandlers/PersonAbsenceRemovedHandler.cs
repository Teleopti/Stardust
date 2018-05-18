using System;
using System.Collections.Generic;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class PersonAbsenceRemovedHandler : IHandle<PersonAbsenceRemovedEvent>
	{
		private readonly IComponentContext _componentContext;

		public PersonAbsenceRemovedHandler(IComponentContext componentContext)
		{
			_componentContext = componentContext;
		}

		public void Handle (PersonAbsenceRemovedEvent parameters, 
			CancellationTokenSource cancellationTokenSource, 
			Action<string> sendProgress,
			ref IEnumerable<object> returnObjects)
		{
			var theRealOne = _componentContext.Resolve<IHandleEvent<PersonAbsenceRemovedEvent>>();
			theRealOne.Handle(parameters);
		}
	}
}