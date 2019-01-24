using System;
using System.Net;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
{
	public class ChangePasswordTest
	{
		[Test]
		public void HappyPath()
		{
			var pathToTenantServer = RandomName.Make() + "/";
			var postHttpRequest = MockRepository.GenerateStub<IPostHttpRequest>();
			var jsonSerializer = MockRepository.GenerateStub<IJsonSerializer>();
			var changePasswordInput = new ChangePasswordInput();
			var logonClientModelSerialized = RandomName.Make();
			jsonSerializer.Stub(x => x.SerializeObject(changePasswordInput)).Return(logonClientModelSerialized);
			var result = new ChangePasswordResult { Success = true};
			postHttpRequest.Stub(
				x => x.Send<object>(pathToTenantServer + "ChangePassword/Modify", logonClientModelSerialized))
					.Return(result);
			
			var target = new ChangePasswordTenantClient(new TenantServerConfiguration(pathToTenantServer),  postHttpRequest, jsonSerializer, new ResponseException());
			target.SetNewPassword(changePasswordInput).Success.Should().Be.True();
		}

		[Test]
		public void ShouldCatch400()
		{
			var ex = new WebException();
			var responseException = MockRepository.GenerateStub<IResponseException>();
			responseException.Stub(x => x.ExceptionStatus(ex)).Return(HttpStatusCode.BadRequest);
			var pathToTenantServer = RandomName.Make() + "/";
			var postHttpRequest = MockRepository.GenerateStub<IPostHttpRequest>();
			var jsonSerializer = MockRepository.GenerateStub<IJsonSerializer>();
			var changePasswordInput = new ChangePasswordInput();
			var logonClientModelSerialized = RandomName.Make();
			jsonSerializer.Stub(x => x.SerializeObject(changePasswordInput)).Return(logonClientModelSerialized);
			postHttpRequest.Stub(x => x.Send<object>(pathToTenantServer + "ChangePassword/Modify", logonClientModelSerialized)).Throw(ex);

			var target = new ChangePasswordTenantClient(new TenantServerConfiguration(pathToTenantServer), postHttpRequest, jsonSerializer, responseException);
			target.SetNewPassword(changePasswordInput).Success.Should().Be.False();
		}

		[Test]
		public void ShouldCatch403()
		{
			var ex = new WebException();
			var responseException = MockRepository.GenerateStub<IResponseException>();
			responseException.Stub(x => x.ExceptionStatus(ex)).Return(HttpStatusCode.Forbidden);
			var pathToTenantServer = RandomName.Make() + "/";
			var postHttpRequest = MockRepository.GenerateStub<IPostHttpRequest>();
			var jsonSerializer = MockRepository.GenerateStub<IJsonSerializer>();
			var changePasswordInput = new ChangePasswordInput();
			var logonClientModelSerialized = RandomName.Make();
			jsonSerializer.Stub(x => x.SerializeObject(changePasswordInput)).Return(logonClientModelSerialized);
			postHttpRequest.Stub(x => x.Send<object>(pathToTenantServer + "ChangePassword/Modify", logonClientModelSerialized)).Throw(ex);

			var target = new ChangePasswordTenantClient(new TenantServerConfiguration(pathToTenantServer), postHttpRequest, jsonSerializer, responseException);
			target.SetNewPassword(changePasswordInput).Success.Should().Be.False();
		}

		[Test]
		public void ShouldNotCatchOtherStatusCodes()
		{
			var ex = new WebException();
			var responseException = MockRepository.GenerateStub<IResponseException>();
			responseException.Stub(x => x.ExceptionStatus(ex)).Return(HttpStatusCode.Conflict);
			var pathToTenantServer = RandomName.Make() + "/";
			var postHttpRequest = MockRepository.GenerateStub<IPostHttpRequest>();
			var jsonSerializer = MockRepository.GenerateStub<IJsonSerializer>();
			var changePasswordInput = new ChangePasswordInput();
			var logonClientModelSerialized = RandomName.Make();
			jsonSerializer.Stub(x => x.SerializeObject(changePasswordInput)).Return(logonClientModelSerialized);
			postHttpRequest.Stub(x => x.Send<object>(pathToTenantServer + "ChangePassword/Modify", logonClientModelSerialized)).Throw(ex);

			var target = new ChangePasswordTenantClient(new TenantServerConfiguration(pathToTenantServer), postHttpRequest, jsonSerializer, responseException);
			Assert.Throws<WebException>(() => target.SetNewPassword(changePasswordInput));
		}

		[Test]
		public void ShouldNotCatchGeneralExceptions()
		{
			var pathToTenantServer = RandomName.Make() + "/";
			var postHttpRequest = MockRepository.GenerateStub<IPostHttpRequest>();
			var jsonSerializer = MockRepository.GenerateStub<IJsonSerializer>();
			var changePasswordInput = new ChangePasswordInput();
			var logonClientModelSerialized = RandomName.Make();
			jsonSerializer.Stub(x => x.SerializeObject(changePasswordInput)).Return(logonClientModelSerialized);
			postHttpRequest.Stub(x => x.Send<object>(pathToTenantServer + "ChangePassword/Modify", logonClientModelSerialized)).Throw(new NotImplementedException());

			var target = new ChangePasswordTenantClient(new TenantServerConfiguration(pathToTenantServer), postHttpRequest, jsonSerializer, new ResponseException());
			Assert.Throws<NotImplementedException>(() => target.SetNewPassword(changePasswordInput));
		}
	}
}