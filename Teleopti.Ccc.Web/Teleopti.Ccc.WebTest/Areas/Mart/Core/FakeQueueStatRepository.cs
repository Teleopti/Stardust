using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Mart.Core;
using Teleopti.Ccc.Web.Areas.Mart.Models;

namespace Teleopti.Ccc.WebTest.Areas.Mart.Core
{
	public class FakeQueueStatRepository : IQueueStatRepository
	{
		private int _dateId = 1515;
		public List<FactQueueModel> SavedQueueModelBatch { get; set; }
		public DateTime DateTimeInUtc { get; set; }
		public LogObjectSource LogObject { get; set; }
		public int Latency { get; set; }
		public int BatchCounter { get; set; }

		public FakeQueueStatRepository()
		{
			SavedQueueModelBatch = new List<FactQueueModel>();
		}

		public FactQueueModel SavedQueueModel
		{
			get { return SavedQueueModelBatch[0]; }
		}

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
		
		public void SaveBatch(IList<FactQueueModel> factQueueModels, string nhibDataSourceName)
		{
			SavedQueueModelBatch.AddRange(factQueueModels);
			BatchCounter++;
		}

		public void SetLatency(int latency)
		{
			Latency = latency;
		}

		public void SetInvalidDateId()
		{
			_dateId = -1;
		}
	}
}