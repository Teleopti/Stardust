using System;
using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Messaging.Client;

namespace Teleopti.Ccc.TestCommon
{
	public static class DatabaseTestSetup
	{
		private static int createdDataHash = 0;

		public static void Setup(Func<CreateDataContext, int> createData)
		{
			if (createdDataHash != 0)
			{
				DataSourceHelper.RestoreApplicationDatabase(createdDataHash);
				DataSourceHelper.RestoreAnalyticsDatabase(createdDataHash);
				return;
			}

			withContainer(container =>
			{
				var dataSourcesFactory = container.Resolve<IDataSourcesFactory>();
				var dataSourceScope = container.Resolve<IDataSourceScope>();
				
				var dataSource = DataSourceHelper.CreateDatabasesAndDataSource(dataSourcesFactory);
				
				using (dataSourceScope.OnThisThreadUse(dataSource))
				{
					createdDataHash = createData.Invoke(new CreateDataContext
					{
						DataSource = dataSource,
						DataSourceScope = dataSourceScope,
						WithUnitOfWork = container.Resolve<WithUnitOfWork>(),
						UpdatedByScope = container.Resolve<IUpdatedByScope>(),
						BusinessUnits = container.Resolve<IBusinessUnitRepository>(),
						Persons = container.Resolve<IPersonRepository>()
					});
				}
				if (createdDataHash == 0)
					throw new Exception("create data function needs to return a number representing the data created");

				DataSourceHelper.BackupApplicationDatabase(createdDataHash);
				DataSourceHelper.BackupAnalyticsDatabase(createdDataHash);
			});
		}

		private static void withContainer(Action<IComponentContext> action)
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(new ConfigReader()) {FeatureToggle = "http://notinuse"})));
			builder.RegisterType<NoMessageSender>().As<IMessageSender>().SingleInstance();
			builder.RegisterType<FakeHangfireEventClient>().As<IHangfireEventClient>().SingleInstance();
			var container = builder.Build();
			
			action.Invoke(container);
			
			container.Dispose();
		}
	}

	public class CreateDataContext
	{
		public IDataSource DataSource;
		public IDataSourceScope DataSourceScope;
		public WithUnitOfWork WithUnitOfWork;
		public IUpdatedByScope UpdatedByScope;
		public IPersonRepository Persons;
		public IBusinessUnitRepository BusinessUnits;
	}
}