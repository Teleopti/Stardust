using System;
using System.Data.SqlClient;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Core
{
	public class CheckDatabaseVersions
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public CheckDatabaseVersions(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public VersionResultModel GetVersions(VersionCheckModel versionCheckModel)
		{
			var result = new VersionResultModel { AppVersionOk = false };
			try
			{
				var builder = new SqlConnectionStringBuilder(_currentTenantSession.CurrentSession().Connection.ConnectionString);
				using (var sqlConn = new SqlConnection(builder.ConnectionString))
				{
					sqlConn.Open();

					var versionInfo = new DatabaseVersionInformation(new DatabaseFolder(new DbManagerFolder()), sqlConn);
					result.HeadVersion = versionInfo.GetDatabaseVersion();
					sqlConn.Close();
				}
				
				builder = new SqlConnectionStringBuilder(versionCheckModel.AppConnectionString);
				using (var sqlConn = new SqlConnection(builder.ConnectionString))
				{
					sqlConn.Open();

					var versionInfo = new DatabaseVersionInformation(new DatabaseFolder(new DbManagerFolder()), sqlConn);
					result.ImportAppVersion = versionInfo.GetDatabaseVersion();
					sqlConn.Close();
				}

				result.AppVersionOk = result.HeadVersion.Equals(result.ImportAppVersion);
			}
			catch (Exception)
			{
				return result;
			}

			return result;
		}
	}
}