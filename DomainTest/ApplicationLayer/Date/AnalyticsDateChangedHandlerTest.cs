using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Dates;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Date
{
	[TestFixture]
	public class AnalyticsDateChangedHandlerTest
	{
		private Func<AnalyticsDateChangedHandler> target;
		private FakeAnalyticsDateRepository _analyticsDateRepository;
		private IAnalyticsIntervalRepository _analyticsIntervalRepository;
		private IAnalyticsTimeZoneRepository _analyticsTimeZoneRepository;
		private FakeAnalyticsBridgeTimeZoneRepository _analyticsBridgeTimeZoneRepository;
		private IDistributedLockAcquirer _distributedLockAcquirer;

		[SetUp]
		public void Setup()
		{
			_analyticsIntervalRepository = new FakeAnalyticsIntervalRepository();
			_analyticsTimeZoneRepository = new FakeAnalyticsTimeZoneRepository();
			_analyticsBridgeTimeZoneRepository = new FakeAnalyticsBridgeTimeZoneRepository();
			_distributedLockAcquirer = new FakeDistributedLockAcquirer();

			target = () => new AnalyticsDateChangedHandler(_analyticsDateRepository, _analyticsIntervalRepository, _analyticsTimeZoneRepository, _analyticsBridgeTimeZoneRepository, _distributedLockAcquirer);
		}

		[Test]
		public void ShouldAddOneEntryForEachTimezoneDateIntervalCombination()
		{
			_analyticsDateRepository = new FakeAnalyticsDateRepository(new DateTime(2016, 01, 01), new DateTime(2016, 01, 31));

			target().Handle(new AnalyticsDatesChangedEvent());

			_analyticsBridgeTimeZoneRepository.Bridges.Count.Should().Be.EqualTo(31*96*2-4); // 31 days, 96 intervals, 2 timezones, excluding 4 who are on the next day and can't be mapped
		}

		[Test]
		public void ShouldRepeatFourIntervalsInLocalWhenWinterTimeChangeHappens()
		{
			_analyticsDateRepository = new FakeAnalyticsDateRepository(new DateTime(2016, 10, 29), new DateTime(2016, 10, 31)); // Winter time happens on 30

			target().Handle(new AnalyticsDatesChangedEvent());

			var bridgesDuringDstShift = _analyticsBridgeTimeZoneRepository.Bridges.Where(x => x.DateId == 1 && x.TimeZoneId == 2).ToList();
			bridgesDuringDstShift
				.Count(x => x.LocalIntervalId == 8 || x.LocalIntervalId == 9 || x.LocalIntervalId == 10 || x.LocalIntervalId == 11).Should().Be.EqualTo(8);
		}

		[Test]
		public void ShouldSkipFourIntervalsInLocalWhenSummerTimeChangeHappens()
		{
			_analyticsDateRepository = new FakeAnalyticsDateRepository(new DateTime(2016, 03, 26), new DateTime(2016, 03, 28)); // Summer time happens on 27

			target().Handle(new AnalyticsDatesChangedEvent());

			var bridgesDuringDstShift = _analyticsBridgeTimeZoneRepository.Bridges.Where(x => x.DateId == 1 && x.TimeZoneId == 2).ToList();
			bridgesDuringDstShift
				.Count(x => x.LocalIntervalId == 8 || x.LocalIntervalId == 9 || x.LocalIntervalId == 10 || x.LocalIntervalId == 11).Should().Be.EqualTo(0);

		}
	}
}
