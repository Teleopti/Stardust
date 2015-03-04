using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public interface ITenantDataManager
	{
		void SaveTenantData(IList<TenantAuthenticationData> tenantAuthenticationData);
		void DeleteTenantPersons(IList<Guid> personsToBeDeleted);
	}

	public class TenantDataManager : ITenantDataManager
	{
		private readonly string _pathToTenantServer;

		public TenantDataManager(string pathToTenantServer)
		{
			_pathToTenantServer = pathToTenantServer;
		}

		public void SaveTenantData(IList<TenantAuthenticationData> tenantAuthenticationData)
		{
			var uriBuilder = new UriBuilder(_pathToTenantServer + "Tenant/Save");
		}

		public void DeleteTenantPersons(IList<Guid> personsToBeDeleted)
		{
			var uriBuilder = new UriBuilder(_pathToTenantServer + "Tenant/Delete");
		}
	}

	//todo: tenant, used when toggle is of, remove this when toggle is removed
	public class EmptyTenantDataManager : ITenantDataManager
	{
		public void SaveTenantData(IList<TenantAuthenticationData> tenantAuthenticationData)
		{
			
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