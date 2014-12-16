using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[TestFixture]
	public class NoBrokerTest
	{
		[Test, Ignore]
		public void ShouldNotSendAnyMessages()
		{
			var sender = new FakeMessageSender();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", Guid.NewGuid())
				.Make();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 9:00"), sender);

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "ready"
			});

			sender.AllNotifications.Should().Be.Empty();
		}
	}
}