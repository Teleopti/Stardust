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

		protected override void Isolate(IIsolate isolate)
		{
			base.Isolate(isolate);

			isolate.UseTestDouble(new FakeUrl("http://someserver/")).For<IMessageBrokerUrl>();
			isolate.UseTestDouble<MessageBrokerServerBridge>().For<IHttpServer>();
			isolate.UseTestDouble(new FakeCurrentDatasource(new DataSourceState())).For<ICurrentDataSource>();
			isolate.UseTestDouble(new FakeCurrentBusinessUnit()).For<ICurrentBusinessUnit>();
			
			isolate.UseTestDouble<FakeMessageBrokerUnitOfWorkScope>().For<IMessageBrokerUnitOfWorkScope>();
			isolate.UseTestDouble<FakeSignalR>().For<ISignalR>();
			isolate.UseTestDouble<FakeMailboxRepository>().For<IMailboxRepository>();
		}
	}
}