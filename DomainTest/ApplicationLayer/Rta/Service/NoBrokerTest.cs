using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	[Toggle(Toggles.RTA_NoBroker_31237)]
	public class NoBrokerTest
	{
		public IRta target;
		public FakeMessageSender sender;
		public FakeRtaDatabase database;

		[Test]
		public void ShouldNotSendAnyMessages()
		{
			database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", Guid.NewGuid())
				;

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "ready"
			});

			sender.AllNotifications.Should().Be.Empty();
		}
	}
}