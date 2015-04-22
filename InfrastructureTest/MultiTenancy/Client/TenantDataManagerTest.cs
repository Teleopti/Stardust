﻿using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
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
			var target = new TenantDataManager(new TenantServerConfiguration(pathToTenantServer), postHttpRequest, jsonSerializer);
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
			var target = new TenantDataManager(new TenantServerConfiguration(pathToTenantServer), postHttpRequest, jsonSerializer);
			target.SaveTenantData(authDatas);

			postHttpRequest.AssertWasCalled(x => x.Send<object>(pathToTenantServer + "PersonInfo/Persist", serializedAuthDatas));
		}

		[Test]
		public void ShouldReturnSuccessWhenSuccessfulSave()
		{
			var tenantAuthenticationData = new TenantAuthenticationData();
			var serializedAuthData = RandomName.Make();
			var pathToTenantServer = RandomName.Make();
			var postHttpRequest = MockRepository.GenerateMock<IPostHttpRequest>();
			var jsonSerializer = MockRepository.GenerateStub<IJsonSerializer>();
			jsonSerializer.Stub(x => x.SerializeObject(tenantAuthenticationData)).Return(serializedAuthData);
			var saveResult = new PersistPersonInfoResult { PasswordStrengthIsValid = true };
			var target = new TenantDataManager(new TenantServerConfiguration(pathToTenantServer), postHttpRequest, jsonSerializer);
			postHttpRequest.Stub(
				x => x.Send<PersistPersonInfoResult>(pathToTenantServer + "PersonInfo/PersistNew", serializedAuthData))
				.Return(saveResult);
			var result = target.SaveTenantData(tenantAuthenticationData);
			result.Success.Should().Be.True();
		}

		[Test]
		public void ShouldReturnSuccessFalseWhenUnsuccessfulSave()
		{
			var tenantAuthenticationData = new TenantAuthenticationData();
			var serializedAuthData = RandomName.Make();
			var pathToTenantServer = RandomName.Make();
			var postHttpRequest = MockRepository.GenerateMock<IPostHttpRequest>();
			var jsonSerializer = MockRepository.GenerateStub<IJsonSerializer>();
			jsonSerializer.Stub(x => x.SerializeObject(tenantAuthenticationData)).Return(serializedAuthData);
			var saveResult = new PersistPersonInfoResult { PasswordStrengthIsValid = false };
			var target = new TenantDataManager(new TenantServerConfiguration(pathToTenantServer), postHttpRequest, jsonSerializer);
			postHttpRequest.Stub(
				x => x.Send<PersistPersonInfoResult>(pathToTenantServer + "PersonInfo/PersistNew", serializedAuthData))
				.Return(saveResult);
			var result = target.SaveTenantData(tenantAuthenticationData);
			result.Success.Should().Be.False();
		}
	}
}