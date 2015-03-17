using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
{
	public class TenantDataManagerTest
	{
		[Test]
		public void ShouldDeleteTenantData()
		{
			var personIds = new[] {Guid.NewGuid()};
			var serializedPersonIds = RandomName.Make();
			var pathToTenantServer = RandomName.Make();
			var postHttpRequest = MockRepository.GenerateMock<IPostHttpRequest>();
			var jsonSerializer = MockRepository.GenerateStub<IJsonSerializer>();
			jsonSerializer.Stub(x => x.SerializeObject(personIds)).Return(serializedPersonIds);
			var target = new TenantDataManager(pathToTenantServer, postHttpRequest, jsonSerializer);
			target.DeleteTenantPersons(personIds);

			postHttpRequest.AssertWasCalled(x => x.Send<object>(pathToTenantServer + "PersonInfo/Delete", serializedPersonIds));
		}

		[Test]
		public void ShouldSaveTenantData()
		{
			var authDatas = Enumerable.Empty<TenantAuthenticationData>();
			var serializedAuthDatas = RandomName.Make();
			var pathToTenantServer = RandomName.Make();
			var postHttpRequest = MockRepository.GenerateMock<IPostHttpRequest>();
			var jsonSerializer = MockRepository.GenerateStub<IJsonSerializer>();
			jsonSerializer.Stub(x => x.SerializeObject(authDatas)).Return(serializedAuthDatas);
			var target = new TenantDataManager(pathToTenantServer, postHttpRequest, jsonSerializer);
			target.SaveTenantData(authDatas);

			postHttpRequest.AssertWasCalled(x => x.Send<object>(pathToTenantServer + "PersonInfo/Persist", serializedAuthDatas));
		}
	}
}