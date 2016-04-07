using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Ccc.Intraday.TestApplication.Infrastructure;

namespace Teleopti.Ccc.Intraday.TestApplication
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
			
			var parameterList = new[]
												{
													 new SqlParameter("today", DateTime.Now.Date),
													 new SqlParameter("@time_zone_code", TimeZoneInfo.Local.Id)
												};

			DataSet resultSet = dbCommand.ExecuteDataSet(parameterList);

			if (resultSet == null || resultSet.Tables.Count == 0)
			{
				return workloadList;
			}

			var table = resultSet.Tables[0];
			var previousWorkloadId = -2;
			var previousSkillName = String.Empty;
			WorkloadInfo workloadInfo = null;

			foreach (DataRow row in table.Rows)
			{
				var workloadId = (int) row["WorkloadId"];
				var skillName = (string)row["SkillName"];
				if (workloadId != previousWorkloadId)
				{
					if (workloadInfo != null)
						workloadList.Add(workloadInfo);

					workloadInfo = new WorkloadInfo
					{
						WorkloadId = workloadId, 
						SkillName = skillName,
						Queues = new List<QueueInfo>()
					};
				}

				workloadInfo.Queues.Add(new QueueInfo
				{
					QueueId = (int)row["QueueId"],
					DatasourceId = (int)row["DatasourceId"],
					HasDataToday = (bool)row["HasQueueStats"]
				});

				previousWorkloadId = workloadId;
				previousSkillName = skillName;
			}

			if (workloadInfo != null)
			{
				workloadList.Add(workloadInfo);
				workloadInfo = new WorkloadInfo
				{
					WorkloadId = previousWorkloadId,
					SkillName = previousSkillName,
					Queues = new List<QueueInfo>()
				};
			}

			return workloadList;
		}
	}
}