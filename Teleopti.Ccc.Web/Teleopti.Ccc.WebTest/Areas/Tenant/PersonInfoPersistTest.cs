using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.Tenant;
using Teleopti.Ccc.Web.Areas.Tenant.Model;

namespace Teleopti.Ccc.WebTest.Areas.Tenant
{
	[TenantTest]
	public class PersonInfoPersistTest
	{
		public PersonInfoController Target;
		public FindTenantByNameQueryFake FindTenantByNameQuery;
		public TenantUnitOfWorkAspectFake TenantUnitOfWorkAspect;
		public CheckPasswordStrengthFake CheckPasswordStrength;
		public PersistPersonInfoFake PersistPersonInfo;

		[Test]
		public void HappyPath()
		{
			var tenantName = RandomName.Make();
			FindTenantByNameQuery.Add(new Infrastructure.MultiTenancy.Server.Tenant(tenantName));
			var personInfoModel = new PersonInfoModel { Tenant = tenantName, ApplicationLogonName = RandomName.Make(), Password = RandomName.Make() };

			var result = (PersistPersonInfoResult)Target.Persist(personInfoModel).Data;

			allPropertiesShouldBeTrue(result);
			PersistPersonInfo.LastPersist.ApplicationLogonName.Should().Be.EqualTo(personInfoModel.ApplicationLogonName);
			TenantUnitOfWorkAspect.LastCommitSucceded.Value.Should().Be.True();
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
			FindTenantByNameQuery.Add(new Infrastructure.MultiTenancy.Server.Tenant(tenantName));
			var personInfoModel = new PersonInfoModel { Tenant = tenantName, ApplicationLogonName = RandomName.Make(), Password = RandomName.Make()};
			CheckPasswordStrength.WillThrow(new PasswordStrengthException());

			var result = (PersistPersonInfoResult)Target.Persist(personInfoModel).Data;

			result.PasswordStrengthIsValid.Should().Be.False();
			TenantUnitOfWorkAspect.LastCommitSucceded.Value.Should().Be.False();
		}

		[Test]
		public void ShouldHandleDuplicateApplicationLogonNameException()
		{
			var tenantName = RandomName.Make();
			FindTenantByNameQuery.Add(new Infrastructure.MultiTenancy.Server.Tenant(tenantName));
			var personInfoModel = new PersonInfoModel { Tenant = tenantName, ApplicationLogonName = RandomName.Make(), Password = RandomName.Make() };
			PersistPersonInfo.WillThrow(new DuplicateApplicationLogonNameException());

			var result = (PersistPersonInfoResult)Target.Persist(personInfoModel).Data;

			result.ApplicationLogonNameIsValid.Should().Be.False();
			TenantUnitOfWorkAspect.LastCommitSucceded.Value.Should().Be.False();
		}

		[Test]
		public void ShouldHandleDuplicateIdentityException()
		{
			var tenantName = RandomName.Make();
			FindTenantByNameQuery.Add(new Infrastructure.MultiTenancy.Server.Tenant(tenantName));
			var personInfoModel = new PersonInfoModel { Tenant = tenantName, ApplicationLogonName = RandomName.Make(), Password = RandomName.Make() };
			PersistPersonInfo.WillThrow(new DuplicateIdentityException());

			var result = (PersistPersonInfoResult)Target.Persist(personInfoModel).Data;

			result.IdentityIsValid.Should().Be.False();
			TenantUnitOfWorkAspect.LastCommitSucceded.Value.Should().Be.False();
		}
	}
}