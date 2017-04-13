using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Dates;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Date
{
	[TestFixture]
	[DomainTest]
	public class AnalyticsDateChangedHandlerTest : ISetup
	{
		public AnalyticsDateChangedHandler Target;
		public FakeAnalyticsDateRepository AnalyticsDateRepository;
		public FakeAnalyticsBridgeTimeZoneRepository AnalyticsBridgeTimeZoneRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<AnalyticsDateChangedHandler>();
		}

		[Test]
		public void ShouldAddOneEntryForEachTimezoneDateIntervalCombination()
		{
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2016, 01, 01), new DateTime(2016, 01, 31));

			Target.Handle(new AnalyticsDatesChangedEvent());

			AnalyticsBridgeTimeZoneRepository.Bridges.Count.Should().Be.EqualTo(31*96*2-4); // 31 days, 96 intervals, 2 timezones, excluding 4 who are on the next day and can't be mapped
		}

		[Test]
		public void ShouldRepeatFourIntervalsInLocalWhenWinterTimeChangeHappens()
		{
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2016, 10, 29), new DateTime(2016, 10, 31)); // Winter time happens on 30

			Target.Handle(new AnalyticsDatesChangedEvent());

			var bridgesDuringDstShift = AnalyticsBridgeTimeZoneRepository.Bridges.Where(x => x.DateId == 1 && x.TimeZoneId == 2).ToList();
			bridgesDuringDstShift
				.Count(x => x.LocalIntervalId == 8 || x.LocalIntervalId == 9 || x.LocalIntervalId == 10 || x.LocalIntervalId == 11).Should().Be.EqualTo(8);
		}

		[Test]
		public void ShouldSkipFourIntervalsInLocalWhenSummerTimeChangeHappens()
		{
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2016, 03, 26), new DateTime(2016, 03, 28)); // Summer time happens on 27

			Target.Handle(new AnalyticsDatesChangedEvent());

			var bridgesDuringDstShift = AnalyticsBridgeTimeZoneRepository.Bridges.Where(x => x.DateId == 1 && x.TimeZoneId == 2).ToList();
			bridgesDuringDstShift
				.Count(x => x.LocalIntervalId == 8 || x.LocalIntervalId == 9 || x.LocalIntervalId == 10 || x.LocalIntervalId == 11).Should().Be.EqualTo(0);

		}

		[Test]
		public void ShouldAddOneEntryForEachTimezoneDateIntervalCombinationForTimeZoneChanged()
		{
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2016, 01, 01), new DateTime(2016, 01, 31));

			Target.Handle(new AnalyticsTimeZoneChangedEvent());

			AnalyticsBridgeTimeZoneRepository.Bridges.Count.Should().Be.EqualTo(31 * 96 * 2 - 4); // 31 days, 96 intervals, 2 timezones, excluding 4 who are on the next day and can't be mapped
		}

		[Test]
		public void ShouldRepeatFourIntervalsInLocalWhenWinterTimeChangeHappensForTimeZoneChanged()
		{
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2016, 10, 29), new DateTime(2016, 10, 31)); // Winter time happens on 30

			Target.Handle(new AnalyticsTimeZoneChangedEvent());

			var bridgesDuringDstShift = AnalyticsBridgeTimeZoneRepository.Bridges.Where(x => x.DateId == 1 && x.TimeZoneId == 2).ToList();
			bridgesDuringDstShift
				.Count(x => x.LocalIntervalId == 8 || x.LocalIntervalId == 9 || x.LocalIntervalId == 10 || x.LocalIntervalId == 11).Should().Be.EqualTo(8);
		}

		[Test]
		public void ShouldSkipFourIntervalsInLocalWhenSummerTimeChangeHappensForTimeZoneChanged()
		{
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2016, 03, 26), new DateTime(2016, 03, 28)); // Summer time happens on 27

			Target.Handle(new AnalyticsTimeZoneChangedEvent());

			var bridgesDuringDstShift = AnalyticsBridgeTimeZoneRepository.Bridges.Where(x => x.DateId == 1 && x.TimeZoneId == 2).ToList();
			bridgesDuringDstShift
				.Count(x => x.LocalIntervalId == 8 || x.LocalIntervalId == 9 || x.LocalIntervalId == 10 || x.LocalIntervalId == 11).Should().Be.EqualTo(0);

		}
	}
}
