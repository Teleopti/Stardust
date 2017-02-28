using Autofac;
using log4net;
using Rhino.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages.General;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class GeneralBusBootStrapper : BusBootStrapper
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (GeneralBusBootStrapper));

		public GeneralBusBootStrapper(IContainer container) : base(container)
		{
		}

		protected override void OnEndStart()
		{
			base.OnEndStart();

			var bus = Container.Resolve<IServiceBus>();
			var toggleManager = Container.Resolve<IToggleManager>();
			if (toggleManager.IsEnabled(Toggles.ETL_MoveBadgeCalculationToETL_38421))
				return;

			Task.Run(() => Container.Resolve<DataSourceForTenantWrapper>().DataSource()().DoOnAllTenants_AvoidUsingThis(tenant =>
			{
				IList<Guid> businessUnitCollection;
				using (var unitOfWork = tenant.Application.CreateAndOpenUnitOfWork())
				{
					var businessUnitRepository = new BusinessUnitRepository(unitOfWork);
					businessUnitCollection = businessUnitRepository.LoadAll().Select(b => b.Id.GetValueOrDefault()).ToList();
				}

				foreach (var businessUnitId in businessUnitCollection)
				{
					bus.Send(new BadgeCalculationInitMessage
					{
						LogOnDatasource = tenant.DataSourceName,
						LogOnBusinessUnitId = businessUnitId,
						Timestamp = DateTime.UtcNow
					});

					if (Logger.IsDebugEnabled)
					{
						Logger.DebugFormat(
							"Sending BadgeCalculationInitMessage to Service Bus for Datasource={0} and BusinessUnitId={1}",
							tenant.DataSourceName,
							businessUnitId);
					}
				}
			}));
		}
	}
}