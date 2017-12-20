using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Intraday.TestApplication.Infrastructure;

namespace Teleopti.Ccc.Intraday.TestApplication
{
	public class UniqueQueueProvider
	{
		private readonly string _appDbConnectonString;
		private readonly string _analyticsConnectionString;

		public UniqueQueueProvider(string appDbConnectonString, string analyticsConnectionString)
		{
			_appDbConnectonString = appDbConnectonString;
			_analyticsConnectionString = analyticsConnectionString;
		}

		public QueueInfo Get(WorkloadInfo workload)
		{
			var uniqueQueueName = string.Format("IntradayTestApplication_{0}", workload.WorkloadId);

			var existingQueue = workload.Queues.FirstOrDefault(x => x.QueueName.Equals(uniqueQueueName, StringComparison.InvariantCulture));
			if (existingQueue != null)
				return existingQueue;

			var newQueueId = addQueueToAnalytics(workload.WorkloadId, uniqueQueueName);
			addQueueToApplicationDb(workload.WorkloadId, workload.WorkloadCode, uniqueQueueName, newQueueId);

			var newQueue = new QueueInfo
			{
				QueueId = newQueueId,
				DatasourceId = 1
			};

			workload.Queues.Add(newQueue);

			return newQueue;
		}

		private void addQueueToApplicationDb(int workloadId, Guid workloadCode, string uniqueQueueName, int newQueueId)
		{
			var queueCode = Guid.NewGuid();
			var fakeQueueAggId = 90000 + workloadId;

			StringBuilder sqlText = new StringBuilder();
			sqlText.AppendLine("INSERT INTO QueueSource ");
			sqlText.AppendFormat("SELECT '{0}', 1, '{1}', '{2}', {3}, {4}, {4}, 1, null, '{5}', '{5}'"
				, queueCode, 
				new Guid("3F0886AB-7B25-4E95-856A-0D726EDC2A67"), 
				DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), 
				newQueueId, 
				fakeQueueAggId,
				uniqueQueueName);
			var dbCommand = new DatabaseCommand(CommandType.Text, sqlText.ToString(), _appDbConnectonString);
			dbCommand.ExecuteNonQuery(new SqlParameter[] {});

			sqlText.Clear();
			sqlText.AppendLine("INSERT INTO QueueSourceCollection ");
			sqlText.AppendFormat("SELECT '{0}', '{1}'", workloadCode, queueCode);
			dbCommand = new DatabaseCommand(CommandType.Text, sqlText.ToString(), _appDbConnectonString);
			dbCommand.ExecuteNonQuery(new SqlParameter[] { });
		}

		private int addQueueToAnalytics(int workloadId, string queueName)
		{
			var dbCommand = new DatabaseCommand(CommandType.StoredProcedure, "mart.web_intraday_simulator_new_queue", _analyticsConnectionString);

			var parameterList = new[]
												{
													 new SqlParameter("@workload_id", workloadId),
													 new SqlParameter("@queue_name", queueName)
												};

			var newQueueId = (int)dbCommand.ExecuteScalar(parameterList);
			return newQueueId;
		}
	}
}