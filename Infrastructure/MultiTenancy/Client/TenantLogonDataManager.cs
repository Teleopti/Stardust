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

		public TenantLogonDataManager(ITenantServerConfiguration tenantServerConfiguration, 
			IPostHttpRequest postHttpRequest,
			IJsonSerializer jsonSerializer)
		{
			_tenantServerConfiguration = tenantServerConfiguration;
			_postHttpRequest = postHttpRequest;
			_jsonSerializer = jsonSerializer;
		}

		public IEnumerable<LogonInfoModel> GetLogonInfoModelsForGuids(IEnumerable<Guid> personGuids)
		{
			var ret = new List<LogonInfoModel>();
			foreach (var json in personGuids.Batch(200).Select(batch => _jsonSerializer.SerializeObject(batch)))
			{
				ret.AddRange(_postHttpRequest.Send<IEnumerable<LogonInfoModel>>(_tenantServerConfiguration.FullPath("PersonInfo/LogonInfoFromGuids"), json));
			}
			return ret;
		}
	}
}