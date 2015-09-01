using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[RtaTest]
	[Toggle(Toggles.RTA_MultiTenancy_32539)]
	[TestFixture]
	public class MultiTenancyTest
	{
		public FakeRtaDatabase Database;
		public IRta Target;
		public FakeApplicationData ApplicationData;

		[Test]
		public void ShouldAcceptAuthenticationKeyForATenant()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			Database
				.WithTenant("tenant1", "key1")
				.WithTenant("tenant2", "key2")
				.WithUser("user1", person1)
				.WithUser("user2", person2);

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user1",
				AuthenticationKey = "key1"
			});
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user2",
				AuthenticationKey = "key2"
			});

			Database.PersistedAgentStates.ElementAt(0).PersonId.Should().Be(person1);
			Database.PersistedOnTenants.ElementAt(0).Should().Be("tenant1");
			Database.PersistedAgentStates.ElementAt(1).PersonId.Should().Be(person2);
			Database.PersistedOnTenants.ElementAt(1).Should().Be("tenant2");
		}

		//[Test]
		//public void ShouldUseDatasourceForTheTenant()
		//{
		//	var person1 = Guid.NewGuid();
		//	var person2 = Guid.NewGuid();
		//	Database
		//		.WithTenant("tenant1", "key1")
		//		.WithTenant("tenant2", "key2")
		//		.WithSource("tenant1", "source")
		//		.WithSource("tenant2", "source")
		//		.WithUser("user1", person1)
		//		.WithUser("user2", person2)
		//		;
		//}

	}
}