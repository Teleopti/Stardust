using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Mart.Core;
using Teleopti.Ccc.Web.Areas.Mart.Models;

namespace Teleopti.Ccc.WebTest.Areas.Mart.Core
{
	public class FakeQueueStatRepository : IQueueStatRepository
	{
		private int _dateId = 1515;
		public FactQueueModel SavedQueueModel { get; set; }
		public DateTime DateTimeInUtc { get; set; }
		public LogObjectSource LogObject { get; set; }

		public LogObjectSource GetLogObject(int logObjectId, string nhibDataSourceName)
		{
			LogObject = new LogObjectSource { Id = logObjectId, TimeZoneCode = "W. Europe Standard Time" };
			return LogObject;
		}

		public int GetQueueId(string queueName, string queueId, int logObjectId, string nhibDataSourceName)
		{
			return 10;
		}

		public int GetDateId(DateTime dateTime, string nhibDataSourceName)
		{
			if (_dateId != -1)
				DateTimeInUtc = dateTime;
			return _dateId;
		}

		public int GetIntervalLength(string nhibDataSourceName)
		{
			return 15;
		}

		public void Save(IList<FactQueueModel> factQueueModel, string nhibDataSourceName)
		{
			SavedQueueModel = factQueueModel[0];
		}

		public void SetInvalidDateId()
		{
			_dateId = -1;
		}
	}
}