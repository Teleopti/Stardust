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

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Islands.CommandHandler
{
	[DomainTest]
	[TestFixture(SUT.IntradayOptimization)]
	[TestFixture(SUT.Scheduling)]
	public class CommandHandlerExternalStaffTest
	{
		private readonly SUT _sut;
		public IntradayOptimizationCommandHandler IntradayOptimizationCommandHandler;
		public SchedulingCommandHandler SchedulingCommandHandler;
		public FakeEventPublisher EventPublisher;
		public FakePersonRepository PersonRepository;
		public FakeSkillRepository SkillRepository;
		public FakeSkillCombinationResourceReader SkillCombinationResourceReader;
		public OptimizationPreferencesDefaultValueProvider OptimizationPreferencesProvider;
		public SchedulingOptionsProvider SchedulingOptionsProvider;

		public CommandHandlerExternalStaffTest(SUT sut)
		{
			_sut = sut;
		}

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

			executeTarget(new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10));

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

			executeTarget(period.ToDateOnlyPeriod(TimeZoneInfo.Utc), teamBlockType);

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

			executeTarget(period.ToDateOnlyPeriod(TimeZoneInfo.Utc), teamBlockType);

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single().AgentsInIsland
				.Should().Have.SameValuesAs(agent1.Id.Value, agent2.Id.Value);
		}
		
		private void executeTarget(DateOnlyPeriod period, TeamBlockType? teamBlockType = null)
		{
			switch (_sut)
			{
				case SUT.Scheduling:
					if (teamBlockType.HasValue)
					{
						SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
						{
							UseTeam = teamBlockType == TeamBlockType.Team
						});
					}
					SchedulingCommandHandler.Execute(new SchedulingCommand { Period = period });
					break;
				case SUT.IntradayOptimization:
					if (teamBlockType.HasValue)
					{
						OptimizationPreferencesProvider.SetFromTestsOnly(new OptimizationPreferences
						{
							Extra = teamBlockType.Value.CreateExtraPreferences()
						});
					}
					IntradayOptimizationCommandHandler.Execute(new IntradayOptimizationCommand { Period = period });
					break;
				default:
					throw new NotSupportedException();
			}
		}
	}
}