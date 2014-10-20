using System;
using NUnit.Framework;
using Teleopti.Ccc.Web.Areas.Mart.Core;
using Teleopti.Ccc.Web.Areas.Mart.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Mart.Core
{
	[TestFixture]
	public class QueueStatsTest
	{

		[Test, ExpectedException(typeof(QueueStatException))]
		public void ShouldThrowIfLogobjectNameIsEmpty()
		{
			var fakeRepos = new FakeQueueStatRepository();
			var target = new QueueStatHandler(fakeRepos);
			target.Handle(new QueueStatsModel());
		}

		[Test]
		public void ShouldGetQueueIdFromDatabase()
		{
			var fakeRepos = new FakeQueueStatRepository();
			var target = new QueueStatHandler(fakeRepos);
			var facts = target.Handle(new QueueStatsModel { LogObjectName = "SomeAcdSomewhere", QueueName = "kön", DateAndTimeString = "2014-12-12 15:00" });
			Assert.That(facts[0].QueueId, Is.EqualTo(10));
		}

		[Test]
		public void ShouldGetDateIdFromDatabase()
		{
			var fakeRepos = new FakeQueueStatRepository();
			var target = new QueueStatHandler(fakeRepos);
			var queueStatsModel = new QueueStatsModel
			{
				LogObjectName = "SomeAcdSomewhere",
				QueueName = "kön",
				DateAndTimeString = "2014-12-12 15:00"
			};
			var facts = target.Handle(queueStatsModel);
			Assert.That(facts[0].DateId, Is.EqualTo(1515));
			var  timeZone =TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			Assert.That(fakeRepos.DateTimeInUtc, Is.EqualTo(TimeZoneHelper.ConvertToUtc(DateTime.Parse(queueStatsModel.DateAndTimeString), timeZone)));
		}

		[Test]
		public void ShouldCalculateIntervalId()
		{
			var fakeRepos = new FakeQueueStatRepository();
			var target = new QueueStatHandler(fakeRepos);
			var queueStatsModel = new QueueStatsModel
			{
				LogObjectName = "SomeAcdSomewhere",
				QueueName = "kön",
				DateAndTimeString = "2014-12-12 15:00"
			};
			var facts = target.Handle(queueStatsModel);
			Assert.That(facts[0].IntervalId, Is.EqualTo(56));
		}
	}

	class FakeQueueStatRepository : IQueueStatRepository
	{
		public DateTime DateTimeInUtc;

		public LogObject GetLogObject(string logobjectName, string nhibDataSourceName)
		{
			if (string.IsNullOrEmpty(logobjectName))
				return null;

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

	
	}
}
