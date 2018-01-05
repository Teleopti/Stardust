﻿using System;
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
	public class CommandHandlerDecreaseIslandsTest : ResourcePlannerCommandHandlerTest
	{
		public MergeIslandsSizeLimit MergeIslandsSizeLimit;
		public FakePersonRepository PersonRepository;
		public FakeEventPublisher EventPublisher;

		[Test]
		public void ShouldNotMergeIslandsIfBigEnough()
		{
			var agentsOnEachSkill = MergeIslandsSizeLimit.Limit + 1;
			var skillA = new Skill().WithId();
			var skillB = new Skill().WithId();
			var skillAagents = Enumerable.Range(0, agentsOnEachSkill).Select(x => new Person().WithPersonPeriod(skillA).WithId());
			var skillBagents = Enumerable.Range(0, agentsOnEachSkill).Select(x => new Person().WithPersonPeriod(skillB).WithId());
			skillAagents.Union(skillBagents).ForEach(x => PersonRepository.Has(x));

			ExecuteTarget();
			
			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Count()
				.Should().Be.EqualTo(2);
		} 
		
		[Test]
		public void ShouldMergeSmallEnoughIslands()
		{
			var agentsOnEachSkill = MergeIslandsSizeLimit.Limit - 1;
			var skillA = new Skill().WithId();
			var skillB = new Skill().WithId();
			var skillAagents = Enumerable.Range(0, agentsOnEachSkill).Select(x => new Person().WithPersonPeriod(skillA).WithId()).ToArray();
			var skillBagents = Enumerable.Range(0, agentsOnEachSkill).Select(x => new Person().WithPersonPeriod(skillB).WithId()).ToArray();
			var allAgents = skillAagents.Union(skillBagents).ForEach(x => PersonRepository.Has(x));

			ExecuteTarget();

			var island = EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single();
			island.AgentsInIsland.Should().Have.SameValuesAs(allAgents.Select(x => x.Id.Value));
			island.Skills.Should().Have.SameValuesAs(skillA.Id.Value, skillB.Id.Value);
		}

		public CommandHandlerDecreaseIslandsTest(SUT sut, bool noPytteIslands47500) : base(sut, noPytteIslands47500)
		{
			if(!noPytteIslands47500)
				Assert.Ignore("Only valid when toggle is true");
		}
	}
}