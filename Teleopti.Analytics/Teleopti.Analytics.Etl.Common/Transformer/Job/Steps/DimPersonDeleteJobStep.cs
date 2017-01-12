using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Infrastructure.Util;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class DimPersonDeleteJobStep : JobStepBase
	{
		private readonly CloudSafeSqlExecute _executor = new CloudSafeSqlExecute();
		private readonly Func<SqlConnection> _connection;

		public DimPersonDeleteJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "dim_person delete data";
			IsBusinessUnitIndependent = true;
			_connection = () =>
			{
				var conn = new SqlConnection(jobParameters.Helper.SelectedDataSource.Analytics.ConnectionString);
				conn.Open();
				return conn;
			};
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			var deleted = 0;
			while (anyToBeDeleted())
				deleted+=_jobParameters.Helper.Repository.DimPersonDeleteData(RaptorTransformerHelper.CurrentBusinessUnit);
			return deleted;
		}

		private bool anyToBeDeleted()
		{
			var result = false;
			_executor.Run(_connection, connection =>
			{
				var command = connection.CreateCommand();
				command.CommandText = @"
					SELECT top 1 person_id
					FROM mart.dim_person 
					WHERE to_be_deleted = 1";
				command.CommandType = CommandType.Text;
				result = command.ExecuteScalar() != null;
			});
			return result;
		}
	}
}