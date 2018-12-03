using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Badge;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.LogObject;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.Sdk.ServiceBusTest.AgentBadge
{
	[TestFixture]
	public class LogObjectDateCheckerTest
	{
		private const int intervalsPerDay = 96;
		private IStatisticRepository _statisticRepository;
		private LogObjectDateChecker _target;

		[SetUp]
		public void Setup()
		{
			_statisticRepository = MockRepository.GenerateMock<IStatisticRepository>();
		}

		[Test]
		public void ShoudBeTrueIfHistoricalDataComesThanDate()
		{
			var now = DateTime.UtcNow;
			_statisticRepository.Stub(x => x.GetLogObjectDetails()).Return(new List<HistoricalDataDetail>
			{
				new HistoricalDataDetail
				{
					DateValue = now.AddDays(-1)
				}
			});

			_target = new LogObjectDateChecker(_statisticRepository);
			Assert.IsTrue(_target.HistoricalDataIsEarlierThan(new DateOnly(now)));
		}

		[Test]
		public void ShoudBeFalseIfHistoricalDataComesLaterThanDate()
		{
			var now = DateTime.UtcNow;
			_statisticRepository.Stub(x => x.GetLogObjectDetails()).Return(new List<HistoricalDataDetail>
			{
				new HistoricalDataDetail
				{
					DateValue = now.AddDays(1)
				}
			});

			_target = new LogObjectDateChecker(_statisticRepository);
			Assert.IsFalse(_target.HistoricalDataIsEarlierThan(new DateOnly(now)));
		}

		[Test]
		public void ShoudBeFalseIfHistoricalDataComesInDateButEarlierInterval()
		{
			var now = DateTime.UtcNow;
			_statisticRepository.Stub(x => x.GetLogObjectDetails()).Return(new List<HistoricalDataDetail>
			{
				new HistoricalDataDetail
				{
					IntervalsPerDay = intervalsPerDay,
					DateValue = now,
					IntervalValue = intervalsPerDay - 2
				}
			});

			_target = new LogObjectDateChecker(_statisticRepository);
			Assert.IsTrue(_target.HistoricalDataIsEarlierThan(new DateOnly(now)));
		}

		[Test]
		public void ShoudBeTrueIfHistoricalDataComesInDateAndLatestInterval()
		{
			var now = DateTime.UtcNow;
			_statisticRepository.Stub(x => x.GetLogObjectDetails()).Return(new List<HistoricalDataDetail>
			{
				new HistoricalDataDetail
				{
					IntervalsPerDay = intervalsPerDay,
					DateValue = now,
					IntervalValue = intervalsPerDay - 1
				}
			});

			_target = new LogObjectDateChecker(_statisticRepository);
			Assert.IsFalse(_target.HistoricalDataIsEarlierThan(new DateOnly(now)));
		}
	}
}