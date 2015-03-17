using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public interface ITenantDataManager
	{
		Task<bool> SaveTenantData(IEnumerable<TenantAuthenticationData> tenantAuthenticationData);
		void DeleteTenantPersons(IEnumerable<Guid> personsToBeDeleted);
	}


	public class TenantDataManager : ITenantDataManager
	{
		private readonly string _pathToTenantServer;

		public TenantDataManager(string pathToTenantServer)
		{
			_pathToTenantServer = pathToTenantServer;
		}

		public async Task<bool> SaveTenantData(IEnumerable<TenantAuthenticationData> tenantAuthenticationData)
		{
			var client = new HttpClient();

			// * Would be good if we use same way doint the call here and in authenticationquerier. reuse same interface first and then switch to "HttpClient" in its impl
			string json = JsonConvert.SerializeObject(tenantAuthenticationData);
			//
			var response = await client.PostAsync(_pathToTenantServer + "PersonInfo/Persist", new StringContent(json, Encoding.UTF8, "application/json"));
			if (!response.IsSuccessStatusCode)
				return false;

			return true;
		}

		public void DeleteTenantPersons(IEnumerable<Guid> personsToBeDeleted)
		{
			var client = new HttpClient();

			// * Would be good if we use same way doint the call here and in authenticationquerier. reuse same interface first and then switch to "HttpClient" in its impl
			var json = JsonConvert.SerializeObject(personsToBeDeleted);
			//
			client.PostAsync(_pathToTenantServer + "PersonInfo/Delete", new StringContent(json, Encoding.UTF8, "application/json"));
		}
	}

	//todo: tenant, used when toggle is of, remove this when toggle is removed
	public class EmptyTenantDataManager : ITenantDataManager
	{
		readonly TaskCompletionSource<bool> _fakeThing = new TaskCompletionSource<bool>();

		public async Task<bool> SaveTenantData(IEnumerable<TenantAuthenticationData> tenantAuthenticationData)
		{
			_fakeThing.SetResult(true);
			await _fakeThing.Task;
			return _fakeThing.Task.Result;
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
		public bool Changed { get; set; }
		public Guid? PersonId { get; set; }
	}
}