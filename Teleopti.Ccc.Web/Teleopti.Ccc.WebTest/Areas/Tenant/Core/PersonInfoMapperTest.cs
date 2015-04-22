using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.Tenant.Core;
using Teleopti.Ccc.Web.Areas.Tenant.Model;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Tenant.Core
{
	public class PersonInfoMapperTest
	{
		[Test]
		public void PersonIdShouldBeSet()
		{
			var id = Guid.NewGuid();
			var target = new PersonInfoMapper(MockRepository.GenerateMock<IFindTenantByNameQuery>(), new CheckPasswordStrengthSuccessful());
			var result = target.Map(new PersonInfoModel { PersonId = id });
			result.Id.Should().Be.EqualTo(id);
		}

		[Test]
		public void IdentityShouldBeSet()
		{
			var identity = RandomName.Make();
			var target = new PersonInfoMapper(MockRepository.GenerateMock<IFindTenantByNameQuery>(), new CheckPasswordStrengthSuccessful());
			var result = target.Map(new PersonInfoModel { Identity = identity });
			result.Identity.Should().Be.EqualTo(identity);
		}

		[Test]
		public void ApplicationLogonShouldBeSet()
		{
			var applicationLogon = RandomName.Make();
			var target = new PersonInfoMapper(MockRepository.GenerateMock<IFindTenantByNameQuery>(), new CheckPasswordStrengthSuccessful());
			var result = target.Map(new PersonInfoModel { ApplicationLogonName = applicationLogon, Password = RandomName.Make()});
			result.ApplicationLogonName.Should().Be.EqualTo(applicationLogon);
		}

		[Test]
		public void ApplicationLogonNorPasswordShouldBeSetIfApplicationLogonIsMissing()
		{
			var target = new PersonInfoMapper(MockRepository.GenerateMock<IFindTenantByNameQuery>(), new CheckPasswordStrengthFailing());
			var result = target.Map(new PersonInfoModel {Password = RandomName.Make(), ApplicationLogonName = null});
			result.ApplicationLogonName.Should().Be.Null();
			result.Password.Should().Be.Null();
		}

		[Test]
		public void ApplicationLogonNorPasswordShouldBeSetIfPasswordIsMissing()
		{
			var target = new PersonInfoMapper(MockRepository.GenerateMock<IFindTenantByNameQuery>(), new CheckPasswordStrengthFailing());
			var result = target.Map(new PersonInfoModel { ApplicationLogonName = RandomName.Make(), Password = null});
			result.ApplicationLogonName.Should().Be.Null();
			result.Password.Should().Be.Null();
		}

		[Test]
		public void PasswordShouldBeSet()
		{
			var password = RandomName.Make();
			var target = new PersonInfoMapper(MockRepository.GenerateMock<IFindTenantByNameQuery>(), new CheckPasswordStrengthSuccessful());
			var result = target.Map(new PersonInfoModel { Password = password, ApplicationLogonName = RandomName.Make()});
			result.Password.Should().Be.EqualTo(EncryptPassword.ToDbFormat(password));
		}

		[Test]
		public void PasswordStrengthExceptionsShouldBubbleUp()
		{
			var password = RandomName.Make();
			var passwordStrength = MockRepository.GenerateStub<ICheckPasswordStrength>();
			passwordStrength.Expect(x => x.Validate(password)).Throw(new PasswordStrengthException());
			var target = new PersonInfoMapper(MockRepository.GenerateMock<IFindTenantByNameQuery>(), passwordStrength);

			Assert.Throws<PasswordStrengthException>(() => target.Map(new PersonInfoModel{Password = password, ApplicationLogonName = RandomName.Make()}));
		}
		
		[Test]
		public void TerminalDateShouldBeSet()
		{
			var terminalDate = DateOnly.Today;
			var target = new PersonInfoMapper(MockRepository.GenerateMock<IFindTenantByNameQuery>(), new CheckPasswordStrengthSuccessful());
			var result = target.Map(new PersonInfoModel { TerminalDate = terminalDate.Date});
			result.TerminalDate.Should().Be.EqualTo(terminalDate);
		}

		[Test]
		public void NullTerminalDateShouldBeSet()
		{
			var target = new PersonInfoMapper(MockRepository.GenerateMock<IFindTenantByNameQuery>(), new CheckPasswordStrengthSuccessful());
			var result = target.Map(new PersonInfoModel { TerminalDate = null });
			result.TerminalDate.HasValue.Should().Be.False();
		}

		[Test]
		//TODO: tenant - when we impl auth for persisting stuff - use tenant from auth user instead?
		public void TenantShouldBeSet()
		{
			var tenant = new Infrastructure.MultiTenancy.Server.Tenant(RandomName.Make());
			var findTenantQuery = MockRepository.GenerateMock<IFindTenantByNameQuery>();
			findTenantQuery.Expect(x => x.Find(tenant.Name)).Return(tenant);
			var target = new PersonInfoMapper(findTenantQuery, new CheckPasswordStrengthSuccessful());
			var result = target.Map(new PersonInfoModel {Tenant = tenant.Name});
			result.Tenant.Should().Be.EqualTo(tenant.Name);
		}
	}
}