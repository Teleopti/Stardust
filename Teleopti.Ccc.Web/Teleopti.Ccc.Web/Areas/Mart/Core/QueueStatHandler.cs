﻿using System;
using System.Collections.Generic;
using System.Net;
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

		public IList<FactQueueModel> Handle(QueueStatsModel queueData)
		{
			var logobject = _queueStatRepository.GetLogObject(queueData.LogObjectName, queueData.NhibName);
			if (logobject == null)
			{
				throw new QueueStatException(HttpStatusCode.BadRequest,"The Log Object Name is not valid");
			}
			var queueId = _queueStatRepository.GetQueueId(queueData.QueueName, queueData.QueueId, logobject.Id, queueData.NhibName);
			var dateTimeUtc = TimeZoneHelper.ConvertToUtc(DateTime.Parse(queueData.DateAndTimeString),
				TimeZoneInfo.FindSystemTimeZoneById(logobject.TimeZoneCode));
			var dateId = _queueStatRepository.GetDateId(dateTimeUtc, queueData.NhibName);
			var intervalId = getIntervalInDay(dateTimeUtc, queueData.NhibName);

			return new List<FactQueueModel>
			{
				new FactQueueModel
				{
					LogObjectId = logobject.Id, 
					QueueId = queueId,
					DateId = dateId,
					IntervalId = intervalId
				}
			};
		}

		private int getIntervalInDay(DateTime dateTimeUtc, string nhibName)
		{
			var systemIntervalLength = _queueStatRepository.GetIntervalLength(nhibName);
			return (int)dateTimeUtc.TimeOfDay.TotalMinutes/systemIntervalLength;
		}
	}

	public class QueueStatException : Exception
	{
		public QueueStatException(HttpStatusCode statusCode, string message):base(message)
		{
			StatusCode = statusCode;
		}
		public HttpStatusCode StatusCode { get; private set; }
	}
}