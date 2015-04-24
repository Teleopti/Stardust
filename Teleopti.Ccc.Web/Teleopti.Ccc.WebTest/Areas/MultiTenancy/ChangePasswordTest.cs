using System.Net;
using System.Web;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.MultiTenancy;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy
{
	[TenantTest]
	public class ChangePasswordTest
	{
		public ChangePasswordController Target;
		public ApplicationUserTenantQueryFake ApplicationUserTenantQuery;
		public TenantUnitOfWorkManagerFake TenantUnitOfWorkManager;

		[Test, Ignore("not yet done")]
		public void HappyPath()
		{
			var model = new ChangePasswordModel
			{
				UserName = RandomName.Make(),
				OldPassword = RandomName.Make(),
				NewPassword = RandomName.Make()
			};
			
			Target.Modify(model);
		}

		[Test]
		public void InvalidUsername()
		{
			var model = new ChangePasswordModel
			{
				UserName = RandomName.Make(),
				OldPassword = RandomName.Make(),
				NewPassword = RandomName.Make()
			};
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), model.OldPassword);
			ApplicationUserTenantQuery.Add(personInfo);
			
			var ex = Assert.Throws<HttpException>(() => Target.Modify(model));
			ex.GetHttpCode().Should().Be.EqualTo(HttpStatusCode.Forbidden);
			TenantUnitOfWorkManager.WasCommitted.Should().Be.False();
		}
	}
}