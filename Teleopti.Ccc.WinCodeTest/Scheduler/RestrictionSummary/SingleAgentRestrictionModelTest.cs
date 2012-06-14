using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.RestrictionSummary
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture, SetUICulture("en-US")]
    public class SingleAgentRestrictionModelTest
    {
        private SingleAgentRestrictionModel _target;
        private IList<IPerson> _loadedPersons;
        private IPerson _person;
        private DateTimePeriod _loadedDateTimePeriod;
        private ICccTimeZoneInfo _timeZoneInfo;
        private ISchedulingResultStateHolder _stateHolder;
        private ISchedulingOptions _schedulingOptions;
        private MockRepository _mocks;
        private ISchedulePeriod _schedulePeriod;
        private IWorkShiftWorkTime _workShiftWorkTime;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            //_stateHolder = _mocks.DynamicMock<ISchedulingResultStateHolder>();
            _stateHolder = new SchedulingResultStateHolder();
            
            IScheduleDateTimePeriod scheduleDateTimePeriod = new ScheduleDateTimePeriod(new DateTimePeriod(2010, 1, 1, 2010, 1, 2));
            IScheduleDictionary dic = new ScheduleDictionaryForTest(ScenarioFactory.CreateScenarioAggregate(), scheduleDateTimePeriod, new Dictionary<IPerson, IScheduleRange>());
            _stateHolder.Schedules = dic;
            _schedulingOptions = new RestrictionSchedulingOptions
            {
                UseAvailability = true,
                UsePreferences = true,
                UseStudentAvailability = true,
                UseRotations = true,
                UseScheduling = true
            };
            _person = PersonFactory.CreatePerson("Lars", "Lagerbäck");
            _person.PermissionInformation.SetDefaultTimeZone(new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));
            IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(DateOnly.MinValue);
            _person.AddPersonPeriod(personPeriod);
            _schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(2010, 5, 3),
                                                                         SchedulePeriodType.Week, 4);

            _person.RemoveAllSchedulePeriods();
            _person.AddSchedulePeriod(SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(2009, 2, 2),
                                                                         SchedulePeriodType.Week, 4));
            _person.AddSchedulePeriod(SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(2009, 3, 2),
                                                             SchedulePeriodType.Week, 4));
            _person.AddSchedulePeriod(_schedulePeriod);
            _loadedPersons = new List<IPerson> { _person };
            DateTime startDate = new DateTime(2010, 5, 2, 22, 0, 0, DateTimeKind.Utc);
            DateTime endDate = new DateTime(2010, 5, 31, 22, 0, 0, DateTimeKind.Utc);
            _loadedDateTimePeriod = new DateTimePeriod(startDate, endDate);
            _timeZoneInfo = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            _workShiftWorkTime = _mocks.StrictMock<IWorkShiftWorkTime>();
            _target = new SingleAgentRestrictionModel(_loadedDateTimePeriod, _timeZoneInfo, _workShiftWorkTime);
        }

        [Test]
        public void CanCreateInstance()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void CanGetProperties()
        {
            Assert.IsNotNull(_target.PersonsAffectedPeriodDates);
        }

        [Test]
        public void CanInitialize()
        {
            _target.Initialize(_loadedPersons, _stateHolder, _schedulingOptions);
            Assert.AreEqual(_stateHolder, _target.StateHolder);
        }

        [Test]
        public void VerifyGetRowData()
        {
            _target.Initialize(_loadedPersons, _stateHolder, _schedulingOptions);
            AgentInfoHelper helper = _target.GetRowData(0);
            Assert.IsNotNull(helper);
            Assert.AreEqual(_person, helper.Person);
        }

        [Test]
        public void ShouldGetDataAfterReload()
        {
            _target.Initialize(_loadedPersons, _stateHolder, _schedulingOptions);
            _target.Reload(_loadedPersons);
            AgentInfoHelper helper = _target.GetRowData(0);
            Assert.IsNotNull(helper);
            Assert.AreEqual(_person, helper.Person);
        }

        [Test]
        public void VerifyGetRowDataReturnsNullIfIndexToBig()
        {
            _target.Initialize(_loadedPersons, _stateHolder, _schedulingOptions);
            AgentInfoHelper helper = _target.GetRowData(100);
            Assert.IsNull(helper);
        }

        [Test]
        public void VerifyTotalNumberOfDaysOff()
        {
            _target.Initialize(_loadedPersons, _stateHolder, _schedulingOptions);
            int daysOff = _target.TotalNumberOfDaysOff(0);
            Assert.AreEqual(0, daysOff);
        }

        [Test]
        public void VerifySelectedPersonDate()
        {
            _target.Initialize(_loadedPersons, _stateHolder, _schedulingOptions);
            _target.SetSelectedPersonDate(0);
            Assert.IsNotNull(_target.SelectedPersonDate);
        }

        [Test]
        public void VerifyNumberOfDatesInPersonsAffectedPeriodDates()
        {
            //Two affected periods = 2 dates
            _target.Initialize(_loadedPersons, _stateHolder, _schedulingOptions);
            Assert.AreEqual(2, _target.PersonsAffectedPeriodDates.Count);
        }

        [Test]
        public void CanSortPersonsAffectedPeriodDates()
        {
            IPerson person = PersonFactory.CreatePerson("Roland", "Anderson");
            ISchedulePeriod schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(2010, 5, 3),
                                                                         SchedulePeriodType.Day, 4);

            IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(DateOnly.MinValue.AddDays(100));
            person.AddPersonPeriod(personPeriod);
            schedulePeriod.SetDaysOff(2);
            schedulePeriod.AverageWorkTimePerDay = new TimeSpan(4,0,0);
            person.RemoveAllSchedulePeriods();
            person.AddSchedulePeriod(SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(2009, 2, 2),
                                                                         SchedulePeriodType.Week, 4));
            person.AddSchedulePeriod(SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(2009, 3, 2),
                                                             SchedulePeriodType.Week, 4));
            person.AddSchedulePeriod(schedulePeriod);
            _loadedPersons.Add(person);
            _target.Initialize(_loadedPersons, _stateHolder, _schedulingOptions);

            //PersonName
            _target.SortOnColumn(0, false);
            Assert.AreEqual(person, _target.PersonsAffectedPeriodDates[0].Key);
            _target.SortOnColumn(0, true);
            Assert.AreEqual(_person, _target.PersonsAffectedPeriodDates[0].Key);

            //NumberOfWarnings
            AgentInfoHelper helper = _target.GetRowData(1);
            helper.NumberOfWarnings = 5;
            _target.SortOnColumn(1, false);
            Assert.AreEqual(_person, _target.PersonsAffectedPeriodDates[0].Key);
            DateOnly expectedDate = new DateOnly(2010, 5, 31);
            Assert.AreEqual(expectedDate, _target.PersonsAffectedPeriodDates[0].Value);
            _target.SortOnColumn(1, true);
            Assert.AreEqual(_person, _target.PersonsAffectedPeriodDates[9].Key);
            Assert.AreEqual(expectedDate, _target.PersonsAffectedPeriodDates[9].Value);

            //PeriodType
            _target.SortOnColumn(2, false);
            helper = _target.GetRowData(1);
            Assert.AreEqual(_person, _target.PersonsAffectedPeriodDates[0].Key);
            Assert.AreEqual("Week", helper.PeriodType);
            _target.SortOnColumn(2, true);
            helper = _target.GetRowData(1);
            Assert.AreEqual("Day", helper.PeriodType);
            Assert.AreEqual(person, _target.PersonsAffectedPeriodDates[0].Key);

            //StartDate
            expectedDate = new DateOnly(2010, 5, 31);
            _target.SortOnColumn(3, false);
            Assert.AreEqual(expectedDate, _target.PersonsAffectedPeriodDates[0].Value);
            _target.SortOnColumn(3, true);
            expectedDate = new DateOnly(2010, 5, 3);
            Assert.AreEqual(expectedDate, _target.PersonsAffectedPeriodDates[0].Value);

            //EndDate            
            expectedDate = new DateOnly(2010, 5, 31);
            _target.SortOnColumn(4, false);
            Assert.AreEqual(expectedDate, _target.PersonsAffectedPeriodDates[0].Value);
            expectedDate = new DateOnly(2010, 5, 3);
            _target.SortOnColumn(4, true);
            Assert.AreEqual(expectedDate, _target.PersonsAffectedPeriodDates[0].Value);

            ////SchedulePeriodTargetTime
            //TimeSpan expectedTargetTime = _target.GetRowData(1).SchedulePeriodTargetTime;
            //_target.SortOnColumn(5, false);
            //Assert.AreEqual(expectedTargetTime, new TimeSpan());
            //_target.SortOnColumn(5, true);
            //Assert.AreEqual(expectedTargetTime, new TimeSpan());

            //SchedulePeriodTargetDaysOff
            _target.SortOnColumn(6, false);
            Assert.AreEqual(person, _target.PersonsAffectedPeriodDates[0].Key);
            _target.SortOnColumn(6, true);
            Assert.AreEqual(_person, _target.PersonsAffectedPeriodDates[0].Key);

            //CurrentContractTime
            _target.SortOnColumn(7, false);
            Assert.AreEqual(_person, _target.PersonsAffectedPeriodDates[0].Key);
            _target.SortOnColumn(7, true);
            Assert.AreEqual(_person, _target.PersonsAffectedPeriodDates[0].Key);

            //CurrentDaysOff
            _target.SortOnColumn(8, false);
            Assert.AreEqual(_person, _target.PersonsAffectedPeriodDates[0].Key);
            _target.SortOnColumn(8, true);
            Assert.AreEqual(_person, _target.PersonsAffectedPeriodDates[0].Key);

            //MinPossiblePeriodTime
            _target.SortOnColumn(9, false);
            Assert.AreEqual(_person, _target.PersonsAffectedPeriodDates[0].Key);
            _target.SortOnColumn(9, true);
            Assert.AreEqual(_person, _target.PersonsAffectedPeriodDates[0].Key);

            //MaxPossiblePeriodTime
            _target.SortOnColumn(10, false);
            Assert.AreEqual(_person, _target.PersonsAffectedPeriodDates[0].Key);
            _target.SortOnColumn(10, true);
            Assert.AreEqual(_person, _target.PersonsAffectedPeriodDates[0].Key);

            //NumberOfDatesWithPreferenceOrScheduledDaysOff
            _target.SortOnColumn(11, false);
            Assert.AreEqual(_person, _target.PersonsAffectedPeriodDates[0].Key);
            _target.SortOnColumn(11, true);
            Assert.AreEqual(_person, _target.PersonsAffectedPeriodDates[0].Key);

            //NumberOfWarnings/Period ok
            helper = _target.GetRowData(1);
            helper.NumberOfWarnings = 5;
            _target.SortOnColumn(12, false);
            Assert.AreEqual(_person, _target.PersonsAffectedPeriodDates[0].Key);
            expectedDate = new DateOnly(2010, 5, 31);
            Assert.AreEqual(expectedDate, _target.PersonsAffectedPeriodDates[0].Value);
            _target.SortOnColumn(12, true);
            Assert.AreEqual(person, _target.PersonsAffectedPeriodDates[8].Key);
            Assert.AreEqual(expectedDate, _target.PersonsAffectedPeriodDates[8].Value);
        }

        [Test]
        public void VerifySetSelectedPersonDate()
        {
            _target.Initialize(_loadedPersons, _stateHolder, _schedulingOptions);
            Assert.IsTrue(_target.PersonsAffectedPeriodDates.Count >0);
            _target.SetSelectedPersonDate(1);
            Assert.AreEqual(_target.PersonsAffectedPeriodDates[1], _target.SelectedPersonDate);

            _target.SetSelectedPersonDate(100);
            Assert.AreEqual(_target.PersonsAffectedPeriodDates[1], _target.SelectedPersonDate);
        }

        [Test]
        public void VerifyIndexOf()
        {
            _target.Initialize(_loadedPersons, _stateHolder, _schedulingOptions);
            _target.SetSelectedPersonDate(1);
            Assert.AreEqual(1, _target.IndexOf());
        }
    }
}
