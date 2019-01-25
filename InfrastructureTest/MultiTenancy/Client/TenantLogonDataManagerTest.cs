using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
{
	[TenantClientTest]
	public class TenantLogonDataManagerTest
	{
		public PostHttpRequestFake HttpRequestFake;
		public GetHttpRequestFake GetRequestFake;
		public ITenantLogonDataManagerClient Target;
		public FakeCurrentTenantCredentials CurrentTenantCredentials;

		[Test]
		public void ShouldBatchCallsToGetLogonInfoModelsForGuids()
		{
			var guids = Enumerable.Repeat(0,400).Select(_ => Guid.NewGuid());

			var returnVal = Enumerable.Repeat(0,200).Select(_ => new LogonInfoModel());
			
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