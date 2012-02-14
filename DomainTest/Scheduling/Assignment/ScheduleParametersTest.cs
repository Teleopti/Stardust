using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class ScheduleParametersTest 
    {
        private IScheduleParameters target;
        private IScenario scenario;
        private IPerson person;
        private DateTimePeriod period;

        [SetUp]
        public void Setup()
        {
            scenario = ScenarioFactory.CreateScenarioAggregate();
            person = PersonFactory.CreatePerson("test");
            period = new DateTimePeriod(2000,1,1,2001,1,1);
            target = new ScheduleParameters(scenario, person, period);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreSame(scenario, target.Scenario);
            Assert.AreSame(person, target.Person);
            Assert.AreEqual(period, target.Period);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ScenarioCannotBeNull()
        {
            target = new ScheduleParameters(null, person, period);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonCannotBeNull()
        {
            target = new ScheduleParameters(scenario, null, period);
        }
    }
}
