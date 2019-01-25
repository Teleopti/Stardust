using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	public class IntradayOptimizationStandardDeviationTest : IntradayOptimizationScenarioTest
	{
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public IntradayOptimizationFromWeb Target;
		public FakePlanningPeriodRepository PlanningPeriodRepository;

		[Test]
		public void ShouldNotRollBackEvenIfStandardDevGetsHigherToggle42767()
		{
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			phoneActivity.RequiresSkill = true;
			var mailActivity = ActivityFactory.CreateActivity("mail");
			mailActivity.RequiresSkill = true;
			var phoneSkill = SkillRepository.Has("phone", phoneActivity, new TimePeriod(8, 16)).InTimeZone(TimeZoneInfo.Utc);
			phoneSkill.DefaultResolution = 30;

			var mailSkill = SkillRepository.Has("mail", mailActivity, new TimePeriod(0, 24)).InTimeZone(TimeZoneInfo.Utc);
			mailSkill.DefaultResolution = 60;
			var dateOnly = new DateOnly(2015, 10, 12);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Day, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract")
			{
				WorkTimeDirective = worktimeDirective,
				EmploymentType = EmploymentType.FixedStaffDayWorkTime,
				WorkTime = new WorkTime(TimeSpan.FromHours(8))
			};
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet1 =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 0, 8, 0, 15),
					new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			ruleSet1.AddExtender(new ActivityRelativeStartExtender(mailActivity, new TimePeriodWithSegment(4, 0, 4, 0, 15),
				new TimePeriodWithSegment(0, 0, 4, 0, 120)));
			var ruleSet2 =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(mailActivity, new TimePeriodWithSegment(8, 0, 8, 0, 15),
					new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));

			var agent1 = PersonRepository.Has(contract, schedulePeriod, ruleSet1, phoneSkill, mailSkill);
			agent1.SetName(new Name("1", "1"));
			var agent2 = PersonRepository.Has(contract, schedulePeriod, ruleSet2, phoneSkill, mailSkill);
			agent2.SetName(new Name("2", "2"));

			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			SkillDayRepository.Has(new List<ISkillDay>
				{
					phoneSkill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromHours(1)),
					mailSkill.CreateEmailSkillDayWithIncomingDemandOncePerDay(scenario,dateOnly,TimeSpan.FromHours(5), new TimePeriod(0,24))
				});

			PersonAssignmentRepository.Has(agent1, scenario, phoneActivity, new ShiftCategory("x"), dateOnly, new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.Has(agent2, scenario, mailActivity, new ShiftCategory("x"), dateOnly, new TimePeriod(8, 0, 16, 0));

			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(dateOnly, agent1).ShiftCategory.Should().Be.EqualTo(shiftCategory);
		}
	}
}