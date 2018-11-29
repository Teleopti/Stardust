using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;


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
        public void ScenarioCannotBeNull()
        {
			Assert.Throws<ArgumentNullException>(() => target = new ScheduleParameters(null, person, period));
        }

        [Test]
        public void PersonCannotBeNull()
        {
			Assert.Throws<ArgumentNullException>(() => target = new ScheduleParameters(scenario, null, period));
        }
    }
}
