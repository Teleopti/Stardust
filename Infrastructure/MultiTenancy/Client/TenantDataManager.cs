using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public interface ITenantDataManager
	{
		void SaveTenantData(IEnumerable<TenantAuthenticationData> tenantAuthenticationData);
		void DeleteTenantPersons(IEnumerable<Guid> personsToBeDeleted);
	}


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
			_postHttpRequest.Send<object>(_pathToTenantServer + "PersonInfo/Persist", null, json);
		}

		public void DeleteTenantPersons(IEnumerable<Guid> personsToBeDeleted)
		{
			var json = _jsonSerializer.SerializeObject(personsToBeDeleted);
			_postHttpRequest.Send<object>(_pathToTenantServer + "PersonInfo/Persist", null, json);
		}
	}

	public class EmptyTenantDataManager : ITenantDataManager
	{
		public void SaveTenantData(IEnumerable<TenantAuthenticationData> tenantAuthenticationData)
		{
		}

		public void DeleteTenantPersons(IEnumerable<Guid> personsToBeDeleted)
		{
		}
	}

	public class TenantAuthenticationData
	{
		public string Tenant { get; set; }
		public string ApplicationLogonName { get; set; }
		public string Password { get; set; }
		public string Identity { get; set; }
		public DateTime? TerminalDate { get; set; }
		//TODO: tenant - don't serialize this one
		public bool Changed { get; set; }
		public Guid? PersonId { get; set; }
	}
}