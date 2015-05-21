using System;
using System.Linq;
using System.Web;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Web.Areas.MultiTenancy;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy
{
	[TenantTest]
	public class PersonInfoDeleteTest
	{
		public PersonInfoController Target;
		public DeletePersonInfoFake DeletePersonInfo;
		public TenantAuthenticationFake TenantAuthentication;
		public TenantUnitOfWorkFake TenantUnitOfWork;

		[Test]
		public void ShouldDeletedPersonInfos()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();

			Target.Delete(new[] { personId1, personId2 });

			DeletePersonInfo.WasDeleted.Should().Have.SameValuesAs(personId1, personId2);
			TenantUnitOfWork.WasCommitted.Should().Be.True();
		}

		[Test]
		public void ShouldThrowIfNoValidTenantCredentialsAtDelete()
		{
			TenantAuthentication.NoAccess();
			var res = Assert.Throws<HttpException>(() => Target.Delete(Enumerable.Empty<Guid>()));
			res.GetHttpCode().Should().Be.EqualTo(TenantUnitOfWorkAspect.NoTenantAccessHttpErrorCode);
			TenantUnitOfWork.WasCommitted.Should().Be.False();
		}
	}
}