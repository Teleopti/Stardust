using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[RtaTest]
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

			Database.StoredState.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotAcceptAuthenticationKeyWithoutTenant()
		{
			Assert.Throws(typeof (InvalidAuthenticationKeyException),
				() => Target.SaveState(new ExternalUserStateForTest
				{
					AuthenticationKey = "key"
				}));

			Database.StoredState.Should().Be.Null();
		}

		[Test]
		public void ShouldAcceptLegacyAuthenticationKeyIf1Tenant()
		{
			Database
				.WithUser("user")
				.WithTenant(LegacyAuthenticationKey.TheKey);

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user",
				AuthenticationKey = LegacyAuthenticationKey.TheKey
			});

			Database.StoredState.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotAcceptLegacyAuthenticationKeyIf2Tenants()
		{
			Database
				.WithTenant(LegacyAuthenticationKey.TheKey)
				.WithTenant("key");

			Assert.Throws(typeof (LegacyAuthenticationKeyException),
				() => Target.SaveState(new ExternalUserStateForTest
				{
					AuthenticationKey = LegacyAuthenticationKey.TheKey
				}));

			Database.StoredState.Should().Be.Null();
		}

		[Test]
		public void ShouldAcceptIfThirdAndFourthLetterOfAuthenticationKeyIsCorrupted_BecauseOfEncodingIssuesWithThe3rdLetterOfTheDefaultKeyAndWeAreNotAllowedToChangeTheDefault()
		{
			Database
				.WithUser("user")
				.WithTenant(LegacyAuthenticationKey.TheKey);

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user",
				AuthenticationKey = LegacyAuthenticationKey.TheKey.Remove(2, 2).Insert(2, "_")
			});

			Database.StoredState.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldThrowIfMultipleTenantsAndLegacyAuthenticationKeyIsUsed()
		{
			Database.WithTenant(LegacyAuthenticationKey.TheKey);
			Database.WithTenant("key");

			Assert.Throws(typeof (LegacyAuthenticationKeyException),
				() => Target.SaveState(new ExternalUserStateForTest
				{
					UserCode = "user",
					AuthenticationKey = LegacyAuthenticationKey.TheKey.Remove(2, 2).Insert(2, "_")
				}));
			Database.StoredState.Should().Be.Null();
		}


		[Test]
		public void ShouldThrowInvalidAuthenticationKeyExceptionWithPriorState()
		{
			Database.WithUser("user");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user"
			});

			Assert.Throws(typeof (InvalidAuthenticationKeyException),
				() => Target.SaveState(new ExternalUserStateForTest
				{
					AuthenticationKey = "key",
					UserCode = "user"
				}));
		}

	}
}