using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class TenantLogonDataManagerClient : ITenantLogonDataManagerClient
	{
		private readonly ITenantServerConfiguration _tenantServerConfiguration;
		private readonly IPostHttpRequest _postHttpRequest;
		private readonly IGetHttpRequest _getHttpRequest;
		private readonly IJsonSerializer _jsonSerializer;
		private readonly ICurrentTenantCredentials _currentTenantCredentials;

		public TenantLogonDataManagerClient(ITenantServerConfiguration tenantServerConfiguration, 
			IPostHttpRequest postHttpRequest,
			IGetHttpRequest getHttpRequest,
			IJsonSerializer jsonSerializer,
			ICurrentTenantCredentials currentTenantCredentials)
		{
			_tenantServerConfiguration = tenantServerConfiguration;
			_postHttpRequest = postHttpRequest;
			_getHttpRequest = getHttpRequest;
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
			return _getHttpRequest.GetSecured<LogonInfoModel>(
				_tenantServerConfiguration.FullPath("PersonInfo/LogonFromName"), new NameValueCollection{{"logonName",logonName}}, 
				tenantCredentials);
		}

		public LogonInfoModel GetLogonInfoForIdentity(string identity)
		{
			var tenantCredentials = _currentTenantCredentials.TenantCredentials();
			return _getHttpRequest.GetSecured<LogonInfoModel>(
				_tenantServerConfiguration.FullPath("PersonInfo/LogonFromIdentity"), new NameValueCollection{{"identity", identity }},
				tenantCredentials);
		}

		public IEnumerable<LogonInfoModel> GetLogonInfoForIdentities(IEnumerable<string> identities)
		{
			var ret = new List<LogonInfoModel>();
			var tenantCredentials = _currentTenantCredentials.TenantCredentials();
			foreach (var json in identities.Batch(200).Select(batch => _jsonSerializer.SerializeObject(batch)))
			{
				ret.AddRange(_postHttpRequest.SendSecured<IEnumerable<LogonInfoModel>>(_tenantServerConfiguration.FullPath("PersonInfo/LogonInfosFromIdentities"), json, tenantCredentials));
			}
			return ret;
		}
	}
}