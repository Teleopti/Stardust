using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Web.Areas.Tenant;
using Teleopti.Ccc.Web.Areas.Tenant.Core;
using Teleopti.Ccc.Web.Areas.Tenant.Model;

namespace Teleopti.Ccc.WebTest.Areas.Tenant
{
	public class PersonInfoControllerTest
	{
		[Test]
		public void ShouldPersistPersonInfo()
		{
			var dto = new PersonInfoModel();
			var entity = new PersonInfo();
			var persister = MockRepository.GenerateMock<IPersonInfoPersister>();
			var mapper = MockRepository.GenerateMock<IPersonInfoMapper>();
			mapper.Expect(x => x.Map(dto)).Return(entity);

			var target = new PersonInfoController(persister, mapper);
			target.Persist(new List<PersonInfoModel>{dto});

			persister.AssertWasCalled(x=>x.Persist(entity));
		}
	}
}