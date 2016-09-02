﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_CascadingSkills_38524)]
	public class SchedulingCascadingTest : ISetup
	{
		public FullScheduling Target;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository AssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;

		[Test]
		public void ShouldBaseBestShiftOnNonShoveledResourceCalculation()
		{
			const int numberOfAgents = 100;
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var earlyInterval = new TimePeriod(7, 45, 8, 0);
			var lateInterval = new TimePeriod(15, 45, 16, 0);
			var date = DateOnly.Today;
			var activity = ActivityRepository.Has("_");
			var skillA = SkillRepository.Has("A", activity, 1);
			var skillB = SkillRepository.Has("B", activity, 2);
			var scenario = ScenarioRepository.Has("default");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
					new TimePeriodWithSegment(earlyInterval, TimeSpan.FromMinutes(15)),
					new TimePeriodWithSegment(lateInterval, TimeSpan.FromMinutes(15)), shiftCategory));

			var contract = new Contract("_")
			{
				WorkTimeDirective =
					new WorkTimeDirective(TimeSpan.FromHours(1), TimeSpan.FromHours(168), TimeSpan.FromHours(1), TimeSpan.FromHours(1))
			};

			for (var i = 0; i < numberOfAgents; i++)
			{
				PersonRepository.Has(contract, new SchedulePeriod(date, SchedulePeriodType.Day, 1), ruleSet, skillA, skillB);
			}
			SkillDayRepository.Has(new[]
			{
				skillA.CreateSkillDayWithDemand(scenario, date, 1),
				skillB.CreateSkillDayWithDemandOnInterval(scenario, date, 1, new Tuple<TimePeriod, double>(lateInterval, 1000))
				//should not shovel resources here when deciding what shift to choose
			});

			Target.DoScheduling(date.ToDateOnlyPeriod());

			var allAssignmentsStartTime = AssignmentRepository.LoadAll().Select(x => x.Period.StartDateTime.TimeOfDay);
			allAssignmentsStartTime.Count(x => x == new TimeSpan(7, 45, 0))
				.Should().Be.EqualTo(numberOfAgents/2);
			allAssignmentsStartTime.Count(x => x == new TimeSpan(8, 0, 0))
				.Should().Be.EqualTo(numberOfAgents/2);
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			//TODO: Remove this!
			system.UseTestDouble<FakeGroupScheduleGroupPageDataProvider>().For<IGroupScheduleGroupPageDataProvider>();
		}
	}
}