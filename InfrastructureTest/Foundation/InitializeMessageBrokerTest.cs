using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	[TestFixture]
	public class InitializeMessageBrokerTest
	{
		[Test]
		public void ShouldStart()
		{
			var messageBrokerComposite = MockRepository.GenerateMock<IMessageBrokerComposite>();
			messageBrokerComposite.Stub(x => x.ServerUrl).PropertyBehavior();
			var target=new InitializeMessageBroker(messageBrokerComposite);
			var httpLocalhost = "http://localhost:52858";
			target.Start(new Dictionary<string, string>
			{
				{"MessageBroker", httpLocalhost},
				{"MessageBrokerLongPolling", "true"}
			});

			messageBrokerComposite.ServerUrl.Should().Be.EqualTo(httpLocalhost);
			messageBrokerComposite.AssertWasCalled(x=>x.StartBrokerService(true));
		}

		[Test]
		public void ShouldStartWithoutLongPolling()
		{
			var messageBrokerComposite = MockRepository.GenerateMock<IMessageBrokerComposite>();
			messageBrokerComposite.Stub(x => x.ServerUrl).PropertyBehavior();
			var target = new InitializeMessageBroker(messageBrokerComposite);
			var httpLocalhost = "http://localhost:52858";
			target.Start(new Dictionary<string, string>
			{
				{"MessageBroker", httpLocalhost}
			});

			messageBrokerComposite.ServerUrl.Should().Be.EqualTo(httpLocalhost);
			messageBrokerComposite.AssertWasCalled(x => x.StartBrokerService());
		}
	}
}