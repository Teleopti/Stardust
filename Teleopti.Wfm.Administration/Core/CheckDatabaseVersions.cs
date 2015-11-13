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
				var versionInfo = new DatabaseVersionInformation(new DatabaseFolder(new DbManagerFolder()), new ExecuteSql(() =>
				{
					var conn =
						new SqlConnection(
							new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString)
								.ConnectionString);
					conn.Open();
					return conn;
				}, new NullLog()));
				result.HeadVersion = versionInfo.GetDatabaseVersion();

				var importVersionInfo = new DatabaseVersionInformation(new DatabaseFolder(new DbManagerFolder()),
					new ExecuteSql(() =>
					{
						var conn = new SqlConnection(new SqlConnectionStringBuilder(appConnectionString).ConnectionString);
						conn.Open();
						return conn;
					}, new NullLog()));
				result.ImportAppVersion = importVersionInfo.GetDatabaseVersion();

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