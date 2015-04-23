using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Web.Areas.Tenant;

namespace Teleopti.Ccc.WebTest.Areas.Tenant
{
	[TenantTest]
	public class PersonInfoDeleteTest
	{
		public PersonInfoController Target;
		public DeletePersonInfoFake DeletePersonInfo;

		[Test]
		public void ShouldDeletedPersonInfos()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();

			Target.Delete(new[] { personId1, personId2 });

			DeletePersonInfo.WasDeleted.Should().Have.SameValuesAs(personId1, personId2);
		}
	}
}