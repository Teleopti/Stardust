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

		public void Handle(QueueStatsModel queueData, string dataSource)
		{
			if(string.IsNullOrEmpty(queueData.LogObjectName))
				throw new ArgumentException();

			var logobject = _queueStatRepository.GetLogObject(queueData.LogObjectName, dataSource);
			var queueId = _queueStatRepository.GetQueueId(queueData.QueueName, queueData.QueueId, logobject.Id, dataSource);
			var dateTimeUtc = TimeZoneHelper.ConvertToUtc(DateTime.Parse(queueData.DateAndTimeString),
				TimeZoneInfo.FindSystemTimeZoneById(logobject.TimeZoneCode));
			var dateId = _queueStatRepository.GetDateId(dateTimeUtc, dataSource);
			if (dateId == -1)
				throw new ArgumentException();
			var intervalId = getIntervalInDay(dateTimeUtc, dataSource);

			var factQueueModels = new List<FactQueueModel>
			{
				new FactQueueModel
				{
					LogObjectId = logobject.Id,
					QueueId = queueId,
					DateId = dateId,
					IntervalId = intervalId,
					OfferedCalls = queueData.OfferedCalls,
					AnsweredCalls = queueData.AnsweredCalls,
					AnsweredCallsWithinServiceLevel = queueData.AnsweredCallsWithinServiceLevel, 
					AbandonedCalls = queueData.AbandonedCalls,
					AbandonedCallsWithinServiceLevel = queueData.AbandonedCallsWithinServiceLevel,
					AbandonedShortCalls = queueData.AbandonedShortCalls,
					OverflowOutCalls = queueData.OverflowOutCalls,
					OverflowInCalls = queueData.OverflowInCalls,
					TalkTime = queueData.TalkTime,
					AfterCallWork = queueData.AfterCallWork,
					HandleTime = queueData.TalkTime + queueData.AfterCallWork,
					SpeedOfAnswer = queueData.SpeedOfAnswer,
					TimeToAbandon = queueData.TimeToAbandon,
					LongestDelayInQueueAnswered = queueData.LongestDelayInQueueAnswered,
					LongestDelayInQueueAbandoned = queueData.LongestDelayInQueueAbandoned 
				}
			};
			_queueStatRepository.Save(factQueueModels, dataSource);
		}

		private int getIntervalInDay(DateTime dateTimeUtc, string nhibName)
		{
			var systemIntervalLength = _queueStatRepository.GetIntervalLength(nhibName);
			return (int)dateTimeUtc.TimeOfDay.TotalMinutes/systemIntervalLength;
		}
	}
}