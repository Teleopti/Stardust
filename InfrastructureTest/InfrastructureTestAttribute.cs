using System;
using System.Collections.Generic;
using Autofac;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
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
		public IEnumerable<IMessageSender> MessageSenders;
		private IDisposable _messageSenderScope;
		public FakeMessageSender MessageSender;
		public IDataSourceForTenant DataSourceForTenant;
		public IDataSourcesFactory DataSourcesFactory;

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
			system.AddService(TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(ConnectionStringHelper.ConnectionStringUsedInTests));
			system.UseTestDouble<FakeConnectionStrings>().For<IConnectionStrings>();
			system.UseTestDouble<MutableFakeCurrentHttpContext>().For<ICurrentHttpContext>();
			system.UseTestDouble<FakeMessageSender>().For<Interfaces.MessageBroker.Client.IMessageSender>(); // Does not fake all message senders, just adds one to the list
			system.UseTestDouble<SetNoLicenseActivator>().For<ISetLicenseActivator>();
		}

		protected override void BeforeTest()
		{
			base.BeforeTest();

			var nonFakedDataSourceForTenant = DataSourceForTenant as DataSourceForTenant;
			if (nonFakedDataSourceForTenant != null)
				nonFakedDataSourceForTenant.MakeSureDataSourceExists_UseOnlyFromTests(DataSourcesFactory.Create("App", ConnectionStringHelper.ConnectionStringUsedInTests, null));
				
			_messageSenderScope = MessageSendersScope.GloballyUse(MessageSenders);
		}

		protected override void AfterTest()
		{
			base.AfterTest();

			_messageSenderScope.Dispose();
		}
	}
}