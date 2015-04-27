using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class ChangePassword : IChangePassword
	{
		private readonly ITenantServerConfiguration _tenantServerConfiguration;
		private readonly IPostHttpRequest _postHttpRequest;
		private readonly IJsonSerializer _jsonSerializer;


		public ChangePassword(ITenantServerConfiguration tenantServerConfiguration, 
																IPostHttpRequest postHttpRequest,
																IJsonSerializer jsonSerializer)
		{
			_tenantServerConfiguration = tenantServerConfiguration;
			_postHttpRequest = postHttpRequest;
			_jsonSerializer = jsonSerializer;
		}
		
		public ChangePasswordResult SetNewPassword(ChangePasswordInput newPasswordInput)
		{
			var json = _jsonSerializer.SerializeObject(newPasswordInput);
			var result = _postHttpRequest.Send<ChangePasswordResult>(_tenantServerConfiguration.Path + "Authenticate/ChangePassword", json);

			return result;
		}
	}

	public class EmptyChangePassword : IChangePassword
	{
		public ChangePasswordResult SetNewPassword(ChangePasswordInput newPasswordInput)
		{
			return new ChangePasswordResult{Success = true};
		}
	}
}