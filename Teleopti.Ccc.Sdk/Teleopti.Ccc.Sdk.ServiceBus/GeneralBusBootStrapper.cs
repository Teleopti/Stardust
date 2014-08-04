using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Rhino.ServiceBus;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class GeneralBusBootStrapper : BusBootStrapper
	{
		protected override void OnEndStart()
		{
			var bus = Container.Resolve<IServiceBus>();
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
					bus.Send(new AgentBadgeCalculateMessage { Datasource = dataSource.DataSourceName, BusinessUnitId = businessUnitId, Timestamp = DateTime.UtcNow, IsInitialization = true });
				}
			}
		}
	}
}