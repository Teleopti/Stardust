using System;

namespace Teleopti.Ccc.Web.Areas.Mart.Core
{
	public interface IQueueStatRepository
	{
		LogObject GetLogObject(string logobjectName);
		int GetQueueId(string queueName, string queueId);
		int GetDateId(DateTime dateTime);
		int GetIntervalLength();
	}

	public class QueueStatRepository :IQueueStatRepository
	{
		public LogObject GetLogObject(string logobjectName)
		{
			return new LogObject { Id = 2, TimeZoneCode = "W. Europe Standard Time" };
		}

		public int GetQueueId(string queueName, string queueId)
		{
			return 1;
		}

		public int GetDateId(DateTime dateTime)
		{
			return 1515;
		}

		public int GetIntervalLength()
		{
			return 15;
		}
	}
}