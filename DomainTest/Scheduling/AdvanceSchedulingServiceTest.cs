using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
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
        private ISkillDayPeriodIntervalData _skillDayPeriodIntervalData;
        private IDynamicBlockFinder _dynamicBlockFinder;
        private ITeamExtractor _teamExtractor;
        private IRestrictionAggregator _restrictionAggregator;
        private IWorkShiftFilterService _workShiftFilterService;
        private ITeamScheduling _teamScheduling;
        private ISchedulingOptions _schedulingOptions;
    	private IWorkShiftSelector _workShiftSelector;
        private IGroupPersonBuilderBasedOnContractTime _groupPersonBuilderBasedOnContractTime;
        private IScheduleMatrixPro _scheduleMatrixPro;
        private IScheduleDayPro _scheduleDayPro;
        private IScheduleDay _scheduleDay;
        private IGroupPerson _grouPerson;
        private IEffectiveRestriction _effectiveRestriction;
        private IPerson _person;
        private IPersonPeriod _personPeriod;
        private IPersonSkill _personSkill;
        private ISkill _skill;
        private IShiftProjectionCache _scheduleProjectionCache;
        private IVirtualSchedulePeriod _virtualSchedulePeriod;
        private IActivity _activity;


        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _schedulingOptions = _mocks.StrictMock<ISchedulingOptions>();
            _skillDayPeriodIntervalData =  _mocks.StrictMock<ISkillDayPeriodIntervalData>();
            _dynamicBlockFinder = _mocks.StrictMock<IDynamicBlockFinder>();
            _teamExtractor = _mocks.StrictMock<ITeamExtractor>();
            _restrictionAggregator = _mocks.StrictMock<IRestrictionAggregator>();
            _workShiftFilterService = _mocks.StrictMock<IWorkShiftFilterService>();
            _teamScheduling = _mocks.StrictMock<ITeamScheduling>();
        	_workShiftSelector = _mocks.StrictMock<IWorkShiftSelector>();
            _groupPersonBuilderBasedOnContractTime = _mocks.StrictMock<IGroupPersonBuilderBasedOnContractTime>();
            _scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
            _scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDay = _mocks.StrictMock<IScheduleDay>();
            var matrixList = new List<IScheduleMatrixPro>{_scheduleMatrixPro};
            _effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
            _grouPerson = _mocks.StrictMock<IGroupPerson>();
            _person = _mocks.StrictMock<IPerson>();
            _personPeriod = _mocks.StrictMock<IPersonPeriod>();
            _personSkill = _mocks.StrictMock<IPersonSkill>();
            _skill = _mocks.StrictMock<ISkill>();
            _virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
            _scheduleProjectionCache = _mocks.StrictMock<IShiftProjectionCache>();
            _activity = _mocks.StrictMock<IActivity>();
            _target = new AdvanceSchedulingService(_skillDayPeriodIntervalData, 
                                                _dynamicBlockFinder, 
                                                _teamExtractor, 
                                                _restrictionAggregator,
                                                matrixList, 
												_workShiftFilterService,
												_teamScheduling, 
												_schedulingOptions,
                                                _workShiftSelector, _groupPersonBuilderBasedOnContractTime);
        }

        [Test]
        public void ShouldVerifyExecution()
        {
            var scheduleDayProList = new List<IScheduleDayPro>{_scheduleDayPro };
            var scheduleDayProCollection = new ReadOnlyCollection<IScheduleDayPro>(scheduleDayProList);
            var dateOnly = new DateOnly();
            var dateOnlyList = new List<DateOnly> {dateOnly};
            var groupPersonList = new List<IGroupPerson> {_grouPerson};
            IDictionary<TimeSpan, ISkillIntervalData> skillInternalDataList = new Dictionary<TimeSpan, ISkillIntervalData>( );
            var personPeriodList = new List<IPersonPeriod> {_personPeriod};
            var personSkillList = new List<IPersonSkill> {_personSkill};
            var matrixList = new List<IScheduleMatrixPro> {_scheduleMatrixPro};
            var shiftProjectionCacheList = new List<IShiftProjectionCache> {_scheduleProjectionCache};
            var dateOnlyPeriod = new DateOnlyPeriod(dateOnly,dateOnly.AddDays(1));
            using(_mocks.Record())
            {
                //first sub method
                Expect.Call(_scheduleMatrixPro.EffectivePeriodDays).Return(scheduleDayProCollection);
                Expect.Call(_scheduleDayPro.Day).Return(dateOnly).Repeat.AtLeastOnce()  ;
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.Twice() ;
                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Twice();
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(scheduleDayProCollection);

                Expect.Call(_dynamicBlockFinder.ExtractBlockDays(dateOnly)).IgnoreArguments().Return(dateOnlyList);
                Expect.Call(_teamExtractor.GetRamdomTeam(dateOnly)).IgnoreArguments().Return(_grouPerson);
                Expect.Call(_grouPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> {_person}));
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
                
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(dateOnlyPeriod);
                Expect.Call(_groupPersonBuilderBasedOnContractTime.SplitTeams(_grouPerson, dateOnly)).IgnoreArguments().
                    Return(groupPersonList);
                Expect.Call(_restrictionAggregator.Aggregate(dateOnlyList, _grouPerson, _schedulingOptions)).
                    IgnoreArguments().Return(_effectiveRestriction);
                Expect.Call(_skillDayPeriodIntervalData.GetIntervalDistribution(dateOnlyList)).IgnoreArguments().Return(
                    skillInternalDataList);
                Expect.Call(_scheduleMatrixPro.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_person.PersonPeriodCollection).Return(personPeriodList);
                Expect.Call(_personPeriod.PersonSkillCollection).Return(personSkillList);
                Expect.Call(_personSkill.Skill).Return(_skill);
                Expect.Call(_skill.Activity).Return(_activity);
                Expect.Call(_workShiftFilterService.Filter(dateOnly, _person, matrixList, _effectiveRestriction,
                                                           _schedulingOptions)).
                    IgnoreArguments().Return(shiftProjectionCacheList);
                Expect.Call(_schedulingOptions.WorkShiftLengthHintOption).Return(new WorkShiftLengthHintOption());
                Expect.Call(_schedulingOptions.UseMinimumPersons).Return(false);
                Expect.Call(_schedulingOptions.UseMaximumPersons).Return(false);
                Expect.Call(_workShiftSelector.Select(shiftProjectionCacheList, new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>(), 
                                                      new WorkShiftLengthHintOption(), 
                                                      true,
                                                      true)).IgnoreArguments().Return(_scheduleProjectionCache ) ;
                Expect.Call(() => _teamScheduling.Execute(dateOnlyList, matrixList, _grouPerson, _effectiveRestriction,
                                                    _scheduleProjectionCache)).IgnoreArguments();
            }
            using(_mocks.Playback() )
            {
                Assert.That(_target.Execute(new Dictionary<string, IWorkShiftFinderResult>()), Is.True);
            }
           
        }

       
    }

    
}
