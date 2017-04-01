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
		private readonly IFindTenantByName _findTenantByName;
		private readonly CurrentTenantUserFake _currentTenantUser;
		public ImportAgentHandler(IComponentContext componentContext, IFindTenantByName findTenantByName, 
			ICurrentTenantUser currentTenantUser)
		{
			_componentContext = componentContext;
			_findTenantByName = findTenantByName;
			_currentTenantUser = currentTenantUser as CurrentTenantUserFake;
		}

		public void Handle(ImportAgentEvent parameters, CancellationTokenSource cancellationTokenSource, Action<string> sendProgress)
		{
			setCurrentTenant(parameters);
			var theRealOne = _componentContext.Resolve<IHandleEvent<ImportAgentEvent>>();
			theRealOne.Handle(parameters);
		}

		[TenantUnitOfWork]
		protected virtual void setCurrentTenant(ImportAgentEvent parameters)
		{
			var tenant = _findTenantByName.Find(parameters.LogOnDatasource);
			_currentTenantUser.Set(new PersonInfo(tenant, Guid.Empty));
		}

	}
}
