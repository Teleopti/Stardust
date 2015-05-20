using System;
using System.IO;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;

namespace Teleopti.Ccc.Sdk.LogicTest.MultiTenancy
{
	[TenantSdkTest]
	[TestCommon.IoC.Toggle(Toggles.MultiTenancy_LogonUseNewSchema_33049)]
	public class TenantPeopleSaverTest
	{
		public PostHttpRequestFake HttpRequestFake;
		public ITenantPeopleSaver Target { get; set; }
		//TODO: tenant - remove me when etl no longer go to tenant service
		public CurrentTenantCredentialsFake CurrentTenantCredentials;

		[Test]
		public void ShouldMakeIdentityOfDomainAndWin()
		{
#pragma warning disable 618
			var id = Guid.NewGuid();
			var dto = new PersonDto
			{
				Id = id,
				WindowsDomain = "DOMÄÄÄN",
				WindowsLogOnName = "NAMNET"
			};
#pragma warning restore 618
			HttpRequestFake.SetReturnValue(new PersistPersonInfoResult { ApplicationLogonNameIsValid = true, IdentityIsValid = true, PasswordStrengthIsValid = true});
			Target.SaveTenantData(dto, id, "Teleopti WFM");
			var sent = HttpRequestFake.SentJson;
			sent.Should().Contain("Identity\":\"DOMÄÄÄN\\\\NAMNET");
		}

		[Test]
		public void ShouldMakeIdentityOfWin()
		{
#pragma warning disable 618
			var id = Guid.NewGuid();
			var dto = new PersonDto
			{
				Id = id,
				WindowsLogOnName = "NAMNET"
			};
#pragma warning restore 618
			HttpRequestFake.SetReturnValue(new PersistPersonInfoResult { ApplicationLogonNameIsValid = true, IdentityIsValid = true, PasswordStrengthIsValid = true });
			Target.SaveTenantData(dto, id, "Teleopti WFM");
			var sent = HttpRequestFake.SentJson;
			sent.Should().Contain("Identity\":\"NAMNET");
		}

		[Test]
		public void ShouldOverrideIdentityOfWin()
		{
#pragma warning disable 618
			var id = Guid.NewGuid();
			var dto = new PersonDto
			{
				Id = id,
				WindowsLogOnName = "NAMNET",
				Identity = "THEREALIDENTITY"
			};
#pragma warning restore 618
			HttpRequestFake.SetReturnValue(new PersistPersonInfoResult { ApplicationLogonNameIsValid = true, IdentityIsValid = true, PasswordStrengthIsValid = true });
			Target.SaveTenantData(dto, id, "Teleopti WFM");
			var sent = HttpRequestFake.SentJson;
			sent.Should().Contain("Identity\":\"THEREALIDENTITY");
		}

		[Test, ExpectedException(typeof(InvalidDataException))]
		public void ShouldThrowOnNotValidAnything()
		{
			var id = Guid.NewGuid();
			var dto = new PersonDto
			{
				Id = id,
				Identity = "DUMMY"
			};

			HttpRequestFake.SetReturnValue(new PersistPersonInfoResult { ApplicationLogonNameIsValid = true, IdentityIsValid = false, PasswordStrengthIsValid = true });
			Target.SaveTenantData(dto, id, "Teleopti WFM");

		}
	}
}