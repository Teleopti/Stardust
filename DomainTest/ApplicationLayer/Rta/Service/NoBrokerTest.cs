using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	public class NoBrokerTest
	{
		public Domain.ApplicationLayer.Rta.Service.Rta target;
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