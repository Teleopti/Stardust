using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{

    [TestFixture]
    public class BestBlockShiftCategoryFinderTest
    {
        private IBestBlockShiftCategoryFinder _target;
        private IBlockSchedulingWorkShiftFinderService _finderService;
        private MockRepository _mocks;
        private IShiftCategory _shiftCategory1;
        private IShiftCategory _shiftCategory2;
        private DateOnly _dateOnly1;
        private DateOnly _dateOnly2;
        private IList<DateOnly> _dates;
        private IPerson _person;
        private IActivity _activity;
        private IWorkShift _workShift1;
        private IWorkShift _workShift2;
        private IWorkShift _workShift3;
        private IShiftCategory _category;
        private IPersonSkillPeriodsDataHolderManager _personSkillPeriodsDataHolderManager;
        private IShiftProjectionCacheManager _shiftProjectionCacheManager;
        private ISchedulingOptions _options;
        private IEffectiveRestriction _effectiveRestriction;
        private IShiftProjectionCacheFilter _shiftProjectionCacheFilter;
        private ISchedulingResultStateHolder _stateHolder;
        private IVirtualSchedulePeriod _schedulePeriod;
        private IPermissionInformation _permissionInformation;
    	private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
    	private IGroupShiftCategoryFairnessCreator _groupShiftCategoryFairnessCreator;
    	private ShiftCategoryFairnessFactors _schCategoryFairnessFactors;
        private List<IShiftCategory> _shiftCategories;
    	private ISchedulingOptions _schedulingOptions;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _finderService = _mocks.StrictMock<IBlockSchedulingWorkShiftFinderService>();
        	_effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
            _shiftCategory1 = new ShiftCategory("cat1");
            _shiftCategory2 = new ShiftCategory("cat2");
             _shiftCategories = new List<IShiftCategory> { _shiftCategory1, _shiftCategory2 };
            
    		_groupShiftCategoryFairnessCreator = _mocks.StrictMock<IGroupShiftCategoryFairnessCreator>();
            
            _dateOnly1 = new DateOnly(2009, 2, 2);
            _dateOnly2 = new DateOnly(2009, 2, 3);
            _dates = new List<DateOnly> { _dateOnly1, _dateOnly2 };
            _person = _mocks.StrictMock<IPerson>();
            _schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
            _permissionInformation = new PermissionInformation(_person);
            _permissionInformation.SetDefaultTimeZone(
                new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")));

            _personSkillPeriodsDataHolderManager = _mocks.StrictMock<IPersonSkillPeriodsDataHolderManager>();
            _shiftProjectionCacheManager = _mocks.StrictMock<IShiftProjectionCacheManager>();
            _options = new SchedulingOptions();
    	    _options.WorkShiftLengthHintOption = WorkShiftLengthHintOption.AverageWorkTime; // 4
    	    _options.UseMaximumPersons = true;
    	    _options.UseMinimumPersons = true;
            _shiftProjectionCacheFilter = _mocks.StrictMock<IShiftProjectionCacheFilter>();
            _stateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            
            _effectiveRestriction = new EffectiveRestriction(
               new StartTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0)),
               new EndTimeLimitation(new TimeSpan(15, 0, 0), new TimeSpan(18, 0, 0)),
               new WorkTimeLimitation(new TimeSpan(5, 0, 0), new TimeSpan(8, 0, 0)),
               null, null, null, new List<IActivityRestriction>());

    		_schCategoryFairnessFactors = new ShiftCategoryFairnessFactors(new Dictionary<IShiftCategory, double>(), 0);
			_schedulingOptions = new SchedulingOptions();

            _target = new BestBlockShiftCategoryFinder(_shiftProjectionCacheManager,_shiftProjectionCacheFilter,_personSkillPeriodsDataHolderManager,
                _stateHolder,_finderService, _effectiveRestrictionCreator, _groupShiftCategoryFairnessCreator,);

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyFindACategory()
        {
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            var personPeriod = _mocks.StrictMock<IPersonPeriod>();
            var ruleSetBag = _mocks.StrictMock<IRuleSetBag>();
            IFairnessValueResult fairnessValue = new FairnessValueResult {FairnessPoints = 5, TotalNumberOfShifts = 3};
        	var cashes = getCashes();
            IBlockFinderResult result = new BlockFinderResult(null, _dates,
                                                              new Dictionary<string, IWorkShiftFinderResult>());
            IScheduleDateTimePeriod scheduleDateTimePeriod = _mocks.StrictMock<IScheduleDateTimePeriod>();
            DateTime startDateTime = new DateTime(2009, 2, 1, 11, 0, 0, 0, DateTimeKind.Utc);
            DateTimePeriod dateTimePeriod = new DateTimePeriod(startDateTime.AddDays(-1), startDateTime.AddDays(1));
			var persons = new List<IPerson> { _person };
            IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> dataHolderDic = new Dictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>>();
            dataHolderDic.Add(_activity, new Dictionary<DateTime, ISkillStaffPeriodDataHolder>());
            using (_mocks.Record())
            {
                Expect.Call(scheduleDictionary.Period).Return(scheduleDateTimePeriod).Repeat.Times(1);
                Expect.Call(scheduleDateTimePeriod.VisiblePeriod).Return(dateTimePeriod).Repeat.Times(1);

                Expect.Call(_stateHolder.ShiftCategories).Return(_shiftCategories);
                Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(_dateOnly1, _permissionInformation.DefaultTimeZone(), ruleSetBag, false)).Return(getCashes()).Repeat.Twice();
                Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(_dateOnly2, _permissionInformation.DefaultTimeZone(), ruleSetBag, false)).Return(getCashes()).Repeat.Twice();

                Expect.Call(_personSkillPeriodsDataHolderManager.GetPersonSkillPeriodsDataHolderDictionary(_dateOnly1,
																										   _schedulePeriod)).
                    Return(dataHolderDic).Repeat.Twice();
                Expect.Call(_personSkillPeriodsDataHolderManager.GetPersonSkillPeriodsDataHolderDictionary(_dateOnly2,
																										   _schedulePeriod)).
                    Return(dataHolderDic).Repeat.Twice();
                Expect.Call(_person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(_person.VirtualSchedulePeriod(_dateOnly1)).Return(_schedulePeriod).Repeat.Twice();
                Expect.Call(_person.VirtualSchedulePeriod(_dateOnly2)).Return(_schedulePeriod).Repeat.Twice();
                Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
                Expect.Call(_person.Period(_dateOnly1)).Return(personPeriod).Repeat.AtLeastOnce();
                Expect.Call(_person.Period(_dateOnly2)).Return(personPeriod).Repeat.AtLeastOnce();
                Expect.Call(_schedulePeriod.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(personPeriod.RuleSetBag).Return(ruleSetBag).Repeat.AtLeastOnce();
                Expect.Call(_schedulePeriod.AverageWorkTimePerDay).Return(new TimeSpan(8, 0, 0)).Repeat.Any();

                Expect.Call(_stateHolder.Schedules).Return(scheduleDictionary).Repeat.Any();

				Expect.Call(_finderService.BestShiftValue(_dateOnly1, cashes, null, fairnessValue, fairnessValue, 5, TimeSpan.FromHours(48), false, _schCategoryFairnessFactors, 4, true, true, _schedulingOptions)).Return(10).IgnoreArguments();
				Expect.Call(_finderService.BestShiftValue(_dateOnly2, cashes, null, fairnessValue, fairnessValue, 5, TimeSpan.FromHours(48), false, _schCategoryFairnessFactors, 4, true, true, _schedulingOptions)).Return(10).IgnoreArguments();

				Expect.Call(_finderService.BestShiftValue(_dateOnly1, cashes, null, fairnessValue, fairnessValue, 5, TimeSpan.FromHours(48), false, _schCategoryFairnessFactors, 4, true, true, _schedulingOptions)).Return(15).IgnoreArguments();
				Expect.Call(_finderService.BestShiftValue(_dateOnly2, cashes, null, fairnessValue, fairnessValue, 5, TimeSpan.FromHours(48), false, _schCategoryFairnessFactors, 4, true, true, _schedulingOptions)).Return(10).IgnoreArguments();

				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(persons, _dates[0], _options, scheduleDictionary)).Return(_effectiveRestriction).Repeat.AtLeastOnce();
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(persons, _dates[1], _options, scheduleDictionary)).Return(_effectiveRestriction).Repeat.AtLeastOnce();
                
                Expect.Call(_shiftProjectionCacheFilter.FilterOnMainShiftOptimizeActivitiesSpecification(cashes)).IgnoreArguments().Return
                    (cashes).Repeat.AtLeastOnce();
                Expect.Call(_shiftProjectionCacheFilter.FilterOnShiftCategory(null, cashes, null)).Return(cashes)
                    .IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(_shiftProjectionCacheFilter.FilterOnRestrictionAndNotAllowedShiftCategories(new DateOnly(),null, cashes, null, null, null)).Return(
                                                                                                                  cashes)
                    .IgnoreArguments().Repeat.AtLeastOnce();

				Expect.Call(_shiftProjectionCacheFilter.FilterOnBusinessRules(persons, scheduleDictionary, _dateOnly1, cashes, null)).
					IgnoreArguments().Repeat.AtLeastOnce().Return(cashes);

				Expect.Call(_shiftProjectionCacheFilter.FilterOnPersonalShifts(persons, scheduleDictionary, _dateOnly1, cashes, null)).IgnoreArguments
					().Repeat.AtLeastOnce().Return(cashes);

            	Expect.Call(_person.WorkflowControlSet).Return(null).Repeat.AtLeastOnce();
            }
            using (_mocks.Playback())
            {
				var ret = _target.BestShiftCategoryForDays(result, _person, fairnessValue, fairnessValue, _options);
                Assert.AreEqual(_shiftCategory2, ret.BestShiftCategory);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyFindACategoryWhenDayReturnsMinValue()
        {
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            var ruleSetBag = _mocks.StrictMock<IRuleSetBag>();
            var personPeriod = _mocks.StrictMock<IPersonPeriod>();
            IScheduleDateTimePeriod scheduleDateTimePeriod = _mocks.StrictMock<IScheduleDateTimePeriod>();
            DateTime startDateTime = new DateTime(2009, 2, 1, 11, 0, 0, 0, DateTimeKind.Utc);
            DateTimePeriod dateTimePeriod = new DateTimePeriod(startDateTime.AddDays(-1), startDateTime.AddDays(1));

            IFairnessValueResult fairnessValue = new FairnessValueResult {FairnessPoints = 5, TotalNumberOfShifts = 3};
        	var cashes = getCashes();
            IBlockFinderResult result = new BlockFinderResult(null, _dates,
                                                              new Dictionary<string, IWorkShiftFinderResult>());
        	var persons = new List<IPerson> {_person};
            IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> dataHolderDic = new Dictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>>();
            dataHolderDic.Add(_activity, new Dictionary<DateTime, ISkillStaffPeriodDataHolder>());

            using (_mocks.Record())
			{
                Expect.Call(_stateHolder.ShiftCategories).Return(_shiftCategories);
				Expect.Call(_stateHolder.Schedules).Return(scheduleDictionary).Repeat.Any();
				Expect.Call(_person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(_person.VirtualSchedulePeriod(_dateOnly1)).Return(_schedulePeriod).IgnoreArguments().Repeat.Any();
                Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
			    Expect.Call(_person.Period(_dateOnly1)).Return(personPeriod).Repeat.AtLeastOnce();
                Expect.Call(_person.Period(_dateOnly2)).Return(personPeriod).Repeat.AtLeastOnce();
			    Expect.Call(_schedulePeriod.Person).Return(_person).Repeat.AtLeastOnce();

                Expect.Call(scheduleDictionary.Period).Return(scheduleDateTimePeriod).Repeat.Times(1);
                Expect.Call(scheduleDateTimePeriod.VisiblePeriod).Return(dateTimePeriod).Repeat.Times(1);
                
                Expect.Call(personPeriod.RuleSetBag).Return(ruleSetBag).Repeat.AtLeastOnce();

                Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(_dateOnly1, _permissionInformation.DefaultTimeZone(), ruleSetBag, false)).Return(cashes).Repeat.Twice();
                Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(_dateOnly2, _permissionInformation.DefaultTimeZone(), ruleSetBag, false)).Return(cashes).Repeat.Twice();

                Expect.Call(_personSkillPeriodsDataHolderManager.GetPersonSkillPeriodsDataHolderDictionary(_dateOnly1,
                                                                                                           _schedulePeriod)).
                    Return(dataHolderDic).Repeat.Twice();
                Expect.Call(_personSkillPeriodsDataHolderManager.GetPersonSkillPeriodsDataHolderDictionary(_dateOnly2,
                                                                                                           _schedulePeriod)).
                    Return(dataHolderDic).Repeat.Twice();

                Expect.Call(_schedulePeriod.AverageWorkTimePerDay).Return(new TimeSpan(8, 0, 0)).Repeat.Any();
			    Expect.Call(_finderService.BestShiftValue(_dateOnly1, cashes, null, fairnessValue, fairnessValue, 5,
														  TimeSpan.FromHours(8), false, null, 4, true, true, _schedulingOptions)).IgnoreArguments().
			        Return(10);
			    Expect.Call(_finderService.BestShiftValue(_dateOnly2, cashes, null, fairnessValue, fairnessValue, 5,
														  TimeSpan.FromHours(8), false, null, 4, true, true, _schedulingOptions)).IgnoreArguments().
			        Return(double.MinValue);
                Expect.Call(_finderService.BestShiftValue(_dateOnly1, cashes, null, fairnessValue, fairnessValue, 5,
														  TimeSpan.FromHours(8), false, null, 4, true, true, _schedulingOptions)).IgnoreArguments().
                    Return(10);
                Expect.Call(_finderService.BestShiftValue(_dateOnly2, cashes, null, fairnessValue, fairnessValue, 5,
														  TimeSpan.FromHours(8), false, null, 4, true, true, _schedulingOptions)).IgnoreArguments().
                    Return(double.MinValue);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(persons, _dates[0], _options, scheduleDictionary)).Return(_effectiveRestriction).Repeat.AtLeastOnce();
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(persons, _dates[1], _options, scheduleDictionary)).Return(_effectiveRestriction).Repeat.AtLeastOnce();
                
                Expect.Call(_shiftProjectionCacheFilter.FilterOnShiftCategory(null, cashes, null)).Return(cashes)
                    .IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(_shiftProjectionCacheFilter.FilterOnMainShiftOptimizeActivitiesSpecification(cashes)).
                    IgnoreArguments().Return(cashes).Repeat.AtLeastOnce();
                Expect.Call(_shiftProjectionCacheFilter.FilterOnRestrictionAndNotAllowedShiftCategories(new DateOnly(), null, cashes, null, null, null)).Return(
                                                                                                                  cashes)
                    .IgnoreArguments().Repeat.AtLeastOnce();

				
				Expect.Call(_shiftProjectionCacheFilter.FilterOnBusinessRules(persons,scheduleDictionary,_dateOnly1, cashes,  null)).
					IgnoreArguments().Repeat.AtLeastOnce().Return(cashes);

				Expect.Call(_shiftProjectionCacheFilter.FilterOnPersonalShifts(persons, scheduleDictionary, _dateOnly1, cashes, null)).IgnoreArguments
					().Repeat.AtLeastOnce().Return(cashes);

				Expect.Call(_person.WorkflowControlSet).Return(null).Repeat.AtLeastOnce();

            }
            using (_mocks.Playback())
            {
                var ret = _target.BestShiftCategoryForDays(result, _person, fairnessValue, fairnessValue, _options);
                Assert.IsNull( ret.BestShiftCategory);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [Test]
		public void ShouldFailWhenShiftListIsNull()
		{
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var personPeriod = _mocks.StrictMock<IPersonPeriod>();
			var ruleSetBag = _mocks.StrictMock<IRuleSetBag>();
            var persons = new List<IPerson> { _person };
			IBlockFinderResult result = new BlockFinderResult(null, new List<DateOnly>{_dateOnly1},new Dictionary<string, IWorkShiftFinderResult>());
            IScheduleDateTimePeriod scheduleDateTimePeriod = _mocks.StrictMock<IScheduleDateTimePeriod>();
            DateTime startDateTime = new DateTime(2009, 2, 1, 11, 0, 0, 0, DateTimeKind.Utc);
            DateTimePeriod dateTimePeriod = new DateTimePeriod(startDateTime.AddDays(-1), startDateTime.AddDays(1));
			
			IFairnessValueResult fairnessValue = new FairnessValueResult { FairnessPoints = 5, TotalNumberOfShifts = 3 };
            Expect.Call(_stateHolder.ShiftCategories).Return(_shiftCategories);
			Expect.Call(_stateHolder.Schedules).Return(scheduleDictionary).Repeat.Times(2);
            Expect.Call(scheduleDictionary.Period).Return(scheduleDateTimePeriod).Repeat.Times(1);
            Expect.Call(scheduleDateTimePeriod.VisiblePeriod).Return(dateTimePeriod).Repeat.Times(1);


			Expect.Call(_person.PermissionInformation).Return(_permissionInformation);
			Expect.Call(_person.VirtualSchedulePeriod(_dateOnly1)).Return(_schedulePeriod).IgnoreArguments().Repeat.Twice();
			Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.Twice();
            Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(persons, _dateOnly1, _options, scheduleDictionary)).Return(_effectiveRestriction).Repeat.AtLeastOnce();

		    Expect.Call(_schedulePeriod.Person).Return(_person).Repeat.AtLeastOnce();
		    Expect.Call(_person.Period(_dateOnly1)).Return(personPeriod).Repeat.AtLeastOnce();
			Expect.Call(personPeriod.RuleSetBag).Return(ruleSetBag).Repeat.Twice();
			Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(_dateOnly1, _permissionInformation.DefaultTimeZone(), ruleSetBag, false)).Return(null).Repeat.Twice();

			_mocks.ReplayAll();
			var ret = _target.BestShiftCategoryForDays(result, _person, fairnessValue, fairnessValue, _options);
			Assert.IsNull(ret.BestShiftCategory);
			_mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldFailIfAllMembersInGroupPersonIsScheduled()
		{
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var groupPerson = _mocks.StrictMock<IGroupPerson>();
			var members = new List<IPerson> {_person};
			var range = _mocks.StrictMock<IScheduleRange>();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();

            IScheduleDateTimePeriod scheduleDateTimePeriod = _mocks.StrictMock<IScheduleDateTimePeriod>();
            DateTime startDateTime = new DateTime(2009, 2, 1, 11, 0, 0, 0, DateTimeKind.Utc); 
            DateTimePeriod dateTimePeriod = new DateTimePeriod(startDateTime.AddDays(-1), startDateTime.AddDays(1));
			IBlockFinderResult result = new BlockFinderResult(null, new List<DateOnly> { _dateOnly1 }, new Dictionary<string, IWorkShiftFinderResult>());

			IFairnessValueResult fairnessValue = new FairnessValueResult { FairnessPoints = 5, TotalNumberOfShifts = 3 };
            Expect.Call(_stateHolder.ShiftCategories).Return(_shiftCategories);
			Expect.Call(_stateHolder.Schedules).Return(scheduleDictionary).Repeat.Times(2);
            Expect.Call(scheduleDictionary.Period).Return(scheduleDateTimePeriod).Repeat.Times(1);
            Expect.Call(scheduleDateTimePeriod.VisiblePeriod).Return(dateTimePeriod).Repeat.Times(1);
            
            Expect.Call(groupPerson.PermissionInformation).Return(_permissionInformation);
			Expect.Call(groupPerson.VirtualSchedulePeriod(_dateOnly1)).Return(_schedulePeriod).IgnoreArguments();
			Expect.Call(_schedulePeriod.IsValid).Return(true);
			Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(members));

			Expect.Call(scheduleDictionary[_person]).Return(range);
			Expect.Call(range.ScheduledDay(_dateOnly1)).Return(scheduleDay);
			Expect.Call(scheduleDay.IsScheduled()).Return(true);

			_mocks.ReplayAll();
			var ret = _target.BestShiftCategoryForDays(result, groupPerson, fairnessValue, fairnessValue, _options);
			Assert.IsNull(ret.BestShiftCategory);
			Assert.That(ret.FailureCause, Is.EqualTo(FailureCause.AlreadyAssigned));
			_mocks.VerifyAll();
		}

        [Test]
        public void ShouldNotLoopOutsideVisiblePeriodPlusMinusTenDays()
        {
            IBlockFinderResult blockFinderResult = _mocks.StrictMock<IBlockFinderResult>();
            IFairnessValueResult fairnessValueResult = _mocks.StrictMock<IFairnessValueResult>();
            DateOnlyPeriod period = new DateOnlyPeriod(2001, 02, 01, 2001, 02, 28);
            IScheduleDictionary scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            DateOnly dateOnly = new DateOnly(2010, 1, 1);
            DateTime startDateTime = new DateTime(2010, 1, 1, 11, 0, 0, 0, DateTimeKind.Utc); 
            IVirtualSchedulePeriod virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
            IScheduleDateTimePeriod scheduleDateTimePeriod = _mocks.StrictMock<IScheduleDateTimePeriod>();
            DateTimePeriod dateTimePeriod = new DateTimePeriod(startDateTime.AddDays(-1), startDateTime.AddDays(1));
            IList<IShiftCategory> shiftCategoryList = new List<IShiftCategory>{_shiftCategory1};

            using (_mocks.Record())
            {
                Expect.Call(blockFinderResult.BlockDays).Return(period.DayCollection());
                Expect.Call(_stateHolder.Schedules).Return(scheduleDictionary).Repeat.AtLeastOnce();
                Expect.Call(_stateHolder.ShiftCategories).Return(shiftCategoryList);
                Expect.Call(_person.PermissionInformation).Return(_permissionInformation);
                Expect.Call(scheduleDictionary.Period).Return(scheduleDateTimePeriod).Repeat.Times(1);
                Expect.Call(scheduleDateTimePeriod.VisiblePeriod).Return(dateTimePeriod).Repeat.Times(1);
                Expect.Call(_person.VirtualSchedulePeriod(dateOnly)).Return(virtualSchedulePeriod).IgnoreArguments().Repeat.Times(28);
                
            }
            using (_mocks.Playback())
            {
                _target.BestShiftCategoryForDays(blockFinderResult, _person, fairnessValueResult, fairnessValueResult, _options);
            }
        }

        private IList<IShiftProjectionCache> getCashes()
        {
            var tmpList = getWorkShifts();
        	return tmpList.Select(shift => new ShiftProjectionCache(shift)).Cast<IShiftProjectionCache>().ToList();
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
