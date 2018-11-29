using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture, SetUICulture("en-US")]
    public class AgentInfoHelperTest
    {
        private AgentInfoHelper _target;
		private MatrixListFactory _matrixListFactory;
        private IPerson _person;
        private TimeZoneInfo _timeZoneInfo;
        private DateTime _dateTime;
        private ISchedulingResultStateHolder _stateHolder;
		private SchedulerStateHolder _schedulerStateHolder;
        private SchedulingOptions _schedulingOptions;
        private DateOnly _dateOnly;
        private ISchedulePeriod _schedulePeriod;
        private const int targetDaysOff = 2;
        private readonly TimeSpan _averageWorkTimePerDay = new TimeSpan(8, 0, 0);
        IScenario _scenario;
	    private IWorkShiftMinMaxCalculator workShiftMinMaxCalculator;
		private IDisposable auth;

		[SetUp]
		public void Setup()
		{
			auth = CurrentAuthorization.ThreadlyUse(new FullPermission());
			_dateTime = new DateTime(2009, 12, 12, 0, 0, 0, DateTimeKind.Utc);
			_dateOnly = new DateOnly(2009, 12, 12);
			_person = PersonFactory.CreatePersonWithPersonPeriod(_dateOnly, new List<ISkill>());
			_stateHolder = SchedulingResultStateHolderFactory.Create(new DateTimePeriod(_dateTime, _dateTime.AddDays(7)));
			_schedulerStateHolder = new SchedulerStateHolder(_stateHolder, new CommonStateHolder(new DisableDeletedFilter(null)), new FakeTimeZoneGuard(TimeZoneInfo.Utc));
			_schedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(
				new DateOnlyPeriod(_dateOnly, _dateOnly.AddDays(7)), TimeZoneInfo.Utc);
			_scenario = ScenarioFactory.CreateScenarioAggregate();
			var gridLockManager = new GridlockManager();
			var matrixUserLocker = new MatrixUserLockLocker(() => gridLockManager);
			var notPermittedLocker = new MatrixNotPermittedLocker(new ThisAuthorization(new FullPermission()));
			var personListExtraxtor = new PersonListExtractorFromScheduleParts();
			var periodExtractor = new PeriodExtractorFromScheduleParts();
			_matrixListFactory = new MatrixListFactory(matrixUserLocker, notPermittedLocker, personListExtraxtor, periodExtractor);

			_timeZoneInfo = (TimeZoneInfo.Utc);

			var dic = new ScheduleDictionaryForTest(_scenario,
				new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1, 2020, 1, 1)),
				new Dictionary<IPerson, IScheduleRange>());
			var dayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(_person, dic.Scenario, _dateOnly,
				new TimeSpan(), new TimeSpan(), new TimeSpan());
			var range = new ScheduleRange(dic, dayOff, new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make()), CurrentAuthorization.Make());
			range.Add(dayOff);
			dic.AddTestItem(_person, range);
			_stateHolder.Schedules = dic;
			_schedulingOptions = new RestrictionSchedulingOptions
			{
				UsePreferences = true,
				UseRotations = true,
				UseAvailability = true,
				UseStudentAvailability = true
			};
			((RestrictionSchedulingOptions) _schedulingOptions).UseScheduling = true;
			_schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(_dateOnly);
			_schedulePeriod.SetDaysOff(targetDaysOff);
			_schedulePeriod.AverageWorkTimePerDayOverride = _averageWorkTimePerDay;
			_person.AddSchedulePeriod(_schedulePeriod);
			workShiftMinMaxCalculator = new WorkShiftMinMaxCalculator(new PossibleMinMaxWorkShiftLengthExtractor(new RestrictionExtractor(new RestrictionCombiner(), new RestrictionRetrievalOperation()), new WorkShiftWorkTime(new RuleSetProjectionService(new ShiftCreatorService(new CreateWorkShiftsFromTemplate()))),new RuleSetBagExtractorProvider()),new SchedulePeriodTargetTimeCalculator(), new WorkShiftWeekMinMaxCalculator() );
			_target = new AgentInfoHelper(_person, _dateOnly, _stateHolder, _schedulingOptions, _matrixListFactory, workShiftMinMaxCalculator);
			_target.SchedulePeriodData(true);
		}

		[TearDown]
		public void Teardown()
		{
			auth?.Dispose();
		}

		[Test]
        public void CanCreateInstance()
        {		
            Assert.IsNotNull(_target);
        }

        [Test]
        public void ShouldNotIncreaseCurrentDaysOffByTwoWhenReloadingSchedulePeriodData()
        {
            var currentDaysOff = _target.CurrentDaysOff;
            _target.SchedulePeriodData(true);
            Assert.AreEqual(currentDaysOff,_target.CurrentDaysOff);
        }

        [Test]
        public void VerifySelectedDate()
        {
            var expectedDate = new DateOnly(TimeZoneHelper.ConvertFromUtc(_dateTime, _timeZoneInfo));
            Assert.AreEqual(expectedDate, _target.SelectedDate); 
        }

        [Test]
        public void VerifySchedulePeriod()
        {
            Assert.IsNotNull(_target.SchedulePeriod);
        }

        [Test]
        public void VerifyPeriodInLegalState()
        {
            Assert.IsFalse(_target.PeriodInLegalState);
        }
        
        [Test]
        public void VerifyCurrentDaysOff()
        {
            Assert.AreEqual(1, _target.CurrentDaysOff);
        }

        [Test]
        public void VerifyCurrentContractTime()
        {
            Assert.AreEqual(new TimeSpan(0), _target.CurrentContractTime);
        }

        [Test]
        public void VerifyCurrentPaidTime()
        {
            Assert.AreEqual(new TimeSpan(0), _target.CurrentPaidTime);
        }
        [Test]
        public void VerifyCurrentWorkedDay()
        {
            Assert.AreEqual(1, _target.CurrentOccupiedSlots);
        }
       
        [Test]
        public void VerifyIncludeScheduling()
        {
            Assert.IsTrue(_target.IncludeScheduling());
            ((RestrictionSchedulingOptions) _target.SchedulingOptions).UseScheduling = false;
            Assert.IsFalse(_target.IncludeScheduling());
        }

        [Test]
        public void VerifyPerson()
        {
            Assert.AreEqual(_person, _target.Person);
        }

        [Test]
        public void VerifySchedulePeriodTargetDaysOff()
        {
            Assert.AreEqual(targetDaysOff, _target.SchedulePeriodTargetDaysOff);
        }

        [Test]
        public void VerifySchedulePeriodTargetMinMax()
        {
            var period = new TimePeriod(40, 0, 40, 0);
            Assert.AreEqual(period, _target.SchedulePeriodTargetMinMax);
        }

        [Test]
        public void VerifySchedulePeriodTargetTime()
        {
            int time = (int)_schedulePeriod.AverageWorkTimePerDay.TotalHours *(7 - targetDaysOff);
            var targetTime = new TimeSpan(time, 0, 0);
            Assert.AreEqual(targetTime,_target.SchedulePeriodTargetTime);
        }

        [Test]
        public void VerifyWeekInLegalState()
        {
            Assert.IsTrue(_target.WeekInLegalState);
        }

        
        [Test]
        public void VerifyHandleNullSchedulePeriod()
        {
			_target = new AgentInfoHelper(_person, new DateOnly(1888, 1, 1), _stateHolder, _schedulingOptions, _matrixListFactory, workShiftMinMaxCalculator);
            _target.SchedulePeriodData(true);
            Assert.IsFalse(_target.WeekInLegalState);
        }

		[Test]
		public void ShouldReturnStatusOfMatrix()
		{
			Assert.IsTrue(_target.HasMatrix);
			_target = new AgentInfoHelper(_person, new DateOnly(1888, 1, 1), _stateHolder, _schedulingOptions,  _matrixListFactory, workShiftMinMaxCalculator);	
			Assert.IsFalse(_target.HasMatrix);
		}

        [Test]
        public void ShouldNotIncreaseCurrentValuesForEachCallToLoadSchedulePeriodData()
        {
            prepareScheduleDictionary();

			_target = new AgentInfoHelper(_person, _dateOnly, _stateHolder, _schedulingOptions, _matrixListFactory, workShiftMinMaxCalculator);
            _target.SchedulePeriodData(true);
            _target.SchedulePeriodData(true);

            Assert.AreEqual(4, _target.CurrentOccupiedSlots);
            Assert.AreEqual(TimeSpan.FromHours(13), _target.CurrentContractTime);
            Assert.AreEqual(TimeSpan.FromHours(13), _target.CurrentPaidTime);
            Assert.AreEqual(TimeSpan.FromHours(13), _target.CurrentPaidTime);
           
        }

        [Test]
        public void VerifyCurrentWorkedDays()
        {
            prepareScheduleDictionary();

			_target = new AgentInfoHelper(_person, _dateOnly, _stateHolder, _schedulingOptions, _matrixListFactory, workShiftMinMaxCalculator);
            _target.SchedulePeriodData(true);

            Assert.AreEqual(4, _target.CurrentOccupiedSlots);
        }

        private void prepareScheduleDictionary()
        {
            IScheduleParameters parameters = new ScheduleParameters(_scenario, _person,
                                                                    new DateTimePeriod(2000, 1, 1, 2020, 1, 1));
            var dic = new ScheduleDictionaryForTest(_scenario,
                                                    new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1, 2020, 1, 1)),
                                                    new Dictionary<IPerson, IScheduleRange>());

            dic.UsePermissions(false);
            //schemadata
            ScheduleRange range = addAssignment(dic, parameters);

            addPersonAbsences(range);

            addPersonDayOff(range);

            dic.AddTestItem(_person, range);
			_stateHolder.Schedules = dic;
        }

        private void addPersonDayOff(ScheduleRange range)
        {
					var personDayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(_person, _scenario, _dateOnly.AddDays(5), new DayOffTemplate(new Description("test")));
            range.Add(personDayOff);
        }

        private void addPersonAbsences(ScheduleRange range)
        {
            IAbsence absence1 = new Absence { InContractTime = false, InPaidTime = false, InWorkTime = false };
            IPersonAbsence personAbsence1 = getPersonAbsence(absence1, new DateTimePeriod(_dateTime.AddDays(1),
                                                                                          _dateTime.AddDays(2)));
            range.Add(personAbsence1);

            IAbsence absence = new Absence {InContractTime = true, InPaidTime = true, InWorkTime = true};
            IPersonAbsence personAbsence = getPersonAbsence(absence, new DateTimePeriod(_dateTime.AddDays(2),
                                                                                        _dateTime.AddDays(3)));
            range.Add(personAbsence);
        }

        private ScheduleRange addAssignment(ScheduleDictionaryForTest dic, IScheduleParameters parameters)
        {
            IPersonAssignment assignment = getPersonAssignment();

            //lägg på schemadata på range
            var range = new ScheduleRange(dic, parameters, new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make()), CurrentAuthorization.Make());
            range.Add(assignment);
            return range;
        }

        private IPersonAbsence getPersonAbsence(IAbsence absence, DateTimePeriod period)
        {
            IAbsenceLayer absenceLayer1 = new AbsenceLayer(absence, period);
            return new PersonAbsence(_person, _scenario, absenceLayer1);
        }

        private IPersonAssignment getPersonAssignment()
        {
            var mainShift = new EditableShift(ShiftCategoryFactory.CreateShiftCategory("Day"));
            IActivity activity = ActivityFactory.CreateActivity("Phone");
            activity.InContractTime = true;
            activity.InWorkTime = true;
            activity.InPaidTime = true;
            var layer = new EditableShiftLayer(activity,
                                                                 new DateTimePeriod(_dateTime.AddHours(7),
                                                                                    _dateTime.AddHours(12)));
            mainShift.LayerCollection.Add(layer);
            var assignment = new PersonAssignment(_person, _scenario, _dateOnly);
            new EditableShiftMapper().SetMainShiftLayers(assignment, mainShift);
            return assignment;
        }

        [Test]
        public void VerifyNumberOfWarnings()
        {
			_target = new AgentInfoHelper(_person, new DateOnly(1888, 1, 1), _stateHolder, _schedulingOptions, _matrixListFactory, workShiftMinMaxCalculator)
                             {NumberOfWarnings = 5};
            _target.SchedulePeriodData(true);
            Assert.IsTrue(_target.NumberOfWarnings == 5);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_target.Period.Value.EndDate, _target.EndDate);
            Assert.AreEqual(_target.Period.Value.StartDate, _target.StartDate);
            Assert.AreEqual(_target.NumberOfDatesWithPreferenceOrScheduledDaysOff, _target.NumberOfDatesWithPreferenceOrScheduledDaysOff);
            Assert.AreEqual("Week", _target.PeriodType);

            ISchedulePeriod schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(_dateOnly, SchedulePeriodType.Day, 14);
            IPerson person = PersonFactory.CreatePerson("Person");
            person.RemoveAllSchedulePeriods();
            person.AddSchedulePeriod(schedulePeriod);
			_target = new AgentInfoHelper(person, _dateOnly, _stateHolder, _schedulingOptions, _matrixListFactory, workShiftMinMaxCalculator);
            _target.SchedulePeriodData(true);
            Assert.AreEqual("Day", _target.PeriodType);

            person.RemoveAllSchedulePeriods();
            schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(_dateOnly, SchedulePeriodType.Month, 1);
            person.AddSchedulePeriod(schedulePeriod);
			_target = new AgentInfoHelper(person, _dateOnly, _stateHolder, _schedulingOptions, _matrixListFactory, workShiftMinMaxCalculator);
            _target.SchedulePeriodData(true);
            Assert.AreEqual("Month", _target.PeriodType);

            Assert.AreEqual(_target.Person.Name.ToString(), _target.PersonName);
        }

        [Test]
        public void VerifyRestrictionProperties()
        {
            Assert.AreEqual(1, _target.PreferenceFulfillment.Value);
            Assert.AreEqual(1, _target.MustHavesFulfillment.Value);
            Assert.AreEqual(1, _target.RotationFulfillment.Value);
            Assert.AreEqual(1, _target.AvailabilityFulfillment.Value);
            Assert.AreEqual(1, _target.StudentAvailabilityFulfillment.Value);
        }

		[Test]
		public void Coverage()
		{
			Assert.AreEqual(0, _target.TimePerDefinitionSet.Count);
			Assert.AreEqual(" (2 - 2)", _target.DayOffTolerance);
		}
    }
}
