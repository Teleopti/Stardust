using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{

    [TestFixture]
    public class BestBlockShiftCategoryFinderTest
    {
        private IBestBlockShiftCategoryFinder _target;
        private MockRepository _mocks;
        private DateOnly _dateOnly1;
        private DateOnly _dateOnly2;
        private IList<DateOnly> _dates;
        private IPerson _person;
        private IActivity _activity;
        private IWorkShift _workShift1;
        private IWorkShift _workShift2;
        private IWorkShift _workShift3;
        private IShiftCategory _category;
        private IShiftProjectionCacheManager _shiftProjectionCacheManager;
        private ISchedulingOptions _options;
        private IEffectiveRestriction _effectiveRestriction;
        private ISchedulingResultStateHolder _stateHolder;
        private IVirtualSchedulePeriod _schedulePeriod;
        private IPermissionInformation _permissionInformation;
    	private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
    	private ISchedulingOptions _schedulingOptions;
    	private IPossibleCombinationsOfStartEndCategoryRunner _possibleCombinationsOfStartEndCategoryRunner;
    	private IPossibleCombinationsOfStartEndCategoryCreator _possibleCombinationsOfStartEndCategoryCreator;
    	private IWorkShiftWorkTime _workShiftWorkTime;
    	private IShiftCategoryFairnessCalculator _fairnessCalculator;
		private IGroupShiftCategoryFairnessCreator _groupShiftCategoryFairnessCreator;
		private IGroupShiftLengthDecider _groupShiftLengthDecider;
    	

    	[SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
        	_effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
    		_activity = _mocks.DynamicMock<IActivity>();
    		_workShiftWorkTime = _mocks.DynamicMock<IWorkShiftWorkTime>();
            _dateOnly1 = new DateOnly(2009, 2, 2);
            _dateOnly2 = new DateOnly(2009, 2, 3);
            _dates = new List<DateOnly> { _dateOnly1, _dateOnly2 };
            _person = _mocks.StrictMock<IPerson>();
            _schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
            _permissionInformation = new PermissionInformation(_person);
            _permissionInformation.SetDefaultTimeZone(
                new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")));

            _shiftProjectionCacheManager = _mocks.StrictMock<IShiftProjectionCacheManager>();
            _options = new SchedulingOptions();
    	    _options.WorkShiftLengthHintOption = WorkShiftLengthHintOption.AverageWorkTime; // 4
    	    _options.UseMaximumPersons = true;
    	    _options.UseMinimumPersons = true;
            _stateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            
            _effectiveRestriction = new EffectiveRestriction(
               new StartTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0)),
               new EndTimeLimitation(new TimeSpan(15, 0, 0), new TimeSpan(18, 0, 0)),
               new WorkTimeLimitation(new TimeSpan(5, 0, 0), new TimeSpan(8, 0, 0)),
               null, null, null, new List<IActivityRestriction>());
			_schedulingOptions = new SchedulingOptions();
    		_possibleCombinationsOfStartEndCategoryRunner = _mocks.StrictMock<IPossibleCombinationsOfStartEndCategoryRunner>();
    		_possibleCombinationsOfStartEndCategoryCreator = _mocks.StrictMock<IPossibleCombinationsOfStartEndCategoryCreator>();
    		_fairnessCalculator = _mocks.StrictMock<IShiftCategoryFairnessCalculator>();
    		_groupShiftCategoryFairnessCreator = _mocks.StrictMock<IGroupShiftCategoryFairnessCreator>();
    		_groupShiftLengthDecider = _mocks.DynamicMock<IGroupShiftLengthDecider>();
    		_target = new BestBlockShiftCategoryFinder(_workShiftWorkTime, _shiftProjectionCacheManager, _stateHolder,
    		                                           _effectiveRestrictionCreator,
    		                                           _possibleCombinationsOfStartEndCategoryRunner,
    		                                           _possibleCombinationsOfStartEndCategoryCreator, 
													   _groupShiftCategoryFairnessCreator,
													   _fairnessCalculator, _groupShiftLengthDecider);
    		

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyFindACategory()
        {
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            var personPeriod = _mocks.StrictMock<IPersonPeriod>();
            var ruleSetBag = _mocks.StrictMock<IRuleSetBag>();
        	var gPerson = _mocks.DynamicMock<IGroupPerson>();

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

                Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(_dateOnly1, _permissionInformation.DefaultTimeZone(), ruleSetBag, false)).Return(getCashes()).Repeat.AtLeastOnce();
                Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(_dateOnly2, _permissionInformation.DefaultTimeZone(), ruleSetBag, false)).Return(getCashes()).Repeat.AtLeastOnce();

                Expect.Call(_person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(_person.VirtualSchedulePeriod(_dateOnly1)).Return(_schedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_person.VirtualSchedulePeriod(_dateOnly2)).Return(_schedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
                Expect.Call(_person.Period(_dateOnly1)).Return(personPeriod).Repeat.AtLeastOnce();
                Expect.Call(_person.Period(_dateOnly2)).Return(personPeriod).Repeat.AtLeastOnce();
                Expect.Call(_schedulePeriod.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(personPeriod.RuleSetBag).Return(ruleSetBag).Repeat.AtLeastOnce();
				Expect.Call(ruleSetBag.MinMaxWorkTime(_workShiftWorkTime, _dateOnly1, _effectiveRestriction)).Return
            		(new WorkTimeMinMax());
				Expect.Call(ruleSetBag.MinMaxWorkTime(_workShiftWorkTime, _dateOnly2, _effectiveRestriction)).Return
					(new WorkTimeMinMax());
            	Expect.Call(_possibleCombinationsOfStartEndCategoryCreator.FindCombinations(new WorkTimeMinMax(),
            	                                                                            _schedulingOptions)).Return(
            	                                                                            	new HashSet
            	                                                                            		<IPossibleStartEndCategory>{new PossibleStartEndCategory()}).IgnoreArguments().Repeat.AtLeastOnce();
            	Expect.Call(_person.WorkflowControlSet).Return(null).Repeat.Any();
            	Expect.Call(scheduleDictionary.FairnessPoints()).Return(null).Repeat.Any();
				Expect.Call(() =>  _possibleCombinationsOfStartEndCategoryRunner.RunTheList(new List<IPossibleStartEndCategory>(),
																					 getCashes(), _dateOnly1, gPerson,
																					 _schedulingOptions, false, null,null,null,persons, _effectiveRestriction)).IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(_schedulePeriod.AverageWorkTimePerDay).Return(new TimeSpan(8, 0, 0)).Repeat.Any();

                Expect.Call(_stateHolder.Schedules).Return(scheduleDictionary).Repeat.Any();
				
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(persons, _dates[0], _options, scheduleDictionary)).Return(_effectiveRestriction).Repeat.AtLeastOnce();
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(persons, _dates[1], _options, scheduleDictionary)).Return(_effectiveRestriction).Repeat.AtLeastOnce();

				//Expect.Call(_shiftProjectionCacheFilter.FilterOnMainShiftOptimizeActivitiesSpecification(getCashes(),
				//                                                                                         new Domain.
				//                                                                                            Specification.All
				//                                                                                            <IMainShift>())).
				//    IgnoreArguments().Return(getCashes());
            }
            using (_mocks.Playback())
            {
				 _target.BestShiftCategoryForDays(result, _person, _options, null);
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
			
        	var cashes = getCashes();
            IBlockFinderResult result = new BlockFinderResult(null, _dates,
                                                              new Dictionary<string, IWorkShiftFinderResult>());
        	var persons = new List<IPerson> {_person};
            IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> dataHolderDic = new Dictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>>();
            dataHolderDic.Add(_activity, new Dictionary<DateTime, ISkillStaffPeriodDataHolder>());

            using (_mocks.Record())
			{
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

                Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(_dateOnly1, _permissionInformation.DefaultTimeZone(), ruleSetBag, false)).Return(cashes).Repeat.AtLeastOnce();
                Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(_dateOnly2, _permissionInformation.DefaultTimeZone(), ruleSetBag, false)).Return(cashes).Repeat.AtLeastOnce();

				Expect.Call(ruleSetBag.MinMaxWorkTime(_workShiftWorkTime, _dateOnly1, _effectiveRestriction)).Return(new WorkTimeMinMax());
				Expect.Call(ruleSetBag.MinMaxWorkTime(_workShiftWorkTime, _dateOnly2, _effectiveRestriction)).Return(new WorkTimeMinMax());
				Expect.Call(_possibleCombinationsOfStartEndCategoryCreator.FindCombinations(new WorkTimeMinMax(),
																							_schedulingOptions)).Return(
																								new HashSet
																									<IPossibleStartEndCategory>()).IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(_person.WorkflowControlSet).Return(null).Repeat.Any();
				Expect.Call(scheduleDictionary.FairnessPoints()).Return(null).Repeat.Any();
				
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(persons, _dates[0], _options, scheduleDictionary)).Return(_effectiveRestriction).Repeat.AtLeastOnce();
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(persons, _dates[1], _options, scheduleDictionary)).Return(_effectiveRestriction).Repeat.AtLeastOnce();

				//Expect.Call(_shiftProjectionCacheFilter.FilterOnMainShiftOptimizeActivitiesSpecification(cashes,
				//                                                                                         new Domain.Specification.
				//                                                                                            All<IMainShift>())).
				//    IgnoreArguments().Return(getCashes());
			}
            using (_mocks.Playback())
            {
                var ret = _target.BestShiftCategoryForDays(result, _person, _options, null);
                Assert.IsNull( ret.BestPossible);
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

            Expect.Call(_stateHolder.Schedules).Return(scheduleDictionary).Repeat.Any();
            Expect.Call(scheduleDictionary.Period).Return(scheduleDateTimePeriod).Repeat.Times(1);
            Expect.Call(scheduleDateTimePeriod.VisiblePeriod).Return(dateTimePeriod).Repeat.Times(1);

			Expect.Call(_person.PermissionInformation).Return(_permissionInformation);
			Expect.Call(_person.VirtualSchedulePeriod(_dateOnly1)).Return(_schedulePeriod).IgnoreArguments();
			Expect.Call(_schedulePeriod.IsValid).Return(true);
            Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(persons, _dateOnly1, _options, scheduleDictionary)).Return(_effectiveRestriction).Repeat.AtLeastOnce();

		    Expect.Call(_schedulePeriod.Person).Return(_person).Repeat.AtLeastOnce();
		    Expect.Call(_person.Period(_dateOnly1)).Return(personPeriod).Repeat.AtLeastOnce();
        	Expect.Call(personPeriod.RuleSetBag).Return(ruleSetBag);
			Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(_dateOnly1, _permissionInformation.DefaultTimeZone(), ruleSetBag, false)).Return(null);

			_mocks.ReplayAll();
			var ret = _target.BestShiftCategoryForDays(result, _person, _options,null);
			Assert.IsNull(ret.BestPossible);
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
			var ret = _target.BestShiftCategoryForDays(result, groupPerson, _options, null);
			Assert.IsNull(ret.BestPossible);
			Assert.That(ret.FailureCause, Is.EqualTo(FailureCause.AlreadyAssigned));
			_mocks.VerifyAll();
		}

        [Test]
        public void ShouldNotLoopOutsideVisiblePeriodPlusMinusTenDays()
        {
            IBlockFinderResult blockFinderResult = _mocks.StrictMock<IBlockFinderResult>();
            DateOnlyPeriod period = new DateOnlyPeriod(2001, 02, 01, 2001, 02, 28);
            IScheduleDictionary scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            DateOnly dateOnly = new DateOnly(2010, 1, 1);
            DateTime startDateTime = new DateTime(2010, 1, 1, 11, 0, 0, 0, DateTimeKind.Utc); 
            IVirtualSchedulePeriod virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
            IScheduleDateTimePeriod scheduleDateTimePeriod = _mocks.StrictMock<IScheduleDateTimePeriod>();
            DateTimePeriod dateTimePeriod = new DateTimePeriod(startDateTime.AddDays(-1), startDateTime.AddDays(1));
            
            using (_mocks.Record())
            {
                Expect.Call(blockFinderResult.BlockDays).Return(period.DayCollection());
                Expect.Call(_stateHolder.Schedules).Return(scheduleDictionary).Repeat.AtLeastOnce();
                Expect.Call(_person.PermissionInformation).Return(_permissionInformation);
                Expect.Call(scheduleDictionary.Period).Return(scheduleDateTimePeriod).Repeat.Times(1);
                Expect.Call(scheduleDateTimePeriod.VisiblePeriod).Return(dateTimePeriod).Repeat.Times(1);
                Expect.Call(_person.VirtualSchedulePeriod(dateOnly)).Return(virtualSchedulePeriod).IgnoreArguments().Repeat.Times(28);
                
            }
            using (_mocks.Playback())
            {
                _target.BestShiftCategoryForDays(blockFinderResult, _person,  _options, null);
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
