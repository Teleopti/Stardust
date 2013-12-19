using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Export
{
    [TestFixture]
    public class ImportForecastToSkillCommandTest
    {
        private IImportForecastToSkillCommand _target;
        private MockRepository _mocks;
        private ISendBusMessage _sendBusMessage;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _sendBusMessage = _mocks.DynamicMock<ISendBusMessage>();
            _target = new ImportForecastToSkillCommand(_sendBusMessage);
        }

        [Test]
        public void ShouldImportForecastToSkill()
        {
            var sourceSkill = SkillFactory.CreateSkill("Source Skill");
            var targetSkill = SkillFactory.CreateSkill("Target Skill");
            var skill = SkillFactory.CreateSkill("hej");
            skill.DefaultResolution = 15;
            var skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(skill);
            var dateTimePeriod = new DateTimePeriod(new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                                                    new DateTime(2000, 1, 1, 10, 15, 0, DateTimeKind.Utc));
            var skillStaffPeriod = new SkillStaffPeriod(dateTimePeriod, new Task(10,TimeSpan.FromMinutes(2),TimeSpan.FromMinutes(1)), new ServiceAgreement(),
                                                        new Domain.Calculation.StaffingCalculatorService());
            skillStaffPeriodDictionary.Add(dateTimePeriod, skillStaffPeriod);
            var forecastRow = new ForecastsRow {AfterTaskTime = 1, TaskTime = 12, Tasks = 10};
            using(_mocks.Record())
            {
                Expect.Call(() => _sendBusMessage.Process(new[] { forecastRow }, targetSkill, new DateOnlyPeriod())).IgnoreArguments();
            }
            using (_mocks.Playback())
            {
                _target.Execute(sourceSkill, targetSkill, skillStaffPeriodDictionary, new DateOnlyPeriod());
            }
        }
    }
}
