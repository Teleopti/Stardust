using System;
using System.Configuration;
using System.Data.SqlClient;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Core
{
	public interface ICheckDatabaseVersions
	{
		VersionResultModel GetVersions(string connectionString);
	}

	public class CheckDatabaseVersions : ICheckDatabaseVersions
	{
		public VersionResultModel GetVersions(string appConnectionString)
		{
			var result = new VersionResultModel { AppVersionOk = false };
			try
			{
				var builder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);
				using (var sqlConn = new SqlConnection(builder.ConnectionString))
				{
					sqlConn.Open();

					var versionInfo = new DatabaseVersionInformation(new DatabaseFolder(new DbManagerFolder()), sqlConn);
					result.HeadVersion = versionInfo.GetDatabaseVersion();
                    sqlConn.Close();
				}

				builder = new SqlConnectionStringBuilder(appConnectionString);
				using (var sqlConn = new SqlConnection(builder.ConnectionString))
				{
					sqlConn.Open();
					var versionInfo = new DatabaseVersionInformation(new DatabaseFolder(new DbManagerFolder()), sqlConn);
					result.ImportAppVersion = versionInfo.GetDatabaseVersion();
					sqlConn.Close();
				}

				result.AppVersionOk = result.HeadVersion.Equals(result.ImportAppVersion);
			}
			catch (Exception e)
			{
				result.Error = e.Message;
				return result;
			}

			return result;
		}
	}
}