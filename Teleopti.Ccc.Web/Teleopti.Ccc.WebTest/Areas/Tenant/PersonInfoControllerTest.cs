using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
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
			var entity1 = new PersonInfo();
			var entity2 = new PersonInfo();
			var persister = MockRepository.GenerateMock<IPersistPersonInfo>();
			var mapper = MockRepository.GenerateMock<IPersonInfoMapper>();
			mapper.Expect(x => x.Map(personInfoModel1)).Return(entity1);
			mapper.Expect(x => x.Map(personInfoModel2)).Return(entity2);

			var target = new PersonInfoController(persister, mapper, null);
			target.Persist(new[]{personInfoModel1, personInfoModel2});

			persister.AssertWasCalled(x=>x.Persist(entity1));
			persister.AssertWasCalled(x=>x.Persist(entity2));
		}

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

		[Test]
		public void ShouldNotReturnErrors()
		{
			var personInfoModel = new PersonInfoModel();
			var personInfo = new PersonInfo();
			var persister = MockRepository.GenerateMock<IPersistPersonInfo>();
			var mapper = MockRepository.GenerateMock<IPersonInfoMapper>();
			mapper.Expect(x => x.Map(personInfoModel)).Return(personInfo);

			var target = new PersonInfoController(persister, mapper, null);
			var result = (PersistPersonInfoResult)target.PersistNew(personInfoModel).Data;
			allPropertiesShouldBeTrue(result);
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
			var personInfoModel = new PersonInfoModel();
			var persister = MockRepository.GenerateMock<IPersistPersonInfo>();
			var mapper = MockRepository.GenerateMock<IPersonInfoMapper>();
			mapper.Stub(x => x.Map(personInfoModel)).Throw(new PasswordStrengthException());

			var target = new PersonInfoController(persister, mapper, null);
			var result = (PersistPersonInfoResult)target.PersistNew(personInfoModel).Data;
			result.PasswordStrengthIsValid.Should().Be.False();
		}

		[Test]
		public void ShouldHandleDuplicateApplicationLogonNameException()
		{
			var personInfoModel = new PersonInfoModel();
			var personInfo = new PersonInfo();
			var persister = MockRepository.GenerateMock<IPersistPersonInfo>();
			var mapper = MockRepository.GenerateMock<IPersonInfoMapper>();
			mapper.Stub(x => x.Map(personInfoModel)).Return(personInfo);
			persister.Stub(x => x.Persist(personInfo)).Throw(new DuplicateApplicationLogonNameException());

			var target = new PersonInfoController(persister, mapper, null);
			var result = (PersistPersonInfoResult)target.PersistNew(personInfoModel).Data;
			result.ApplicationLogonNameIsValid.Should().Be.False();
		}

		[Test]
		public void ShouldHandleDuplicateIdentityException()
		{
			var personInfoModel = new PersonInfoModel();
			var personInfo = new PersonInfo();
			var persister = MockRepository.GenerateMock<IPersistPersonInfo>();
			var mapper = MockRepository.GenerateMock<IPersonInfoMapper>();
			mapper.Stub(x => x.Map(personInfoModel)).Return(personInfo);
			persister.Stub(x => x.Persist(personInfo)).Throw(new DuplicateIdentityException());

			var target = new PersonInfoController(persister, mapper, null);
			var result = (PersistPersonInfoResult)target.PersistNew(personInfoModel).Data;
			result.IdentityIsValid.Should().Be.False();
		}
	}
}