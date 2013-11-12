using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class SchedulerSkillDayHelperTest
    {
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private DateOnlyPeriod _dateTimePeriod;
        private ISkillDayRepository _skillDayRepository;
        private IScenario _scenario;
        private MockRepository _mocks;
        private SchedulerSkillDayHelper _target;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _skillDayRepository = _mocks.StrictMock<ISkillDayRepository>();
            _scenario = _mocks.StrictMock<IScenario>();
            _dateTimePeriod = new DateOnlyPeriod(2011, 1, 17, 2011, 1, 31);
            _target = new SchedulerSkillDayHelper(_schedulingResultStateHolder, _dateTimePeriod, _skillDayRepository,
                                                  _scenario);
        }

        [Test]
        public void ShouldGetSkillDaysFromRepositoryAndAddToStateHolder()
        {
            var skill = _mocks.StrictMock<ISkill>();
            var skillType = _mocks.StrictMock<ISkillType>();
            var skillDay = _mocks.StrictMock<ISkillDay>();
            var skillStaffPeriod = _mocks.StrictMock<ISkillStaffPeriod>();
            var payLoad = _mocks.StrictMock<ISkillStaff>();
            Expect.Call(_schedulingResultStateHolder.Skills).Return(new List<ISkill> {skill});
            Expect.Call(skill.SkillType).Return(skillType);
            Expect.Call(skillType.ForecastSource).Return(ForecastSource.NonBlendSkill);
            Expect.Call(_skillDayRepository.GetAllSkillDays(_dateTimePeriod, new List<ISkillDay>(), skill, _scenario,
                                                            _ => { })).Return(new List<ISkillDay>{skillDay}).IgnoreArguments();
            Expect.Call(skillDay.SkillStaffPeriodCollection).Return(
                new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod> {skillStaffPeriod}));
            Expect.Call(skillStaffPeriod.Payload).Return(payLoad);
            Expect.Call(() => payLoad.NoneBlendDemand = 20);
            Expect.Call(() => payLoad.ServiceAgreementData = new ServiceAgreement(new ServiceLevel(new Percent(1), 1), new Percent(0),
                                                                    new Percent(1))).IgnoreArguments();
            Expect.Call(_schedulingResultStateHolder.SkillDays).Return(new Dictionary<ISkill, IList<ISkillDay>>());
            Expect.Call(() => _schedulingResultStateHolder.SkillDays = new Dictionary<ISkill, IList<ISkillDay>>()).IgnoreArguments();
        	Expect.Call(skillStaffPeriod.CalculateStaff);
            _mocks.ReplayAll();
            _target.AddSkillDaysToStateHolder(ForecastSource.NonBlendSkill,20);
            _mocks.VerifyAll();
        }
    }

    

}