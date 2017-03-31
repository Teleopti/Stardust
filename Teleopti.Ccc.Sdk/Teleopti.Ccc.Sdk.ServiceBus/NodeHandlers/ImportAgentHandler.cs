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
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class ImportAgentHandler : IHandle<ImportAgentEvent>
	{
		private readonly IComponentContext _componentContext;
		private Action<string> _sendProgress;
		private CurrentTenantUserFake _currentTenantUser;
		private readonly IFindPersonInfoByCredentials _findPersonByCredentials;
		public ImportAgentHandler(
			IComponentContext componentContext,
			ICurrentTenantUser currentTenantUser,
			IFindPersonInfoByCredentials findPersonByCredentials)
		{
			_componentContext = componentContext;
			_currentTenantUser = currentTenantUser as CurrentTenantUserFake;
			_findPersonByCredentials = findPersonByCredentials;
		}
		
		public void Handle(ImportAgentEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress)
		{
			_currentTenantUser.Set(_findPersonByCredentials.Find(parameters.TenantInfo.PersonId, parameters.TenantInfo.TenantPassword));
			var theRealOne = _componentContext.Resolve<IHandleEvent<ImportAgentEvent>>();
			theRealOne.Handle(parameters);

		}
	}
}
