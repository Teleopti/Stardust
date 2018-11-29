using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting.Export
{
    [TestFixture]
    public class ExportSkillToFileCommandModelTest
    {
        private ExportSkillToFileCommandModel model;

        [Test]
        public void ShouldSetValues()
        {
            var exportTypeAgentsAndCalls = TypeOfExport.AgentsAndCalls;
            var filename = @"C:\temp\FileName.csv";
            var skill = SkillFactory.CreateSkill("TestSkill");
            var period = new DateOnlyPeriod(2012, 08, 06,
                                            2012, 08, 10);
            var scenario = ScenarioFactory.CreateScenarioAggregate("TestScenario", true);

            model = new ExportSkillToFileCommandModel
                        {
                            ExportType = exportTypeAgentsAndCalls,
                            FileName = filename,
                            Period = period,
                            Scenario = scenario,
                            Skill = skill
                        };

            Assert.AreEqual(exportTypeAgentsAndCalls, model.ExportType);
            Assert.AreEqual(filename, model.FileName);
            Assert.AreEqual(skill, model.Skill);
            Assert.AreEqual(period, model.Period);
            Assert.AreEqual(scenario, model.Scenario);
            Assert.AreEqual(skill, model.Skill);
        }
    }
}
