using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.Sdk.LogicTest.MultiTenancy
{
	[TenantSdkTest]
	[Toggle(Toggles.MultiTenancy_LogonUseNewSchema_33049)]
	public class TenantPeopleLoaderTest
	{
		public PostHttpRequestFake HttpRequestFake;
		public ITenantPeopleLoader Target { get; set; }

		[Test]
		public void ShouldSetLogonInfoOnDtos()
		{
#pragma warning disable 618
			var id = Guid.NewGuid();
			var dtos = new List<PersonDto>
			{
				new PersonDto {Id= id, ApplicationLogOnName = "DUMMY", WindowsDomain = "DOMÄÄÄN", WindowsLogOnName = "NAMNET", ApplicationLogOnPassword = "rappakalja"}
			};
			var infos = new List<LogonInfoModel>
			{
				new LogonInfoModel {Identity = "TOPTINET\\Ola", LogonName = "Ola", PersonId = id}
			};
			HttpRequestFake.SetReturnValue(infos);
			
			Target.FillDtosWithLogonInfo(dtos);
			dtos.First().ApplicationLogOnName.Should().Be.EqualTo("Ola");

			dtos.First().WindowsDomain.Should().Be.EqualTo("TOPTINET");
			dtos.First().WindowsLogOnName.Should().Be.EqualTo("Ola");
#pragma warning restore 618
			dtos.First().ApplicationLogOnPassword.Should().Be.EqualTo("");
		}

		[Test]
		public void ShouldHandleWhenIdentityIsNotWindows()
		{
			#pragma warning disable 618
			var id = Guid.NewGuid();
			var dtos = new List<PersonDto>
			{
				new PersonDto {Id= id, ApplicationLogOnName = "DUMMY", WindowsDomain = "DOMÄÄÄN", WindowsLogOnName = "NAMNET", ApplicationLogOnPassword = "rappakalja"}
			};
			var infos = new List<LogonInfoModel>
			{
				new LogonInfoModel {Identity = "someotheridentitysalesforceforexample", LogonName = "Ola", PersonId = id}
			};
			HttpRequestFake.SetReturnValue(infos);

			Target.FillDtosWithLogonInfo(dtos);
			dtos.First().ApplicationLogOnName.Should().Be.EqualTo("Ola");
			dtos.First().WindowsDomain.Should().Be.EqualTo("");
			dtos.First().WindowsLogOnName.Should().Be.EqualTo("");
#pragma warning restore 618
			dtos.First().Identity.Should().Be.EqualTo("someotheridentitysalesforceforexample");
		}
	}
}