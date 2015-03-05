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
		public void ShouldPersistPersonInfos()
		{
			var personInfoModel1 = new PersonInfoModel();
			var personInfoModel2 = new PersonInfoModel();
			var model = new PersonInfoModels{PersonInfos = new[]{personInfoModel1, personInfoModel2}};
			var entity1 = new PersonInfo();
			var entity2 = new PersonInfo();
			var persister = MockRepository.GenerateMock<IPersonInfoPersister>();
			var mapper = MockRepository.GenerateMock<IPersonInfoMapper>();
			mapper.Expect(x => x.Map(personInfoModel1)).Return(entity1);
			mapper.Expect(x => x.Map(personInfoModel2)).Return(entity2);

			var target = new PersonInfoController(persister, mapper);
			target.Persist(model);

			persister.AssertWasCalled(x=>x.Persist(entity1));
			persister.AssertWasCalled(x=>x.Persist(entity2));
		}
	}
}