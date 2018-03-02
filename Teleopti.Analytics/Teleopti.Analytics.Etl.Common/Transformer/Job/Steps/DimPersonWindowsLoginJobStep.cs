using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Infrastructure.Util;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class DimPersonWindowsLoginJobStep : JobStepBase
	{
		private const int windowsDomainColumnLength = 100;
		private const int windowsUserNameColumnLength = 100;

		private const string updateStatement =
			"UPDATE mart.dim_person SET windows_domain=@windows_domain, windows_username=@windows_username "
			+ "where person_code=@person_code";

		private readonly Func<SqlConnection> _connection;
		private readonly CloudSafeSqlExecute _executor = new CloudSafeSqlExecute();

		public DimPersonWindowsLoginJobStep(IJobParameters jobParameters) : base(jobParameters)
		{
			_connection = () =>
			{
				var conn = new SqlConnection(jobParameters.Helper.SelectedDataSource.Analytics.ConnectionString);
				conn.Open();
				return conn;
			};
			Name = "dim_person_windows_login";
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			var windowsLogonInfosInAnalytics = _jobParameters.Helper.Repository.GetWindowsLogonInfos().ToList();

			var personIdInAnalytics = windowsLogonInfosInAnalytics.Select(x => x.PersonCode).Distinct().ToList();
			var logonInfos = JobParameters.TenantLogonInfoLoader.GetLogonInfoModelsForGuids(personIdInAnalytics);
			var windowsLogonInfosInApp = PersonTransformer.TransformWindowsLogonInfo(logonInfos);

			var toBeUpdated = new List<WindowsLogonInfo>();
			foreach (var windowsLogonInfoInApp in windowsLogonInfosInApp)
			{
				// A person may have different windows login information in dim_person (Bug #47602)
				var windowsLogonInfoInAnalytics = windowsLogonInfosInAnalytics
					.Where(x => x.PersonCode == windowsLogonInfoInApp.PersonCode).ToList();
				if (!windowsLogonInfoInAnalytics.Any())
				{
					continue;
				}

				// DomainName or WindowsUserName may longer than column length (Bug #47274)
				var truncatedDomainName = truncateString(windowsLogonInfoInApp.WindowsDomain, windowsDomainColumnLength);
				var truncatedWinUserName = truncateString(windowsLogonInfoInApp.WindowsUsername, windowsUserNameColumnLength);
				if (windowsLogonInfoInAnalytics.All(x => x.WindowsDomain == truncatedDomainName &&
														 x.WindowsUsername == truncatedWinUserName))
				{
					continue;
				}

				toBeUpdated.Add(new WindowsLogonInfo
				{
					PersonCode = windowsLogonInfoInApp.PersonCode,
					WindowsDomain = truncatedDomainName,
					WindowsUsername = truncatedWinUserName
				});
			}

			var affectedRows = 0;
			if (!toBeUpdated.Any()) return affectedRows;

			_executor.Run(_connection, conn =>
			{
				var sqlTransaction = conn.BeginTransaction();

				try
				{
					foreach (var windowsLogonInfo in toBeUpdated)
					{
						var sqlCommand = conn.CreateCommand();
						sqlCommand.Transaction = sqlTransaction;

						sqlCommand.CommandType = CommandType.Text;
						sqlCommand.Parameters.AddWithValue("@windows_domain", windowsLogonInfo.WindowsDomain);
						sqlCommand.Parameters.AddWithValue("@windows_username", windowsLogonInfo.WindowsUsername);
						sqlCommand.Parameters.AddWithValue("@person_code", windowsLogonInfo.PersonCode);
						sqlCommand.CommandText = updateStatement;

						affectedRows += sqlCommand.ExecuteNonQuery();
					}

					sqlTransaction.Commit();
				}
				catch (Exception)
				{
					sqlTransaction.Rollback();
					throw;
				}
			});

			return affectedRows;
		}

		private static string truncateString(string rawString, int lengthLimition)
		{
			return rawString.Length > lengthLimition
				? rawString.Substring(0, lengthLimition)
				: rawString;
		}
	}
}