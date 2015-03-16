using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
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
			var persister = MockRepository.GenerateMock<IPersistPersonInfo>();
			var mapper = MockRepository.GenerateMock<IPersonInfoMapper>();
			mapper.Expect(x => x.Map(personInfoModel1)).Return(entity1);
			mapper.Expect(x => x.Map(personInfoModel2)).Return(entity2);

			var target = new PersonInfoController(persister, mapper, null);
			target.Persist(model);

			persister.AssertWasCalled(x=>x.Persist(entity1));
			persister.AssertWasCalled(x=>x.Persist(entity2));
		}

		[Test]
		public void ShouldDeletedPersonInfos()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var model = new PersonInfoDeletes {PersonInfosToDelete = new[] {personId1, personId2}};
			var deleter = MockRepository.GenerateMock<IDeletePersonInfo>();
			
			var target = new PersonInfoController(null, null, deleter);
			target.Delete(model);

			deleter.AssertWasCalled(x => x.Delete(personId1));
			deleter.AssertWasCalled(x => x.Delete(personId2));
		}
	}
}