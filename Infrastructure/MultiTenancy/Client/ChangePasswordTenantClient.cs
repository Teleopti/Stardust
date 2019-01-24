using System.Net;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class ChangePasswordTenantClient : IChangePasswordTenantClient
	{
		private readonly ITenantServerConfiguration _tenantServerConfiguration;
		private readonly IPostHttpRequest _postHttpRequest;
		private readonly IJsonSerializer _jsonSerializer;
		private readonly IResponseException _responseException;


		public ChangePasswordTenantClient(ITenantServerConfiguration tenantServerConfiguration, 
																IPostHttpRequest postHttpRequest,
																IJsonSerializer jsonSerializer,
																IResponseException responseException)
		{
			_tenantServerConfiguration = tenantServerConfiguration;
			_postHttpRequest = postHttpRequest;
			_jsonSerializer = jsonSerializer;
			_responseException = responseException;
		}
		
		public ChangePasswordResult SetNewPassword(ChangePasswordInput newPasswordInput)
		{
			var json = _jsonSerializer.SerializeObject(newPasswordInput);
			try
			{
				_postHttpRequest.Send<object>(_tenantServerConfiguration.FullPath("ChangePassword/Modify"), json);
				return new ChangePasswordResult { Success = true };
			}
			catch (WebException wEx)
			{
				var statusCode =_responseException.ExceptionStatus(wEx);
				if (statusCode != null)
				{
					if (statusCode == HttpStatusCode.Forbidden || statusCode == HttpStatusCode.BadRequest)
					{
						return new ChangePasswordResult {Success = false};
					}
				}
				throw;
			}
		}
	}
}