﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftFilters;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftCalculation
{
	[TestFixture]
	public class WorkShiftFilterServiceTest
	{
		private IWorkShiftFilterService _target;
		private MockRepository _mocks;
		private IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
		private IShiftLengthDecider _shiftLengthDecider;
		private ISchedulingOptions _schedulingOptions;
		private TimeZoneInfo _timeZoneInfo;
		private IScheduleMatrixPro _matrix;
		private IPerson _person;
		private IWorkShift _workShift1;
		private IWorkShift _workShift2;
		private IWorkShift _workShift3;
		private IShiftCategory _category;
		private IActivity _activity;
		private IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;
		private IActivityRestrictionsShiftFilter _activityRestrictionsShiftFilter;
		private IBusinessRulesShiftFilter _businessRulesShiftFilter;
		private ICommonMainShiftFilter _commonMainShiftFilter;
		private IContractTimeShiftFilter _contractTimeShiftFilter;
		private IDisallowedShiftCategoriesShiftFilter _disallowedShiftCategoriesShiftFilter;
		private IEffectiveRestrictionShiftFilter _effectiveRestrictionShiftFilter;
		private IMainShiftOptimizeActivitiesSpecificationShiftFilter _mainShiftOptimizeActivitiesSpecificationShiftFilter;
		private INotOverWritableActivitiesShiftFilter _notOverWritableActivitiesShiftFilter;
		private IPersonalShiftsShiftFilter _personalShiftsShiftFilter;
		private IShiftCategoryRestrictionShiftFilter _shiftCategoryRestrictionShiftFilter;
		private IShiftProjectionCachesFromAdjustedRuleSetBagShiftFilter _shiftProjectionCachesFromAdjustedRuleSetBagShiftFilter;
		private ITimeLimitsRestrictionShiftFilter _timeLimitsRestrictionShiftFilter;
		private IWorkTimeLimitationShiftFilter _workTimeLimitationShiftFilter;
		private DateOnly _dateOnly;
		private WorkShiftFinderResult _finderResult;
		private IVirtualSchedulePeriod _schedulePeriod;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_workShiftMinMaxCalculator = _mocks.StrictMock<IWorkShiftMinMaxCalculator>();
			_shiftLengthDecider = _mocks.StrictMock<IShiftLengthDecider>();
			_schedulingOptions = new SchedulingOptions();
			_schedulingOptions.WorkShiftLengthHintOption = WorkShiftLengthHintOption.AverageWorkTime;
			_schedulingOptions.MainShiftOptimizeActivitySpecification = null;
			_dateOnly = new DateOnly(2013, 3, 1);
			var zone = TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time");
			_timeZoneInfo = (zone);
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_person = _mocks.StrictMock<IPerson>();
			_finderResult = new WorkShiftFinderResult(_person, _dateOnly);
			_activityRestrictionsShiftFilter = _mocks.StrictMock<IActivityRestrictionsShiftFilter>();
			_businessRulesShiftFilter = _mocks.StrictMock<IBusinessRulesShiftFilter>();
			_commonMainShiftFilter = _mocks.StrictMock<ICommonMainShiftFilter>();
			_contractTimeShiftFilter = _mocks.StrictMock<IContractTimeShiftFilter>();
			_disallowedShiftCategoriesShiftFilter = _mocks.StrictMock<IDisallowedShiftCategoriesShiftFilter>();
			_effectiveRestrictionShiftFilter = _mocks.StrictMock<IEffectiveRestrictionShiftFilter>();
			_mainShiftOptimizeActivitiesSpecificationShiftFilter =
				_mocks.StrictMock<IMainShiftOptimizeActivitiesSpecificationShiftFilter>();
			_notOverWritableActivitiesShiftFilter = _mocks.StrictMock<INotOverWritableActivitiesShiftFilter>();
			_personalShiftsShiftFilter = _mocks.StrictMock<IPersonalShiftsShiftFilter>();
			_shiftCategoryRestrictionShiftFilter = _mocks.StrictMock<IShiftCategoryRestrictionShiftFilter>();
			_shiftProjectionCachesFromAdjustedRuleSetBagShiftFilter =
				_mocks.StrictMock<IShiftProjectionCachesFromAdjustedRuleSetBagShiftFilter>();
			_timeLimitsRestrictionShiftFilter = _mocks.StrictMock<ITimeLimitsRestrictionShiftFilter>();
			_workTimeLimitationShiftFilter = _mocks.StrictMock<IWorkTimeLimitationShiftFilter>();
			_shiftLengthDecider = _mocks.StrictMock<IShiftLengthDecider>();
			_target = new WorkShiftFilterService(_activityRestrictionsShiftFilter, _businessRulesShiftFilter,
												 _commonMainShiftFilter, _contractTimeShiftFilter,
												 _disallowedShiftCategoriesShiftFilter, _effectiveRestrictionShiftFilter,
												 _mainShiftOptimizeActivitiesSpecificationShiftFilter,
												 _notOverWritableActivitiesShiftFilter, _personalShiftsShiftFilter,
												 _shiftCategoryRestrictionShiftFilter,
												 _shiftProjectionCachesFromAdjustedRuleSetBagShiftFilter,
												 _timeLimitsRestrictionShiftFilter, _workTimeLimitationShiftFilter,
												 _shiftLengthDecider, _workShiftMinMaxCalculator);
			_personalShiftMeetingTimeChecker = _mocks.StrictMock<IPersonalShiftMeetingTimeChecker>();
		}

		[Test]
		public void ShouldReturnNullIfEffectiveRestrictionIsNull()
		{
			var dateOnly = new DateOnly(2012, 12, 12);
			IEffectiveRestriction effectiveRestriction = null;
			using (_mocks.Record())
			{
			}
			using (_mocks.Playback())
			{
				var retShift = _target.Filter(dateOnly, _person, new List<IScheduleMatrixPro> { _matrix }, effectiveRestriction,
											  _schedulingOptions);
				Assert.IsNull(retShift);
			}
		}

		[Test]
		public void ShouldFilterWorkShifts()
		{
			var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(), new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var caches = getCashes();
			using (_mocks.Record())
			{
				Expect.Call(_person.Id).Return(Guid.Empty).Repeat.AtLeastOnce();
				Expect.Call(_person.VirtualSchedulePeriod(_dateOnly)).Return(_schedulePeriod).IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
				Expect.Call(_effectiveRestrictionShiftFilter.Filter(_schedulingOptions, effectiveRestriction, _finderResult))
				      .Return(true);
				Expect.Call(_shiftProjectionCachesFromAdjustedRuleSetBagShiftFilter.Filter(_dateOnly, _person, false,
																			   effectiveRestriction)).Return(caches);
				Expect.Call(_commonMainShiftFilter.Filter(caches, effectiveRestriction)).Return(caches);
				Expect.Call(_mainShiftOptimizeActivitiesSpecificationShiftFilter.Filter(caches, null)).IgnoreArguments().Return(caches);
				Expect.Call(_shiftCategoryRestrictionShiftFilter.Filter(effectiveRestriction.ShiftCategory, caches, _finderResult)).Return(caches);
				Expect.Call(_disallowedShiftCategoriesShiftFilter.Filter(_schedulingOptions.NotAllowedShiftCategories, caches, _finderResult)).Return(caches);
				Expect.Call(_activityRestrictionsShiftFilter.Filter(_dateOnly, _person, caches, effectiveRestriction, _finderResult)).Return(caches);
				Expect.Call(_timeLimitsRestrictionShiftFilter.Filter(_dateOnly, _person, caches, effectiveRestriction, _finderResult))
				      .Return(caches);
				Expect.Call(_workTimeLimitationShiftFilter.Filter(caches, effectiveRestriction, _finderResult)).Return(caches);
				Expect.Call(_contractTimeShiftFilter.Filter(_dateOnly, new List<IScheduleMatrixPro> {_matrix}, caches,
				                                            _schedulingOptions, _finderResult)).Return(caches);
				Expect.Call(_businessRulesShiftFilter.Filter(_person, caches, _dateOnly, _finderResult)).Return(caches);
				Expect.Call(_notOverWritableActivitiesShiftFilter.Filter(_dateOnly, _person, caches, _finderResult)).Return(caches);
				Expect.Call(_personalShiftsShiftFilter.Filter(_dateOnly, _person, caches, _finderResult)).Return(caches);
				Expect.Call(_shiftLengthDecider.FilterList(caches, _workShiftMinMaxCalculator, _matrix, _schedulingOptions))
				      .Return(caches);
			}
			using (_mocks.Playback())
			{
				var retShift = _target.Filter(_dateOnly, _person, new List<IScheduleMatrixPro> { _matrix }, effectiveRestriction, _schedulingOptions);
				Assert.IsNotNull(retShift);
			}
		}

		private IList<IShiftProjectionCache> getCashes()
		{
			var dateOnly = new DateOnly(2009, 2, 2);
			var tmpList = getWorkShifts();
			var retList = new List<IShiftProjectionCache>();
			foreach (IWorkShift shift in tmpList)
			{
				var cache = new ShiftProjectionCache(shift, _personalShiftMeetingTimeChecker);
				cache.SetDate(dateOnly, _timeZoneInfo);
				retList.Add(cache);
			}
			return retList;
		}

		private IEnumerable<IWorkShift> getWorkShifts()
		{
			_activity = ActivityFactory.CreateActivity("sd");
			_category = ShiftCategoryFactory.CreateShiftCategory("dv");
			_workShift1 = WorkShiftFactory.CreateWorkShift(new TimeSpan(7, 0, 0), new TimeSpan(15, 0, 0),
														  _activity, _category);
			_workShift2 = WorkShiftFactory.CreateWorkShift(new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0),
														  _activity, _category);
			_workShift3 = WorkShiftFactory.CreateWorkShift(new TimeSpan(10, 0, 0), new TimeSpan(19, 0, 0),
																	  _activity, _category);

			return new List<IWorkShift> { _workShift1, _workShift2, _workShift3 };
		}
	}
}
