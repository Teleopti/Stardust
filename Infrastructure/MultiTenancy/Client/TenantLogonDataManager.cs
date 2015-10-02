using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class TenantLogonDataManager : ITenantLogonDataManager
	{
		private readonly ITenantServerConfiguration _tenantServerConfiguration;
		private readonly IPostHttpRequest _postHttpRequest;
		private readonly IJsonSerializer _jsonSerializer;
		private readonly ICurrentTenantCredentials _currentTenantCredentials;

		public TenantLogonDataManager(ITenantServerConfiguration tenantServerConfiguration, 
			IPostHttpRequest postHttpRequest,
			IJsonSerializer jsonSerializer,
			ICurrentTenantCredentials currentTenantCredentials)
		{
			_tenantServerConfiguration = tenantServerConfiguration;
			_postHttpRequest = postHttpRequest;
			_jsonSerializer = jsonSerializer;
			_currentTenantCredentials = currentTenantCredentials;
		}

		public IEnumerable<LogonInfoModel> GetLogonInfoModelsForGuids(IEnumerable<Guid> personGuids)
		{
			var ret = new List<LogonInfoModel>();
			var tenantCredentials = _currentTenantCredentials.TenantCredentials();
			foreach (var json in personGuids.Batch(200).Select(batch => _jsonSerializer.SerializeObject(batch)))
			{
				ret.AddRange(_postHttpRequest.SendSecured<IEnumerable<LogonInfoModel>>(_tenantServerConfiguration.FullPath("PersonInfo/LogonInfoFromGuids"), json, tenantCredentials));
			}
			return ret;
		}

		public LogonInfoModel GetLogonInfoForLogonName(string logonName)
		{
			var tenantCredentials = _currentTenantCredentials.TenantCredentials();
			return _postHttpRequest.SendSecured<LogonInfoModel>(
				_tenantServerConfiguration.FullPath("PersonInfo/LogonInfoFromLogonName"), _jsonSerializer.SerializeObject(new {logonName}),
				tenantCredentials);
		}
	}
}