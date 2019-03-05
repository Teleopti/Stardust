using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Infrastructure.Events;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;

namespace Teleopti.Ccc.InfrastructureTest.MessageBroker
{
	[TestFixture]
	[DatabaseTest]
	public class MessageBrokerServerMailboxPurgerTest
	{
		public MessageBrokerMailboxPurger Target;
		public ILogOnOffContext Context;

		[Test]
		public void ShouldWork()
		{
			Context.Logout();
			Target.Handle(new SharedMinuteTickEvent());
			Target.Handle(new SharedMinuteTickEvent());
		}
	}
}