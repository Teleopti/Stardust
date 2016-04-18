using System;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;


namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class AbsenceRequstHandler : IHandle<NewAbsenceRequestCreatedEvent>
	{
		private readonly IComponentContext _componentContext;

		public AbsenceRequstHandler(IComponentContext componentContext)
		{
			_componentContext = componentContext;
		}

		public void Handle(NewAbsenceRequestCreatedEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress)
		{
			var theRealOne = _componentContext.Resolve<IHandleEvent<NewAbsenceRequestCreatedEvent>>();
			theRealOne.Handle(parameters);
		}
	}
}