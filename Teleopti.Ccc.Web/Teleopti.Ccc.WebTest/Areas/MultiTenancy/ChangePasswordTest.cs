using System;
using System.Net;
using System.Web;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Security;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.MultiTenancy;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy
{
	[TenantTest]
	public class ChangePasswordTest
	{
		public PasswordController Target;
		public FindPersonInfoFake FindPersonInfo;
		public TenantUnitOfWorkFake TenantUnitOfWork;
		public CheckPasswordStrengthFake CheckPasswordStrength;
		public TenantAuthenticationFake TenantAuthentication;

		[Test]
		public void HappyPath()
		{
			var model = new ChangePasswordModel
			{
				PersonId = Guid.NewGuid(),
				OldPassword = RandomName.Make(),
				NewPassword = RandomName.Make()
			};
			var personInfo = new PersonInfo(new Tenant(string.Empty), model.PersonId);
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), model.OldPassword, new OneWayEncryption());
			FindPersonInfo.Add(personInfo);

			Target.Modify(model);
			TenantUnitOfWork.WasCommitted.Should().Be.True();
		}

		[Test]
		public void ShouldAcceptAccessWithoutTenantCredentials()
		{
			TenantAuthentication.NoAccess();
			
			HappyPath();
		}

		[Test]
		public void InvalidIdShouldFailWith403()
		{
			var model = new ChangePasswordModel
			{
				PersonId = Guid.NewGuid(),
				OldPassword = RandomName.Make(),
				NewPassword = RandomName.Make()
			};
			var personInfo = new PersonInfo(new Tenant(string.Empty), Guid.NewGuid());
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), model.OldPassword, new OneWayEncryption());
			FindPersonInfo.Add(personInfo);

			var ex = Assert.Throws<HttpException>(() => Target.Modify(model));

			ex.GetHttpCode().Should().Be.EqualTo(HttpStatusCode.Forbidden);
			TenantUnitOfWork.WasCommitted.Should().Be.False();
		}

		[Test]
		public void InvalidOldPasswordShouldFailWith403()
		{
			var model = new ChangePasswordModel
			{
				PersonId = Guid.NewGuid(),
				OldPassword = RandomName.Make(),
				NewPassword = RandomName.Make()
			};
			var personInfo = new PersonInfo(new Tenant(string.Empty), model.PersonId);
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), RandomName.Make(), new OneWayEncryption());
			FindPersonInfo.Add(personInfo);

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
				PersonId = Guid.NewGuid(),
				OldPassword = pw,
				NewPassword = pw
			};
			var personInfo = new PersonInfo(new Tenant(string.Empty), model.PersonId);
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), pw, new OneWayEncryption());
			FindPersonInfo.Add(personInfo);

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
				PersonId = Guid.NewGuid(),
				OldPassword = RandomName.Make(),
				NewPassword = RandomName.Make()
			};
			var personInfo = new PersonInfo(new Tenant(string.Empty), model.PersonId);
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), model.OldPassword, new OneWayEncryption());
			FindPersonInfo.Add(personInfo);

			var ex = Assert.Throws<HttpException>(() => Target.Modify(model));
			ex.GetHttpCode().Should().Be.EqualTo(HttpStatusCode.BadRequest);
			TenantUnitOfWork.WasCommitted.Should().Be.False();
		}

		[Test]
		public void InvalidOldPasswordShouldIncreaseInvalidAttempts()
		{
			var model = new ChangePasswordModel
			{
				PersonId = Guid.NewGuid(),
				OldPassword = RandomName.Make(),
				NewPassword = RandomName.Make()
			};
			var personInfo = new PersonInfo(new Tenant(string.Empty), model.PersonId);
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), RandomName.Make(), new OneWayEncryption());
			FindPersonInfo.Add(personInfo);

			var invalidAttemptsBefore = personInfo.ApplicationLogonInfo.InvalidAttempts;
			Assert.Throws<HttpException>(() => Target.Modify(model));

			personInfo.ApplicationLogonInfo.InvalidAttempts.Should().Be.EqualTo(invalidAttemptsBefore + 1);
		}

		[Test]
		public void LockedAccountShouldBeUnlockedAfterSuccessfulPasswordChange()
		{
			var model = new ChangePasswordModel
			{
				PersonId = Guid.NewGuid(),
				OldPassword = RandomName.Make(),
				NewPassword = RandomName.Make()
			};
			var personInfo = new PersonInfo(new Tenant(string.Empty), model.PersonId);
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), model.OldPassword, new OneWayEncryption());
			personInfo.ApplicationLogonInfo.Lock();
			FindPersonInfo.Add(personInfo);

			Target.Modify(model);
			personInfo.ApplicationLogonInfo.IsLocked.Should().Be.False();
		}
	}
}