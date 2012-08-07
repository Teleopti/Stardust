using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Export
{
    [TestFixture]
    public class ExportSkillToFileCommandModelTest
    {
        private ExportSkillToFileCommandModel _target;

        [SetUp]
        public void Setup()
        {
            _target = new ExportSkillToFileCommandModel();
            _target.ExportType = TypeOfExport.AgentsAndCalls;
            _target.FileName = "test.csv";
            _target.Period = new DateOnlyPeriod(new DateOnly(2012,01,01), new DateOnly(2012,01,02));
            _target.Scenario = ScenarioFactory.CreateScenarioAggregate();
            _target.Skill = SkillFactory.CreateSkill("Test Skill");
        }

        [Test]
        public void ShouldInitializeModel()
        {
            Assert.That(_target.ExportType, Is.EqualTo(TypeOfExport.AgentsAndCalls));
            Assert.IsNotNull(_target.FileName);
            Assert.IsNotNull(_target.Period);
            Assert.IsNotNull(_target.Scenario);
            Assert.IsNotNull(_target.Skill);
        }

    }
}
