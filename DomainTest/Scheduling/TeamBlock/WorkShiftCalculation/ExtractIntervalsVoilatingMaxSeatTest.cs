using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftCalculation
{
	[TestFixture]
	public class ExtractIntervalsVoilatingMaxSeatTest
	{
		private IExtractIntervalsVoilatingMaxSeat _target;
		private ITeamBlockInfo _teamBlockInfo;
		private IBlockInfo _blockInfo;
		private MockRepository _mock;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IMaxSeatInformationGeneratorBasedOnIntervals _maxSeatInformationGeneratorBasedOnIntervals;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_maxSeatInformationGeneratorBasedOnIntervals = _mock.StrictMock<IMaxSeatInformationGeneratorBasedOnIntervals>();
			_target = new ExtractIntervalsVoilatingMaxSeat(_maxSeatInformationGeneratorBasedOnIntervals);
			_teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();
			_schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
		}

		[Test]
		public void ReturnAggregatedResultForASingleDayBlock()
		{
			_blockInfo = new BlockInfo(new DateOnlyPeriod(2014, 6, 11, 2014, 6, 11));
			IDictionary<DateTime, IntervalLevelMaxSeatInfo> intervalOnDay = new Dictionary<DateTime, IntervalLevelMaxSeatInfo>();
			var startDate = new DateTime(2014, 6, 11, 07, 00, 00, DateTimeKind.Utc);
			var endDate = new DateTime(2014, 6, 11, 08, 00, 00, DateTimeKind.Utc);
			intervalOnDay.Add(startDate, new IntervalLevelMaxSeatInfo(false, 1));
			intervalOnDay.Add(endDate , new IntervalLevelMaxSeatInfo(true, 3));
			IDictionary<DateTime, IntervalLevelMaxSeatInfo> intervalOnDaySecDay = new Dictionary<DateTime, IntervalLevelMaxSeatInfo>();
			intervalOnDaySecDay.Add(startDate.AddDays(1), new IntervalLevelMaxSeatInfo(false, 2));
			intervalOnDaySecDay.Add(endDate.AddDays(1), new IntervalLevelMaxSeatInfo(true, 4));
			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo).Repeat.Twice() ;
				Expect.Call(_maxSeatInformationGeneratorBasedOnIntervals.GetMaxSeatInfo(_teamBlockInfo, new DateOnly(2014, 6, 11),
					_schedulingResultStateHolder, TimeZoneInfo.Utc,false )).Return(intervalOnDay);
				Expect.Call(_maxSeatInformationGeneratorBasedOnIntervals.GetMaxSeatInfo(_teamBlockInfo, new DateOnly(2014, 6, 12),
					_schedulingResultStateHolder, TimeZoneInfo.Utc, false)).Return(intervalOnDaySecDay);
			}
			using (_mock.Playback())
			{
				var aggregatedIntervals = _target.IdentifyIntervalsWithBrokenMaxSeats(_teamBlockInfo, _schedulingResultStateHolder,
					TimeZoneInfo.Utc,_blockInfo.BlockPeriod.StartDate );
				Assert.IsFalse(aggregatedIntervals[startDate].IsMaxSeatReached);
				Assert.AreEqual(1, aggregatedIntervals[startDate].MaxSeatBoostingFactor);

				Assert.IsTrue(aggregatedIntervals[endDate].IsMaxSeatReached);
				Assert.AreEqual(3, aggregatedIntervals[endDate].MaxSeatBoostingFactor);

				Assert.IsFalse(aggregatedIntervals[startDate.AddDays(1)].IsMaxSeatReached);
				Assert.AreEqual(2, aggregatedIntervals[startDate.AddDays(1)].MaxSeatBoostingFactor);

				Assert.IsTrue(aggregatedIntervals[endDate.AddDays(1)].IsMaxSeatReached);
				Assert.AreEqual(4, aggregatedIntervals[endDate.AddDays(1)].MaxSeatBoostingFactor);
			}

		}

		[Test]
		public void ReturnAggregatedResultWithoutOverlappingIntervals()
		{
			_blockInfo = new BlockInfo(new DateOnlyPeriod(2014, 6, 11, 2014, 6, 12));
			var intervalOnDay1 = new Dictionary<DateTime, IntervalLevelMaxSeatInfo>();
			var date1 = new DateTime(2014, 6, 11, 07, 00, 00, DateTimeKind.Utc);
			var date2 = new DateTime(2014, 6, 11, 08, 00, 00, DateTimeKind.Utc);
			var date3 = new DateTime(2014, 6, 12, 09, 00, 00, DateTimeKind.Utc);
			var date4 = new DateTime(2014, 6, 12, 10, 00, 00, DateTimeKind.Utc);

			intervalOnDay1.Add(date1, new IntervalLevelMaxSeatInfo(false, 1));
			intervalOnDay1.Add(date2, new IntervalLevelMaxSeatInfo(true, 3));

			var intervalOnDay2 = new Dictionary<DateTime, IntervalLevelMaxSeatInfo>();
			intervalOnDay2.Add(date3 , new IntervalLevelMaxSeatInfo(true, 4));
			intervalOnDay2.Add(date4 , new IntervalLevelMaxSeatInfo(false, 1));

			var intervalOnDay3 = new Dictionary<DateTime, IntervalLevelMaxSeatInfo>();
			intervalOnDay3.Add(new DateTime(2014, 6, 13, 07, 00, 00, DateTimeKind.Utc), new IntervalLevelMaxSeatInfo(true, 5));

			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo).Repeat.AtLeastOnce() ;
				Expect.Call(_maxSeatInformationGeneratorBasedOnIntervals.GetMaxSeatInfo(_teamBlockInfo, new DateOnly(2014, 6, 11),
					_schedulingResultStateHolder, TimeZoneInfo.Utc,false )).Return(intervalOnDay1);
				Expect.Call(_maxSeatInformationGeneratorBasedOnIntervals.GetMaxSeatInfo(_teamBlockInfo, new DateOnly(2014, 6, 12),
					_schedulingResultStateHolder, TimeZoneInfo.Utc,false)).Return(intervalOnDay2);
				Expect.Call(_maxSeatInformationGeneratorBasedOnIntervals.GetMaxSeatInfo(_teamBlockInfo, new DateOnly(2014, 6, 13),
					_schedulingResultStateHolder, TimeZoneInfo.Utc, false)).Return(intervalOnDay3);
			}
			using (_mock.Playback())
			{
				var aggregatedIntervals = _target.IdentifyIntervalsWithBrokenMaxSeats(_teamBlockInfo, _schedulingResultStateHolder,
					TimeZoneInfo.Utc,_blockInfo.BlockPeriod.StartDate );
				Assert.IsFalse(aggregatedIntervals[new DateTime(2014, 6, 11, 07, 00, 00, DateTimeKind.Utc)].IsMaxSeatReached);
				Assert.AreEqual(1, aggregatedIntervals[new DateTime(2014, 6, 11, 07, 00, 00, DateTimeKind.Utc)].MaxSeatBoostingFactor);

				Assert.IsTrue(aggregatedIntervals[new DateTime(2014, 6, 11, 08, 00, 00, DateTimeKind.Utc)].IsMaxSeatReached);
				Assert.AreEqual(3, aggregatedIntervals[new DateTime(2014, 6, 11, 08, 00, 00, DateTimeKind.Utc)].MaxSeatBoostingFactor);

				Assert.IsTrue(aggregatedIntervals[new DateTime(2014, 6, 11, 09, 00, 00, DateTimeKind.Utc)].IsMaxSeatReached);
				Assert.AreEqual(4, aggregatedIntervals[new DateTime(2014, 6, 11, 09, 00, 00, DateTimeKind.Utc)].MaxSeatBoostingFactor);

				Assert.IsFalse(aggregatedIntervals[new DateTime(2014, 6, 11, 10, 00, 00, DateTimeKind.Utc)].IsMaxSeatReached);
				Assert.AreEqual(1, aggregatedIntervals[new DateTime(2014, 6, 11, 10, 00, 00, DateTimeKind.Utc)].MaxSeatBoostingFactor);

				Assert.IsTrue(aggregatedIntervals[new DateTime(2014, 6, 12, 07, 00, 00, DateTimeKind.Utc)].IsMaxSeatReached);
				Assert.AreEqual(5, aggregatedIntervals[new DateTime(2014, 6, 12, 07, 00, 00, DateTimeKind.Utc)].MaxSeatBoostingFactor);
			}

		}

		[Test]
		public void ReturnAggregatedResultWithOverlappingIntervals()
		{
			_blockInfo = new BlockInfo(new DateOnlyPeriod(2014, 6, 11, 2014, 6, 13));
			var intervalOnDay1 = new Dictionary<DateTime, IntervalLevelMaxSeatInfo>();

			intervalOnDay1.Add(new DateTime(2014, 6, 11, 07, 00, 00, DateTimeKind.Utc), new IntervalLevelMaxSeatInfo(false, 1));
			intervalOnDay1.Add(new DateTime(2014, 6, 11, 08, 00, 00, DateTimeKind.Utc), new IntervalLevelMaxSeatInfo(true, 3));

			var intervalOnDay2 = new Dictionary<DateTime, IntervalLevelMaxSeatInfo>();
			intervalOnDay2.Add(new DateTime(2014, 6, 12, 08, 00, 00, DateTimeKind.Utc), new IntervalLevelMaxSeatInfo(true, 4));
			intervalOnDay2.Add(new DateTime(2014, 6, 12, 09, 00, 00, DateTimeKind.Utc), new IntervalLevelMaxSeatInfo(false, 1));

			var intervalOnDay3 = new Dictionary<DateTime, IntervalLevelMaxSeatInfo>();
			intervalOnDay3.Add(new DateTime(2014, 6, 13, 09, 00, 00, DateTimeKind.Utc), new IntervalLevelMaxSeatInfo(true, 2));
			intervalOnDay3.Add(new DateTime(2014, 6, 13, 10, 00, 00, DateTimeKind.Utc), new IntervalLevelMaxSeatInfo(false, 1));

			var intervalOnDay4 = new Dictionary<DateTime, IntervalLevelMaxSeatInfo>();
			intervalOnDay4.Add(new DateTime(2014, 6, 14, 09, 00, 00, DateTimeKind.Utc), new IntervalLevelMaxSeatInfo(true, 3));
			intervalOnDay4.Add(new DateTime(2014, 6, 14, 10, 00, 00, DateTimeKind.Utc), new IntervalLevelMaxSeatInfo(false, 1));
			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo).Repeat.Twice() ;
				Expect.Call(_maxSeatInformationGeneratorBasedOnIntervals.GetMaxSeatInfo(_teamBlockInfo, new DateOnly(2014, 6, 11),
					_schedulingResultStateHolder, TimeZoneInfo.Utc,false )).Return(intervalOnDay1);
				Expect.Call(_maxSeatInformationGeneratorBasedOnIntervals.GetMaxSeatInfo(_teamBlockInfo, new DateOnly(2014, 6, 12),
					_schedulingResultStateHolder, TimeZoneInfo.Utc, false)).Return(intervalOnDay2);
				Expect.Call(_maxSeatInformationGeneratorBasedOnIntervals.GetMaxSeatInfo(_teamBlockInfo, new DateOnly(2014, 6, 13),
					_schedulingResultStateHolder, TimeZoneInfo.Utc, false)).Return(intervalOnDay3);
				Expect.Call(_maxSeatInformationGeneratorBasedOnIntervals.GetMaxSeatInfo(_teamBlockInfo, new DateOnly(2014, 6, 14),
					_schedulingResultStateHolder, TimeZoneInfo.Utc,false )).Return(intervalOnDay4);
			}
			using (_mock.Playback())
			{
				var aggregatedIntervals = _target.IdentifyIntervalsWithBrokenMaxSeats(_teamBlockInfo, _schedulingResultStateHolder,
					TimeZoneInfo.Utc,_blockInfo.BlockPeriod.StartDate );
				Assert.IsFalse(aggregatedIntervals[new DateTime(2014, 6, 11, 07, 00, 00, DateTimeKind.Utc)].IsMaxSeatReached);
				Assert.AreEqual(1, aggregatedIntervals[new DateTime(2014, 6, 11, 07, 00, 00, DateTimeKind.Utc)].MaxSeatBoostingFactor);

				Assert.IsTrue(aggregatedIntervals[new DateTime(2014, 6, 11, 08, 00, 00, DateTimeKind.Utc)].IsMaxSeatReached);
				Assert.AreEqual(7, aggregatedIntervals[new DateTime(2014, 6, 11, 08, 00, 00, DateTimeKind.Utc)].MaxSeatBoostingFactor);

				Assert.IsTrue(aggregatedIntervals[new DateTime(2014, 6, 11, 09, 00, 00, DateTimeKind.Utc)].IsMaxSeatReached);
				Assert.AreEqual(2, aggregatedIntervals[new DateTime(2014, 6, 11, 09, 00, 00, DateTimeKind.Utc)].MaxSeatBoostingFactor);

				Assert.IsFalse(aggregatedIntervals[new DateTime(2014, 6, 11, 10, 00, 00, DateTimeKind.Utc)].IsMaxSeatReached);
				Assert.AreEqual(1, aggregatedIntervals[new DateTime(2014, 6, 11, 10, 00, 00, DateTimeKind.Utc)].MaxSeatBoostingFactor);

				Assert.IsTrue( aggregatedIntervals[new DateTime(2014, 6, 12, 09, 00, 00, DateTimeKind.Utc)].IsMaxSeatReached);
				Assert.AreEqual(3, aggregatedIntervals[new DateTime(2014, 6, 12, 09, 00, 00, DateTimeKind.Utc)].MaxSeatBoostingFactor);

				Assert.IsFalse( aggregatedIntervals[new DateTime(2014, 6, 12, 10, 00, 00, DateTimeKind.Utc)].IsMaxSeatReached);
				Assert.AreEqual(1, aggregatedIntervals[new DateTime(2014, 6, 12, 10, 00, 00, DateTimeKind.Utc)].MaxSeatBoostingFactor);
			}

		}

		[Test]
		public void ReturnAggregatedResultWithNoOverlappingIntervals()
		{
			_blockInfo = new BlockInfo(new DateOnlyPeriod(2014, 6, 11, 2014, 6, 12));
			var intervalOnDay1 = new Dictionary<DateTime, IntervalLevelMaxSeatInfo>();
			intervalOnDay1.Add(new DateTime(2014, 6, 11, 07, 00, 00, DateTimeKind.Utc), new IntervalLevelMaxSeatInfo(false, 1));
			intervalOnDay1.Add(new DateTime(2014, 6, 11, 08, 00, 00, DateTimeKind.Utc), new IntervalLevelMaxSeatInfo(true, 3));

			var intervalOnDay2 = new Dictionary<DateTime, IntervalLevelMaxSeatInfo>();
			intervalOnDay2.Add(new DateTime(2014, 6, 12, 07, 00, 00, DateTimeKind.Utc), new IntervalLevelMaxSeatInfo(false, 1));
			intervalOnDay2.Add(new DateTime(2014, 6, 12, 08, 00, 00, DateTimeKind.Utc), new IntervalLevelMaxSeatInfo(false, 1));

			var intervalOnDay3 = new Dictionary<DateTime, IntervalLevelMaxSeatInfo>();
			intervalOnDay3.Add(new DateTime(2014, 6, 13, 10, 00, 00, DateTimeKind.Utc), new IntervalLevelMaxSeatInfo(false, 1));
			intervalOnDay3.Add(new DateTime(2014, 6, 13, 11, 00, 00, DateTimeKind.Utc), new IntervalLevelMaxSeatInfo(false, 1));

			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo).Repeat.Twice() ;
				Expect.Call(_maxSeatInformationGeneratorBasedOnIntervals.GetMaxSeatInfo(_teamBlockInfo, new DateOnly(2014, 6, 11),
					_schedulingResultStateHolder, TimeZoneInfo.Utc,false )).Return(intervalOnDay1);
				Expect.Call(_maxSeatInformationGeneratorBasedOnIntervals.GetMaxSeatInfo(_teamBlockInfo, new DateOnly(2014, 6, 12),
					_schedulingResultStateHolder, TimeZoneInfo.Utc, false)).Return(intervalOnDay2);
				Expect.Call(_maxSeatInformationGeneratorBasedOnIntervals.GetMaxSeatInfo(_teamBlockInfo, new DateOnly(2014, 6, 13),
					_schedulingResultStateHolder, TimeZoneInfo.Utc, false)).Return(intervalOnDay3);
			}
			using (_mock.Playback())
			{
				var aggregatedIntervals = _target.IdentifyIntervalsWithBrokenMaxSeats(_teamBlockInfo, _schedulingResultStateHolder,
					TimeZoneInfo.Utc,_blockInfo.BlockPeriod.StartDate );
				Assert.IsFalse(aggregatedIntervals[new DateTime(2014, 6, 11, 07, 00, 00, DateTimeKind.Utc)].IsMaxSeatReached);
				Assert.AreEqual(1, aggregatedIntervals[new DateTime(2014, 6, 11, 07, 00, 00, DateTimeKind.Utc)].MaxSeatBoostingFactor);

				Assert.IsTrue(aggregatedIntervals[new DateTime(2014, 6, 11, 08, 00, 00, DateTimeKind.Utc)].IsMaxSeatReached);
				Assert.AreEqual(3, aggregatedIntervals[new DateTime(2014, 6, 11, 08, 00, 00, DateTimeKind.Utc)].MaxSeatBoostingFactor);

				Assert.IsFalse(aggregatedIntervals[new DateTime(2014, 6, 12, 10, 00, 00, DateTimeKind.Utc)].IsMaxSeatReached);
				Assert.AreEqual(1, aggregatedIntervals[new DateTime(2014, 6, 12, 10, 00, 00, DateTimeKind.Utc)].MaxSeatBoostingFactor);

				Assert.False( aggregatedIntervals[new DateTime(2014, 6, 12, 11, 00, 00, DateTimeKind.Utc)].IsMaxSeatReached);
				Assert.AreEqual(1, aggregatedIntervals[new DateTime(2014, 6, 12, 11, 00, 00, DateTimeKind.Utc)].MaxSeatBoostingFactor);
			}

		}
	}

	
}
