using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[RtaTest]
	[Toggle(Toggles.RTA_MultiTenancy_32539)]
	[TestFixture]
	[Ignore]
	public class AuthenticationKeyTenantTest
	{
		public FakeRtaDatabase Database;
		public IRta Target;

		[Test]
		public void ShouldAcceptAuthenticationKeyForATenant()
		{
			Database.WithTenant("tenant", "key");

			Target.SaveState(new ExternalUserStateForTest
			{
				AuthenticationKey = "key"
			});

			Database.PersistedAgentState.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotAcceptAuthenticationKeyWithoutTenant()
		{
			var state = new ExternalUserStateForTest
			{
				AuthenticationKey = "key"
			};

			Assert.Throws(typeof(InvalidAuthenticationKeyException), () => Target.SaveState(state));

			Database.PersistedAgentState.Should().Be.Null();
		}

		[Test]
		public void ShouldAcceptLegacyAuthenticationKeyIf1Tenant()
		{
			Database.WithTenant("tenant", Domain.ApplicationLayer.Rta.Service.Rta.LegacyAuthenticationKey);

			Target.SaveState(new ExternalUserStateForTest
			{
				AuthenticationKey = Domain.ApplicationLayer.Rta.Service.Rta.LegacyAuthenticationKey
			});

			Database.PersistedAgentState.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotAcceptLegacyAuthenticationKeyIf2Tenants()
		{
			Database.WithTenant("tenant", Domain.ApplicationLayer.Rta.Service.Rta.LegacyAuthenticationKey);
			Database.WithTenant("tenant2", "key");
			var state = new ExternalUserStateForTest
			{
				AuthenticationKey = Domain.ApplicationLayer.Rta.Service.Rta.LegacyAuthenticationKey
			};

			Assert.Throws(typeof(InvalidAuthenticationKeyException), () => Target.SaveState(state));

			Database.PersistedAgentState.Should().Be.Null();
		}
	}
}