using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class ScheduleOptimizationTest
	{
		[Test]
		public void ShouldCreateDemand()
		{
			var skill = SkillFactory.CreateSkill("skill");
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);

			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var skillDay1 = creatSkillDayWithDemand(TimeSpan.FromHours(0.5), new DateOnly(2015,1,5), skill, scenario);
			var skillDay2 = creatSkillDayWithDemand(TimeSpan.FromHours(10), new DateOnly(2015, 1, 2), skill, scenario);

			skillDay1.ForecastedIncomingDemand.TotalHours.Should().Be.LessThan(skillDay2.ForecastedIncomingDemand.TotalHours);
		}

		private ISkillDay creatSkillDayWithDemand(TimeSpan fromHours, DateOnly dateOnly, ISkill skill, IScenario scenario)
		{
			var skillDataPeriods = new List<ISkillDataPeriod>();
			var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
			var skillDataPeriod = new SkillDataPeriod(ServiceAgreement.DefaultValues(), new SkillPersonData(),
				dateOnlyPeriod.ToDateTimePeriod(skill.TimeZone)) {ManualAgents = fromHours.TotalHours};

			skillDataPeriods.Add(skillDataPeriod);

			var workloadDays = new List<IWorkloadDay>();
			var workloadDay = new WorkloadDay();
			var workload = skill.WorkloadCollection.First();
			workloadDay.CreateFromTemplate(dateOnly,workload,(IWorkloadDayTemplate) workload.GetTemplate(TemplateTarget.Workload, dateOnly.DayOfWeek));
			workloadDays.Add(workloadDay);

			var skillDay = new SkillDay(dateOnly, skill, scenario, workloadDays, skillDataPeriods);

			skillDay.SkillDayCalculator = new SkillDayCalculator(skill,new List<ISkillDay>{skillDay},dateOnlyPeriod);
			return skillDay;
		}
	}
}
