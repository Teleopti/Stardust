using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.DomainTest.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	public class TimeLimitsRestrictionShiftFilterTest
	{
		private MockRepository _mocks;
		private DateOnly _dateOnly;
		private Activity _activity;
		private ShiftCategory _category;
		private TimeZoneInfo _timeZoneInfo;
		private IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;
		private IValidDateTimePeriodShiftFilter _validDateTimePeriodShiftFilter;
		private ILatestStartTimeLimitationShiftFilter _latestStartTimeLimitationShiftFilter;
		private IEarliestEndTimeLimitationShiftFilter _earliestEndTimeLimitationShiftFilter;
		private ITimeLimitsRestrictionShiftFilter _target;
		private EffectiveRestriction _effectiveRestriction;
		private IPerson _person;

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
			_person = PersonFactory.CreatePerson("bill");
			_personalShiftMeetingTimeChecker = _mocks.StrictMock<IPersonalShiftMeetingTimeChecker>();
			_validDateTimePeriodShiftFilter = _mocks.StrictMock<IValidDateTimePeriodShiftFilter>();
			_latestStartTimeLimitationShiftFilter = _mocks.StrictMock<ILatestStartTimeLimitationShiftFilter>();
			_earliestEndTimeLimitationShiftFilter = _mocks.StrictMock<IEarliestEndTimeLimitationShiftFilter>();
			_effectiveRestriction = new EffectiveRestriction(
				new StartTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0)),
				new EndTimeLimitation(new TimeSpan(15, 0, 0), new TimeSpan(18, 0, 0)),
				new WorkTimeLimitation(new TimeSpan(5, 0, 0), new TimeSpan(8, 0, 0)),
				null, null, null, new List<IActivityRestriction>());
			_target = new TimeLimitsRestrictionShiftFilter(_validDateTimePeriodShiftFilter, _latestStartTimeLimitationShiftFilter,
														   _earliestEndTimeLimitationShiftFilter);
		}

		[Test]
		public void CanFilterOnEffectiveRestriction()
		{
			var ret = _target.Filter(_dateOnly, _person, getCashes(), _effectiveRestriction, new WorkShiftFinderResultForTest());
			Assert.IsNotNull(ret);
		}

		[Test]
		public void CanFilterOnRestrictionTimeLimitsWithEmptyList()
		{
			var ret = _target.Filter(_dateOnly, _person, new List<IShiftProjectionCache>(), _effectiveRestriction, new WorkShiftFinderResultForTest());
			Assert.IsNotNull(ret);
		}

		[Test]
		public void ShouldCheckParameters()
		{
			var result = _target.Filter(_dateOnly, null, new List<IShiftProjectionCache>(), _effectiveRestriction, new WorkShiftFinderResultForTest());
			Assert.IsNull(result);

			result = _target.Filter(_dateOnly, _person, null, _effectiveRestriction, new WorkShiftFinderResultForTest());
			Assert.IsNull(result);

			result = _target.Filter(_dateOnly, _person, new List<IShiftProjectionCache>(), null, new WorkShiftFinderResultForTest());
			Assert.IsNull(result);
			
			result = _target.Filter(_dateOnly, _person, new List<IShiftProjectionCache>(), _effectiveRestriction, new WorkShiftFinderResultForTest());
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
