using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
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
		public FindTenantByNameQueryFake FindTenantByNameQuery;
		public CheckPasswordStrengthFake CheckPasswordStrength;
		public PersistPersonInfoFake PersistPersonInfo;
		public TenantUnitOfWorkFake TenantUnitOfWork;

		[Test]
		public void HappyPath()
		{
			var tenantName = RandomName.Make();
			FindTenantByNameQuery.Add(new Tenant(tenantName));
			var personInfoModel = new PersonInfoModel { Tenant = tenantName, ApplicationLogonName = RandomName.Make(), Password = RandomName.Make() };

			var result = Target.Persist(personInfoModel).Result<PersistPersonInfoResult>();

			allPropertiesShouldBeTrue(result);
			PersistPersonInfo.LastPersist.ApplicationLogonName.Should().Be.EqualTo(personInfoModel.ApplicationLogonName);
			TenantUnitOfWork.WasCommitted.Should().Be.True();
		}
		private static void allPropertiesShouldBeTrue(PersistPersonInfoResult result)
		{
			foreach (var propertyInfo in result.GetType().GetProperties())
			{
				((bool)propertyInfo.GetValue(result)).Should().Be.True();
			}
		}

		[Test]
		public void ShouldHandlePasswordStrengthException()
		{
			var tenantName = RandomName.Make();
			FindTenantByNameQuery.Add(new Tenant(tenantName));
			var personInfoModel = new PersonInfoModel { Tenant = tenantName, ApplicationLogonName = RandomName.Make(), Password = RandomName.Make()};
			CheckPasswordStrength.WillThrow(new PasswordStrengthException());

			var result = Target.Persist(personInfoModel).Result<PersistPersonInfoResult>();

			result.PasswordStrengthIsValid.Should().Be.False();
			TenantUnitOfWork.WasCommitted.Should().Be.False();
		}

		[Test]
		public void ShouldHandleDuplicateApplicationLogonNameException()
		{
			var tenantName = RandomName.Make();
			FindTenantByNameQuery.Add(new Tenant(tenantName));
			var personInfoModel = new PersonInfoModel { Tenant = tenantName, ApplicationLogonName = RandomName.Make(), Password = RandomName.Make() };
			TenantUnitOfWork.WillThrowAtCommit(new DuplicateApplicationLogonNameException());

			var result = Target.Persist(personInfoModel).Result<PersistPersonInfoResult>();

			result.ApplicationLogonNameIsValid.Should().Be.False();
			TenantUnitOfWork.WasCommitted.Should().Be.False();
		}

		[Test]
		public void ShouldHandleDuplicateIdentityException()
		{
			var tenantName = RandomName.Make();
			FindTenantByNameQuery.Add(new Tenant(tenantName));
			var personInfoModel = new PersonInfoModel { Tenant = tenantName, ApplicationLogonName = RandomName.Make(), Password = RandomName.Make() };
			TenantUnitOfWork.WillThrowAtCommit(new DuplicateIdentityException());

			var result = Target.Persist(personInfoModel).Result<PersistPersonInfoResult>();

			result.IdentityIsValid.Should().Be.False();
			TenantUnitOfWork.WasCommitted.Should().Be.False();
		}
	}
}