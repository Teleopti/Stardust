using Autofac;
using Stardust.Node.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class ImportAgentHandler : IHandle<ImportAgentEvent>
	{
		private readonly IComponentContext _componentContext;
		public ImportAgentHandler(
			IComponentContext componentContext
			)
		{
			_componentContext = componentContext;
		}

		public void Handle(ImportAgentEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress)
		{
			var theRealOne = _componentContext.Resolve<IHandleEvent<ImportAgentEvent>>();
			theRealOne.Handle(parameters);
		}
		
	
	}
}
