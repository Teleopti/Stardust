using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Autofac;
using Newtonsoft.Json;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;
using log4net;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class GeneralBusBootStrapper : BusBootStrapper
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(GeneralBusBootStrapper));
		protected override void OnEndStart()
		{
			var bus = Container.Resolve<IServiceBus>();
			var toggleManager = Container.Resolve<IToggleManager>();
			var isEnabled = false;
			try
			{
				isEnabled = toggleManager.IsEnabled(Toggles.MyTimeWeb_AgentBadge_28913);
			}
			catch (JsonReaderException)
			{
				return;
			}
			if (!isEnabled)
				return;

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
					bus.Send(new BadgeCalculationInitMessage { Datasource = dataSource.DataSourceName, BusinessUnitId = businessUnitId, Timestamp = DateTime.UtcNow});
					Logger.DebugFormat(
						"Sending BadgeCalculationInitMessage to Service Bus for Datasource={0} and BusinessUnitId={1}", dataSource.DataSourceName,
						businessUnitId);
				}
			}
		}
	}
}