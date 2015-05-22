using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy.Core
{
	public class PersonInfoMapperTest
	{
		[Test]
		public void PersonIdShouldBeSet()
		{
			var id = Guid.NewGuid();
			var target = new PersonInfoMapper(new CurrentTenantFake(), new CheckPasswordStrengthFake());
			var result = target.Map(new PersonInfoModel { PersonId = id });
			result.Id.Should().Be.EqualTo(id);
		}

		[Test]
		public void IdentityShouldBeSet()
		{
			var identity = RandomName.Make();
			var target = new PersonInfoMapper(new CurrentTenantFake(), new CheckPasswordStrengthFake());
			var result = target.Map(new PersonInfoModel { Identity = identity });
			result.Identity.Should().Be.EqualTo(identity);
		}

		[Test]
		public void ApplicationLogonShouldBeSet()
		{
			var applicationLogon = RandomName.Make();
			var target = new PersonInfoMapper(new CurrentTenantFake(), new CheckPasswordStrengthFake());
			var result = target.Map(new PersonInfoModel { ApplicationLogonName = applicationLogon, Password = RandomName.Make()});
			result.ApplicationLogonName.Should().Be.EqualTo(applicationLogon);
		}

		[Test]
		public void ApplicationLogonNorPasswordShouldBeSetIfApplicationLogonIsMissing()
		{
			var target = new PersonInfoMapper(new CurrentTenantFake(), new CheckPasswordStrengthFake());
			var result = target.Map(new PersonInfoModel {Password = RandomName.Make(), ApplicationLogonName = null});
			result.ApplicationLogonName.Should().Be.Null();
			result.ApplicationLogonPassword.Should().Be.Null();
		}

		[Test]
		public void ApplicationLogonNorPasswordShouldBeSetIfPasswordIsMissing()
		{
			var target = new PersonInfoMapper(new CurrentTenantFake(), new CheckPasswordStrengthFake());
			var result = target.Map(new PersonInfoModel { ApplicationLogonName = RandomName.Make(), Password = null});
			result.ApplicationLogonName.Should().Be.Null();
			result.ApplicationLogonPassword.Should().Be.Null();
		}

		[Test]
		public void PasswordShouldBeSet()
		{
			var password = RandomName.Make();
			var target = new PersonInfoMapper(new CurrentTenantFake(), new CheckPasswordStrengthFake());
			var result = target.Map(new PersonInfoModel { Password = password, ApplicationLogonName = RandomName.Make()});
			result.ApplicationLogonPassword.Should().Be.EqualTo(EncryptPassword.ToDbFormat(password));
		}

		[Test]
		public void PasswordStrengthExceptionsShouldBubbleUp()
		{
			var password = RandomName.Make();
			var passwordStrength = MockRepository.GenerateStub<ICheckPasswordStrength>();
			passwordStrength.Expect(x => x.Validate(password)).Throw(new PasswordStrengthException());
			var target = new PersonInfoMapper(new CurrentTenantFake(), passwordStrength);

			Assert.Throws<PasswordStrengthException>(() => target.Map(new PersonInfoModel{Password = password, ApplicationLogonName = RandomName.Make()}));
		}

		[Test]
		public void TenantShouldBeSet()
		{
			var tenant = new Tenant(RandomName.Make());
			var currentTenant = new CurrentTenantFake();
			currentTenant.Set(tenant);

			var target = new PersonInfoMapper(currentTenant, new CheckPasswordStrengthFake());
			var result = target.Map(new PersonInfoModel());
			result.Tenant.Name.Should().Be.EqualTo(tenant.Name);
		}
	}
}