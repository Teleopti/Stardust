using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Web.Areas.MultiTenancy;
using Teleopti.Ccc.WebTest.TestHelper;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy
{
	[TenantTest]
	public class PersonInfoLogonInfoFromGuidsTest
	{
		public PersonInfoController Target;
		public FindLogonInfoFake FindLogonInfo;

		[Test]
		public void ShouldFetchLogonInfos()
		{
			var logonInfo1 = new LogonInfo{PersonId = Guid.NewGuid()};
			var logonInfo2 = new LogonInfo{PersonId = Guid.NewGuid()};

			FindLogonInfo.Add(logonInfo1);
			FindLogonInfo.Add(logonInfo2);

			Target.LogonInfoFromGuids(new[] {logonInfo1.PersonId, logonInfo2.PersonId}).Result<IEnumerable<LogonInfo>>()
				.Should().Have.SameValuesAs(logonInfo1, logonInfo2);
		}
	}
}