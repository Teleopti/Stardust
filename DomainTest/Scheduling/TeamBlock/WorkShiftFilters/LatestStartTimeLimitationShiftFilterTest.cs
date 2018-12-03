using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	public class LatestStartTimeLimitationShiftFilterTest
	{
		private DateOnly _dateOnly;
		private TimeZoneInfo _timeZoneInfo;
		private IActivity _activity;
		private ShiftCategory _category;
		private LatestStartTimeLimitationShiftFilter _target;

		[SetUp]
		public void Setup()
		{
			_dateOnly = new DateOnly(2013, 3, 1);
			_activity = ActivityFactory.CreateActivity("sd").WithId();
			_category = ShiftCategoryFactory.CreateShiftCategory("dv").WithId();
			_timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("UTC");
			_target = new LatestStartTimeLimitationShiftFilter();
		}

		[Test]
		public void ShouldFilterAccordingToEarliestEndTimeLimitation()
		{
			var earlistEnd = new DateTime(2013, 3, 1, 7, 30, 0, DateTimeKind.Utc);
			var result = _target.Filter(getCashes(), earlistEnd);
			Assert.That(result.Count, Is.EqualTo(1));
		}

		[Test]
		public void ShouldCheckParameters()
		{
			var result = _target.Filter(new List<ShiftProjectionCache>(), new DateTime());
			Assert.That(result.Count, Is.EqualTo(0));
		}

		private IList<ShiftProjectionCache> getCashes()
		{
			var tmpList = getWorkShifts();
			var retList = new List<ShiftProjectionCache>();
			var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(_dateOnly, _timeZoneInfo);
			foreach (IWorkShift shift in tmpList)
			{
				var cache = new ShiftProjectionCache(shift, dateOnlyAsDateTimePeriod);
				retList.Add(cache);
			}
			return retList;
		}

        [Test]
        public void TestIfEmptyListIsReturnedForNullMainShiftProjection()
        {
            var cache1 = new ShiftProjectionCache(new WorkShift(new ShiftCategory("Day")), new DateOnlyAsDateTimePeriod(_dateOnly,_timeZoneInfo));
            var shiftList = new List<ShiftProjectionCache> {cache1};
            
            var earlistEnd = new DateTime(2013, 3, 1, 7, 30, 0, DateTimeKind.Utc);
            var result = _target.Filter(shiftList, earlistEnd);
            Assert.AreEqual(result.Count ,0);
        }

		private IEnumerable<IWorkShift> getWorkShifts()
		{
			var workShift1 = WorkShiftFactory.CreateWorkShift(new TimeSpan(7, 0, 0), new TimeSpan(15, 0, 0),
															  _activity, _category);
			var workShift2 = WorkShiftFactory.CreateWorkShift(new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0),
														  _activity, _category);
			var workShift3 = WorkShiftFactory.CreateWorkShift(new TimeSpan(10, 0, 0), new TimeSpan(19, 0, 0),
																	  _activity, _category);

			return new List<IWorkShift> { workShift1, workShift2, workShift3 };
		}
	}
}
