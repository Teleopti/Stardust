using System.Linq;
using NHibernate.Util;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ApplicationLayer
{
	//Reuse tests here later when same/similar stuff are used for creating islands when scheduling, DO opt etc

	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_SplitBigIslands_42049)]
	public class IntradayOptimizationCommandHandlerIncreaseIslandsTest
	{
		public IntradayOptimizationCommandHandler Target;
		public FakeEventPublisher EventPublisher;
		public FakePersonRepository PersonRepository;
		public ReduceIslandsLimits ReduceIslandsLimits;

		[Test, Ignore("Anderstestet")]
		public void ShouldMakeTwoIslandsByMakingAgentsSingleSkilledIfOtherSkillgroupIsBigEnough()
		{
			ReduceIslandsLimits.SetMinimumNumberOfAgentsInIsland_UseOnlyFromTest(5);
			var skillA = new Skill();
			var skillB = new Skill();
			var skillAagents = Enumerable.Repeat(new Person().KnowsSkill(skillA), 40);
			var skillABagents = Enumerable.Repeat(new Person().KnowsSkill(skillA, skillB), 9);
			skillAagents.Union(skillABagents).ForEach(PersonRepository.Has);

			Target.Execute(new IntradayOptimizationCommand { Period = DateOnly.Today.ToDateOnlyPeriod() });

			EventPublisher.PublishedEvents.OfType<OptimizationWasOrdered>().Count()
				.Should().Be.EqualTo(2);
		}
	}
}