using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.Client.Http;

namespace Teleopti.MessagingTest
{
	public class MessagingTestAttribute : DomainTestAttribute
	{
		protected override FakeConfigReader Config()
		{
			var config =  base.Config();
			config.FakeSetting("MessageBrokerMailboxPollingIntervalInSeconds", "60");
			return config;
		}

		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);

			system.UseTestDouble(new FakeUrl("http://someserver/")).For<IMessageBrokerUrl>();
			system.UseTestDouble<MessageBrokerServerBridge>().For<IHttpServer>();
			system.UseTestDouble(new FakeCurrentDatasource(new DataSourceState())).For<ICurrentDataSource>();
			system.UseTestDouble(new FakeCurrentBusinessUnit()).For<ICurrentBusinessUnit>();
		}
	}
}