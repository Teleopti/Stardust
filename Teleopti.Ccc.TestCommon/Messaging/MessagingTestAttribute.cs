using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.MessageBrokerUnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.TestCommon.Messaging
{
	public class MessagingTestAttribute : IoCTestAttribute
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
			
			system.UseTestDouble<FakeMessageBrokerUnitOfWorkScope>().For<IMessageBrokerUnitOfWorkScope>();
			system.UseTestDouble<FakeSignalR>().For<ISignalR>();
			system.UseTestDouble<FakeMailboxRepository>().For<IMailboxRepository>();
		}
	}
}