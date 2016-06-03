using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(RunInProcessEventPublisher))]
	[Toggle(Toggles.ResourcePlanner_CascadingSkills_38524)]
	public class IntradayOptimizationCascadingTest : ISetup
	{
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public IntradayOptimizationFromWeb Target;

		[Test, Ignore]
		public void ShouldOnlyConsiderPrimarySkillDuringOptimization()
		{
			var dateOnly = DateOnly.Today;
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var shiftCategory = new ShiftCategory("_").WithId();
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);

			var activity = new Activity("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));

			var skillA = SkillRepository.Has("skillA", activity, 1);
			var skillB = SkillRepository.Has("skillB", activity, 2);

			var agentA = PersonRepository.Has(contract, schedulePeriod, skillA, skillB).InTimeZone(TimeZoneInfo.Utc);
			agentA.Period(dateOnly).RuleSetBag = new RuleSetBag(ruleSet);

			var dateTime = TimeZoneHelper.ConvertToUtc(dateOnly.Date, agentA.PermissionInformation.DefaultTimeZone());
			SkillDayRepository.Has(new List<ISkillDay>
							 {
								skillA.CreateSkillDayWithDemandOnInterval(scenario,dateOnly,0, new Tuple<TimePeriod, double>(new TimePeriod(17, 15, 17, 30), 1)),
								skillB.CreateSkillDayWithDemandOnInterval(scenario,dateOnly,0, new Tuple<TimePeriod, double>(new TimePeriod(8, 0, 8, 15), 2))
							 });

			PersonAssignmentRepository.Has(agentA, scenario, activity, shiftCategory, dateOnly, new TimePeriod(8, 0, 17, 0));

			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(dateOnly, agentA).Period
					.Should().Be.EqualTo(new DateTimePeriod(dateTime.AddHours(8).AddMinutes(15), dateTime.AddHours(17).AddMinutes(15)));
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			// share the same current principal on all threads
			system.UseTestDouble(new FakeCurrentTeleoptiPrincipal(Thread.CurrentPrincipal as ITeleoptiPrincipal)).For<ICurrentTeleoptiPrincipal>();
		}
	}
}