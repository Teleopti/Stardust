using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service
{
	[RtaTest]
	[TestFixture]
	public class AuthenticationKeyTenantTest
	{
		public FakeDatabase Database;
		public Rta Target;

		[Test]
		public void ShouldAcceptAuthenticationKeyForATenant()
		{
			Database
				.WithAgent("user")
				.WithTenant("tenant", "key");

			Target.ProcessState(new StateForTest
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
				() => Target.ProcessState(new StateForTest
				{
					AuthenticationKey = "key"
				}));

			Database.StoredState.Should().Be.Null();
		}

		[Test]
		public void ShouldAcceptLegacyAuthenticationKeyIf1Tenant()
		{
			Database
				.WithAgent("user")
				.WithTenant("tenant", LegacyAuthenticationKey.TheKey);

			Target.ProcessState(new StateForTest
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
				.WithTenant("tenant1", LegacyAuthenticationKey.TheKey)
				.WithTenant("tenant2", "key");

			Assert.Throws(typeof (LegacyAuthenticationKeyException),
				() => Target.ProcessState(new StateForTest
				{
					AuthenticationKey = LegacyAuthenticationKey.TheKey
				}));

			Database.StoredState.Should().Be.Null();
		}

		[Test]
		public void ShouldAcceptIfThirdAndFourthLetterOfAuthenticationKeyIsCorrupted_BecauseOfEncodingIssuesWithThe3rdLetterOfTheDefaultKeyAndWeAreNotAllowedToChangeTheDefault()
		{
			Database
				.WithAgent("user")
				.WithTenant("tenant", LegacyAuthenticationKey.TheKey);

			Target.ProcessState(new StateForTest
			{
				UserCode = "user",
				AuthenticationKey = LegacyAuthenticationKey.TheKey.Remove(2, 2).Insert(2, "_")
			});

			Database.StoredState.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldThrowIfMultipleTenantsAndLegacyAuthenticationKeyIsUsed()
		{
			Database.WithTenant("tenant1", LegacyAuthenticationKey.TheKey);
			Database.WithTenant("tenant2", "key");

			Assert.Throws(typeof (LegacyAuthenticationKeyException),
				() => Target.ProcessState(new StateForTest
				{
					UserCode = "user",
					AuthenticationKey = LegacyAuthenticationKey.TheKey.Remove(2, 2).Insert(2, "_")
				}));
			Database.StoredState.Should().Be.Null();
		}


		[Test]
		public void ShouldThrowInvalidAuthenticationKeyExceptionWithPriorState()
		{
			Database.WithAgent("user");
			Target.ProcessState(new StateForTest
			{
				UserCode = "user"
			});

			Assert.Throws(typeof (InvalidAuthenticationKeyException),
				() => Target.ProcessState(new StateForTest
				{
					AuthenticationKey = "key",
					UserCode = "user"
				}));
		}

	}
}