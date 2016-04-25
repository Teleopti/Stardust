using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.InfrastructureTest.Persisters.Schedules;
using Teleopti.Ccc.InfrastructureTest.Rta;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest
{
	public class InfrastructureTestAttribute : IoCTestAttribute
	{
		public ITransactionHooksScope TransactionHooksScope;
		public IEnumerable<ITransactionHook> TransactionHooks;
		private IDisposable _transactionHookScope;
		public FakeMessageSender MessageSender;
		public FakeTransactionHook TransactionHook;
		public IDataSourceForTenant DataSourceForTenant;
		public ILicenseRepository Licenses;

		protected override FakeConfigReader Config()
		{
			var config = base.Config();
			config.FakeConnectionString("MessageBroker", InfraTestConfigReader.AnalyticsConnectionString);
			config.FakeConnectionString("Tenancy", InfraTestConfigReader.ConnectionString);
			config.FakeConnectionString("Hangfire", InfraTestConfigReader.AnalyticsConnectionString);
			return config;
		}

		//
		// Should fake:
		// Config
		// Http
		// Message broker
		//
		// Should NOT fake:
		// Database
		// Hangfire		<--(??)
		// Bus			<--(??)
		//
		// ... we guess ...
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);

			// Tenant stuff
			system.AddModule(new TenantServerModule(configuration));
			system.UseTestDouble<TenantAuthenticationFake>().For<ITenantAuthentication>();
			system.AddService(TenantUnitOfWorkManager.Create(InfraTestConfigReader.ConnectionString));

			// Hangfire bus maybe? ;)
			system.UseTestDouble<FakeHangfireEventClient>().For<IHangfireEventClient>();
			system.UseTestDouble<FakeServiceBusSender>().For<IServiceBusSender>();

			// message broker
			system.UseTestDouble(new FakeSignalR()).For<ISignalR>();
			system.UseTestDouble<FakeMessageSender>().For<IMessageSender>();

			system.UseTestDouble<FakeTransactionHook>().For<ITransactionHook>(); // just adds one hook to the list
			system.UseTestDouble<TestConnectionStrings>().For<IConnectionStrings>();
			system.UseTestDouble<MutableFakeCurrentHttpContext>().For<ICurrentHttpContext>();

			// fake for now. if real repo needs to be included in the scope....
			system.UseTestDouble<FakeLicenseRepository>().For<ILicenseRepository, ILicenseRepositoryForLicenseVerifier>();
		}

		protected override void BeforeTest()
		{
			base.BeforeTest();
			
			DataSourceForTenant.MakeSureDataSourceCreated(
				SetupFixtureForAssembly.DataSource.DataSourceName,
				InfraTestConfigReader.ConnectionString,
				InfraTestConfigReader.AnalyticsConnectionString,
				null);

			MessageSender.AllNotifications.Clear();
			TransactionHook.Clear();

			_transactionHookScope = TransactionHooksScope.GloballyUse(TransactionHooks);
		}

		protected override void AfterTest()
		{
			base.AfterTest();

			_transactionHookScope.Dispose();
		}
	}
}