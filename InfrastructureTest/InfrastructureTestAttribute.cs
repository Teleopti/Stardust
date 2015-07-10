using System;
using System.Collections.Generic;
using System.Configuration;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.InfrastructureTest.Rta;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Interfaces.Infrastructure;
using IMessageSender = Teleopti.Interfaces.MessageBroker.Client.IMessageSender;

namespace Teleopti.Ccc.InfrastructureTest
{
	public class InfrastructureTestAttribute : IoCTestAttribute
	{
		public IMessageSendersScope MessageSendersScope;
		public IEnumerable<Infrastructure.UnitOfWork.IMessageSender> MessageSenders;
		private IDisposable _scope;
		public FakeMessageSender MessageSender;

		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);

			system.UseTestDouble(new FakeDatabaseConnectionStringHandler()).For<IDatabaseConnectionStringHandler>();

			system.UseTestDouble(new FakeConfigReader
			{
				ConnectionStrings = new ConnectionStringSettingsCollection
				{
					new ConnectionStringSettings("RtaApplication", ConnectionStringHelper.ConnectionStringUsedInTests),
					new ConnectionStringSettings("MessageBroker", ConnectionStringHelper.ConnectionStringUsedInTestsMatrix)
				}
			}).For<IConfigReader>();

			system.UseTestDouble<MutableFakeCurrentHttpContext>().For<ICurrentHttpContext>();

			system.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
			system.UseTestDouble<FakeMessageSender>().For<IMessageSender>();
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