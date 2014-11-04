﻿using System;
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

		public void Handle(IEnumerable<QueueStatsModel> queueData, string nhibName, int logObjectId)
		{
			foreach (var queueStatsModel in queueData)
			{
				var logobject = _queueStatRepository.GetLogObject(logObjectId, nhibName);
				var queueId = _queueStatRepository.GetQueueId(queueStatsModel.QueueName, queueStatsModel.QueueId, logobject.Id, nhibName);
				var dateTimeUtc = TimeZoneHelper.ConvertToUtc(DateTime.Parse(queueStatsModel.IntervalStart),
					TimeZoneInfo.FindSystemTimeZoneById(logobject.TimeZoneCode));
				var dateId = _queueStatRepository.GetDateId(dateTimeUtc, nhibName);
				if (dateId == -1)
					throw new ArgumentException();
				var intervalId = getIntervalInDay(dateTimeUtc, nhibName);

				var factQueueModels = new List<FactQueueModel>
				{
					new FactQueueModel
					{
						LogObjectId = logobject.Id,
						QueueId = queueId,
						DateId = dateId,
						IntervalId = intervalId,
						OfferedCalls = queueStatsModel.OfferedCalls,
						AnsweredCalls = queueStatsModel.AnsweredCalls,
						AnsweredCallsWithinServiceLevel = queueStatsModel.AnsweredCallsWithinServiceLevel,
						AbandonedCalls = queueStatsModel.AbandonedCalls,
						AbandonedCallsWithinServiceLevel = queueStatsModel.AbandonedCallsWithinServiceLevel,
						AbandonedShortCalls = queueStatsModel.AbandonedShortCalls,
						OverflowOutCalls = queueStatsModel.OverflowOutCalls,
						OverflowInCalls = queueStatsModel.OverflowInCalls,
						TalkTime = queueStatsModel.TalkTime,
						AfterCallWork = queueStatsModel.AfterCallWork,
						HandleTime = queueStatsModel.TalkTime + queueStatsModel.AfterCallWork,
						SpeedOfAnswer = queueStatsModel.SpeedOfAnswer,
						TimeToAbandon = queueStatsModel.TimeToAbandon,
						LongestDelayInQueueAnswered = queueStatsModel.LongestDelayInQueueAnswered,
						LongestDelayInQueueAbandoned = queueStatsModel.LongestDelayInQueueAbandoned
					}
				};
				_queueStatRepository.Save(factQueueModels, nhibName);
			}
		}

		private int getIntervalInDay(DateTime dateTimeUtc, string nhibName)
		{
			var systemIntervalLength = _queueStatRepository.GetIntervalLength(nhibName);
			return (int)dateTimeUtc.TimeOfDay.TotalMinutes/systemIntervalLength;
		}
	}
}