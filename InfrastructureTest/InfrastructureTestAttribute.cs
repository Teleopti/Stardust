using System.Collections.Generic;
using System.Configuration;
using Autofac;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.InfrastructureTest.Rta;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.InfrastructureTest
{
	public class InfrastructureTestAttribute : IoCTestAttribute
	{
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

			system.UseTestDouble<NoMessageSender>().For<IMessageSender>();

			system.UseTestDouble<MutableFakeCurrentHttpContext>().For<ICurrentHttpContext>();
		}
	}

	public class NoMessageSender : IMessageSender
	{
		public void Send(Message message)
		{
		}

		public void SendMultiple(IEnumerable<Message> notifications)
		{
		}
	}
}