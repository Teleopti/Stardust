using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
	[TestFixture]
	public class OvertimeDateTimePeriodExtractorTest
	{
		private OvertimeDateTimePeriodExtractor _target;
		private DateTimePeriod _shiftPeriod;
		private DateTime _shiftEndingTime;
		private DateTime _shiftStartTime;
		private int _minimumResolution;
		private MinMax<TimeSpan> _overtimeDuration;
		
		[SetUp]
		public void SetUp()
		{
			_target = new OvertimeDateTimePeriodExtractor();
			_shiftStartTime = new DateTime(2014, 02, 26, 15, 0, 0, DateTimeKind.Utc);
			_shiftEndingTime = new DateTime(2014, 02, 26, 16, 0, 0, DateTimeKind.Utc);
			_shiftPeriod = new DateTimePeriod(_shiftStartTime, _shiftEndingTime);
			_minimumResolution = 15;
			_overtimeDuration = new MinMax<TimeSpan>(TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(15));
		}

		[Test]
		public void ShouldReturnPeriodBeforeAndAfterWithNoSplit()
		{
			var expectedBefore = new DateTimePeriod(_shiftStartTime.AddMinutes(-15), _shiftStartTime);
			var expectedAfter = new DateTimePeriod(_shiftEndingTime, _shiftEndingTime.AddMinutes(15));
			var specifiedPeriod = new DateTimePeriod(_shiftStartTime.AddHours(-10), _shiftEndingTime.AddHours(10));

			var result = _target.Extract(_minimumResolution, _overtimeDuration, _shiftPeriod, specifiedPeriod);
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual(1, result[0].DateTimePeriods.Count);
			Assert.AreEqual(1, result[1].DateTimePeriods.Count);
			Assert.AreEqual(expectedBefore, result[0].DateTimePeriods[0]);
			Assert.AreEqual(expectedAfter, result[1].DateTimePeriods[0]);
		}

		[Test]
		public void ShouldReturnNoPeriodsWhenSpecifiedPeriodDontIntersectWithShift()
		{
			var specifiedPeriod = new DateTimePeriod(_shiftEndingTime.AddMinutes(15), _shiftEndingTime.AddMinutes(30));
			_overtimeDuration = new MinMax<TimeSpan>(specifiedPeriod.ElapsedTime(), specifiedPeriod.ElapsedTime());

			var result = _target.Extract(_minimumResolution, _overtimeDuration, _shiftPeriod, specifiedPeriod);
			Assert.AreEqual(0, result.Count);
		}

		[Test]
		public void ShouldReturnPeriodsWhenSpecifiedPeriodContainsThem()
		{
			var specifiedPeriod = new DateTimePeriod(_shiftStartTime.AddMinutes(-30), _shiftEndingTime.AddMinutes(30));
			_overtimeDuration = new MinMax<TimeSpan>(TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(15));
			var expectedBefore = new DateTimePeriod(_shiftStartTime.AddMinutes(-15), _shiftStartTime);
			var expectedAfter = new DateTimePeriod(_shiftEndingTime, _shiftEndingTime.AddMinutes(15));

			var result = _target.Extract(_minimumResolution, _overtimeDuration, _shiftPeriod, specifiedPeriod);
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual(1, result[0].DateTimePeriods.Count);
			Assert.AreEqual(1, result[1].DateTimePeriods.Count);
			Assert.AreEqual(expectedBefore, result[0].DateTimePeriods[0]);
			Assert.AreEqual(expectedAfter, result[1].DateTimePeriods[0]);
		}

		[Test]
		public void ShouldNotReturnPeriodsWhenSpecifiedPeriodDontContainsThem()
		{
			var specifiedPeriod = new DateTimePeriod(_shiftStartTime.AddMinutes(-15), _shiftEndingTime.AddMinutes(15));
			_overtimeDuration = new MinMax<TimeSpan>(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));

			var result = _target.Extract(_minimumResolution, _overtimeDuration, _shiftPeriod, specifiedPeriod);
			Assert.AreEqual(0, result.Count);
		}

		[Test]
		public void ShouldReturnPeriodsWhenSpecifiedPeriodIsAdjacentToShift()
		{
			var specifiedPeriod = new DateTimePeriod(_shiftStartTime.AddMinutes(-15), _shiftStartTime);
			_overtimeDuration = new MinMax<TimeSpan>(TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(15));
			var expectedBefore = new DateTimePeriod(_shiftStartTime.AddMinutes(-15), _shiftStartTime);

			var result = _target.Extract(_minimumResolution, _overtimeDuration, _shiftPeriod, specifiedPeriod);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(1, result[0].DateTimePeriods.Count);
			Assert.AreEqual(expectedBefore, result[0].DateTimePeriods[0]);
		}
	}
}
