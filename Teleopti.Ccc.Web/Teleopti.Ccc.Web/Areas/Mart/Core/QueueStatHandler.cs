using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Mart.Models;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.Web.Areas.Mart.Core
{
	public class QueueStatHandler : IQueueStatHandler
	{
		private readonly IQueueStatRepository _queueStatRepository;

		public QueueStatHandler(IQueueStatRepository queueStatRepository)
		{
			_queueStatRepository = queueStatRepository;
		}

		public void Handle(QueueStatsModel queueData)
		{
			if(string.IsNullOrEmpty(queueData.LogObjectName))
				throw new ArgumentException();
			
			var queueId = _queueStatRepository.GetQueueId(queueData.QueueName, queueData.QueueId, logobject.Id, queueData.NhibName);
			var dateTimeUtc = TimeZoneHelper.ConvertToUtc(DateTime.Parse(queueData.DateAndTimeString),
				TimeZoneInfo.FindSystemTimeZoneById(logobject.TimeZoneCode));
			var dateId = _queueStatRepository.GetDateId(dateTimeUtc, queueData.NhibName);
			var intervalId = getIntervalInDay(dateTimeUtc, queueData.NhibName);

			var factQueueModels = new List<FactQueueModel>
			{
				new FactQueueModel
				{
					LogObjectId = logobject.Id,
					QueueId = queueId,
					DateId = dateId,
					IntervalId = intervalId
				}
			};
			_queueStatRepository.Save(factQueueModels);
		}

		private int getIntervalInDay(DateTime dateTimeUtc, string nhibName)
		{
			var systemIntervalLength = _queueStatRepository.GetIntervalLength(nhibName);
			return (int)dateTimeUtc.TimeOfDay.TotalMinutes/systemIntervalLength;
		}
	}
}