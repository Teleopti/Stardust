using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class SchedulerSkillDayHelperTest
    {
		[Test]
		public void ShouldGetSkillDaysFromRepositoryAndAddToStateHolder()
		{
			var datePeriod = new DateOnlyPeriod(2011, 1, 17, 2011, 1, 31);
			var schedulingResultStateHolder = MockRepository.GenerateMock<ISchedulingResultStateHolder>();
            var schedulerStateHolder = MockRepository.GenerateMock<ISchedulerStateHolder>();
            var skillDayRepository = MockRepository.GenerateMock<ISkillDayRepository>();
            var scenario = MockRepository.GenerateMock<IScenario>();
			var skill = MockRepository.GenerateMock<ISkill>();
			var skillType = MockRepository.GenerateMock<ISkillType>();
			var skillDay = MockRepository.GenerateMock<ISkillDay>();
			var skillStaffPeriod = MockRepository.GenerateMock<ISkillStaffPeriod>();
			var payLoad = MockRepository.GenerateMock<ISkillStaff>();
            
			schedulerStateHolder.Stub(x => x.SchedulingResultState).Return(schedulingResultStateHolder);
			schedulerStateHolder.Stub(x => x.RequestedScenario).Return(scenario);
			schedulingResultStateHolder.Stub(x => x.Skills).Return(new [] { skill });
            skill.Stub(x => x.SkillType).Return(skillType);
            skillType.Stub(x => x.ForecastSource).Return(ForecastSource.NonBlendSkill);
            skillDayRepository.Stub(x => x.GetAllSkillDays(datePeriod, new List<ISkillDay>(), skill, scenario,_ => { })).Return(new List<ISkillDay>{skillDay}).IgnoreArguments();
            skillDay.Stub(x => x.SkillStaffPeriodCollection).Return(new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod> {skillStaffPeriod}));
            skillStaffPeriod.Stub(x => x.Payload).Return(payLoad);
			schedulingResultStateHolder.Stub(x => x.SkillDays).Return(new Dictionary<ISkill, IList<ISkillDay>>());

			var target = new SchedulerSkillDayHelper(schedulerStateHolder, skillDayRepository);
			target.AddSkillDaysToStateHolder(datePeriod, ForecastSource.NonBlendSkill, 20);

			skillStaffPeriod.AssertWasCalled(x => x.CalculateStaff());
	        payLoad.AssertWasCalled(x => x.NoneBlendDemand = 20);
        }
    }
}