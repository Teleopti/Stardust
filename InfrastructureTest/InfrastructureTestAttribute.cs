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
		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			base.RegisterInContainer(builder, configuration);

			builder.RegisterInstance(new FakeDatabaseConnectionStringHandler()).As<IDatabaseConnectionStringHandler>();

			builder.RegisterInstance(new FakeConfigReader
			{
				ConnectionStrings = new ConnectionStringSettingsCollection
				{
					new ConnectionStringSettings("RtaApplication", ConnectionStringHelper.ConnectionStringUsedInTests),
					new ConnectionStringSettings("MessageBroker", ConnectionStringHelper.ConnectionStringUsedInTestsMatrix)
				}
			}).As<IConfigReader>().AsSelf().SingleInstance();

			builder.RegisterType<NoMessageSender>().As<IMessageSender>();

			builder.RegisterType<MutableFakeCurrentHttpContext>().AsSelf().As<ICurrentHttpContext>().SingleInstance();
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