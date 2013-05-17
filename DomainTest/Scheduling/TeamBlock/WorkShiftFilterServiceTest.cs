using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
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
		private ITeamBlockInfo _teamBlockInfo;
		private GroupPerson _groupPerson;
		private List<IScheduleMatrixPro> _matrixList;
		private ICommonActivityFilter _commonActivityFilter;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
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
			_person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(new Person(), _dateOnly);
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
			_commonActivityFilter = _mocks.StrictMock<ICommonActivityFilter>();
			_shiftLengthDecider = _mocks.StrictMock<IShiftLengthDecider>();
			_target = new WorkShiftFilterService(_activityRestrictionsShiftFilter, _businessRulesShiftFilter,
												 _commonMainShiftFilter, _contractTimeShiftFilter,
												 _disallowedShiftCategoriesShiftFilter, _effectiveRestrictionShiftFilter,
												 _mainShiftOptimizeActivitiesSpecificationShiftFilter,
												 _notOverWritableActivitiesShiftFilter, _personalShiftsShiftFilter,
												 _shiftCategoryRestrictionShiftFilter,
												 _shiftProjectionCachesFromAdjustedRuleSetBagShiftFilter,
												 _timeLimitsRestrictionShiftFilter, _workTimeLimitationShiftFilter,
												 _shiftLengthDecider, _workShiftMinMaxCalculator, _commonActivityFilter);
			_personalShiftMeetingTimeChecker = _mocks.StrictMock<IPersonalShiftMeetingTimeChecker>();
			_groupPerson = new GroupPerson(new List<IPerson>{_person}, _dateOnly, "Hej", Guid.NewGuid());
			_matrixList = new List<IScheduleMatrixPro> { _matrix };
			IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>>{ _matrixList };
			var teaminfo = new TeamInfo(_groupPerson, groupMatrixes);
			var blockInfo = new BlockInfo(new DateOnlyPeriod(_dateOnly, _dateOnly));
			_teamBlockInfo = new TeamBlockInfo(teaminfo, blockInfo);
		}

		[Test]
		public void ShouldReturnNullIfEffectiveRestrictionIsNull()
		{
			var dateOnly = new DateOnly(2012, 12, 12);

			var retShift = _target.Filter(dateOnly, _teamBlockInfo, null,
			                              _schedulingOptions, _finderResult);
			Assert.IsNull(retShift);
		}

		[Test]
		public void ShouldReturnNullIfTeamBlockInfoIsNull()
		{
			var dateOnly = new DateOnly(2012, 12, 12);
			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(), new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var retShift = _target.Filter(dateOnly, null, effectiveRestriction,
			                              _schedulingOptions, _finderResult);
			Assert.IsNull(retShift);
		}

		[Test]
		public void ShouldReturnNullIfMatrixListIsEmpty()
		{
			var dateOnly = new DateOnly(2012, 12, 12);
			var groupPerson = new GroupPerson(new List<IPerson> { _person }, dateOnly, "Hej", Guid.NewGuid());
			var matrixList = new List<IScheduleMatrixPro>();
			IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>> { matrixList };
			var teaminfo = new TeamInfo(groupPerson, groupMatrixes);
			var blockInfo = new BlockInfo(new DateOnlyPeriod(dateOnly, dateOnly));
			var teamBlockInfo = new TeamBlockInfo(teaminfo, blockInfo);
			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(), new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var retShift = _target.Filter(dateOnly, teamBlockInfo, effectiveRestriction,
										  _schedulingOptions, _finderResult);
			Assert.IsNull(retShift);
		}
	
		[Test]
		public void ShouldReturnNullIfCurrentSchedulePeriodIsInvalid()
		{
			var dateOnly = new DateOnly(2012, 12, 12);
			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(), new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var retShift = _target.Filter(dateOnly, _teamBlockInfo, effectiveRestriction,
			                              _schedulingOptions, _finderResult);
			Assert.IsNull(retShift);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldFilterWorkShifts()
		{
			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(), new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			_schedulingOptions.MainShiftOptimizeActivitySpecification = new All<IMainShift>();
			var schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var caches = getCashes();
			using (_mocks.Record())
			{
				Expect.Call(_matrix.SchedulePeriod).Return(schedulePeriod);
				Expect.Call(schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(_dateOnly, _dateOnly));
				Expect.Call(_effectiveRestrictionShiftFilter.Filter(_schedulingOptions, effectiveRestriction, _finderResult))
				      .Return(true);
				Expect.Call(_shiftProjectionCachesFromAdjustedRuleSetBagShiftFilter.Filter(_dateOnly, _groupPerson, false)).Return(caches);
				Expect.Call(_commonMainShiftFilter.Filter(caches, effectiveRestriction)).Return(caches);
				Expect.Call(_mainShiftOptimizeActivitiesSpecificationShiftFilter.Filter(caches, _schedulingOptions.MainShiftOptimizeActivitySpecification)).Return(caches);
				Expect.Call(_shiftCategoryRestrictionShiftFilter.Filter(effectiveRestriction.ShiftCategory, caches, _finderResult)).Return(caches);
				Expect.Call(_disallowedShiftCategoriesShiftFilter.Filter(_schedulingOptions.NotAllowedShiftCategories, caches, _finderResult)).Return(caches);
				Expect.Call(_activityRestrictionsShiftFilter.Filter(_dateOnly, _groupPerson, caches, effectiveRestriction, _finderResult)).Return(caches);
				Expect.Call(_timeLimitsRestrictionShiftFilter.Filter(_dateOnly, _groupPerson, caches, effectiveRestriction, _finderResult))
				      .Return(caches);
				Expect.Call(_workTimeLimitationShiftFilter.Filter(caches, effectiveRestriction, _finderResult)).Return(caches);
				Expect.Call(_contractTimeShiftFilter.Filter(_dateOnly, new List<IScheduleMatrixPro> {_matrix}, caches,
				                                            _schedulingOptions, _finderResult)).Return(caches);
				//Expect.Call(_businessRulesShiftFilter.Filter(_groupPerson, caches, _dateOnly, _finderResult)).Return(caches);
				Expect.Call(_notOverWritableActivitiesShiftFilter.Filter(_dateOnly, _groupPerson, caches, _finderResult)).Return(caches);
				Expect.Call(_personalShiftsShiftFilter.Filter(_dateOnly, _groupPerson, caches, _finderResult)).Return(caches);
				Expect.Call(_shiftLengthDecider.FilterList(caches, _workShiftMinMaxCalculator, _matrix, _schedulingOptions))
				      .Return(caches);
				Expect.Call(_commonActivityFilter.Filter(caches, _schedulingOptions, null)).Return(caches);
			}
			using (_mocks.Playback())
			{
				var retShift = _target.Filter(_dateOnly, _teamBlockInfo, effectiveRestriction, _schedulingOptions, _finderResult);
				Assert.IsNotNull(retShift);
			}
		}
		
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldReturnNullIfShiftListIsNull()
		{
			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(), new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			_schedulingOptions.MainShiftOptimizeActivitySpecification = new All<IMainShift>();
			var schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var caches = getCashes();
			using (_mocks.Record())
			{
				Expect.Call(_matrix.SchedulePeriod).Return(schedulePeriod);
				Expect.Call(schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(_dateOnly, _dateOnly));
				Expect.Call(_effectiveRestrictionShiftFilter.Filter(_schedulingOptions, effectiveRestriction, _finderResult))
				      .Return(true);
				Expect.Call(_shiftProjectionCachesFromAdjustedRuleSetBagShiftFilter.Filter(_dateOnly, _groupPerson, false)).Return(caches);
				Expect.Call(_commonMainShiftFilter.Filter(caches, effectiveRestriction)).Return(caches);
				Expect.Call(_mainShiftOptimizeActivitiesSpecificationShiftFilter.Filter(caches, _schedulingOptions.MainShiftOptimizeActivitySpecification)).Return(caches);
				Expect.Call(_shiftCategoryRestrictionShiftFilter.Filter(effectiveRestriction.ShiftCategory, caches, _finderResult)).Return(caches);
				Expect.Call(_disallowedShiftCategoriesShiftFilter.Filter(_schedulingOptions.NotAllowedShiftCategories, caches, _finderResult)).Return(caches);
				Expect.Call(_activityRestrictionsShiftFilter.Filter(_dateOnly, _groupPerson, caches, effectiveRestriction, _finderResult)).Return(caches);
				Expect.Call(_timeLimitsRestrictionShiftFilter.Filter(_dateOnly, _groupPerson, caches, effectiveRestriction, _finderResult))
				      .Return(caches);
				Expect.Call(_workTimeLimitationShiftFilter.Filter(caches, effectiveRestriction, _finderResult)).Return(caches);
				Expect.Call(_contractTimeShiftFilter.Filter(_dateOnly, new List<IScheduleMatrixPro> {_matrix}, caches,
				                                            _schedulingOptions, _finderResult)).Return(caches);
				Expect.Call(_businessRulesShiftFilter.Filter(_groupPerson, caches, _dateOnly, _finderResult)).Return(caches);
				Expect.Call(_notOverWritableActivitiesShiftFilter.Filter(_dateOnly, _groupPerson, caches, _finderResult)).Return(caches);
				Expect.Call(_personalShiftsShiftFilter.Filter(_dateOnly, _groupPerson, caches, _finderResult)).Return(caches);
				Expect.Call(_shiftLengthDecider.FilterList(caches, _workShiftMinMaxCalculator, _matrix, _schedulingOptions))
				      .Return(null);

			}
			using (_mocks.Playback())
			{
				var retShift = _target.Filter(_dateOnly, _teamBlockInfo, effectiveRestriction, _schedulingOptions, _finderResult);
				Assert.IsNull(retShift);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldReturnNullFilteredWorkShiftsListIsEmpty()
		{
			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(), new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			_schedulingOptions.MainShiftOptimizeActivitySpecification = new All<IMainShift>();
			var schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var caches = getCashes();
			using (_mocks.Record())
			{
				Expect.Call(_matrix.SchedulePeriod).Return(schedulePeriod);
				Expect.Call(schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(_dateOnly, _dateOnly));
				Expect.Call(_effectiveRestrictionShiftFilter.Filter(_schedulingOptions, effectiveRestriction, _finderResult))
				      .Return(true);
				Expect.Call(_shiftProjectionCachesFromAdjustedRuleSetBagShiftFilter.Filter(_dateOnly, _groupPerson, false)).Return(caches);
				Expect.Call(_commonMainShiftFilter.Filter(caches, effectiveRestriction)).Return(caches);
				Expect.Call(_mainShiftOptimizeActivitiesSpecificationShiftFilter.Filter(caches, _schedulingOptions.MainShiftOptimizeActivitySpecification)).Return(caches);
				Expect.Call(_shiftCategoryRestrictionShiftFilter.Filter(effectiveRestriction.ShiftCategory, caches, _finderResult)).Return(caches);
				Expect.Call(_disallowedShiftCategoriesShiftFilter.Filter(_schedulingOptions.NotAllowedShiftCategories, caches, _finderResult)).Return(caches);
				Expect.Call(_activityRestrictionsShiftFilter.Filter(_dateOnly, _groupPerson, caches, effectiveRestriction, _finderResult)).Return(caches);
				Expect.Call(_timeLimitsRestrictionShiftFilter.Filter(_dateOnly, _groupPerson, caches, effectiveRestriction, _finderResult))
				      .Return(caches);
				Expect.Call(_workTimeLimitationShiftFilter.Filter(caches, effectiveRestriction, _finderResult)).Return(caches);
				Expect.Call(_contractTimeShiftFilter.Filter(_dateOnly, new List<IScheduleMatrixPro> {_matrix}, caches,
				                                            _schedulingOptions, _finderResult)).Return(caches);
				//Expect.Call(_businessRulesShiftFilter.Filter(_groupPerson, caches, _dateOnly, _finderResult)).Return(caches);
				Expect.Call(_notOverWritableActivitiesShiftFilter.Filter(_dateOnly, _groupPerson, caches, _finderResult)).Return(caches);
				Expect.Call(_personalShiftsShiftFilter.Filter(_dateOnly, _groupPerson, caches, _finderResult)).Return(caches);
				Expect.Call(_shiftLengthDecider.FilterList(caches, _workShiftMinMaxCalculator, _matrix, _schedulingOptions))
					  .Return(new List<IShiftProjectionCache>());
				Expect.Call(_commonActivityFilter.Filter(caches, _schedulingOptions, null)).Return(caches);
			}
			using (_mocks.Playback())
			{
				var retShift = _target.Filter(_dateOnly, _teamBlockInfo, effectiveRestriction, _schedulingOptions, _finderResult);
				Assert.IsNull(retShift);
			}
		}

		[Test]
		public void ShouldReturnNullIfEffectiveRestrictionShiftFilterSaysFalse()
		{
			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(), new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			_schedulingOptions.MainShiftOptimizeActivitySpecification = new All<IMainShift>();
			var schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			using (_mocks.Record())
			{
				Expect.Call(_matrix.SchedulePeriod).Return(schedulePeriod);
				Expect.Call(schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(_dateOnly, _dateOnly));
				Expect.Call(_effectiveRestrictionShiftFilter.Filter(_schedulingOptions, effectiveRestriction, _finderResult))
				      .Return(false);
			}
			using (_mocks.Playback())
			{
				var retShift = _target.Filter(_dateOnly, _teamBlockInfo, effectiveRestriction, _schedulingOptions, _finderResult);
				Assert.IsNull(retShift);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldReturnGetShiftCategoryFromSchedulingOptions()
		{
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("cat");
			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
			                                                                      new EndTimeLimitation(),
			                                                                      new WorkTimeLimitation(), null, null, null,
			                                                                      new List<IActivityRestriction>());
			IEffectiveRestriction effectiveRestrictionWithShiftCategory = new EffectiveRestriction(new StartTimeLimitation(),
			                                                                                       new EndTimeLimitation(),
			                                                                                       new WorkTimeLimitation(), null,
			                                                                                       null, null,
			                                                                                       new List<IActivityRestriction>
				                                                                                       ())
				{
					ShiftCategory =
						shiftCategory
				};
			_schedulingOptions.MainShiftOptimizeActivitySpecification = new All<IMainShift>();
			_schedulingOptions.ShiftCategory = shiftCategory;
			var schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var caches = getCashes();
			using (_mocks.Record())
			{
				Expect.Call(_matrix.SchedulePeriod).Return(schedulePeriod);
				Expect.Call(schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(_dateOnly, _dateOnly));
				Expect.Call(_effectiveRestrictionShiftFilter.Filter(_schedulingOptions, effectiveRestriction, _finderResult))
					  .Return(true);
				Expect.Call(_shiftProjectionCachesFromAdjustedRuleSetBagShiftFilter.Filter(_dateOnly, _groupPerson, false)).Return(caches);
				Expect.Call(_commonMainShiftFilter.Filter(caches, effectiveRestrictionWithShiftCategory)).Return(caches);
				Expect.Call(_mainShiftOptimizeActivitiesSpecificationShiftFilter.Filter(caches, _schedulingOptions.MainShiftOptimizeActivitySpecification)).Return(caches);
				Expect.Call(_shiftCategoryRestrictionShiftFilter.Filter(effectiveRestrictionWithShiftCategory.ShiftCategory, caches, _finderResult)).Return(caches);
				Expect.Call(_disallowedShiftCategoriesShiftFilter.Filter(_schedulingOptions.NotAllowedShiftCategories, caches, _finderResult)).Return(caches);
				Expect.Call(_activityRestrictionsShiftFilter.Filter(_dateOnly, _groupPerson, caches, effectiveRestrictionWithShiftCategory, _finderResult)).Return(caches);
				Expect.Call(_timeLimitsRestrictionShiftFilter.Filter(_dateOnly, _groupPerson, caches, effectiveRestrictionWithShiftCategory, _finderResult))
					  .Return(caches);
				Expect.Call(_workTimeLimitationShiftFilter.Filter(caches, effectiveRestrictionWithShiftCategory, _finderResult)).Return(caches);
				Expect.Call(_contractTimeShiftFilter.Filter(_dateOnly, new List<IScheduleMatrixPro> { _matrix }, caches,
															_schedulingOptions, _finderResult)).Return(caches);
				//Expect.Call(_businessRulesShiftFilter.Filter(_groupPerson, caches, _dateOnly, _finderResult)).Return(caches);
				Expect.Call(_notOverWritableActivitiesShiftFilter.Filter(_dateOnly, _groupPerson, caches, _finderResult)).Return(caches);
				Expect.Call(_personalShiftsShiftFilter.Filter(_dateOnly, _groupPerson, caches, _finderResult)).Return(caches);
				Expect.Call(_commonActivityFilter.Filter(caches, _schedulingOptions, null)).Return(caches);
				Expect.Call(_shiftLengthDecider.FilterList(caches, _workShiftMinMaxCalculator, _matrix, _schedulingOptions))
					  .Return(new List<IShiftProjectionCache>());
			}
			using (_mocks.Playback())
			{
				_target.Filter(_dateOnly, _teamBlockInfo, effectiveRestriction, _schedulingOptions, _finderResult);
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
