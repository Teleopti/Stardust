using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public interface ITenantDataManager
	{
		Task<bool> SaveTenantData(IList<TenantAuthenticationData> tenantAuthenticationData);
		void DeleteTenantPersons(IList<Guid> personsToBeDeleted);
	}


	public class TenantDataManager : ITenantDataManager
	{
		private readonly string _pathToTenantServer;

		public TenantDataManager(string pathToTenantServer)
		{
			_pathToTenantServer = pathToTenantServer;
		}

		public async Task<bool> SaveTenantData(IList<TenantAuthenticationData> tenantAuthenticationData)
		{
			var client = new HttpClient();
			string json = JsonConvert.SerializeObject(tenantAuthenticationData);
			var response = await client.PostAsync(_pathToTenantServer + "PersonInfo/Persist", new StringContent(json, Encoding.UTF8, "application/json"));
			if (!response.IsSuccessStatusCode)
				return false;

			return true;
		}

		public void DeleteTenantPersons(IList<Guid> personsToBeDeleted)
		{
			var uriBuilder = new UriBuilder(_pathToTenantServer + "PersonInfo/Delete");
		}

		//maybe something like this later when we authorize against method
		//private static string authorizeHeader(string nhibDataSourcename)
		//{
		//	var authKey = "!#¤atAbgT%";
		//	var authText = string.Format("{0}:{1}", nhibDataSourcename, authKey);
		//	return "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(authText));
		//}
	}

	//todo: tenant, used when toggle is of, remove this when toggle is removed
	public class EmptyTenantDataManager : ITenantDataManager
	{
		readonly TaskCompletionSource<bool> _fakeThing = new TaskCompletionSource<bool>();
		
		public async Task<bool> SaveTenantData(IList<TenantAuthenticationData> tenantAuthenticationData)
		{
			_fakeThing.SetResult(true);
			await _fakeThing.Task;
			return _fakeThing.Task.Result;
		}

		public void DeleteTenantPersons(IList<Guid> personsToBeDeleted)
		{

		}
	}

	public class TenantAuthenticationData
	{
		public string Tenant { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public string Identity { get; set; }
		public DateOnly? TerminalDate { get; set; }
		public bool Changed { get; set; }
		public Guid? PersonId { get; set; }
	}
}