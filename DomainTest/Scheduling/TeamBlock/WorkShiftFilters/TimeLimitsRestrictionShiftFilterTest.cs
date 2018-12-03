using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	public class TimeLimitsRestrictionShiftFilterTest
	{
		private MockRepository _mocks;
		private DateOnly _dateOnly;
		private IActivity _activity;
		private ShiftCategory _category;
		private TimeZoneInfo _timeZoneInfo;
		private ValidDateTimePeriodShiftFilter _validDateTimePeriodShiftFilter;
		private ILatestStartTimeLimitationShiftFilter _latestStartTimeLimitationShiftFilter;
		private IEarliestEndTimeLimitationShiftFilter _earliestEndTimeLimitationShiftFilter;
		private TimeLimitsRestrictionShiftFilter _target;
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
			_validDateTimePeriodShiftFilter = new ValidDateTimePeriodShiftFilter();
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
			var ret = _target.Filter(_dateOnly, _person, getCashes(), _effectiveRestriction);
			Assert.IsNotNull(ret);
		}

		[Test]
		public void CanFilterOnRestrictionTimeLimitsWithEmptyList()
		{
			var ret = _target.Filter(_dateOnly, _person, new List<ShiftProjectionCache>(), _effectiveRestriction);
			Assert.IsNotNull(ret);
		}

		[Test]
		public void ShouldCheckParameters()
		{
			var result = _target.Filter(_dateOnly, null, new List<ShiftProjectionCache>(), _effectiveRestriction);
			Assert.IsNull(result);

			result = _target.Filter(_dateOnly, _person, null, _effectiveRestriction);
			Assert.IsNull(result);

			result = _target.Filter(_dateOnly, _person, new List<ShiftProjectionCache>(), null);
			Assert.IsNull(result);
			
			result = _target.Filter(_dateOnly, _person, new List<ShiftProjectionCache>(), _effectiveRestriction);
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
