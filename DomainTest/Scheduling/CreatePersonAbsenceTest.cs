using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class CreatePersonAbsenceTest
    {
        private CreatePersonAbsence _target;

        [SetUp]
        public void Setup()
        {
            _target = new CreatePersonAbsence();
        }

        [Test]
        public void VerifyCreatesAbsenceLayerWithParameters()
        {
            DateTimePeriod expectedPeriod = new DateTimePeriod(2001,1,1,2001,1,2);
            IPerson expectedPerson = PersonFactory.CreatePerson("Roger");
            IScenario expectedScenario = ScenarioFactory.CreateScenarioAggregate();
            IAbsence expectedAbsence = AbsenceFactory.CreateAbsence("Illness");


            IPersonAbsence result =
                _target.Create(expectedAbsence, expectedPeriod, expectedScenario, expectedPerson);

            Assert.AreEqual(expectedPeriod, result.Period);
            Assert.AreEqual(expectedPerson, result.Person);
            Assert.AreEqual(expectedScenario, result.Scenario);
            Assert.AreEqual(expectedAbsence, result.Layer.Payload);
        }
    }
}