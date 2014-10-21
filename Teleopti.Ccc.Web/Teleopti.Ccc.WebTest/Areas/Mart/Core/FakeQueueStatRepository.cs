using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Mart.Core;
using Teleopti.Ccc.Web.Areas.Mart.Models;

namespace Teleopti.Ccc.WebTest.Areas.Mart.Core
{
	public class FakeQueueStatRepository : IQueueStatRepository
	{
		public FactQueueModel SavedQueueModel { get; set; }
		public DateTime DateTimeInUtc { get; set; }

		public LogObject GetLogObject(string logobjectName, string nhibDataSourceName)
		{
			return new LogObject { Id = 2, TimeZoneCode = "W. Europe Standard Time" };
		}

		public int GetQueueId(string queueName, string queueId, int logObjectId, string nhibDataSourceName)
		{
			return 10;
		}

		public int GetDateId(DateTime dateTime, string nhibDataSourceName)
		{
			DateTimeInUtc = dateTime;
			return 1515;
		}

		public int GetIntervalLength(string nhibDataSourceName)
		{
			return 15;
		}

		public void Save(IList<FactQueueModel> factQueueModel, string nhibDataSourceName)
		{
			SavedQueueModel = factQueueModel[0];
		}
	}
}