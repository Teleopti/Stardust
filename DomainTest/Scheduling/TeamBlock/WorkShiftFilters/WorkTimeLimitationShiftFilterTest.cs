using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.DomainTest.ResourceCalculation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	[TestWithStaticDependenciesAvoidUse]
	public class WorkTimeLimitationShiftFilterTest
	{
		private MockRepository _mocks;
		private DateOnly _dateOnly;
		private IActivity _activity;
		private ShiftCategory _category;
		private TimeZoneInfo _timeZoneInfo;
		private IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;
		private EffectiveRestriction _effectiveRestriction;
		private WorkShiftFinderResult _finderResult;
		private IWorkTimeLimitationShiftFilter _target;
		private IWorkShift _workShift1;
		private IWorkShift _workShift2;

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
			_effectiveRestriction = new EffectiveRestriction(
				new StartTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0)),
				new EndTimeLimitation(new TimeSpan(15, 0, 0), new TimeSpan(18, 0, 0)),
				new WorkTimeLimitation(new TimeSpan(5, 0, 0), new TimeSpan(8, 0, 0)),
				null, null, null, new List<IActivityRestriction>());

			_finderResult = new WorkShiftFinderResult(PersonFactory.CreatePerson("bill"), new DateOnly(2009, 2, 3));

			_target = new WorkTimeLimitationShiftFilter(new SwedishCulture());
		}

		[Test]
		public void CanFilterOnRestrictionMinMaxWorkTimeWithEmptyList()
		{
			var ret = _target.Filter(new List<ShiftProjectionCache>(), _effectiveRestriction, _finderResult);
			Assert.IsNotNull(ret);
		}

		[Test]
		public void CanFilterOnRestrictionMinMaxWorkTimeWithNoRestrictions()
		{
			var effectiveRestriction = new EffectiveRestriction(
				new StartTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0)),
				new EndTimeLimitation(new TimeSpan(15, 0, 0), new TimeSpan(18, 0, 0)),
				new WorkTimeLimitation(null, null),
				null, null, null, new List<IActivityRestriction>());
			var ret = _target.Filter(getCashes(), effectiveRestriction, _finderResult);
			Assert.AreEqual(3, ret.Count);
		}

		[Test]
		public void CanFilterOnRestrictionMinMaxWorkTime()
		{
			IList<ShiftProjectionCache> shifts = new List<ShiftProjectionCache>();
			_workShift1 = _mocks.StrictMock<IWorkShift>();
			_workShift2 = _mocks.StrictMock<IWorkShift>();
			var ps1 = _mocks.StrictMock<IProjectionService>();
			var ps2 = _mocks.StrictMock<IProjectionService>();
			var lc1 = _mocks.StrictMock<IVisualLayerCollection>();
			var lc2 = _mocks.StrictMock<IVisualLayerCollection>();

			using (_mocks.Record())
			{
				Expect.Call(_workShift1.ProjectionService()).Return(ps1);
				Expect.Call(_workShift2.ProjectionService()).Return(ps2);
				Expect.Call(ps1.CreateProjection()).Return(lc1);
				Expect.Call(ps2.CreateProjection()).Return(lc2);

				Expect.Call(lc1.WorkTime()).Return(new TimeSpan(7, 0, 0));
				Expect.Call(lc2.WorkTime()).Return(new TimeSpan(10, 0, 0));
			}

			IList<ShiftProjectionCache> retShifts;
			ShiftProjectionCache c1;
			ShiftProjectionCache c2;

			using (_mocks.Playback())
			{
				var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 1, 1), _timeZoneInfo);
				c1 = new ShiftProjectionCache(_workShift1, _personalShiftMeetingTimeChecker);
				c1.SetDate(dateOnlyAsDateTimePeriod);
				shifts.Add(c1);
				c2 = new ShiftProjectionCache(_workShift2, _personalShiftMeetingTimeChecker);
				c2.SetDate(dateOnlyAsDateTimePeriod);
				shifts.Add(c2);
				retShifts = _target.Filter(shifts, _effectiveRestriction, new WorkShiftFinderResultForTest());

			}
			retShifts.Should().Contain(c1);
			retShifts.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldCheckParameters()
		{
			var result = _target.Filter(null, _effectiveRestriction, new WorkShiftFinderResultForTest());
			Assert.IsNull(result);

			result = _target.Filter(new List<ShiftProjectionCache>(), null, new WorkShiftFinderResultForTest());
			Assert.IsNull(result);

			result = _target.Filter(new List<ShiftProjectionCache>(), _effectiveRestriction, null);
			Assert.IsNull(result);
		}

		private IList<ShiftProjectionCache> getCashes()
		{
			var tmpList = getWorkShifts();
			var retList = new List<ShiftProjectionCache>();
			var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(_dateOnly, _timeZoneInfo);
			foreach (IWorkShift shift in tmpList)
			{
				var cache = new ShiftProjectionCache(shift, _personalShiftMeetingTimeChecker);
				cache.SetDate(dateOnlyAsDateTimePeriod);
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
