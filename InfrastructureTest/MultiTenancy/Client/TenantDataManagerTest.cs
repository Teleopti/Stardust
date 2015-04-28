using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
{
	[TenantClientTest]
	[TestCommon.IoC.Toggle(Toggles.MultiTenancy_People_32113)]
	public class TenantDataManagerTest
	{
		public PostHttpRequestFake HttpRequestFake;
		public ITenantDataManager Target;

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
			var saveResult = new PersistPersonInfoResult { PasswordStrengthIsValid = true, ApplicationLogonNameIsValid = false, IdentityIsValid = true };
			HttpRequestFake.SetReturnValue(saveResult);
			var result = Target.SaveTenantData(tenantAuthenticationData);
			HttpRequestFake.CalledUrl.Should().Contain("PersonInfo/Persist");
			result.Success.Should().Be.False();
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
	}
}