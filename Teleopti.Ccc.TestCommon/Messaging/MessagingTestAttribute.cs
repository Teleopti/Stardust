using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker;
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
			var config = base.Config();
			config.FakeSetting("MessageBrokerMailboxPollingIntervalInSeconds", "60");
			return config;
		}

		protected override void Extend(IExtend extend, IocConfiguration configuration)
		{
			base.Extend(extend, configuration);

			extend.AddService<MessageBrokerServerTester>();
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

	public class MessageBrokerServerTester
	{
		private readonly IMessageBrokerServer _server;
		private readonly FakeTime _time;

		public MessageBrokerServerTester(IMessageBrokerServer server, FakeTime time)
		{
			_server = server;
			_time = time;
		}

		public string AddSubscription(Subscription subscription, string connectionId) =>
			_server.AddSubscription(subscription, connectionId);

		public void RemoveSubscription(string route, string connectionId) =>
			_server.RemoveSubscription(route, connectionId);

		public IEnumerable<Message> PopMessages(string route, string mailboxId) =>
			_server.PopMessages(route, mailboxId);

		public void NotifyClients(Message message)
		{
			_server.NotifyClients(message);
			_time.Passes("1".Seconds());
		}

		public void NotifyClientsMultiple(IEnumerable<Message> messages)
		{
			_server.NotifyClientsMultiple(messages);
			_time.Passes("1".Seconds());
		}
	}
}