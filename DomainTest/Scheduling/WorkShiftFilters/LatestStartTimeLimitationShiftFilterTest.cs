using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftFilters;
using Teleopti.Ccc.DomainTest.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftFilters
{
	[TestFixture]
	public class LatestStartTimeLimitationShiftFilterTest
	{
		private MockRepository _mocks;
		private DateOnly _dateOnly;
		private TimeZoneInfo _timeZoneInfo;
		private IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;
		private Activity _activity;
		private ShiftCategory _category;
		private LatestStartTimeLimitationShiftFilter _target;

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
			_target = new LatestStartTimeLimitationShiftFilter();
		}

		[Test]
		public void ShouldFilterAccordingToEarliestEndTimeLimitation()
		{
			var earlistEnd = new DateTime(2013, 3, 1, 7, 30, 0, DateTimeKind.Utc);
			var result = _target.Filter(getCashes(), earlistEnd, new WorkShiftFinderResultForTest());
			Assert.That(result.Count, Is.EqualTo(1));
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
