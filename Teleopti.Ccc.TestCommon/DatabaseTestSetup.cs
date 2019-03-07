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
using Teleopti.Ccc.IocCommon;
using Teleopti.Messaging.Client;

namespace Teleopti.Ccc.TestCommon
{
	public static class DatabaseTestSetup
	{
		private static readonly PerTestWorker<object> data = new PerTestWorker<object>();

		public static T Setup<T>(Func<CreateDataContext, CreateDataResult<T>> createData)
		{
			if (data.Value != null)
			{
				var cachedData = (CreateDataResult<T>) data.Value;
				withContainer(container =>
				{
					var database = container.Resolve<DatabaseTestHelper>();
					database.RestoreApplicationDatabase(cachedData.Hash);
					database.RestoreAnalyticsDatabase(cachedData.Hash);
				});
				return cachedData.Data;
			}

			var createDataResult = default(CreateDataResult<T>);
			withContainer(container =>
			{
				var database = container.Resolve<DatabaseTestHelper>();
				
				database.CreateDatabases(InfraTestConfigReader.TenantName());

				var dataSource = container.Resolve<IDataSourceForTenant>().Tenant(InfraTestConfigReader.TenantName());

				var dataSourceScope = container.Resolve<IDataSourceScope>();
				using (dataSourceScope.OnThisThreadUse(dataSource))
				{
					var result = createData.Invoke(new CreateDataContext
					{
						DataSource = dataSource,
						DataSourceScope = dataSourceScope,
						WithUnitOfWork = container.Resolve<WithUnitOfWork>(),
						UpdatedByScope = container.Resolve<IUpdatedByScope>(),
						BusinessUnits = container.Resolve<IBusinessUnitRepository>(),
						Persons = container.Resolve<IPersonRepository>()
					});
					if (result.Hash == 0)
						throw new Exception("create data function needs to return a number representing the data created");
					createDataResult = result;
				}
				
				data.Set(() => createDataResult);
				database.BackupApplicationDatabase(createDataResult.Hash);
				database.BackupAnalyticsDatabase(createDataResult.Hash);
				
			});

			return createDataResult.Data;
		}
		
		private static void withContainer(Action<IComponentContext> action)
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(new ConfigReader()) {FeatureToggle = "http://notinuse"})));
			builder.RegisterType<NoMessageSender>().As<IMessageSender>().SingleInstance();
			builder.RegisterType<FakeHangfireEventClient>().As<IHangfireEventClient>().SingleInstance();
			builder.RegisterType<DatabaseTestHelper>().SingleInstance();
			builder.RegisterInstance(new FakeConfigReader().FakeInfraTestConfig()).AsSelf().As<IConfigReader>().SingleInstance();
			var container = builder.Build();

			action.Invoke(container);

			container.Dispose();
		}
	}

	public class CreateDataResult<T>
	{
		public int Hash;
		public T Data;
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