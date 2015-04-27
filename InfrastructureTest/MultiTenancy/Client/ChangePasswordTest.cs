using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
{
	public class ChangePasswordTest
	{
		
		[Test]
		public void ChangeIt()
		{
			var pathToTenantServer = RandomName.Make();
			var postHttpRequest = MockRepository.GenerateStub<IPostHttpRequest>();
			var jsonSerializer = MockRepository.GenerateStub<IJsonSerializer>();
			var changePasswordInput = new ChangePasswordInput();
			var logonClientModelSerialized = RandomName.Make();
			jsonSerializer.Stub(x => x.SerializeObject(changePasswordInput)).Return(logonClientModelSerialized);
			var result = new ChangePasswordResult { Success = true};
			postHttpRequest.Stub(
				x => x.Send<ChangePasswordResult>(pathToTenantServer + "Authenticate/ChangePassword", logonClientModelSerialized))
					.Return(result);
			
			var target = new ChangePassword(new TenantServerConfiguration(pathToTenantServer),  postHttpRequest, jsonSerializer);
			target.SetNewPassword(changePasswordInput)
				.Should().Be.SameInstanceAs(result);
		}
	}
}