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
	    private readonly IDataSourceForTenant _dataSourceForTenant;

	    public BusinessUnitStarter(Func<IServiceBus> serviceBusFinder, IDataSourceForTenant dataSourceForTenant)
	    {
		    _serviceBusFinder = serviceBusFinder;
		    _dataSourceForTenant = dataSourceForTenant;
	    }

	    public void SendMessage()
		{
			var bus = _serviceBusFinder.Invoke();

			_dataSourceForTenant.DoOnAllTenants_AvoidUsingThis(tenant =>
					{
						IList<Guid> businessUnitCollection;
						using (var unitOfWork = tenant.Application.CreateAndOpenUnitOfWork())
						{
							var businessUnitRepository = new BusinessUnitRepository(unitOfWork);
							businessUnitCollection = businessUnitRepository.LoadAll().Select(b => b.Id.GetValueOrDefault()).ToList();
						}

						foreach (var businessUnitId in businessUnitCollection)
						{
							bus.Send(new StartUpBusinessUnit { LogOnDatasource = tenant.DataSourceName, LogOnBusinessUnitId = businessUnitId, Timestamp = DateTime.UtcNow });
						}
					});
		}
    }
}
