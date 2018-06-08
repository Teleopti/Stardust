using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Ccc.Intraday.TestCommon.Infrastructure;

namespace Teleopti.Ccc.Intraday.TestCommon
{
	public class WorkloadQueuesProvider : IWorkloadQueuesProvider
	{
		private readonly string _connectionString;

		public WorkloadQueuesProvider(string connectionString)
		{
			_connectionString = connectionString;
		}

		public IList<WorkloadInfo> Provide()
		{
			var workloadList = new List<WorkloadInfo>();
			var dbCommand = new DatabaseCommand(CommandType.StoredProcedure, "mart.web_intraday_simulator_get_workloads", _connectionString);
			
			DataSet resultSet = dbCommand.ExecuteDataSet(new List<SqlParameter>().ToArray());

			if (resultSet == null || resultSet.Tables.Count == 0)
			{
				return workloadList;
			}

			var table = resultSet.Tables[0];
			var previousWorkloadId = -2;
			var previousWorkloadCode = new Guid();
			var previousSkillName = String.Empty;
			WorkloadInfo workloadInfo = null;

			foreach (DataRow row in table.Rows)
			{
				var workloadId = (int) row["WorkloadId"];
				var workloadCode = (Guid) row["WorkloadCode"];
				var skillName = (string)row["SkillName"];
				if (workloadId != previousWorkloadId)
				{
					if (workloadInfo != null)
						workloadList.Add(workloadInfo);

					workloadInfo = new WorkloadInfo
					{
						WorkloadId = workloadId, 
						WorkloadCode = workloadCode,
						SkillName = skillName,
						Queues = new List<QueueInfo>()
					};
				}

				workloadInfo.Queues.Add(new QueueInfo
				{
					QueueId = (int)row["QueueId"],
					QueueName = (string)row["QueueName"],
					DatasourceId = (int)row["DatasourceId"],
				});

				previousWorkloadId = workloadId;
				previousWorkloadCode = workloadCode;
				previousSkillName = skillName;
			}

			if (workloadInfo != null)
			{
				workloadList.Add(workloadInfo);
				workloadInfo = new WorkloadInfo
				{
					WorkloadId = previousWorkloadId,
					WorkloadCode = previousWorkloadCode,
					SkillName = previousSkillName,
					Queues = new List<QueueInfo>()
				};
			}

			return workloadList;
		}
	}
}