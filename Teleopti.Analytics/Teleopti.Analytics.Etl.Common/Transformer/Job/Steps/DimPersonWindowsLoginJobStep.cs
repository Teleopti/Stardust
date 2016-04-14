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
		const string updateStatement = "UPDATE mart.dim_person SET windows_domain='{0}', windows_username='{1}' where person_code='{2}';";
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
			var windowsLogonInfosInAnalytics = _jobParameters.Helper.Repository.GetWindowsLogonInfos().ToArray();
			var logonInfos = JobParameters.TenantLogonInfoLoader.GetLogonInfoModelsForGuids(windowsLogonInfosInAnalytics.Select(x => x.PersonCode));
			var windowsLogonInfosInApp = PersonTransformer.TransformWindowsLogonInfo(logonInfos);
			var toBeUpdated=new List<WindowsLogonInfo>();
			foreach (var windowsLogonInfoInApp in windowsLogonInfosInApp)
			{
				var windowsLogonInfoInAnalytics = windowsLogonInfosInAnalytics.FirstOrDefault(x => x.PersonCode == windowsLogonInfoInApp.PersonCode);
				if (windowsLogonInfoInAnalytics != null &&
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

					SqlCommand sqlCommand = conn.CreateCommand();
					sqlCommand.Transaction = sqlTransaction;

					sqlCommand.CommandType = CommandType.Text;
					foreach (var windowsLogonInfo in toBeUpdated)
					{
						sqlCommand.CommandText += string.Format(updateStatement, windowsLogonInfo.WindowsDomain,
							windowsLogonInfo.WindowsUsername, windowsLogonInfo.PersonCode);
					}

					affectedRows = sqlCommand.ExecuteNonQuery();

					sqlTransaction.Commit();
				});
			}

			return affectedRows;
		}
	}
}