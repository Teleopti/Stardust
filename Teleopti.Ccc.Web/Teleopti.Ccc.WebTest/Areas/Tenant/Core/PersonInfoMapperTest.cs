using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.Tenant.Core;
using Teleopti.Ccc.Web.Areas.Tenant.Model;

namespace Teleopti.Ccc.WebTest.Areas.Tenant.Core
{
	public class PersonInfoMapperTest
	{
		[Test]
		public void PersonIdShouldBeSet()
		{
			var id = Guid.NewGuid();
			var target = new PersonInfoMapper();
			var result = target.Map(new PersonInfoModel { PersonId = id });
			result.Id.Should().Be.EqualTo(id);
		}

		[Test]
		public void NullPersonIdShouldBeSetToDefaultValue()
		{
			var target = new PersonInfoMapper();
			var result = target.Map(new PersonInfoModel { PersonId = null });
			result.Id.Should().Be.EqualTo(Guid.Empty);
		}

		[Test]
		public void IdentityShouldBeSet()
		{
			var identity = RandomName.Make();
			var target = new PersonInfoMapper();
			var result = target.Map(new PersonInfoModel { Identity = identity });
			result.Identity.Should().Be.EqualTo(identity);
		}

		[Test]
		public void ApplicationLogonShouldBeSet()
		{
			var applicationLogon = RandomName.Make();
			var target = new PersonInfoMapper();
			var result = target.Map(new PersonInfoModel { UserName = applicationLogon });
			result.ApplicationLogonName.Should().Be.EqualTo(applicationLogon);
		}
	}

}