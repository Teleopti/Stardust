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
		const string updateStatement = "UPDATE mart.dim_person SET windows_domain=@windows_domain, windows_username=@windows_username where person_code=@person_code";
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
			var windowsLogonInfosInAnalytics = _jobParameters.Helper.Repository.GetWindowsLogonInfos().ToDictionary(k => k.PersonCode);
			var logonInfos = JobParameters.TenantLogonInfoLoader.GetLogonInfoModelsForGuids(windowsLogonInfosInAnalytics.Select(x => x.Key));
			var windowsLogonInfosInApp = PersonTransformer.TransformWindowsLogonInfo(logonInfos);
			var toBeUpdated=new List<WindowsLogonInfo>();
			foreach (var windowsLogonInfoInApp in windowsLogonInfosInApp)
			{
				if (windowsLogonInfosInAnalytics.TryGetValue(windowsLogonInfoInApp.PersonCode,out var windowsLogonInfoInAnalytics) &&
					(windowsLogonInfoInAnalytics.WindowsDomain != windowsLogonInfoInApp.WindowsDomain ||
					 windowsLogonInfoInAnalytics.WindowsUsername != windowsLogonInfoInApp.WindowsUsername))
				{
					toBeUpdated.Add(windowsLogonInfoInApp);
				}
			}

			int affectedRows = 0;
			if (toBeUpdated.Any())
			{
				_executor.Run(_connection, conn =>
				{
					SqlTransaction sqlTransaction = conn.BeginTransaction();

					foreach (var windowsLogonInfo in toBeUpdated)
					{
						SqlCommand sqlCommand = conn.CreateCommand();
						sqlCommand.Transaction = sqlTransaction;

						sqlCommand.CommandType = CommandType.Text;
						sqlCommand.Parameters.AddWithValue("@windows_domain", windowsLogonInfo.WindowsDomain);
						sqlCommand.Parameters.AddWithValue("@windows_username", windowsLogonInfo.WindowsUsername);
						sqlCommand.Parameters.AddWithValue("@person_code", windowsLogonInfo.PersonCode);
						sqlCommand.CommandText = updateStatement;
						
						affectedRows += sqlCommand.ExecuteNonQuery();
					}
					
					sqlTransaction.Commit();
				});
			}

			return affectedRows;
		}
	}
}