using System;
using System.Web;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.MultiTenancy;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;
using Teleopti.Ccc.WebTest.TestHelper;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy
{
	[TenantTest]
	public class PersonInfoPersistTest
	{
		public PersonInfoController Target;
		public CheckPasswordStrengthFake CheckPasswordStrength;
		public PersistPersonInfoFake PersistPersonInfo;
		public TenantUnitOfWorkFake TenantUnitOfWork;
		public TenantAuthenticationFake TenantAuthentication;

		[Test]
		public void HappyPath()
		{
			var personInfoModel = new PersonInfoModel { ApplicationLogonName = RandomName.Make(), Password = RandomName.Make() };

			var result = Target.Persist(personInfoModel).Result<PersistPersonInfoResult>();

			allPropertiesShouldBeTrue(result);
			PersistPersonInfo.LastPersist.ApplicationLogonInfo.LogonName.Should().Be.EqualTo(personInfoModel.ApplicationLogonName);
			TenantUnitOfWork.WasCommitted.Should().Be.True();
		}
		private static void allPropertiesShouldBeTrue(PersistPersonInfoResult result)
		{
			foreach (var propertyInfo in result.GetType().GetProperties())
			{
				if(propertyInfo.Name != "ExistingPerson")
					((bool)propertyInfo.GetValue(result)).Should().Be.True();
				else
					((Guid)propertyInfo.GetValue(result)).Should().Be.EqualTo(Guid.Empty);
			}
		}

		[Test]
		public void ShouldHandlePasswordStrengthException()
		{
			var personInfoModel = new PersonInfoModel { ApplicationLogonName = RandomName.Make(), Password = RandomName.Make()};
			CheckPasswordStrength.WillThrow();

			var result = Target.Persist(personInfoModel).Result<PersistPersonInfoResult>();

			result.PasswordStrengthIsValid.Should().Be.False();
			TenantUnitOfWork.WasCommitted.Should().Be.False();
		}

		[Test]
		public void ShouldHandleDuplicateApplicationLogonNameException()
		{
			var personInfoModel = new PersonInfoModel { ApplicationLogonName = RandomName.Make(), Password = RandomName.Make() };
			TenantUnitOfWork.WillThrowAtCommit(new DuplicateApplicationLogonNameException(Guid.NewGuid()));

			var result = Target.Persist(personInfoModel).Result<PersistPersonInfoResult>();

			result.ApplicationLogonNameIsValid.Should().Be.False();
			result.ExistingPerson.Should().Not.Be.EqualTo(Guid.Empty);
			TenantUnitOfWork.WasCommitted.Should().Be.False();
		}

		[Test]
		public void ShouldHandleDuplicateIdentityException()
		{
			var personInfoModel = new PersonInfoModel { ApplicationLogonName = RandomName.Make(), Password = RandomName.Make() };
			TenantUnitOfWork.WillThrowAtCommit(new DuplicateIdentityException(Guid.Empty));

			var result = Target.Persist(personInfoModel).Result<PersistPersonInfoResult>();

			result.IdentityIsValid.Should().Be.False();
			TenantUnitOfWork.WasCommitted.Should().Be.False();
		}

		[Test]
		public void ShouldThrowIfNoValidTenantCredentialsAtPersist()
		{
			TenantAuthentication.NoAccess();
			var res = Assert.Throws<HttpException>(() => Target.Persist(new PersonInfoModel()).Result<PersistPersonInfoResult>());
			res.GetHttpCode().Should().Be.EqualTo(TenantUnitOfWorkAspect.NoTenantAccessHttpErrorCode);
			TenantUnitOfWork.WasCommitted.Should().Be.False();
		}
	}
}