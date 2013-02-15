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
        private IEffectiveRestriction _effectiveRestriction;
        private BaseLineData _baseLineData;
        private IShiftProjectionCache _scheduleProjectionCache;
        private IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;


        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _baseLineData = new BaseLineData();
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
            var dateOnlyList = new List<DateOnly> {_baseLineData.BaseDateOnly };
            var groupPersonList = new List<IGroupPerson> {_baseLineData.GroupPerson};
            var matrixList = new List<IScheduleMatrixPro> {_scheduleMatrixPro};
            var shiftProjectionCacheList = new List<IShiftProjectionCache> {_scheduleProjectionCache};
            using(_mocks.Record())
            {
                ExpectCodeShouldVerifyExecution(groupPersonList, shiftProjectionCacheList, matrixList, _baseLineData.BaseDateOnly, scheduleDayProCollection, dateOnlyList);
            }
            using(_mocks.Playback() )
            {
                Assert.That(_target.Execute(new Dictionary<string, IWorkShiftFinderResult>(), matrixList, matrixList), Is.True);
            }
           
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private void ExpectCodeShouldVerifyExecution(List<IGroupPerson> groupPersonList,
                                                     List<IShiftProjectionCache> shiftProjectionCacheList, List<IScheduleMatrixPro> matrixList, DateOnly dateOnly,
                                                     ReadOnlyCollection<IScheduleDayPro> scheduleDayProCollection, List<DateOnly> dateOnlyList)
        {
            //first sub method
            Expect.Call(_scheduleMatrixPro.EffectivePeriodDays).Return(scheduleDayProCollection).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDayPro.Day).Return(dateOnly).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
            Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProCollection).Repeat.AtLeastOnce();
            Expect.Call(_scheduleMatrixPro.Person.Equals(_baseLineData.Person1)).IgnoreArguments().Return((bool)true).Repeat.AtLeastOnce();


            Expect.Call(_dynamicBlockFinder.ExtractBlockDays(dateOnly)).IgnoreArguments().Return(dateOnlyList);
            Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_baseLineData.Person1,
                                                                            _baseLineData.BaseDateOnly)).Return(_baseLineData.GroupPerson );
            //Expect.Call(_groupPerson.GroupMembers).Return(_baseLineData.ReadOnlyCollectionPersonList);
            //Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
            
            //Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(dateOnlyPeriod);
            Expect.Call(_groupPersonBuilderBasedOnContractTime.SplitTeams(_baseLineData.GroupPerson , dateOnly)).IgnoreArguments().
                Return(groupPersonList);
            Expect.Call(_restrictionAggregator.Aggregate(dateOnlyList, _baseLineData.GroupPerson, matrixList, _schedulingOptions)).
                IgnoreArguments().Return(_effectiveRestriction);
            Expect.Call(_skillDayPeriodIntervalDataGenerator.Generate(_baseLineData.GroupPerson, dateOnlyList)).IgnoreArguments().Return(
                new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>());
            Expect.Call(_scheduleMatrixPro.Person).Return(_baseLineData.Person1 ).Repeat.AtLeastOnce();
            Expect.Call(_workShiftFilterService.Filter(dateOnly, _baseLineData.Person1 , matrixList, _effectiveRestriction,
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
            Expect.Call(() => _teamScheduling.Execute(dateOnly, dateOnlyList, matrixList, _baseLineData.GroupPerson, _effectiveRestriction,
                                                      _scheduleProjectionCache, new List<DateOnly>(), new List<IPerson>())).IgnoreArguments();

        }
    }

    
}
