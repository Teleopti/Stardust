using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	public class SchedulingCommandHandlerExternalStaffTest
	{
		public SchedulingCommandHandler Target;
		public FakeEventPublisher EventPublisher;
		public FakePersonRepository PersonRepository;
		public FakeSkillRepository SkillRepository;
		public FakeSkillCombinationResourceReader SkillCombinationResourceReader;
		public SchedulingOptionsProvider SchedulingOptionsProvider;

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
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
			{
				UseTeam = teamBlockType == TeamBlockType.Team
			});

			Target.Execute(new SchedulingCommand { Period = period.ToDateOnlyPeriod(TimeZoneInfo.Utc) });

			EventPublisher.PublishedEvents.OfType<SchedulingWasOrdered>().Single().Agents
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
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
			{
				UseTeam = teamBlockType == TeamBlockType.Team
			});

			Target.Execute(new SchedulingCommand { Period = period.ToDateOnlyPeriod(TimeZoneInfo.Utc) });

			EventPublisher.PublishedEvents.OfType<SchedulingWasOrdered>().Single().AgentsInIsland
				.Should().Have.SameValuesAs(agent1.Id.Value, agent2.Id.Value);
		}
	}
}