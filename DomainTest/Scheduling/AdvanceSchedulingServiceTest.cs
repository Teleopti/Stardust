using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class AdvanceSchedulingServiceTest
    {
        private MockRepository _mocks;
        private IAdvanceSchedulingService _target;
        private ISkillDayPeriodIntervalDataGenerator _skillDayPeriodIntervalDataGenerator;
        private IDynamicBlockFinder _dynamicBlockFinder;
        private IRestrictionAggregator _restrictionAggregator;
        private IWorkShiftFilterService _workShiftFilterService;
        private ITeamScheduling _teamScheduling;
        private ISchedulingOptions _schedulingOptions;
    	private IWorkShiftSelector _workShiftSelector;
        private IGroupPersonBuilderBasedOnContractTime _groupPersonBuilderBasedOnContractTime;
        private IScheduleMatrixPro _scheduleMatrixPro;
        private IScheduleDayPro _scheduleDayPro;
        private IScheduleDay _scheduleDay;
        private IGroupPerson _groupPerson;
        private IEffectiveRestriction _effectiveRestriction;
        private IPerson _person;
        private IShiftProjectionCache _scheduleProjectionCache;
        private IVirtualSchedulePeriod _virtualSchedulePeriod;
        private IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;


        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _schedulingOptions = _mocks.StrictMock<ISchedulingOptions>();
            _skillDayPeriodIntervalDataGenerator =  _mocks.StrictMock<ISkillDayPeriodIntervalDataGenerator>();
            _dynamicBlockFinder = _mocks.StrictMock<IDynamicBlockFinder>();
            _restrictionAggregator = _mocks.StrictMock<IRestrictionAggregator>();
            _workShiftFilterService = _mocks.StrictMock<IWorkShiftFilterService>();
            _teamScheduling = _mocks.StrictMock<ITeamScheduling>();
        	_workShiftSelector = _mocks.StrictMock<IWorkShiftSelector>();
            _groupPersonBuilderBasedOnContractTime = _mocks.StrictMock<IGroupPersonBuilderBasedOnContractTime>();
            _scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
            _scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDay = _mocks.StrictMock<IScheduleDay>();
            _effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
            _groupPerson = _mocks.StrictMock<IGroupPerson>();
            _person = _mocks.StrictMock<IPerson>();
            _virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
            _scheduleProjectionCache = _mocks.StrictMock<IShiftProjectionCache>();
            _groupPersonBuilderForOptimization = _mocks.StrictMock<IGroupPersonBuilderForOptimization>();
            _target = new AdvanceSchedulingService(_skillDayPeriodIntervalDataGenerator, 
                                                _dynamicBlockFinder,  
                                                _restrictionAggregator,
												_workShiftFilterService,
												_teamScheduling, 
												_schedulingOptions,
                                                _workShiftSelector, _groupPersonBuilderBasedOnContractTime,_groupPersonBuilderForOptimization);
        }

        [Test]
        public void ShouldVerifyExecution()
        {
            var scheduleDayProList = new List<IScheduleDayPro>{_scheduleDayPro };
            var scheduleDayProCollection = new ReadOnlyCollection<IScheduleDayPro>(scheduleDayProList);
            var dateOnly = new DateOnly();
            var dateOnlyList = new List<DateOnly> {dateOnly};
            var groupPersonList = new List<IGroupPerson> {_groupPerson};
            var matrixList = new List<IScheduleMatrixPro> {_scheduleMatrixPro};
            var shiftProjectionCacheList = new List<IShiftProjectionCache> {_scheduleProjectionCache};
            var dateOnlyPeriod = new DateOnlyPeriod(dateOnly,dateOnly.AddDays(1));
            var person = new Person();
            using(_mocks.Record())
            {
                ExpectCodeShouldVerifyExecution(groupPersonList, dateOnlyPeriod, shiftProjectionCacheList, matrixList, dateOnly, scheduleDayProCollection, dateOnlyList, person);
            }
            using(_mocks.Playback() )
            {
                Assert.That(_target.Execute(new Dictionary<string, IWorkShiftFinderResult>(),new List<IScheduleMatrixPro>(),new List<IScheduleMatrixPro>()), Is.True);
            }
           
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private void ExpectCodeShouldVerifyExecution(List<IGroupPerson> groupPersonList, DateOnlyPeriod dateOnlyPeriod,
                                                     List<IShiftProjectionCache> shiftProjectionCacheList, List<IScheduleMatrixPro> matrixList, DateOnly dateOnly,
                                                     ReadOnlyCollection<IScheduleDayPro> scheduleDayProCollection, List<DateOnly> dateOnlyList,
                                                     Person person)
        {
            //first sub method
            Expect.Call(_scheduleMatrixPro.EffectivePeriodDays).Return(scheduleDayProCollection);
            Expect.Call(_scheduleDayPro.Day).Return(dateOnly).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
            Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
            Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProCollection);
            Expect.Call(_scheduleMatrixPro.Person.Equals(person)).IgnoreArguments().Return((bool)true).Repeat.AtLeastOnce();


            Expect.Call(_dynamicBlockFinder.ExtractBlockDays(dateOnly)).IgnoreArguments().Return(dateOnlyList);
            
            Expect.Call(_groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { _person }));
            Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
            
            Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(dateOnlyPeriod);
            Expect.Call(_groupPersonBuilderBasedOnContractTime.SplitTeams(_groupPerson, dateOnly)).IgnoreArguments().
                Return(groupPersonList);
            Expect.Call(_restrictionAggregator.Aggregate(dateOnlyList, _groupPerson, _schedulingOptions)).
                IgnoreArguments().Return(_effectiveRestriction);
            Expect.Call(_skillDayPeriodIntervalDataGenerator.Generate(_groupPerson, dateOnlyList)).IgnoreArguments().Return(
                new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>());
            Expect.Call(_scheduleMatrixPro.Person).Return(_person).Repeat.AtLeastOnce();
            Expect.Call(_workShiftFilterService.Filter(dateOnly, _person, matrixList, _effectiveRestriction,
                                                       _schedulingOptions)).
                IgnoreArguments().Return(shiftProjectionCacheList);
            Expect.Call(_schedulingOptions.WorkShiftLengthHintOption).Return(new WorkShiftLengthHintOption()).Repeat.AtLeastOnce
                ();
            Expect.Call(_schedulingOptions.UseMinimumPersons).Return(false).Repeat.AtLeastOnce();
            Expect.Call(_schedulingOptions.UseMaximumPersons).Return(false).Repeat.AtLeastOnce();
            Expect.Call(_workShiftSelector.SelectShiftProjectionCache(shiftProjectionCacheList,
                                                                      new Dictionary
                                                                          <IActivity, IDictionary<TimeSpan, ISkillIntervalData>>
                                                                          (),
                                                                      new WorkShiftLengthHintOption(),
                                                                      true,
                                                                      true)).IgnoreArguments().Return(_scheduleProjectionCache);
            Expect.Call(() => _teamScheduling.Execute(dateOnly, dateOnlyList, matrixList, _groupPerson, _effectiveRestriction,
                                                      _scheduleProjectionCache, new List<DateOnly>(), new List<IPerson>())).IgnoreArguments();
        }
    }

    
}
