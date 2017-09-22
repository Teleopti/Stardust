using System;
using System.Collections.Generic;
using log4net;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class TenantDataManager : ITenantDataManager
	{
		private readonly ITenantServerConfiguration _tenantServerConfiguration;
		private readonly IPostHttpRequest _postHttpRequest;
		private readonly IJsonSerializer _jsonSerializer;
		private readonly ICurrentTenantCredentials _currentTenantCredentials;
		private static readonly ILog logger = LogManager.GetLogger(typeof(TenantDataManager));

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

		private RetryPolicy makeRetryPolicy()
		{
			var policy = new RetryPolicy<TenantDataManagerRetryStrategy>(new ExponentialBackoff(9, TimeSpan.FromMilliseconds(500),
					TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(2)));

			policy.Retrying += (sender, args) =>
			{
				logger.Info($"Retry - Count:{args.CurrentRetryCount}, Delay:{args.Delay}, Exception:{args.LastException}");
			};

			return policy;
		}

		public void DeleteTenantPersons(IEnumerable<Guid> personsToBeDeleted)
		{
			var json = _jsonSerializer.SerializeObject(personsToBeDeleted);

			makeRetryPolicy().ExecuteAction(() =>
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

	public class TenantDataManagerRetryStrategy : ITransientErrorDetectionStrategy
	{
		public bool IsTransient(Exception ex)
		{
			return true;
		}
	}
}