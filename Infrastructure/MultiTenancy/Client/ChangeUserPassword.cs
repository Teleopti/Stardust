using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class ChangeUserPassword : IChangeUserPassword
	{
		private readonly ITenantServerConfiguration _tenantServerConfiguration;
		private readonly IPostHttpRequest _postHttpRequest;
		private readonly IJsonSerializer _jsonSerializer;


		public ChangeUserPassword(ITenantServerConfiguration tenantServerConfiguration, 
																IPostHttpRequest postHttpRequest,
																IJsonSerializer jsonSerializer)
		{
			_tenantServerConfiguration = tenantServerConfiguration;
			_postHttpRequest = postHttpRequest;
			_jsonSerializer = jsonSerializer;
		}
		
		public ChangeUserPasswordResult SetNewPassword(ChangePasswordInput newPasswordInput)
		{
			var json = _jsonSerializer.SerializeObject(newPasswordInput);
			var result = _postHttpRequest.Send<ChangeUserPasswordResult>(_tenantServerConfiguration.Path + "Authenticate/ChangePassword", json);

			return result;
		}
	}

	public class EmptyChangeUserPassword : IChangeUserPassword
	{
		public ChangeUserPasswordResult SetNewPassword(ChangePasswordInput newPasswordInput)
		{
			return new ChangeUserPasswordResult{Success = true};
		}
	}
}