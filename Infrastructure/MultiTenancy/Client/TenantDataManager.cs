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

		public TenantDataManager(ITenantServerConfiguration tenantServerConfiguration, 
															IPostHttpRequest postHttpRequest,
															IJsonSerializer jsonSerializer)
		{
			_tenantServerConfiguration = tenantServerConfiguration;
			_postHttpRequest = postHttpRequest;
			_jsonSerializer = jsonSerializer;
		}

		public void SaveTenantData(IEnumerable<TenantAuthenticationData> tenantAuthenticationData)
		{
			var json = _jsonSerializer.SerializeObject(tenantAuthenticationData);
			_postHttpRequest.Send<object>(_tenantServerConfiguration.Path + "PersonInfo/Persist", json);
		}

		public void DeleteTenantPersons(IEnumerable<Guid> personsToBeDeleted)
		{
			var json = _jsonSerializer.SerializeObject(personsToBeDeleted);
			_postHttpRequest.Send<object>(_tenantServerConfiguration.Path + "PersonInfo/Delete", json);
		}

		public SavePersonInfoResult SaveTenantData(TenantAuthenticationData tenantAuthenticationData)
		{
			var json = _jsonSerializer.SerializeObject(tenantAuthenticationData);
			var tmpResult = _postHttpRequest.Send<PersistPersonInfoResult>(_tenantServerConfiguration.Path + "PersonInfo/PersistNew", json);
			var ret = new SavePersonInfoResult {Success = true};
			if (!tmpResult.PasswordStrengthIsValid)
			{
				ret.UserName = tenantAuthenticationData.ApplicationLogonName;
				ret.Success = false;
				ret.FailReason = UserTexts.Resources.PasswordPolicyWarning;
			}

			return ret;
		}
	}
}