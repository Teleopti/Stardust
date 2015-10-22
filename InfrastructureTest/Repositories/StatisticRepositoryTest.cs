using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("LongRunning")]
	public class StatisticRepositoryTest : DatabaseTest
	{
		private IStatisticRepository target;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void HasNoPublicConstructor()
		{
			Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(typeof(StatisticRepository)));
		}

		[Test]
		public void VerifyCorrectRepository()
		{
			Assert.IsNotNull(target);
			Assert.IsInstanceOf<StatisticRepository>(target);
		}

		[Test]
		public void VerifyEmptyQueueListDoesNotCrash()
		{
			ICollection<IStatisticTask> result = target.LoadSpecificDates(new List<IQueueSource>(), new DateTimePeriod(2005, 4, 1, 2005, 4, 6));
			Assert.AreEqual(0, result.Count);
		}

		[Test]
		public void VerifyLoadActiveAgentCount()
		{
			ISkill skill = Mocks.StrictMock<ISkill>();
			Expect.Call(skill.Id).Return(Guid.NewGuid());
			Mocks.ReplayAll();
			DateTimePeriod period = DateTimeFactory.CreateDateTimePeriod(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), 0);
			var result = target.LoadActiveAgentCount(skill, period);
			Assert.AreEqual(0, result.Count);
		}

		[Test]
		public void VerifyLoadReports()
		{
			target.LoadReports().Should().Not.Be.Null();
		}

		[Test]
		public void VerifyLoadQueues()
		{
			target.LoadQueues().Should().Not.Be.Null();
		}

		[Test]
		public void VerifyLoadSpecificDatesDateTimePeriod()
		{
			ICollection<IQueueSource> sources = new List<IQueueSource>();
			sources.Add(new QueueSource("heja", "blåvitt", 191661));
			sources.Add(new QueueSource("heja", "Änglarna", 191667));
			sources.Add(new QueueSource("Kö 1", "Anmälan", 191666));
			sources.Add(new QueueSource("Kö 2", "Ordrar", 191664));
			target.LoadSpecificDates(sources, new DateTimePeriod(2006, 1, 1, 2006, 1, 2));

			//should we test this?
			//can we asure that the data in the database look in a certain way?
		}

		[Test, SetCulture("ar-SA")]
		public void VerifyLoadSpecificDatesDateTimePeriodArabic()
		{
			ICollection<IQueueSource> sources = new List<IQueueSource>();
			sources.Add(new QueueSource("heja", "blåvitt", 191661));
			sources.Add(new QueueSource("heja", "Änglarna", 191667));
			sources.Add(new QueueSource("Kö 1", "Anmälan", 191666));
			sources.Add(new QueueSource("Kö 2", "Ordrar", 191664));
			target.LoadSpecificDates(sources, new DateTimePeriod(2006, 1, 1, 2006, 1, 2));
		}

		[Test]
		public void VerifyLoadDailyStatisticsForSpecificDates()
		{
			ICollection<IQueueSource> sources = new List<IQueueSource>();
			sources.Add(new QueueSource("heja", "blåvitt", 191661));
			sources.Add(new QueueSource("heja", "Änglarna", 191667));
			sources.Add(new QueueSource("Kö 1", "Anmälan", 191666));
			sources.Add(new QueueSource("Kö 2", "Ordrar", 191664));
			target.LoadDailyStatisticForSpecificDates(sources, new DateTimePeriod(2006, 1, 1, 2006, 1, 2), "utc", TimeSpan.FromHours(2));
		}

		[Test]
		public void VerifyGetMostRecentDateForQueueStatistics()
		{
			ICollection<IQueueSource> sources = new List<IQueueSource>();
			sources.Add(new QueueSource("heja", "blåvitt", 191661));
			sources.Add(new QueueSource("heja", "Änglarna", 191667));
			sources.Add(new QueueSource("Kö 1", "Anmälan", 191666));
			sources.Add(new QueueSource("Kö 2", "Ordrar", 191664));
			var date = target.QueueStatisticsUpUntilDate(sources);
			date.HasValue.Should().Be.False();
		}

		[Test]
		public void VerifyLoadAgentStat()
		{
			IList returnList = target.LoadAgentStat(Guid.NewGuid(), DateTime.Now, DateTime.Now, "W. Europe Standard Time", Guid.NewGuid());
			Assert.IsNotNull(returnList);
		}

		[Test]
		public void VerifyLoadAgentQueueStat()
		{
			IList returnList = target.LoadAgentQueueStat(DateTime.Now, DateTime.Now, "W. Europe Standard Time", Guid.NewGuid());
			Assert.IsNotNull(returnList);
		}

		[Test]
		public void VerifyLoadAdherenceData()
		{
			var returnList = target.LoadAdherenceData(DateTime.Now, "W. Europe Standard Time", Guid.NewGuid(),
														Guid.NewGuid(), 1033, 1);
			Assert.IsNotNull(returnList);
		}

		[Test]
		public void VerifyLoadDimQueues()
		{
			target.LoadDimQueues();
		}

		[Test]
		public void VerifyLoadFactQueues()
		{
			target.LoadFactQueues();
		}

		[Test]
		public void CheckForecastDifferShouldWork()
		{
			target.ForecastActualDifferNotifications();
		}

		[Test]
		public void ShouldLoadSkillStatisticForSpecificDates()
		{
			target.LoadSkillStatisticForSpecificDates( new DateTimePeriod(2006, 1, 1, 2006, 1, 2), "W. Europe Standard Time", TimeSpan.FromHours(2));
		}

		protected override void SetupForRepositoryTest()
		{
			target = StatisticRepositoryFactory.Create();
		}
	}
}
