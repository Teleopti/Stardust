using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.RTA
{
    public class RTAStateChecker
    {
        private readonly Func<IServiceBus> _serviceBusFinder;
        
        public RTAStateChecker(Func<IServiceBus> serviceBusFinder)
        {
            _serviceBusFinder = serviceBusFinder;
        }

        public void Check()
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
				    bus.Send(new RTAStateCheckerMessage { Datasource = dataSource.DataSourceName, BusinessUnitId = businessUnitId, Timestamp = DateTime.UtcNow });
			    }
			}
		}
    }
}
