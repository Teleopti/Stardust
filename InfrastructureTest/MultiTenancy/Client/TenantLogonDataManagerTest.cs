using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
{
	[TenantClientTest]
	public class TenantLogonDataManagerTest
	{
		public PostHttpRequestFake HttpRequestFake;
		public GetHttpRequestFake GetRequestFake;
		public ITenantLogonDataManager Target;
		public CurrentTenantCredentialsFake CurrentTenantCredentials;

		[Test]
		public void ShouldBatchCallsToGetLogonInfoModelsForGuids()
		{
			var guids = new List<Guid>();
			for (var i = 0; i < 400; i++)
			{
				guids.Add(Guid.NewGuid());
			}
			var returnVal = new List<LogonInfoModel>();
			for (var i = 0; i < 200; i++)
			{
				returnVal.Add(new LogonInfoModel());
			}

			HttpRequestFake.SetReturnValue(returnVal);

			var result = Target.GetLogonInfoModelsForGuids(guids);
			result.Count().Should().Be.EqualTo(400);
		}

		[Test]
		public void ShouldSendTenantWhenPersisting()
		{
			var tenantCredentials = new TenantCredentials(Guid.NewGuid(), RandomName.Make());
			CurrentTenantCredentials.Has(tenantCredentials);
			HttpRequestFake.SetReturnValue(Enumerable.Empty<LogonInfoModel>());

			Target.GetLogonInfoModelsForGuids(new[]{Guid.NewGuid()});

			HttpRequestFake.SendTenantCredentials.Should().Be.SameInstanceAs(tenantCredentials);
		}

		[Test]
		public void ShouldGetLogonInfoForLogonName()
		{
			var logonName = RandomName.Make();
			var personId = Guid.NewGuid();
			GetRequestFake.SetReturnValue(new LogonInfoModel
			{
				LogonName = logonName,
				PersonId = personId
			});
			
			var result = Target.GetLogonInfoForLogonName(logonName);
			
			result.LogonName.Should().Be.EqualTo(logonName);
			result.PersonId.Should().Be.EqualTo(personId);
		}

		[Test]
		public void ShouldSendTenantWhenPersistingByLogonName()
		{
			GetRequestFake.SetReturnValue(new LogonInfoModel());
			var tenantCredentials = new TenantCredentials(Guid.NewGuid(), RandomName.Make());
			CurrentTenantCredentials.Has(tenantCredentials);

			Target.GetLogonInfoForLogonName("test1");

			GetRequestFake.SendTenantCredentials.Should().Be.SameInstanceAs(tenantCredentials);
		}

		[Test]
		public void ShouldGetLogonInfoForIdentity()
		{
			const string identity = "identity1";
			var personId = Guid.NewGuid();
			GetRequestFake.SetReturnValue(new LogonInfoModel
			{
				Identity = identity,
				PersonId = personId
			});

			var result = Target.GetLogonInfoForIdentity(identity);

			result.Identity.Should().Be.EqualTo(identity);
			result.PersonId.Should().Be.EqualTo(personId);
		}

		[Test]
		public void ShouldSendTenantWhenPersistingByIdentity()
		{
			GetRequestFake.SetReturnValue(new LogonInfoModel());
			var tenantCredentials = new TenantCredentials(Guid.NewGuid(), RandomName.Make());
			CurrentTenantCredentials.Has(tenantCredentials);

			Target.GetLogonInfoForIdentity("identity1");

			GetRequestFake.SendTenantCredentials.Should().Be.SameInstanceAs(tenantCredentials);
		}
	}
}