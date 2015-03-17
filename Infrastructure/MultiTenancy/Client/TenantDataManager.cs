using System;
using System.Collections.Generic;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class TenantDataManager : ITenantDataManager
	{
		private readonly string _pathToTenantServer;
		private readonly IPostHttpRequest _postHttpRequest;
		private readonly IJsonSerializer _jsonSerializer;

		public TenantDataManager(string pathToTenantServer, 
															IPostHttpRequest postHttpRequest,
															IJsonSerializer jsonSerializer)
		{
			_pathToTenantServer = pathToTenantServer;
			_postHttpRequest = postHttpRequest;
			_jsonSerializer = jsonSerializer;
		}

		public void SaveTenantData(IEnumerable<TenantAuthenticationData> tenantAuthenticationData)
		{
			var json = _jsonSerializer.SerializeObject(tenantAuthenticationData);
			_postHttpRequest.Send<object>(_pathToTenantServer + "PersonInfo/Persist", json);
		}

		public void DeleteTenantPersons(IEnumerable<Guid> personsToBeDeleted)
		{
			var json = _jsonSerializer.SerializeObject(personsToBeDeleted);
			_postHttpRequest.Send<object>(_pathToTenantServer + "PersonInfo/Delete", json);
		}
	}
}