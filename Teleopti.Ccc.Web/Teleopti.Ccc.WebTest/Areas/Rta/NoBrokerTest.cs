using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Rta;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[TestFixture]
	[RtaTest]
	[Toggle(Toggles.RTA_NoBroker_31237)]
	public class NoBrokerTest
	{
		public IRta target;
		public FakeMessageSender sender;
		public FakeRtaDatabase database;

		[Test, Ignore]
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