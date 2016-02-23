using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
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
		public IMessageSendersScope MessageSendersScope;
		public IEnumerable<IPersistCallback> MessageSenders;
		private IDisposable _messageSenderScope;
		public FakeMessageSender MessageSender;
		public IDataSourceForTenant DataSourceForTenant;

		protected override FakeConfigReader Config()
		{
			var config = base.Config();
			config.FakeConnectionString("RtaApplication", ConnectionStringHelper.ConnectionStringUsedInTests);
			config.FakeConnectionString("MessageBroker", ConnectionStringHelper.ConnectionStringUsedInTestsMatrix);
			config.FakeConnectionString("Tenancy", ConnectionStringHelper.ConnectionStringUsedInTests);
			return config;
		}

		//
		// Should fake:
		// Config
		// Http
		//
		// Should NOT fake:
		// Database
		// Hangfire
		// Bus
		//
		// ... we guess ...
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);
			system.AddModule(new TenantServerModule(configuration));
			system.AddService(TenantUnitOfWorkManager.Create(ConnectionStringHelper.ConnectionStringUsedInTests));
			system.UseTestDouble(new FakeSignalR()).For<ISignalR>();
			system.UseTestDouble<TestConnectionStrings>().For<IConnectionStrings>();
			system.UseTestDouble<MutableFakeCurrentHttpContext>().For<ICurrentHttpContext>();
			system.UseTestDouble<FakeMessageSender>().For<IMessageSender>(); // Does not fake all message senders, just adds one to the list

			// not sure about this..
			// ..either fake the whole license thing...
			// ..or fake the repository..
			// ..or put a license in the database.
			//system.UseTestDouble<SetNoLicenseActivator>().For<ISetLicenseActivator>();
			system.UseTestDouble<FakeLicenseRepository>().For<ILicenseRepository>();
		}

		protected override void BeforeTest()
		{
			base.BeforeTest();

			DataSourceForTenant.MakeSureDataSourceCreated("App", ConnectionStringHelper.ConnectionStringUsedInTests, null, null);

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