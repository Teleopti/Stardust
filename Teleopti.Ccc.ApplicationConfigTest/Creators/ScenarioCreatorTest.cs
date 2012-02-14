using System;
using NUnit.Framework;
using Teleopti.Ccc.ApplicationConfig.Creators;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfigTest.Creators
{
    [TestFixture]
    public class ScenarioCreatorTest
    {
        private ScenarioCreator _target;

        [SetUp]
        public void Setup()
        {
            _target = new ScenarioCreator();
        }

        [Test]
        public void VerifyCanCreateScenario()
        {
            var scenario = _target.Create("name", new Description("name", "shortName"), true, true, true);
            Assert.AreEqual(new Description("name", "shortName"), scenario.Description);
            Assert.AreEqual(true, scenario.DefaultScenario);
            Assert.AreEqual(true, scenario.EnableReporting);
			Assert.IsTrue(scenario.Restricted);
        }

    }
}
