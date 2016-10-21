using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.DomainTest.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	[LegacyTest]
	public class TimeLimitationShiftFilterTest
	{
		private MockRepository _mocks;
		private DateOnly _dateOnly;
		private IActivity _activity;
		private ShiftCategory _category;
		private TimeZoneInfo _timeZoneInfo;
		private IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;
		private ValidDateTimePeriodShiftFilter _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_dateOnly = new DateOnly(2013, 3, 1);
			_activity = ActivityFactory.CreateActivity("sd");
			_activity.SetId(Guid.NewGuid());
			_category = ShiftCategoryFactory.CreateShiftCategory("dv");
			_category.SetId(Guid.NewGuid());
			_timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("UTC"));
			_personalShiftMeetingTimeChecker = _mocks.StrictMock<IPersonalShiftMeetingTimeChecker>();
			_target = new ValidDateTimePeriodShiftFilter(new TimeZoneGuardWrapper());
		}

		[Test]
		public void ShouldFilterAccordingToTimeLimitation()
		{
			var startTime = new DateTime(2013, 3, 1, 7, 30, 0, DateTimeKind.Utc);
			var endTime = new DateTime(2013, 3, 1, 17, 30, 0, DateTimeKind.Utc);
			var result = _target.Filter(getCashes(), new DateTimePeriod(startTime, endTime), new WorkShiftFinderResultForTest());
			Assert.That(result.Count, Is.EqualTo(1));
		}

		[Test]
		public void ShouldCheckParameters()
		{
			var startTime = new DateTime(2013, 3, 1, 7, 30, 0, DateTimeKind.Utc);
			var endTime = new DateTime(2013, 3, 1, 17, 30, 0, DateTimeKind.Utc);
			
			var result = _target.Filter(null, new DateTimePeriod(startTime, endTime), new WorkShiftFinderResultForTest());
			Assert.IsNull(result);

			result = _target.Filter(new List<IShiftProjectionCache>(), new DateTimePeriod(startTime, endTime), null);
			Assert.IsNull(result);
			
			result = _target.Filter(new List<IShiftProjectionCache>(), new DateTimePeriod(startTime, endTime), new WorkShiftFinderResultForTest());
			Assert.That(result.Count, Is.EqualTo(0));
		}

		private IList<IShiftProjectionCache> getCashes()
		{
			var tmpList = getWorkShifts();
			var retList = new List<IShiftProjectionCache>();
			foreach (IWorkShift shift in tmpList)
			{
				var cache = new ShiftProjectionCache(shift, _personalShiftMeetingTimeChecker);
				cache.SetDate(_dateOnly, _timeZoneInfo);
				retList.Add(cache);
			}
			return retList;
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
