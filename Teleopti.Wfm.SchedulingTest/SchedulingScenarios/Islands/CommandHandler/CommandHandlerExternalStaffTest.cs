using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.Islands.CommandHandler
{
	public class CommandHandlerExternalStaffTest : ResourcePlannerCommandHandlerTest
	{
		public FakeEventPublisher EventPublisher;
		public FakeSkillRepository SkillRepository;
		public FakeSkillCombinationResourceReader SkillCombinationResourceReader;

		[Test]
		public void ShouldConsiderExternalStaffWhenCreatingIslands()
		{
			var skill1 = new Skill().DefaultResolution(60).WithId();
			var skill2 = new Skill().DefaultResolution(60).WithId();
			var agent1 = new Person().WithId().WithPersonPeriod(skill1);
			var agent2 = new Person().WithId().WithPersonPeriod(skill2);
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);
			SkillRepository.Has(skill1);
			SkillRepository.Has(skill2);
			//prevents creating two different islands
			SkillCombinationResourceReader.Has(1,
				new DateTimePeriod(new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc),
					new DateTime(2000, 1, 1, 13, 0, 0, DateTimeKind.Utc)), skill1, skill2);

			ExecuteTarget(new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10));

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Count()
				.Should().Be.EqualTo(1);
		}

		[TestCase(TeamBlockType.Individual)]
		[TestCase(TeamBlockType.Team)]
		public void ShouldNotOptimizeExternalStaff(TeamBlockType teamBlockType)
		{
			var skill = new Skill().DefaultResolution(60).WithId();
			var agent1 = new Person().WithId().WithPersonPeriod(skill);
			var agent2 = new Person().WithId().WithPersonPeriod(skill);
			var period = new DateTimePeriod(new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 13, 0, 0, DateTimeKind.Utc));
			
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);
			SkillRepository.Has(skill);
			SkillCombinationResourceReader.Has(1, period, skill);

			ExecuteTarget(period.ToDateOnlyPeriod(TimeZoneInfo.Utc), teamBlockType);

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single().Agents
				.Should().Have.SameValuesAs(agent1.Id.Value, agent2.Id.Value);
		}

		[TestCase(TeamBlockType.Individual)]
		[TestCase(TeamBlockType.Team)]
		public void ShouldNotIncludeExternalStaffInAgentsInIslands(TeamBlockType teamBlockType)
		{
			var skill = new Skill().DefaultResolution(60).WithId();
			var agent1 = new Person().WithId().WithPersonPeriod(skill);
			var agent2 = new Person().WithId().WithPersonPeriod(skill);
			var period = new DateTimePeriod(new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 13, 0, 0, DateTimeKind.Utc));
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);
			SkillRepository.Has(skill);
			SkillCombinationResourceReader.Has(1, period, skill);

			ExecuteTarget(period.ToDateOnlyPeriod(TimeZoneInfo.Utc), teamBlockType);

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single().AgentsInIsland
				.Should().Have.SameValuesAs(agent1.Id.Value, agent2.Id.Value);
		}
		
		public CommandHandlerExternalStaffTest(SUT sut) : base(sut)
		{
		}
	}
}