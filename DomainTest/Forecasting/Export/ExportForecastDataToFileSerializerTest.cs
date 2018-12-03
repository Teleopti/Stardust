using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting.Export
{
    [TestFixture]
    public class ExportForecastDataToFileSerializerTest
    {
        private IExportForecastDataToFileSerializer _target;
        private IScenario _scenario;
        private ISkill _skill;
        private ISkillDay _skillDay1;
        private ISkillDay _skillDay2;
        private ISkillDay _skillDay3;
        private IList<ISkillDay> _skillDaysList;
        private DateOnlyPeriod _period;
        
        [SetUp]
        public void Setup()
        {
            _target = new ExportForecastDataToFileSerializer();
            _scenario = ScenarioFactory.CreateScenarioAggregate("test scenario", true);
            _skill = SkillFactory.CreateSkill("Test Skill");
            
            _skillDay1 = SkillDayFactory.CreateSkillDay(_skill, new DateOnly(2012, 09, 14));
            _skillDay2 = SkillDayFactory.CreateSkillDay(_skill, new DateOnly(2012, 09, 15));
            _skillDay3 = SkillDayFactory.CreateSkillDay(_skill, new DateOnly(2012, 09, 16));

            _skillDaysList = new List<ISkillDay> {_skillDay1, _skillDay2, _skillDay3};
            _period = new DateOnlyPeriod(new DateOnly(2012, 09, 14), new DateOnly(2012, 09, 14));

            new SkillDayCalculator(_skill, new List<ISkillDay> { _skillDay1, _skillDay2, _skillDay3 }, _period);
        }

        [Test]
        public void ShouldHaveCorrectFormatWhenExportingStaffingOnly()
        {
            var exportForecastModel = new ExportSkillToFileCommandModel
                                          {
                                              Period = new DateOnlyPeriod(new DateOnly(2012, 09, 14),
                                                                          new DateOnly(2012, 09, 14)),
                                              Scenario = _scenario,
                                              Skill = _skill,
                                              ExportType = TypeOfExport.Agents,
                                              FileName = "C:\\OnlyStaffingInfo.csv"
                                          };

            var fileData = _target.SerializeForecastData(_skill, exportForecastModel, _skillDaysList);
            
            Assert.That(fileData.Count(), Is.GreaterThan(0));

            if (fileData.Any())
            {
                const char separator = ',';
                var forecastData = fileData.ElementAt(0).Split(separator);
                Assert.That(forecastData.Count(), Is.EqualTo(7));
            }
        }

        [Test]
        public void ShouldHaveCorrectFormatWhenExportingWorkloadOnly()
        {
            var exportForecastModel = new ExportSkillToFileCommandModel
            {
                Period = new DateOnlyPeriod(new DateOnly(2012, 09, 14),
                                            new DateOnly(2012, 09, 14)),
                Scenario = _scenario,
                Skill = _skill,
                ExportType = TypeOfExport.Calls,
                FileName = "C:\\OnlyWorloadInfo.csv"
            };

            var fileData = _target.SerializeForecastData(_skill, exportForecastModel, _skillDaysList);

            Assert.That(fileData.Count(), Is.GreaterThan(0));

            if (fileData.Any())
            {
                const char separator = ',';
                var forecastData = fileData.ElementAt(0).Split(separator);
                Assert.That(forecastData.Count(), Is.EqualTo(6));
            }
        }

        [Test]
        public void ShouldHaveCorrectFormatWhenExportingWorkloadAndStaffing()
        {
            var exportForecastModel = new ExportSkillToFileCommandModel
            {
                Period = new DateOnlyPeriod(new DateOnly(2012, 09, 14),
                                            new DateOnly(2012, 09, 14)),
                Scenario = _scenario,
                Skill = _skill,
                ExportType = TypeOfExport.AgentsAndCalls,
                FileName = "C:\\BothStaffingAndWorloadInfo.csv"
            };

            var fileData = _target.SerializeForecastData(_skill, exportForecastModel, _skillDaysList);

            Assert.That(fileData.Count(), Is.GreaterThan(0));

            if (fileData.Any())
            {
                const char separator = ',';
                var forecastData = fileData.ElementAt(0).Split(separator);
                Assert.That(forecastData.Count(), Is.EqualTo(7));
            }
        }
    }
}
