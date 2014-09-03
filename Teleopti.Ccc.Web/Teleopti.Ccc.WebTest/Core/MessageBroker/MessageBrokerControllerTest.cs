using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.WebTest.Core.MessageBroker
{
	public class MessageBrokerControllerTest
	{
		[Test, Ignore]
		public void ShouldNotifyClients()
		{
			var notified = false;
			var hubContext = MockRepository.GenerateStub<IHubContext>();
			var clients = MockRepository.GenerateMock<IHubConnectionContext>();
			hubContext.Stub(x => x.Clients).Return(clients);
			var client = new FakeClientBuilder().Make("onEventMessage",
				new Action<Notification, string>((n, r) =>
				{
					notified = true;
				}));
			clients.Stub(x => x.Group(null)).IgnoreArguments().Return(client);
			var target = new MessageBrokerController(() => hubContext);

			target.NotifyClients(new Notification());
			
			notified.Should().Be.True();
		}
	}

	public class MessageBrokerController
	{
		private readonly Func<IHubContext> _hubContext;

		public MessageBrokerController(Func<IHubContext> hubContext)
		{
			_hubContext = hubContext;
		}

		public void NotifyClients(Notification notification)
		{
		}
	}
}
