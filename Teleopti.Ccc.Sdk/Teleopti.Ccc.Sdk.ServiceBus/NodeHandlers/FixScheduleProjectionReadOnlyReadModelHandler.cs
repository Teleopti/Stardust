using System;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class FixScheduleProjectionReadOnlyReadModelHandler : IHandle<FixScheduleProjectionReadOnlyEvent>
	{
		private readonly IComponentContext _componentContext;

		public FixScheduleProjectionReadOnlyReadModelHandler(IComponentContext componentContext)
		{
			_componentContext = componentContext;
		}

		[AsSystem, UnitOfWork]
		public virtual void Handle(FixScheduleProjectionReadOnlyEvent parameters,
			CancellationTokenSource cancellationTokenSource,
			Action<string> sendProgress)
		{
			var theRealOne = _componentContext.Resolve<IHandleEvent<FixScheduleProjectionReadOnlyEvent>>();
			theRealOne.Handle(parameters);
		}
	}
}
