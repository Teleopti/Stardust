using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.MessageBroker
{
	[TestFixture]
	[PrincipalAndStateTest]
	[Toggle(Toggles.Mailbox_Optimization_41900)]
	public class MailboxNotSignedInTest
	{
		public IPrincipalAndStateContext Context;
		public IMessageBrokerServer Server;

		[Test]
		public void ShouldWorkWithoutSignIn()
		{
			Context.Logout();
			Server.PopMessages(new Message().Routes().First(), Guid.NewGuid().ToString());
		}
	}
}