﻿using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;

namespace Teleopti.Ccc.InfrastructureTest.MessageBroker
{
	[TestFixture]
	[DatabaseTest]
	public class MailboxNotSignedInTest
	{
		public ILogOnOffContext Context;
		public IMessageBrokerServer Server;

		[Test]
		public void ShouldWorkWithoutSignIn()
		{
			Context.Logout();
			Server.PopMessages(new Message().Routes().First(), Guid.NewGuid().ToString());
		}
	}
}