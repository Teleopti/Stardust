using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Islands.CommandHandler
{
	[Ignore("To be fixed")]
	public class CommandHandlerDecreaseIslandsTest : ResourcePlannerCommandHandlerTest
	{
		public MergeIslandsSizeLimit MergeIslandsSizeLimit;
		public FakePersonRepository PersonRepository;
		public FakeEventPublisher EventPublisher;

		[Test]
		public void ShouldNotMergeIslandsIfBigEnough()
		{
			var agentsOnEachSkill = MergeIslandsSizeLimit.MaximumNumberOfAgentsInIsland + 1;
			var skillA = new Skill().WithId();
			var skillB = new Skill().WithId();
			var skillAagents = Enumerable.Range(0, agentsOnEachSkill).Select(x => new Person().WithPersonPeriod(skillA).WithId());
			var skillABagents = Enumerable.Range(0, agentsOnEachSkill).Select(x => new Person().WithPersonPeriod(skillB).WithId());
			skillAagents.Union(skillABagents).ForEach(x => PersonRepository.Has(x));

			ExecuteTarget();
			
			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Count()
				.Should().Be.EqualTo(2);
		} 
		
		[Test]
		public void ShouldMergeSmallEnoughIslands()
		{
			MergeIslandsSizeLimit.SetValues_UseOnlyFromTest(1);
			const int agentsOnEachSkill = 10;
			var skillA = new Skill().WithId();
			var skillB = new Skill().WithId();
			var skillAagents = Enumerable.Range(0, agentsOnEachSkill).Select(x => new Person().WithPersonPeriod(skillA).WithId());
			var skillABagents = Enumerable.Range(0, agentsOnEachSkill).Select(x => new Person().WithPersonPeriod(skillB).WithId());
			skillAagents.Union(skillABagents).ForEach(x => PersonRepository.Has(x));

			ExecuteTarget();

			var island = EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single();
			island.AgentsInIsland.Should().Have.SameValuesAs(PersonRepository.LoadAll().Select(x => x.Id.Value));
			island.Skills.Should().Have.SameValuesAs(skillA.Id.Value, skillB.Id.Value);
		}

		public CommandHandlerDecreaseIslandsTest(SUT sut, bool noPytteIslands47500) : base(sut, noPytteIslands47500)
		{
		}
	}
}