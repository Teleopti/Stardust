using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Rta;

namespace Teleopti.Ccc.Sdk.ServiceBus.Rta
{
    public class BusinessUnitInfoFinder
    {
        private readonly Func<IServiceBus> _serviceBusFinder;
        
        public BusinessUnitInfoFinder(Func<IServiceBus> serviceBusFinder)
        {
            _serviceBusFinder = serviceBusFinder;
        }

        public void SendMessage()
		{
			var bus = _serviceBusFinder.Invoke();

			foreach (var dataSource in StateHolderReader.Instance.StateReader.ApplicationScopeData.RegisteredDataSourceCollection.ToList())
			{
			    IList<Guid> businessUnitCollection;
			    using (var unitOfWork = dataSource.Application.CreateAndOpenUnitOfWork())
			    {
				    var businessUnitRepository = new BusinessUnitRepository(unitOfWork);
				    businessUnitCollection = businessUnitRepository.LoadAll().Select(b => b.Id.GetValueOrDefault()).ToList();
			    }

                foreach (var businessUnitId in businessUnitCollection)
			    {
				    bus.Send(new BusinessUnitInfo { Datasource = dataSource.DataSourceName, BusinessUnitId = businessUnitId, Timestamp = DateTime.UtcNow });
			    }
			}
		}
    }
}
