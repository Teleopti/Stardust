using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class ScheduleAffectsSameDayAndPersonTest
    {
        private IScenario _scenario;
        private IScheduleDateTimePeriod _period;
        private IPerson _person;
        private DateOnly _date;
       

        [SetUp]
        public void Setup()
        {
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _period = new ScheduleDateTimePeriod(new DateTimePeriod(2001, 1, 1, 2001, 12, 1));
            _person = PersonFactory.CreatePerson("Bosse");
            _date = new DateOnly(2001, 1, 12);
        }


        [Test]
        public void VerifyReturnsTrueIfItsTheSameDateAndPerson()
        {

            IScheduleDay scheduleDay = ExtractedSchedule.CreateScheduleDay(new ScheduleDictionary(_scenario,_period), _person, _date);
            IScheduleDay scheduleWithSameDateAndPerson = ExtractedSchedule.CreateScheduleDay(new ScheduleDictionary(_scenario,_period), _person, _date);
            ScheduleAffectsSameDayAndPerson scheduleAffectsSameDayAndPerson = new ScheduleAffectsSameDayAndPerson(scheduleDay);
            
            Assert.IsTrue(scheduleAffectsSameDayAndPerson.IsSatisfiedBy(scheduleWithSameDateAndPerson));
        }

        [Test]
        public void VerifyIsNotSatisfiedIfTheDateOrThePersonDoesNotMatchTheOriginalScheduleDay()
        {
            IPerson anotherPerson = PersonFactory.CreatePerson("Not Bosse");
            DateOnly anotherDate = new DateOnly(2001,1,15);

            IScheduleDay scheduleDay = ExtractedSchedule.CreateScheduleDay(new ScheduleDictionary(_scenario, _period), _person, _date);
            IScheduleDay scheduleWithAnotherPerson = ExtractedSchedule.CreateScheduleDay(new ScheduleDictionary(_scenario, _period), anotherPerson, _date);
            IScheduleDay scheduleWithAnotherDate = ExtractedSchedule.CreateScheduleDay(new ScheduleDictionary(_scenario, _period), _person, anotherDate);

            ScheduleAffectsSameDayAndPerson scheduleAffectsSameDayAndPerson = new ScheduleAffectsSameDayAndPerson(scheduleDay);
            Assert.IsFalse(scheduleAffectsSameDayAndPerson.IsSatisfiedBy(scheduleWithAnotherPerson),"Is not satisfied because its another person");
            Assert.IsFalse(scheduleAffectsSameDayAndPerson.IsSatisfiedBy(scheduleWithAnotherDate), "Is not satisfied because its another date");
        }

        [Test]
        public void VerifyIsNotSatisfiedIfAnyOfTheParametersAreNull()
        {
            IScheduleDay scheduleDay = ExtractedSchedule.CreateScheduleDay(new ScheduleDictionary(_scenario, _period), _person, _date);
            ScheduleAffectsSameDayAndPerson scheduleAffectsSameDayAndPerson = new ScheduleAffectsSameDayAndPerson(scheduleDay);
            Assert.IsFalse(scheduleAffectsSameDayAndPerson.IsSatisfiedBy(null),"returns false instead of throwing an error for easier use");
        }
    }
}
