using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
{
	[TenantClientTest]
	[TestCommon.IoC.Toggle(Toggles.MultiTenancy_LogonUseNewSchema_33049)]
	public class TenantLogonDataManagerTest
	{
		public PostHttpRequestFake HttpRequestFake;
		public ITenantLogonDataManager Target;

		[Test]
		public void ShouldBatchCallsToGetLogonInfoModelsForGuids()
		{
			var guids = new List<Guid>();
			for (int i = 0; i < 400; i++)
			{
				guids.Add(Guid.NewGuid());
			}
			var returnVal = new List<LogonInfoModel>();
			for (int i = 0; i < 200; i++)
			{
				returnVal.Add(new LogonInfoModel());
			}

			HttpRequestFake.SetReturnValue(returnVal);

			var result = Target.GetLogonInfoModelsForGuids(guids);
			result.Count.Should().Be.EqualTo(400);
		}
	}
}