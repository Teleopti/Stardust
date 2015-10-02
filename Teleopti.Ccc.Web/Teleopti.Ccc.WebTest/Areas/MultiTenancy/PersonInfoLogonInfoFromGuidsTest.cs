using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
		public void ShouldGetLogonInfo()
		{
			var logonInfo1 = new LogonInfo{PersonId = Guid.NewGuid(), LogonName = "test1"};
			FindLogonInfo.Add(logonInfo1);

			Target.LogonInfoFromLogonName(logonInfo1.LogonName).Result<LogonInfo>().Should().Be.EqualTo(logonInfo1);
			TenantUnitOfWork.WasCommitted.Should().Be.True();
		}
	}

	[TenantTest]
	public class PersonInfoLogonInfoFromGuidsTest
	{
		public PersonInfoController Target;
		public FindLogonInfoFake FindLogonInfo;
		public TenantAuthenticationFake TenantAuthentication;
		public TenantUnitOfWorkFake TenantUnitOfWork;

		[Test]
		public void ShouldFetchLogonInfos()
		{
			var logonInfo1 = new LogonInfo{PersonId = Guid.NewGuid()};
			var logonInfo2 = new LogonInfo{PersonId = Guid.NewGuid()};

			FindLogonInfo.Add(logonInfo1);
			FindLogonInfo.Add(logonInfo2);

			Target.LogonInfoFromGuids(new[] {logonInfo1.PersonId, logonInfo2.PersonId}).Result<IEnumerable<LogonInfo>>()
				.Should().Have.SameValuesAs(logonInfo1, logonInfo2);
			TenantUnitOfWork.WasCommitted.Should().Be.True();
		}


		[Test]
		public void ShouldThrowIfNoValidTenantCredentialsWhenReadingLogonInfo()
		{
			TenantAuthentication.NoAccess();
			var res = Assert.Throws<HttpException>(() => Target.LogonInfoFromGuids(Enumerable.Empty<Guid>()));
			res.GetHttpCode().Should().Be.EqualTo(TenantUnitOfWorkAspect.NoTenantAccessHttpErrorCode);
			TenantUnitOfWork.WasCommitted.Should().Be.False();
		}
	}
}