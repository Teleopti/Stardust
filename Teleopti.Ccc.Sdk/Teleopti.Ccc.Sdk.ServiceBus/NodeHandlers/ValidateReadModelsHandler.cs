using System;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class ValidateReadModelsHandler : IHandle<ValidateReadModelsEvent>
	{
		private readonly IComponentContext _componentContext;


		public ValidateReadModelsHandler(IComponentContext componentContext)
		{
			_componentContext = componentContext;

		}
		[AsSystem]
		public virtual void Handle(ValidateReadModelsEvent parameters, CancellationTokenSource cancellationTokenSource,
			Action<string> sendProgress)
		{
			var theRealOne = _componentContext.Resolve<IHandleEvent<ValidateReadModelsEvent>>();
			theRealOne.Handle(parameters);
		}
	}
}
