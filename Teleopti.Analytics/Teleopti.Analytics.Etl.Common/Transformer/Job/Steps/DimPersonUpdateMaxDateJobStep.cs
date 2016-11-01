using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Infrastructure.Util;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class DimPersonUpdateMaxDateJobStep : JobStepBase
	{
		private const string sqlUpdate = @"UPDATE mart.dim_person 
											SET valid_to_date_id_local = @ValidToDateIdLocal, 
												valid_to_date_id_maxDate = @ValidToDateIdMaxDate, 
												update_date = GETUTCDATE()
											WHERE 
												valid_to_date_id = -2 AND 
												person_id >= 0 AND 
												(valid_to_date_id_local != @ValidToDateIdLocal OR valid_to_date_id_maxDate != @ValidToDateIdMaxDate)";

		private readonly Func<SqlConnection> _connection;
		private readonly CloudSafeSqlExecute _executor = new CloudSafeSqlExecute();

		public DimPersonUpdateMaxDateJobStep(IJobParameters jobParameters) : base(jobParameters)
		{
			_connection = () =>
			{
				var conn = new SqlConnection(jobParameters.Helper.SelectedDataSource.Analytics.ConnectionString);
				conn.Open();
				return conn;
			};
			Name = "dim_person update max date";
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			using (JobParameters.Helper.SelectedDataSource.Analytics.CreateAndOpenUnitOfWork())
			{
				var affectedRows = 0;
				_executor.Run(_connection, conn =>
				{
					var sqlTransaction = conn.BeginTransaction();
					var sqlCommand = conn.CreateCommand();
					sqlCommand.Transaction = sqlTransaction;
					sqlCommand.CommandText = sqlUpdate;
					sqlCommand.Parameters.AddWithValue("ValidToDateIdLocal",
						_jobParameters.Helper.Repository.GetValidToDateIdLocalForEternity());
					sqlCommand.Parameters.AddWithValue("ValidToDateIdMaxDate",
						_jobParameters.Helper.Repository.GetValidToDateIdMaxDateForEternity());
					sqlCommand.CommandType = CommandType.Text;
					affectedRows = sqlCommand.ExecuteNonQuery();
					sqlTransaction.Commit();
				});
				return affectedRows;
			}
		}
	}
}