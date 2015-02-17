using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;
using log4net;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class GeneralBusBootStrapper : BusBootStrapper
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(GeneralBusBootStrapper));

		public GeneralBusBootStrapper(IContainer container) : base(container)
		{
		}

		protected override void OnEndStart()
		{
			var bus = Container.Resolve<IServiceBus>();
			var toggleManager = Container.Resolve<IToggleManager>();
			if (!toggleManager.IsEnabled(Toggles.MyTimeWeb_AgentBadge_28913) &&
			    !toggleManager.IsEnabled(Toggles.Portal_DifferentiateBadgeSettingForAgents_31318))
				return;

			foreach (
				var dataSource in
#pragma warning disable 618
					StateHolderReader.Instance.StateReader.ApplicationScopeData.RegisteredDataSourceCollection.ToList())
#pragma warning restore 618
			{
				IList<Guid> businessUnitCollection;
				using (var unitOfWork = dataSource.Application.CreateAndOpenUnitOfWork())
				{
					var businessUnitRepository = new BusinessUnitRepository(unitOfWork);
					businessUnitCollection = businessUnitRepository.LoadAll().Select(b => b.Id.GetValueOrDefault()).ToList();
				}

				foreach (var businessUnitId in businessUnitCollection)
				{
					bus.Send(new BadgeCalculationInitMessage
					{
						Datasource = dataSource.DataSourceName,
						BusinessUnitId = businessUnitId,
						Timestamp = DateTime.UtcNow
					});
					Logger.DebugFormat(
						"Sending BadgeCalculationInitMessage to Service Bus for Datasource={0} and BusinessUnitId={1}",
						dataSource.DataSourceName,
						businessUnitId);
				}
			}
		}
	}
}