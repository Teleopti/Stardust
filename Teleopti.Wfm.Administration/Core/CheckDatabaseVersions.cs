using System;
using System.Configuration;
using System.Data.SqlClient;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Support.Library.Folders;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Core
{
	public interface ICheckDatabaseVersions
	{
		VersionResultModel GetVersions(string connectionString);
	}

	public class CheckDatabaseVersions : ICheckDatabaseVersions
	{
		private readonly IConfigReader _config;

		public CheckDatabaseVersions(IConfigReader config)
		{
			_config = config;
		}
		
		public VersionResultModel GetVersions(string appConnectionString)
		{
			var result = new VersionResultModel { AppVersionOk = false };
			try
			{
				var versionInfo = new DatabaseVersionInformation(new DatabaseFolder(new DbManagerFolder()), new ExecuteSql(() =>
				{
					var conn =
						new SqlConnection(
							new SqlConnectionStringBuilder(_config.ConnectionString("Tenancy"))
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