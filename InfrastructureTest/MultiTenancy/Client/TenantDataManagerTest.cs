using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
{
	[TenantClientTest]
	public class TenantDataManagerTest
	{
		public PostHttpRequestFake HttpRequestFake;
		public ITenantDataManagerClient Target;
		public FakeCurrentTenantCredentials CurrentTenantCredentials;

		[Test]
		public void ShouldDeleteTenantData()
		{
			var personIds = new[] {Guid.NewGuid()};
			Target.DeleteTenantPersons(personIds);
			HttpRequestFake.CalledUrl.Should().Contain("PersonInfo/Delete");
		}

		[Test]
		public void ShouldReturnSuccessWhenSuccessfulSave()
		{
			var tenantAuthenticationData = new TenantAuthenticationData();
			var saveResult = new PersistPersonInfoResult { PasswordStrengthIsValid = true, ApplicationLogonNameIsValid = true, IdentityIsValid = true};
			HttpRequestFake.SetReturnValue(saveResult);
			var result = Target.SaveTenantData(tenantAuthenticationData);
			HttpRequestFake.CalledUrl.Should().Contain("PersonInfo/Persist");
			result.Success.Should().Be.True();
		}

		[Test]
		public void ShouldReturnSuccessFalseWhenWrongPasswordStrength()
		{
			var tenantAuthenticationData = new TenantAuthenticationData();
			var saveResult = new PersistPersonInfoResult { PasswordStrengthIsValid = false,ApplicationLogonNameIsValid = true, IdentityIsValid = true};
			HttpRequestFake.SetReturnValue(saveResult);
			var result = Target.SaveTenantData(tenantAuthenticationData);
			HttpRequestFake.CalledUrl.Should().Contain("PersonInfo/Persist");
			result.Success.Should().Be.False();
		}

		[Test]
		public void ShouldReturnSuccessFalseWhenDoubleAppLogon()
		{
			var tenantAuthenticationData = new TenantAuthenticationData();
			var saveResult = new PersistPersonInfoResult { PasswordStrengthIsValid = true, ApplicationLogonNameIsValid = false, IdentityIsValid = true, ExistingPerson = Guid.NewGuid()};
			HttpRequestFake.SetReturnValue(saveResult);
			var result = Target.SaveTenantData(tenantAuthenticationData);
			HttpRequestFake.CalledUrl.Should().Contain("PersonInfo/Persist");
			result.Success.Should().Be.False();
			result.ExistingPerson.Should().Not.Be.EqualTo(Guid.Empty);
		}

		[Test]
		public void ShouldReturnSuccessFalseWhenDoubleIdentity()
		{
			var tenantAuthenticationData = new TenantAuthenticationData();
			var saveResult = new PersistPersonInfoResult { PasswordStrengthIsValid = true, ApplicationLogonNameIsValid = true, IdentityIsValid = false };
			HttpRequestFake.SetReturnValue(saveResult);
			var result = Target.SaveTenantData(tenantAuthenticationData);
			HttpRequestFake.CalledUrl.Should().Contain("PersonInfo/Persist");
			result.Success.Should().Be.False();
		}

		[Test]
		public void ShouldSendTenantWhenDeleting()
		{
			var tenantCredentials = new TenantCredentials(Guid.NewGuid(), RandomName.Make());
			CurrentTenantCredentials.Has(tenantCredentials);

			var personIds = new[] { Guid.NewGuid() };
			Target.DeleteTenantPersons(personIds);

			HttpRequestFake.SendTenantCredentials.Should().Be.SameInstanceAs(tenantCredentials);
		}

		[Test]
		public void ShouldSendTenantWhenPersisting()
		{
			var tenantCredentials = new TenantCredentials(Guid.NewGuid(), RandomName.Make());
			CurrentTenantCredentials.Has(tenantCredentials);
			var saveResult = new PersistPersonInfoResult { PasswordStrengthIsValid = true, ApplicationLogonNameIsValid = true, IdentityIsValid = true };
			HttpRequestFake.SetReturnValue(saveResult);

			Target.SaveTenantData(new TenantAuthenticationData());

			HttpRequestFake.SendTenantCredentials.Should().Be.SameInstanceAs(tenantCredentials);
		}
	}
}