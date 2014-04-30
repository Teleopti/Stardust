using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	public class CommonActivityFilterTest
	{
		private ICommonActivityFilter _target;
		private IActivity _activity;
		private ShiftCategory _category;
		private WorkShift _workShift;
		private IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;
		private MockRepository _mocks;
		private SchedulingOptions _schedulingOptions;
		private DateOnly _dateOnly;
		private TimeZoneInfo _timeZoneInfo;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_dateOnly = new DateOnly(2013, 3, 1);
			_timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("UTC"));
			_target = new CommonActivityFilter();
			_activity = ActivityFactory.CreateActivity("sd");
			_activity.SetId(Guid.NewGuid());
			_category = ShiftCategoryFactory.CreateShiftCategory("dv");
			_category.SetId(Guid.NewGuid());
			_workShift = WorkShiftFactory.CreateWorkShift(new TimeSpan(7, 0, 0), new TimeSpan(15, 0, 0),
														  _activity, _category);
			_personalShiftMeetingTimeChecker = _mocks.StrictMock<IPersonalShiftMeetingTimeChecker>();
			_schedulingOptions = new SchedulingOptions();
		}

		[Test]
		public void ShouldCheckParameters()
		{
			var shift = new ShiftProjectionCache(_workShift, _personalShiftMeetingTimeChecker);
			var restriction = new EffectiveRestriction(new StartTimeLimitation(),
														new EndTimeLimitation(),
														new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

			var result = _target.Filter(null, _schedulingOptions, restriction);
			Assert.IsNull(result);

			result = _target.Filter(new List<IShiftProjectionCache>(), _schedulingOptions, restriction);
			Assert.That(result.Count, Is.EqualTo(0));

			result = _target.Filter(new List<IShiftProjectionCache>(), _schedulingOptions, null);
			Assert.That(result.Count, Is.EqualTo(0));

			_schedulingOptions.TeamSameActivity = true;
			result = _target.Filter(new List<IShiftProjectionCache> { shift }, _schedulingOptions, restriction);
			Assert.That(result.Count, Is.EqualTo(1));
		}

		[Test]
		public void ShouldFilterShiftsAccordingToCommonActivity()
		{
			_schedulingOptions.TeamSameActivity = true;
			_schedulingOptions.UseGroupScheduling = true;
			_schedulingOptions.CommonActivity = _activity;
			var restriction = new EffectiveRestriction(new StartTimeLimitation(),
			                                           new EndTimeLimitation(),
			                                           new WorkTimeLimitation(), null, null, null,
			                                           new List<IActivityRestriction>()) {CommonActivity = getCommonActivity()};


			var shifts = getCashes();
			
			using (_mocks.Record())
			{
				
			}
			using (_mocks.Playback())
			{
				var result = _target.Filter(shifts, _schedulingOptions, restriction);
				Assert.That(result.Count, Is.EqualTo(1));
			}
		}

		private ICommonActivity getCommonActivity()
		{
			return new CommonActivity
				{
					Activity = _activity,
					Periods = new List<DateTimePeriod> {new DateTimePeriod(2013, 3, 1, 7, 2013, 3, 1, 15)}
				};
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
			var workShift3 = WorkShiftFactory.CreateWorkShift(new TimeSpan(7, 0, 0), new TimeSpan(15, 0, 0),
															  ActivityFactory.CreateActivity("sd1"), _category);
			return new List<IWorkShift> { workShift1, workShift2, workShift3 };
		}
	}
}
