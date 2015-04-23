using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Web.Areas.Tenant;

namespace Teleopti.Ccc.WebTest.Areas.Tenant
{
	public class PersonInfoControllerTest
	{
		[Test]
		public void ShouldDeletedPersonInfos()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var deleter = MockRepository.GenerateMock<IDeletePersonInfo>();
			
			var target = new PersonInfoController(null, null, deleter);
			target.Delete(new[] {personId1, personId2});

			deleter.AssertWasCalled(x => x.Delete(personId1));
			deleter.AssertWasCalled(x => x.Delete(personId2));
		}
	}
}