using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Rta;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class BusinessUnitStarter
    {
        private readonly Func<IServiceBus> _serviceBusFinder;
        
        public BusinessUnitStarter(Func<IServiceBus> serviceBusFinder)
        {
            _serviceBusFinder = serviceBusFinder;
        }

        public void SendMessage()
		{
			var bus = _serviceBusFinder.Invoke();

					StateHolderReader.Instance.StateReader.ApplicationScopeData.DataSourceForTenant.DoOnAllTenants_AvoidUsingThis(tenant =>
					{
						IList<Guid> businessUnitCollection;
						using (var unitOfWork = tenant.Application.CreateAndOpenUnitOfWork())
						{
							var businessUnitRepository = new BusinessUnitRepository(unitOfWork);
							businessUnitCollection = businessUnitRepository.LoadAll().Select(b => b.Id.GetValueOrDefault()).ToList();
						}

						foreach (var businessUnitId in businessUnitCollection)
						{
							bus.Send(new StartUpBusinessUnit { Datasource = tenant.DataSourceName, BusinessUnitId = businessUnitId, Timestamp = DateTime.UtcNow });
						}
					});
		}
    }
}
