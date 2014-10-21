using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Mart.Models;

namespace Teleopti.Ccc.Web.Areas.Mart.Core
{
	public interface IQueueStatRepository
	{
		LogObject GetLogObject(string logobjectName, string nhibDataSourceName);
		int GetQueueId(string queueName, string queueId, int logObjectId, string nhibDataSourceName);
		int GetDateId(DateTime dateTime, string nhibDataSourceName);
		int GetIntervalLength(string nhibDataSourceName);
		void Save(IList<FactQueueModel> factQueueModel, string nhibDataSourceName);
	}
}