using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class TenantDataManager : ITenantDataManager
	{
		private readonly ITenantServerConfiguration _tenantServerConfiguration;
		private readonly IPostHttpRequest _postHttpRequest;
		private readonly IJsonSerializer _jsonSerializer;
		private readonly ICurrentTenantCredentials _currentTenantCredentials;

		public TenantDataManager(ITenantServerConfiguration tenantServerConfiguration, 
															IPostHttpRequest postHttpRequest,
															IJsonSerializer jsonSerializer,
															ICurrentTenantCredentials currentTenantCredentials)
		{
			_tenantServerConfiguration = tenantServerConfiguration;
			_postHttpRequest = postHttpRequest;
			_jsonSerializer = jsonSerializer;
			_currentTenantCredentials = currentTenantCredentials;
		}

		public void DeleteTenantPersons(IEnumerable<Guid> personsToBeDeleted)
		{
			var json = _jsonSerializer.SerializeObject(personsToBeDeleted);
			_postHttpRequest.SendSecured<object>(_tenantServerConfiguration.FullPath("PersonInfo/Delete"), json, _currentTenantCredentials.TenantCredentials());
		}

		public SavePersonInfoResult SaveTenantData(TenantAuthenticationData tenantAuthenticationData)
		{
			var json = _jsonSerializer.SerializeObject(tenantAuthenticationData);
			var tmpResult = _postHttpRequest.SendSecured<PersistPersonInfoResult>(_tenantServerConfiguration.FullPath("PersonInfo/Persist"), json, _currentTenantCredentials.TenantCredentials());
			var ret = new SavePersonInfoResult {Success = true};
			
			if (!tmpResult.PasswordStrengthIsValid)
			{
				ret.Success = false;
				ret.FailReason = UserTexts.Resources.PasswordPolicyWarning;
			}
			if (!tmpResult.ApplicationLogonNameIsValid)
			{
				ret.Success = false;
				ret.FailReason += string.Format(UserTexts.Resources.ApplicationLogonExists,tenantAuthenticationData.ApplicationLogonName);
			}
			if (!tmpResult.IdentityIsValid)
			{
				ret.Success = false;
				ret.FailReason += string.Format(UserTexts.Resources.IdentityLogonExists, tenantAuthenticationData.Identity);
			}

			return ret;
		}
	}
}