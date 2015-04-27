using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using Rhino.Mocks.Constraints;
using SharpTestsEx;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Broker;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.WebTest.Core.MessageBroker
{
	[TestFixture]
	[IoCTest]
	public class SignalRPublishTest : IRegisterInContainer
	{
		public void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterModule<MessageBrokerWebModule>();
			builder.RegisterModule<MessageBrokerServerModule>();
			builder.RegisterInstance(new FakeSignalR()).AsSelf().As<ISignalR>();
			builder.RegisterInstance(new ActionImmediate()).As<IActionScheduler>();
		}

		public IMessageBrokerServer Server;
		public FakeSignalR SignalR;

		[Test]
		public void ShouldPublishToSignalR()
		{
			var notification = new Notification();
			Server.NotifyClients(notification);
			SignalR.SentMessage.Should().Be(notification);
		}
	}

	public class FakeSignalR : ISignalR
	{
		public Notification SentMessage;
		public string CalledGroup;
		public string CalledRoute;

		public void CallOnEventMessage(string groupName, string route, Notification notification)
		{
			CalledGroup = groupName;
			CalledRoute = route;
			SentMessage = notification;
		}
	}
}
