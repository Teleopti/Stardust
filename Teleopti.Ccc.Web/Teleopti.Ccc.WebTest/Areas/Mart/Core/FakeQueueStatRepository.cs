using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Mart.Core;
using Teleopti.Ccc.Web.Areas.Mart.Models;

namespace Teleopti.Ccc.WebTest.Areas.Mart.Core
{
	public class FakeQueueStatRepository : IQueueStatRepository
	{
		public DateTime DateTimeInUtc;
		public int QueueId;
		public int DateId;
		public int IntervalId;

		public LogObject GetLogObject(string logobjectName)
		{
			return new LogObject {Id = 2,TimeZoneCode = "W. Europe Standard Time"};
		}

		public int GetQueueId(string queueName, string queueId)
		{
			return 10;
		}

		public int GetDateId(DateTime dateTime)
		{
			DateTimeInUtc = dateTime;
			return 1515;
		}

		public int GetIntervalLength()
		{
			return 15;
		}

		public void Save(IList<FactQueueModel> factQueueModel)
		{
			QueueId = factQueueModel[0].QueueId;
			DateId = factQueueModel[0].DateId;
			IntervalId = factQueueModel[0].IntervalId;
		}
	}
}