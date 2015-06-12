using System;
using System.Data.SqlClient;
using Castle.Components.DictionaryAdapter.Xml;
using NHibernate.Dialect.Function;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Core
{
	public class DatabaseHelperWrapper
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public DatabaseHelperWrapper(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public DbCheckResultModel Exists(string databaseConnectionString, DatabaseType databaseType)
		{
			var dbType = "Teleopti WFM application database";
			if (databaseType.Equals(DatabaseType.TeleoptiAnalytics))
				dbType = "Teleopti WFM analytics database";
			try
			{
				new SqlConnectionStringBuilder(databaseConnectionString);
			}
			catch (Exception e)
			{
				return new DbCheckResultModel {Exists = false, Message = string.Format("The connection string for {0} is not in the correct format!",dbType)};

			}
			
			var connection = new SqlConnection(databaseConnectionString);
			try
			{
				connection.Open();
			}
			catch (Exception e)
			{
				return new DbCheckResultModel { Exists = false, Message = string.Format("Can not connect to the {0}. " + e.Message, dbType) };
			}
			//later check so it is not used in other Tenants?
			return new DbCheckResultModel {Exists = true, Message =  string.Format("{0} exists.",dbType)};
			
		}
	}
}