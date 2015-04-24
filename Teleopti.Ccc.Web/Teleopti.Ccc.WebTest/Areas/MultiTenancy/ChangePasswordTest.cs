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
		public TenantUnitOfWorkFake TenantUnitOfWork;
		public CheckPasswordStrengthFake CheckPasswordStrength;

		[Test]
		public void HappyPath()
		{
			var model = new ChangePasswordModel
			{
				UserName = RandomName.Make(),
				OldPassword = RandomName.Make(),
				NewPassword = RandomName.Make()
			};
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), model.UserName, model.OldPassword);
			ApplicationUserTenantQuery.Add(personInfo);

			Target.Modify(model);
			TenantUnitOfWork.WasCommitted.Should().Be.True();
		}

		[Test]
		public void InvalidUsernameShouldFailWith403()
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
			TenantUnitOfWork.WasCommitted.Should().Be.False();
		}

		[Test]
		public void InvalidOldPasswordShouldFailWith403()
		{
			var model = new ChangePasswordModel
			{
				UserName = RandomName.Make(),
				OldPassword = RandomName.Make(),
				NewPassword = RandomName.Make()
			};
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), model.UserName, RandomName.Make());
			ApplicationUserTenantQuery.Add(personInfo);

			var ex = Assert.Throws<HttpException>(() => Target.Modify(model));

			ex.GetHttpCode().Should().Be.EqualTo(HttpStatusCode.Forbidden);
			TenantUnitOfWork.WasCommitted.Should().Be.False();
		}

		[Test]
		public void SameOldAndNewPasswordShouldFailWith400()
		{
			var pw = RandomName.Make();
			var model = new ChangePasswordModel
			{
				UserName = RandomName.Make(),
				OldPassword = pw,
				NewPassword = pw
			};
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), model.UserName, pw);

			ApplicationUserTenantQuery.Add(personInfo);

			var ex = Assert.Throws<HttpException>(() => Target.Modify(model));
			ex.GetHttpCode().Should().Be.EqualTo(HttpStatusCode.BadRequest);
			TenantUnitOfWork.WasCommitted.Should().Be.False();
		}

		[Test]
		public void FailingPasswordStrengthShouldFailWith400()
		{
			CheckPasswordStrength.WillThrow();
			var model = new ChangePasswordModel
			{
				UserName = RandomName.Make(),
				OldPassword = RandomName.Make(),
				NewPassword = RandomName.Make()
			};
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), model.UserName, model.OldPassword);
			ApplicationUserTenantQuery.Add(personInfo);

			var ex = Assert.Throws<HttpException>(() => Target.Modify(model));
			ex.GetHttpCode().Should().Be.EqualTo(HttpStatusCode.BadRequest);
			TenantUnitOfWork.WasCommitted.Should().Be.False();
		}

		[Test]
		public void InvalidOldPasswordShouldIncreaseInvalidAttempts()
		{
			var model = new ChangePasswordModel
			{
				UserName = RandomName.Make(),
				OldPassword = RandomName.Make(),
				NewPassword = RandomName.Make()
			};
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), model.UserName, RandomName.Make());
			ApplicationUserTenantQuery.Add(personInfo);

			var invalidAttemptsBefore = personInfo.ApplicationLogonInfo.InvalidAttempts;
			Assert.Throws<HttpException>(() => Target.Modify(model));

			personInfo.ApplicationLogonInfo.InvalidAttempts.Should().Be.EqualTo(invalidAttemptsBefore + 1);
		}
	}
}