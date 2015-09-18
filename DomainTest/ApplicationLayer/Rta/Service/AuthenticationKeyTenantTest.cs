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
	public class AuthenticationKeyTenantTest
	{
		public FakeRtaDatabase Database;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldAcceptAuthenticationKeyForATenant()
		{
			Database
				.WithUser("user")
				.WithTenant("key");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user",
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
			Database
				.WithUser("user")
				.WithTenant(ConfiguredKeyAuthenticator.InternalEncodingFixedLegacyAuthenticationKey);

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user",
				AuthenticationKey = ConfiguredKeyAuthenticator.InputLegacyAuthenticationKey
			});

			Database.PersistedAgentState.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotAcceptLegacyAuthenticationKeyIf2Tenants()
		{
			Database.WithTenant(ConfiguredKeyAuthenticator.InternalEncodingFixedLegacyAuthenticationKey);
			Database.WithTenant("key");
			var state = new ExternalUserStateForTest
			{
				AuthenticationKey = ConfiguredKeyAuthenticator.InputLegacyAuthenticationKey
			};

			Assert.Throws(typeof(LegacyAuthenticationKeyException), () => Target.SaveState(state));

			Database.PersistedAgentState.Should().Be.Null();
		}

		[Test]
		public void ShouldAcceptIfThirdAndFourthLetterOfAuthenticationKeyIsCorrupted_BecauseOfEncodingIssuesWithThe3rdLetterOfTheDefaultKeyAndWeAreNotAllowedToChangeTheDefault()
		{
			Database
				.WithUser("user")
				.WithTenant(ConfiguredKeyAuthenticator.InternalEncodingFixedLegacyAuthenticationKey);

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user",
				AuthenticationKey = ConfiguredKeyAuthenticator.InputLegacyAuthenticationKey.Remove(2, 2).Insert(2, "_")
			});

			Database.PersistedAgentState.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotAcceptIfThirdAndFourthLetterOfAuthenticationKeyIsCorrupted_BecauseOfEncodingIssuesWithThe3rdLetterOfTheDefaultKeyAndWeAreNotAllowedToChangeTheDefault()
		{
			Database.WithTenant(ConfiguredKeyAuthenticator.InternalEncodingFixedLegacyAuthenticationKey);
			Database.WithTenant("key");

			var state = new ExternalUserStateForTest
			{
				UserCode = "user",
				AuthenticationKey = ConfiguredKeyAuthenticator.InputLegacyAuthenticationKey.Remove(2, 2).Insert(2, "_")
			};

			Assert.Throws(typeof(LegacyAuthenticationKeyException), () => Target.SaveState(state));
			Database.PersistedAgentState.Should().Be.Null();
		}
	}
}