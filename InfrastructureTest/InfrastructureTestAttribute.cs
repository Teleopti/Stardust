using System;
using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.InfrastructureTest.Rta;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web;

namespace Teleopti.Ccc.InfrastructureTest
{
	public class InfrastructureTestAttribute : IoCTestAttribute
	{
		public IMessageSendersScope MessageSendersScope;
		public IEnumerable<IMessageSender> MessageSenders;
		private IDisposable _scope;
		public FakeMessageSender MessageSender;

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

			system.UseTestDouble(new FakeDatabaseConnectionStringHandler()).For<IDatabaseConnectionStringHandler>();
			system.UseTestDouble<MutableFakeCurrentHttpContext>().For<ICurrentHttpContext>();
			system.UseTestDouble<FakeMessageSender>().For<Interfaces.MessageBroker.Client.IMessageSender>(); // Does not fake all message senders, just adds one to the list
		}

		protected override void BeforeTest()
		{
			base.BeforeTest();

			_scope = MessageSendersScope.GloballyUse(MessageSenders);
		}

		protected override void AfterTest()
		{
			base.AfterTest();

			_scope.Dispose();
		}
	}
}