using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class ScheduleAffectsSameDayAndPersonTest
    {
        private IScenario _scenario;
        private IScheduleDateTimePeriod _period;
        private IPerson _person;
        private DateOnly _date;
	    private IPersistableScheduleDataPermissionChecker _permissionChecker;
       

        [SetUp]
        public void Setup()
        {
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _period = new ScheduleDateTimePeriod(new DateTimePeriod(2001, 1, 1, 2001, 12, 1));
            _person = PersonFactory.CreatePerson("Bosse");
            _date = new DateOnly(2001, 1, 12);
			_permissionChecker = new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make());
        }


        [Test]
        public void VerifyReturnsTrueIfItsTheSameDateAndPerson()
        {
			var currentAuthorization = CurrentAuthorization.Make();
			IScheduleDay scheduleDay = ExtractedSchedule.CreateScheduleDay(new ScheduleDictionary(_scenario,_period, _permissionChecker, currentAuthorization), _person, _date, CurrentAuthorization.Make());
            IScheduleDay scheduleWithSameDateAndPerson = ExtractedSchedule.CreateScheduleDay(new ScheduleDictionary(_scenario,_period, _permissionChecker, currentAuthorization), _person, _date, CurrentAuthorization.Make());
            ScheduleAffectsSameDayAndPerson scheduleAffectsSameDayAndPerson = new ScheduleAffectsSameDayAndPerson(scheduleDay);
            
            Assert.IsTrue(scheduleAffectsSameDayAndPerson.IsSatisfiedBy(scheduleWithSameDateAndPerson));
        }

        [Test]
        public void VerifyIsNotSatisfiedIfTheDateOrThePersonDoesNotMatchTheOriginalScheduleDay()
        {
            IPerson anotherPerson = PersonFactory.CreatePerson("Not Bosse");
            DateOnly anotherDate = new DateOnly(2001,1,15);

			var currentAuthorization = CurrentAuthorization.Make();
			IScheduleDay scheduleDay = ExtractedSchedule.CreateScheduleDay(new ScheduleDictionary(_scenario, _period, _permissionChecker, currentAuthorization), _person, _date, CurrentAuthorization.Make());
            IScheduleDay scheduleWithAnotherPerson = ExtractedSchedule.CreateScheduleDay(new ScheduleDictionary(_scenario, _period, _permissionChecker, currentAuthorization), anotherPerson, _date, CurrentAuthorization.Make());
            IScheduleDay scheduleWithAnotherDate = ExtractedSchedule.CreateScheduleDay(new ScheduleDictionary(_scenario, _period, _permissionChecker, currentAuthorization), _person, anotherDate, CurrentAuthorization.Make());

            ScheduleAffectsSameDayAndPerson scheduleAffectsSameDayAndPerson = new ScheduleAffectsSameDayAndPerson(scheduleDay);
            Assert.IsFalse(scheduleAffectsSameDayAndPerson.IsSatisfiedBy(scheduleWithAnotherPerson),"Is not satisfied because its another person");
            Assert.IsFalse(scheduleAffectsSameDayAndPerson.IsSatisfiedBy(scheduleWithAnotherDate), "Is not satisfied because its another date");
        }

        [Test]
        public void VerifyIsNotSatisfiedIfAnyOfTheParametersAreNull()
        {
            IScheduleDay scheduleDay = ExtractedSchedule.CreateScheduleDay(new ScheduleDictionary(_scenario, _period, _permissionChecker, CurrentAuthorization.Make()), _person, _date, CurrentAuthorization.Make());
            ScheduleAffectsSameDayAndPerson scheduleAffectsSameDayAndPerson = new ScheduleAffectsSameDayAndPerson(scheduleDay);
            Assert.IsFalse(scheduleAffectsSameDayAndPerson.IsSatisfiedBy(null),"returns false instead of throwing an error for easier use");
        }
    }
}
