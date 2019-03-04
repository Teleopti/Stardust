using System.Collections.Generic;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Wfm.Administration.Core
{
	public class PurgeNoneEmployeeData : IPurgeNoneEmployeeData
	{
		private readonly ICurrentTenantSession _currentTenantSession;
		private readonly INow _now;
		private readonly ILoadAllTenants loadAllTenants;

		public PurgeNoneEmployeeData(ICurrentTenantSession currentTenantSession, INow now, ILoadAllTenants loadAllTenants)
		{
			_currentTenantSession = currentTenantSession;
			_now = now;
			this.loadAllTenants = loadAllTenants;
		}

		[TenantUnitOfWork]
		[NoTenantAuthentication]
		public virtual void Purge()
		{
			var manager = _currentTenantSession as TenantUnitOfWorkManager;

			using (manager.EnsureUnitOfWorkIsStarted())
			{
				var allTenants = loadAllTenants.Tenants();
				var allPersonIds = new List<string>();
				foreach (var tenant in allTenants)
				{
					allPersonIds.AddRange(getPersonIdsFromTenant(tenant.DataSourceConfiguration.ApplicationConnectionString));
				}

				foreach (var personBatch in allPersonIds.Batch(200))
				{
					_currentTenantSession.CurrentSession().CreateSQLQuery("DELETE FROM Tenant.PersonInfo WHERE Id in (:ids)")
						.SetParameterList("ids", personBatch).ExecuteUpdate();
				}
				
			}
		}

		private IEnumerable<string> getPersonIdsFromTenant(string applicationConnectionString)
		{
			var personIds = new List<string>();
			using (var connection = new SqlConnection(applicationConnectionString))
			{
				connection.Open();
				int monthToKeepSetting;
				using (var selectCommand = new SqlCommand(
					@"select [value] from PurgeSetting where [key] = 'MonthsToKeepPersonalData'", connection))
				{
					monthToKeepSetting =(int)( selectCommand.ExecuteScalar( ) ?? 3);
				}
				
				using (var selectCommand = new SqlCommand(
					@"select Id from Person with (NOLOCK) where TerminalDate < @date and isdeleted = 0 ", connection))
				{
					selectCommand.Parameters.AddWithValue("date", _now.UtcDateTime().AddDays(-7));

					using (var reader = selectCommand.ExecuteReader())
					{
						while (reader.Read())
						{
							personIds.Add(reader.GetGuid(0).ToString());
						}
					}
				}

			}

			return personIds;
		}
	}

	public interface IPurgeNoneEmployeeData
	{
		void Purge();
	}
}