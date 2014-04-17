using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.WeeklyRestSolver
{
    [TestFixture]
    public class WeeklyRestSolverServiceTest
    {
        private IWeeksFromScheduleDaysExtractor _weeksFromScheduleDaysExtractor;
        private MockRepository _mock;
        private IWeeklyRestSolverService _target;
        private IEnsureWeeklyRestRule _ensureWeeklyRestRule;
        private IContractWeeklyRestForPersonWeek _contractWeeklyRestForPersonWeek;
        private IDayOffToTimeSpanExtractor _dayOffToTimeSpanExtractor;
        private IShiftNudgeManager _shiftNudgeManager;
        private IdentifyDayOffWithHighestSpan _identifyDayOffWithHighestSpan;
        private IDeleteScheduleDayFromUnsolvedPersonWeek _deleteScheduleDayFromUnsolvedPersonWeek;
        private IBrokenWeekOutsideSelectionSpecification _brokenWeekOutsideSelectionSpecification;
        private IPerson _person1;
        private ITeamBlockGenerator _teamBlockGenerator;
        private ISchedulePartModifyAndRollbackService _rollbackService;
        private IResourceCalculateDelayer _resourceCalculateDelayer;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private IScheduleMatrixPro _matrix1;
        private IPerson _person2;
        private ISchedulingOptions _schedulingOptions;
        private IOptimizationPreferences _optimizationPreferences;
        private IScheduleRange _scheduleRange1;
        private IScheduleDay _scheduleDay1;
        private PersonWeek _personWeek1;
        private IList<IPerson> _selectedPersons;
        private DateOnlyPeriod _selectedPeriod;
        private IList<IScheduleMatrixPro> _allPersonMatrixList;
        private List<IScheduleDay> _scheduleDayList;
        private IEnumerable<PersonWeek> _personWeekList;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _weeksFromScheduleDaysExtractor = _mock.StrictMock<IWeeksFromScheduleDaysExtractor>();
            _ensureWeeklyRestRule = _mock.StrictMock<IEnsureWeeklyRestRule>();
            _contractWeeklyRestForPersonWeek = _mock.StrictMock<IContractWeeklyRestForPersonWeek>();
            _dayOffToTimeSpanExtractor = _mock.StrictMock<IDayOffToTimeSpanExtractor>();
            _shiftNudgeManager = _mock.StrictMock<IShiftNudgeManager>();
            _identifyDayOffWithHighestSpan = new IdentifyDayOffWithHighestSpan();
            _deleteScheduleDayFromUnsolvedPersonWeek = _mock.StrictMock<IDeleteScheduleDayFromUnsolvedPersonWeek>();
            _brokenWeekOutsideSelectionSpecification = _mock.StrictMock<IBrokenWeekOutsideSelectionSpecification>();
            _target = new WeeklyRestSolverService(_weeksFromScheduleDaysExtractor, _ensureWeeklyRestRule,
                _contractWeeklyRestForPersonWeek, _dayOffToTimeSpanExtractor,
                _shiftNudgeManager, _identifyDayOffWithHighestSpan, _deleteScheduleDayFromUnsolvedPersonWeek,
                _brokenWeekOutsideSelectionSpecification);
            _teamBlockGenerator = _mock.StrictMock<ITeamBlockGenerator>();
            _rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
            _resourceCalculateDelayer = _mock.StrictMock<IResourceCalculateDelayer>();
            _schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
            _person1 = PersonFactory.CreatePerson();
            _person2 = PersonFactory.CreatePerson();
            _matrix1 = _mock.StrictMock<IScheduleMatrixPro>();
            _schedulingOptions = new SchedulingOptions();
            _optimizationPreferences = new OptimizationPreferences();
            _scheduleRange1 = _mock.StrictMock<IScheduleRange>();
            _scheduleDay1 = _mock.StrictMock<IScheduleDay>();
            _personWeek1 = new PersonWeek(_person1,new DateOnlyPeriod(2014,4,14,2014,04,20));
            _selectedPersons = new List<IPerson>() { _person1 };
            _selectedPeriod = new DateOnlyPeriod(new DateOnly(2014, 04, 15), new DateOnly(2014, 04, 15));
            _allPersonMatrixList = new List<IScheduleMatrixPro>() { _matrix1 };
            _scheduleDayList = new List<IScheduleDay>() { _scheduleDay1 };
            _personWeekList = new List<PersonWeek>() { _personWeek1 };
        }

        [Test]
        public void ShouldNotContinueIfTheMatrixIsNull()
        {
            using (_mock.Record())
            {
                Expect.Call(_matrix1.Person).Return(_person2);
            }
            _target.Execute(_selectedPersons, _selectedPeriod, _teamBlockGenerator, _rollbackService,
                _resourceCalculateDelayer, _schedulingResultStateHolder, _allPersonMatrixList, _optimizationPreferences,
                _schedulingOptions);
        }

        [Test]
        public void ShouldExecuteIfNoWeeklyRestIsBroken()
        {
            using (_mock.Record())
            {
                extractingPersonWeek(_selectedPeriod, _scheduleDayList, _personWeekList);
					 Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(_scheduleDayList, true))
					  .Return(_personWeekList);
                Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeek1, _scheduleRange1, TimeSpan.FromHours(40)))
                    .Return(true);
            }
            using (_mock.Playback())
            {
                _target.Execute(_selectedPersons, _selectedPeriod, _teamBlockGenerator, _rollbackService,
                    _resourceCalculateDelayer, _schedulingResultStateHolder, _allPersonMatrixList,
                    _optimizationPreferences, _schedulingOptions);
            }
            
        }

        private void extractingPersonWeek(DateOnlyPeriod selectedPeriod, List<IScheduleDay> scheduleDayList, IEnumerable<PersonWeek> personWeekList)
        {
            Expect.Call(_matrix1.Person).Return(_person1);
            Expect.Call(_schedulingResultStateHolder.Schedules[_person1]).Return(_scheduleRange1);
            Expect.Call(_scheduleRange1.ScheduledDayCollection(selectedPeriod)).Return(scheduleDayList);
            Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(scheduleDayList, false ))
                .Return(personWeekList);
            Expect.Call(_contractWeeklyRestForPersonWeek.GetWeeklyRestFromContract(_personWeek1))
                .Return(TimeSpan.FromHours(40));
        }

        [Test]
        public void ShouldNotContinueIfNudgeFailesExecuteIfWeeklyRestIsBroken()
        {
            DateOnly dayOffDate = new DateOnly(2014, 04, 17);
            IDictionary<DateOnly, TimeSpan> dayOffToSpanDictionary = new Dictionary<DateOnly, TimeSpan>();
            dayOffToSpanDictionary.Add(dayOffDate,TimeSpan.FromHours(10));
            using (_mock.Record())
            {
                extractingPersonWeek(_selectedPeriod, _scheduleDayList, _personWeekList);
					 Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(_scheduleDayList, true))
						.Return(_personWeekList);
	            Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeek1, _scheduleRange1, TimeSpan.FromHours(40)))
		            .Return(false);
                //analyzing failed weeks
                Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeek1, _scheduleRange1, TimeSpan.FromHours(40)))
                    .Return(false);
                Expect.Call(_dayOffToTimeSpanExtractor.GetDayOffWithTimeSpanAmongAWeek(_personWeek1.Week,
                    _scheduleRange1)).Return(dayOffToSpanDictionary);
                Expect.Call(_shiftNudgeManager.TrySolveForDayOff(_personWeek1, dayOffDate, _teamBlockGenerator,
                    _allPersonMatrixList, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder,
                    _selectedPeriod, _selectedPersons, _optimizationPreferences, _schedulingOptions)).Return(false);
                Expect.Call(()=>_deleteScheduleDayFromUnsolvedPersonWeek.DeleteAppropiateScheduleDay(_scheduleRange1,
                    dayOffDate, _rollbackService));
            }
            using (_mock.Playback())
            {
                _target.Execute(_selectedPersons, _selectedPeriod, _teamBlockGenerator, _rollbackService,
                    _resourceCalculateDelayer, _schedulingResultStateHolder, _allPersonMatrixList,
                    _optimizationPreferences, _schedulingOptions);
            }

        }

        [Test]
        public void ShouldNotContinueIfSuccessfullButOtherWeekIsBroken()
        {
            DateOnly dayOffDate = new DateOnly(2014, 04, 17);
            IDictionary<DateOnly, TimeSpan> dayOffToSpanDictionary = new Dictionary<DateOnly, TimeSpan>();
            dayOffToSpanDictionary.Add(dayOffDate, TimeSpan.FromHours(10));
            using (_mock.Record())
            {
                extractingPersonWeek(_selectedPeriod, _scheduleDayList, _personWeekList);
					 Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(_scheduleDayList, true))
						.Return(_personWeekList);
	            Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeek1, _scheduleRange1, TimeSpan.FromHours(40)))
		            .Return(false);
                //analyzing failed weeks
                Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeek1, _scheduleRange1, TimeSpan.FromHours(40)))
                    .Return(false);
                Expect.Call(_dayOffToTimeSpanExtractor.GetDayOffWithTimeSpanAmongAWeek(_personWeek1.Week,
                    _scheduleRange1)).Return(dayOffToSpanDictionary);
                Expect.Call(_shiftNudgeManager.TrySolveForDayOff(_personWeek1, dayOffDate, _teamBlockGenerator,
                    _allPersonMatrixList, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder,
                    _selectedPeriod, _selectedPersons, _optimizationPreferences, _schedulingOptions)).Return(true);
                Expect.Call(_brokenWeekOutsideSelectionSpecification.IsSatisfy(_personWeek1, _personWeekList.ToList(),
                    new Dictionary<PersonWeek, TimeSpan>(), _scheduleRange1)).IgnoreArguments().Return(true);
                Expect.Call(_shiftNudgeManager.RollbackLastScheduledWeek(_rollbackService, _resourceCalculateDelayer))
                    .Return(true);
                Expect.Call(() => _deleteScheduleDayFromUnsolvedPersonWeek.DeleteAppropiateScheduleDay(_scheduleRange1,
                    dayOffDate, _rollbackService));
            }
            using (_mock.Playback())
            {
                _target.Execute(_selectedPersons, _selectedPeriod, _teamBlockGenerator, _rollbackService,
                    _resourceCalculateDelayer, _schedulingResultStateHolder, _allPersonMatrixList,
                    _optimizationPreferences, _schedulingOptions);
            }

        }

        [Test]
        public void ShouldContinueIfNudgeIsSuccessfull()
        {
            DateOnly dayOffDate = new DateOnly(2014, 04, 17);
            IDictionary<DateOnly, TimeSpan> dayOffToSpanDictionary = new Dictionary<DateOnly, TimeSpan>();
            dayOffToSpanDictionary.Add(dayOffDate, TimeSpan.FromHours(10));
            using (_mock.Record())
            {
                extractingPersonWeek(_selectedPeriod, _scheduleDayList, _personWeekList);
	            Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(_scheduleDayList, true))
		            .Return(_personWeekList);
                Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeek1, _scheduleRange1, TimeSpan.FromHours(40)))
                    .Return(false);
                //analyzing failed weeks
                Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeek1, _scheduleRange1, TimeSpan.FromHours(40)))
                    .Return(false);
                Expect.Call(_dayOffToTimeSpanExtractor.GetDayOffWithTimeSpanAmongAWeek(_personWeek1.Week,
                    _scheduleRange1)).Return(dayOffToSpanDictionary);
                Expect.Call(_shiftNudgeManager.TrySolveForDayOff(_personWeek1, dayOffDate, _teamBlockGenerator,
                    _allPersonMatrixList, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder,
                    _selectedPeriod, _selectedPersons, _optimizationPreferences, _schedulingOptions)).Return(true);
                Expect.Call(_brokenWeekOutsideSelectionSpecification.IsSatisfy(_personWeek1, _personWeekList.ToList(),
                    new Dictionary<PersonWeek, TimeSpan>(), _scheduleRange1)).IgnoreArguments().Return(false);
            }
            using (_mock.Playback())
            {
                _target.Execute(_selectedPersons, _selectedPeriod, _teamBlockGenerator, _rollbackService,
                    _resourceCalculateDelayer, _schedulingResultStateHolder, _allPersonMatrixList,
                    _optimizationPreferences, _schedulingOptions);
            }

        }

        [Test]
        public void ShouldNotContinueIfCanceled()
        {
            DateOnly dayOffDate = new DateOnly(2014, 04, 17);
            IDictionary<DateOnly, TimeSpan> dayOffToSpanDictionary = new Dictionary<DateOnly, TimeSpan>();
            dayOffToSpanDictionary.Add(dayOffDate, TimeSpan.FromHours(10));
            using (_mock.Record())
            {
                extractingPersonWeek(_selectedPeriod, _scheduleDayList, _personWeekList);
	            Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(_scheduleDayList, true))
		            .Return(_personWeekList);
                Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeek1, _scheduleRange1, TimeSpan.FromHours(40)))
                    .Return(false);
                //analyzing failed weeks
                Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeek1, _scheduleRange1, TimeSpan.FromHours(40)))
                    .Return(false);
                Expect.Call(_dayOffToTimeSpanExtractor.GetDayOffWithTimeSpanAmongAWeek(_personWeek1.Week,
                    _scheduleRange1)).Return(dayOffToSpanDictionary);
            }
            using (_mock.Playback())
            {
                _target.ResolvingWeek += targetWeekScheduledScheduled;
                _target.Execute(_selectedPersons, _selectedPeriod, _teamBlockGenerator, _rollbackService,
                    _resourceCalculateDelayer, _schedulingResultStateHolder, _allPersonMatrixList,
                    _optimizationPreferences, _schedulingOptions);
                _target.ResolvingWeek -= targetWeekScheduledScheduled;
            }
        }


        private void targetWeekScheduledScheduled(object sender, BlockSchedulingServiceEventArgs  blockSchedulingServiceEventArgs )
        {
            blockSchedulingServiceEventArgs.Cancel  = true;
        }
        
    }
}
