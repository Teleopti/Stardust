using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Web.Areas.MultiTenancy;
using Teleopti.Ccc.WebTest.TestHelper;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy
{
	[TenantTest]
	public class PersonInfoControllerTest
	{
		public PersonInfoController Target;
		public FindLogonInfoFake FindLogonInfo;
		public TenantAuthenticationFake TenantAuthentication;
		public TenantUnitOfWorkFake TenantUnitOfWork;

		[Test]
		public void ShouldGetLogonInfoFromLogonName()
		{
			var logonInfo1 = new LogonInfo{PersonId = Guid.NewGuid(), LogonName = "test1"};
			FindLogonInfo.Add(logonInfo1);

			Target.LogonInfoFromLogonName(logonInfo1.LogonName).Result<LogonInfo>().Should().Be.EqualTo(logonInfo1);
			TenantUnitOfWork.WasCommitted.Should().Be.True();
		}

		[Test]
		public void ShouldGetLogonInfoFromIdentity()
		{
			var logonInfo1 = new LogonInfo { PersonId = Guid.NewGuid(), Identity = "identity1"};
			FindLogonInfo.Add(logonInfo1);

			Target.LogonInfoFromIdentity(logonInfo1.Identity).Result<LogonInfo>().Should().Be.EqualTo(logonInfo1);
			TenantUnitOfWork.WasCommitted.Should().Be.True();
		}
	}
}