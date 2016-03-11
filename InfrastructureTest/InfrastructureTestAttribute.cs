using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.InfrastructureTest.Rta;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest
{
	public class InfrastructureTestAttribute : IoCTestAttribute
	{
		public IMessageSendersScope MessageSendersScope;
		public IEnumerable<IPersistCallback> MessageSenders;
		private IDisposable _messageSenderScope;
		public FakeMessageSender MessageSender;
		public IDataSourceForTenant DataSourceForTenant;

		protected override FakeConfigReader Config()
		{
			var config = base.Config();
			config.FakeConnectionString("MessageBroker", InfraTestConfigReader.AnalyticsConnectionString);
			config.FakeConnectionString("Tenancy", InfraTestConfigReader.ConnectionString);
			return config;
		}

		//
		// Should fake:
		// Config
		// Http
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

			system.UseTestDouble(new FakeSignalR()).For<ISignalR>();
			system.UseTestDouble<TestConnectionStrings>().For<IConnectionStrings>();
			system.UseTestDouble<MutableFakeCurrentHttpContext>().For<ICurrentHttpContext>();
			system.UseTestDouble<FakeMessageSender>().For<IMessageSender>(); // Does not fake all message senders, just adds one to the list
			system.UseTestDouble<SetNoLicenseActivator>().For<ISetLicenseActivator>();
		}

		protected override void BeforeTest()
		{
			base.BeforeTest();

			DataSourceForTenant.MakeSureDataSourceCreated("App", InfraTestConfigReader.ConnectionString, null, null);

			MessageSender.AllNotifications.Clear();

			_messageSenderScope = MessageSendersScope.GloballyUse(MessageSenders);
		}

		protected override void AfterTest()
		{
			base.AfterTest();

			_messageSenderScope.Dispose();
		}
	}
}