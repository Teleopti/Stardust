using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Optimization
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ProTest"), TestFixture]
    public class ScheduleDayProTest
    {
        private ScheduleDayPro _target;
        private ISchedulingResultStateHolder _stateHolder;
        private DateOnly _day;
        private IPerson _person;
        private ScheduleMatrixPro _scheduleMatrix;
        private IVirtualSchedulePeriod _schedulePeriod;
	    private IPersistableScheduleDataPermissionChecker _permissionChecker;
        
        [SetUp]
        public void Setup()
        {
            _stateHolder = new SchedulingResultStateHolder();
            _person = PersonFactory.CreatePerson("Testor");
			_permissionChecker = new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make());

            DateTimePeriod wholePeriod = new DateTimePeriod(1999, 12, 15, 2000, 01, 14);
            IScheduleDateTimePeriod scheduleDateTimePeriod = new ScheduleDateTimePeriod(wholePeriod);
            IScenario scenario = new Scenario("Scenario");
            var scheduleDictionary = new ScheduleDictionaryForTest(scenario, scheduleDateTimePeriod, new Dictionary<IPerson, IScheduleRange>());

            DateTimePeriod dayPeriod = new DateTimePeriod(2000, 01, 01, 2000, 01, 10);
            IScheduleParameters parameters = new ScheduleParameters(scenario, _person, dayPeriod);
            IScheduleRange range = new ScheduleRange(scheduleDictionary, parameters, _permissionChecker, CurrentAuthorization.Make());

            scheduleDictionary.AddTestItem(_person, range);

            _stateHolder.Schedules = scheduleDictionary;


            _day = new DateOnly(2000, 01, 01);

            var splitChecker = new VirtualSchedulePeriodSplitChecker(_person);
            _schedulePeriod = new VirtualSchedulePeriod(_person, _day, splitChecker);
            _scheduleMatrix = ScheduleMatrixProFactory.Create(new DateOnlyPeriod(_day, _day), _stateHolder, _person, _schedulePeriod);
            _target = new ScheduleDayPro(_day, _scheduleMatrix);
        }

        [Test]
        public void TestConstructors()
        {
            Assert.IsNotNull(_target);
            Assert.AreEqual(_stateHolder.Schedules[_person], _target.ActiveScheduleRange);
            Assert.AreSame(_person, _target.Person);
            Assert.AreEqual(_day, _target.Day);
        }

        [Test]
        public void VerifyDaySchedulePart()
        {
            DateTimePeriod wholePeriod = new DateTimePeriod(1999, 12, 15, 2000, 01, 14);
            IScheduleDateTimePeriod scheduleDateTimePeriod = new ScheduleDateTimePeriod(wholePeriod);
            IScenario scenario = new Scenario("Scenario");

            var scheduleDictionary = new ScheduleDictionaryForTest(scenario, scheduleDateTimePeriod, new Dictionary<IPerson, IScheduleRange>());

            DateTimePeriod dayPeriod = new DateTimePeriod(2000, 01, 01, 2000, 01, 10);
            IScheduleParameters parameters = new ScheduleParameters(scenario, _person, dayPeriod);
            IScheduleRange range = new ScheduleRange(scheduleDictionary, parameters, _permissionChecker, CurrentAuthorization.Make());

            scheduleDictionary.AddTestItem(_person, range);

            _stateHolder.Schedules = scheduleDictionary;

            IScheduleDay result = _target.DaySchedulePart();

            Assert.IsNotNull(result);
        }
    }
}