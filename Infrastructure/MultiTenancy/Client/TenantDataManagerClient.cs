using System;
using System.Collections.Generic;
using log4net;
using Polly;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class TenantDataManagerClient : ITenantDataManagerClient
	{
		private readonly ITenantServerConfiguration _tenantServerConfiguration;
		private readonly IPostHttpRequest _postHttpRequest;
		private readonly IJsonSerializer _jsonSerializer;
		private readonly ICurrentTenantCredentials _currentTenantCredentials;
		private static readonly ILog logger = LogManager.GetLogger(typeof(TenantDataManagerClient));

		public TenantDataManagerClient(ITenantServerConfiguration tenantServerConfiguration,
															IPostHttpRequest postHttpRequest,
															IJsonSerializer jsonSerializer,
															ICurrentTenantCredentials currentTenantCredentials)
		{
			_tenantServerConfiguration = tenantServerConfiguration;
			_postHttpRequest = postHttpRequest;
			_jsonSerializer = jsonSerializer;
			_currentTenantCredentials = currentTenantCredentials;
		}

		private Policy makeRetryPolicy()
		{
			var policy = Policy.Handle<Exception>()
				.WaitAndRetry(9, i => TimeSpan.FromSeconds(Math.Min(60d, Math.Pow(2,i))), (exception, time, retry, ctx) => logger.Info($"Retry - Count:{retry}, Delay:{time}, Exception:{exception}"));
			
			return policy;
		}

		public void DeleteTenantPersons(IEnumerable<Guid> personsToBeDeleted)
		{
			var json = _jsonSerializer.SerializeObject(personsToBeDeleted);

			makeRetryPolicy().Execute(() =>
			{
				_postHttpRequest.SendSecured<object>(_tenantServerConfiguration.FullPath("PersonInfo/Delete"), json, _currentTenantCredentials.TenantCredentials());
			});
		}

		public SavePersonInfoResult SaveTenantData(TenantAuthenticationData tenantAuthenticationData)
		{
			var json = _jsonSerializer.SerializeObject(tenantAuthenticationData);
			var tmpResult = _postHttpRequest.SendSecured<PersistPersonInfoResult>(_tenantServerConfiguration.FullPath("PersonInfo/Persist"), json, _currentTenantCredentials.TenantCredentials());
			var ret = new SavePersonInfoResult { Success = true };

			if (!tmpResult.PasswordStrengthIsValid)
			{
				ret.Success = false;
				ret.FailReason = UserTexts.Resources.PasswordPolicyWarning;
			}
			if (!tmpResult.ApplicationLogonNameIsValid)
			{
				ret.Success = false;
				ret.FailReason += string.Format(UserTexts.Resources.ApplicationLogonExists, tenantAuthenticationData.ApplicationLogonName);
				ret.ExistingPerson = tmpResult.ExistingPerson;
			}
			if (!tmpResult.IdentityIsValid)
			{
				ret.Success = false;
				ret.FailReason += string.Format(UserTexts.Resources.IdentityLogonExists, tenantAuthenticationData.Identity);
				ret.ExistingPerson = tmpResult.ExistingPerson;
			}

			return ret;
		}
	}
}