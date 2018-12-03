using System;
using System.Collections.Generic;
using System.Configuration;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.Mart.Core;
using Teleopti.Ccc.Web.Areas.Mart.Models;


namespace Teleopti.Ccc.WebTest.Areas.Mart.Core
{
	[TestFixture]
	public class QueueStatHandlerTest
	{
		[Test]
		public void ShouldGetLogObjectFromDatabase()
		{
			var fakeRepos = new FakeQueueStatRepository();
			var target = new QueueStatHandler(fakeRepos);
			var queueDataList = new List<QueueStatsModel>
			{
				new QueueStatsModel
				{
					QueueName = "kön", 
					IntervalStart = "2014-12-12 15:00"
				}
			};
			target.Handle(queueDataList, "nhib", 1, 0);

			Assert.That(fakeRepos.LogObject.Id, Is.EqualTo(1));
		}

		[Test]
		public void ShouldGetQueueIdFromDatabase()
		{
			var fakeRepos = new FakeQueueStatRepository();
			var target = new QueueStatHandler(fakeRepos);
			var queueDataList = new List<QueueStatsModel>
			{
				new QueueStatsModel
				{
					QueueName = "kön", 
					IntervalStart = "2014-12-12 15:00"
				}
			};
			target.Handle(queueDataList, "nhib", 1, 0);

			Assert.That(fakeRepos.SavedQueueModel.QueueId, Is.EqualTo(10));
		}


		[Test]
		public void ShouldThrowWhenDateIdNotFoundInDatabase()
		{
			var fakeRepos = new FakeQueueStatRepository();
			var target = new QueueStatHandler(fakeRepos);
			fakeRepos.SetInvalidDateId();
			var queueStatsModel = new QueueStatsModel
			{
				QueueName = "kön",
				IntervalStart = "2014-12-12 15:00"
			};
			Assert.Throws<ArgumentException>(() => target.Handle(new List<QueueStatsModel> { queueStatsModel }, "nhib", 1, 0));
		}

		[Test]
		public void ShouldGetDateIdFromDatabase()
		{
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var fakeRepos = new FakeQueueStatRepository();
			var target = new QueueStatHandler(fakeRepos);
			var queueStatsModel = new QueueStatsModel
			{
				QueueName = "kön",
				IntervalStart = "2014-12-12 15:00"
			};
			target.Handle(new List<QueueStatsModel> { queueStatsModel }, "nhib", 1, 0);
			Assert.That(fakeRepos.SavedQueueModel.DateId, Is.EqualTo(1515));
			Assert.That(fakeRepos.DateTimeInUtc, Is.EqualTo(TimeZoneHelper.ConvertToUtc(DateTime.Parse(queueStatsModel.IntervalStart), timeZone)));
		}

		[Test]
		public void ShouldCalculateIntervalId()
		{
			var fakeRepos = new FakeQueueStatRepository();
			var target = new QueueStatHandler(fakeRepos);
			var queueStatsModel = new QueueStatsModel
			{
				QueueName = "kön",
				IntervalStart = "2014-12-12 15:00"
			};
			target.Handle(new List<QueueStatsModel> { queueStatsModel }, "nhib", 1, 0);
			Assert.That(fakeRepos.SavedQueueModel.IntervalId, Is.EqualTo(56));
		}

		[Test]
		public void ShouldSaveQueueData()
		{
			var fakeRepos = new FakeQueueStatRepository();
			var target = new QueueStatHandler(fakeRepos);
			var queueStatsModel = new QueueStatsModel
			{
				QueueName = "kön",
				IntervalStart = "2014-12-12 15:00",
				OfferedCalls = 124,
				AnsweredCalls = 80,
				AnsweredCallsWithinServiceLevel = 50,
				AbandonedCalls = 44,
				AbandonedCallsWithinServiceLevel = 20,
				AbandonedShortCalls = 10,
				OverflowOutCalls = 12,
				OverflowInCalls = 5,
				TalkTime = 120,
				AfterCallWork = 30,
				SpeedOfAnswer = 4,
				TimeToAbandon = 7,
				LongestDelayInQueueAnswered = 40,
				LongestDelayInQueueAbandoned = 43
			};
			target.Handle(new List<QueueStatsModel> { queueStatsModel }, "nhib", 1, 0);
			Assert.That(fakeRepos.SavedQueueModel.OfferedCalls, Is.EqualTo(queueStatsModel.OfferedCalls));
			Assert.That(fakeRepos.SavedQueueModel.AnsweredCalls, Is.EqualTo(queueStatsModel.AnsweredCalls));
			Assert.That(fakeRepos.SavedQueueModel.AnsweredCallsWithinServiceLevel, Is.EqualTo(queueStatsModel.AnsweredCallsWithinServiceLevel));
			Assert.That(fakeRepos.SavedQueueModel.AbandonedCalls, Is.EqualTo(queueStatsModel.AbandonedCalls));
			Assert.That(fakeRepos.SavedQueueModel.AbandonedCallsWithinServiceLevel, Is.EqualTo(queueStatsModel.AbandonedCallsWithinServiceLevel));
			Assert.That(fakeRepos.SavedQueueModel.AbandonedShortCalls, Is.EqualTo(queueStatsModel.AbandonedShortCalls));
			Assert.That(fakeRepos.SavedQueueModel.OverflowOutCalls, Is.EqualTo(queueStatsModel.OverflowOutCalls));
			Assert.That(fakeRepos.SavedQueueModel.OverflowInCalls, Is.EqualTo(queueStatsModel.OverflowInCalls));
			Assert.That(fakeRepos.SavedQueueModel.TalkTime, Is.EqualTo(queueStatsModel.TalkTime));
			Assert.That(fakeRepos.SavedQueueModel.AfterCallWork, Is.EqualTo(queueStatsModel.AfterCallWork));
			Assert.That(fakeRepos.SavedQueueModel.HandleTime, Is.EqualTo(queueStatsModel.TalkTime + queueStatsModel.AfterCallWork));
			Assert.That(fakeRepos.SavedQueueModel.SpeedOfAnswer, Is.EqualTo(queueStatsModel.SpeedOfAnswer));
			Assert.That(fakeRepos.SavedQueueModel.TimeToAbandon, Is.EqualTo(queueStatsModel.TimeToAbandon));
			Assert.That(fakeRepos.SavedQueueModel.LongestDelayInQueueAnswered, Is.EqualTo(queueStatsModel.LongestDelayInQueueAnswered));
			Assert.That(fakeRepos.SavedQueueModel.LongestDelayInQueueAbandoned, Is.EqualTo(queueStatsModel.LongestDelayInQueueAbandoned));
		}

		[Test]
		public void ShouldBatchSaveQueueData()
		{
			int batchSize = Convert.ToInt32(ConfigurationManager.AppSettings["StatsBatchSize"]);

			var fakeRepos = new FakeQueueStatRepository();
			var target = new QueueStatHandler(fakeRepos);
			var modelList = new List<QueueStatsModel>();
			for (int i = 0; i < batchSize + 1; i++)
			{
				modelList.Add(new QueueStatsModel{ QueueName = "kön", IntervalStart = "2014-12-12 15:00"});
			}
			target.Handle(modelList, "nhib", 1, 0);

			Assert.That(fakeRepos.BatchCounter, Is.EqualTo(2));
			Assert.That(fakeRepos.SavedQueueModelBatch.Count, Is.EqualTo(batchSize + 1));
		}
	}
}
