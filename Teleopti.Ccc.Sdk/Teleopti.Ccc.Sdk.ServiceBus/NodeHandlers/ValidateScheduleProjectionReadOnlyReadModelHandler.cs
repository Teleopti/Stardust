using System;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class ValidateScheduleProjectionReadOnlyReadModelHandler : IHandle<ValidateScheduleProjectionReadOnlyEvent>
	{
		private readonly IComponentContext _componentContext;


		public ValidateScheduleProjectionReadOnlyReadModelHandler(IComponentContext componentContext)
		{
			_componentContext = componentContext;

		}
		[UnitOfWork]
		public void Handle(ValidateScheduleProjectionReadOnlyEvent parameters, CancellationTokenSource cancellationTokenSource,
			Action<string> sendProgress)
		{
			var theRealOne = _componentContext.Resolve<IHandleEvent<ValidateScheduleProjectionReadOnlyEvent>>();
			theRealOne.Handle(parameters);
		}
	}
}
