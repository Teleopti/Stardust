using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Mart.Models;

namespace Teleopti.Ccc.Web.Areas.Mart.Core
{
	public interface IQueueStatRepository
	{
		LogObjectSource GetLogObject(int logObjectId, string nhibDataSourceName);
		int GetQueueId(string queueName, string queueId, int logObjectId, string nhibDataSourceName);
		int GetDateId(DateTime dateTime, string nhibDataSourceName);
		int GetIntervalLength(string nhibDataSourceName);
		void SaveBatch(IList<FactQueueModel> factQueueModels, string nhibDataSourceName);
		void SetLatency(int latency);
	}
}