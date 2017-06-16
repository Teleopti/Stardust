using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
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
	[TestFixture(typeof(FakeCancelSchedulingProgress), true)]
	[TestFixture(typeof(FakeCloseSchedulingProgress), true)]
	[TestFixture(typeof(FakeCancelSchedulingProgress), false)]
	[TestFixture(typeof(FakeCloseSchedulingProgress), false)]
	public class DoNotRunNightRestWhiteSpotTest : SchedulingScenario, ISetup
	{
		private readonly Type _schedulingProgressFake;
		public FullScheduling Target;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository AssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public CountCallsToNightRestWhiteSpotSolverServiceFactory NightRestWhiteSpotSolverService;
		public SchedulingOptionsProvider SchedulingOptionsProvider;

		public DoNotRunNightRestWhiteSpotTest(Type schedulingProgressFake, bool resourcePlannerMergeTeamblockClassicScheduling44289) : base(resourcePlannerMergeTeamblockClassicScheduling44289)
		{
			_schedulingProgressFake = schedulingProgressFake;
		}

		[Test]
		public void ShouldNotRunNightlyRestIfCancelled()
		{
			if(ResourcePlannerMergeTeamblockClassicScheduling44289)
				Assert.Ignore("TODO - Should probably be fixed");
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
			{
				RefreshRate = 1 //to force ISchedulingProgress to be called so our "cancel click" will be triggered
			});
			var firstDay = new DateOnly(2015, 10, 12);
			var period = new DateOnlyPeriod(firstDay, firstDay.AddDays(7));
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var scenario = ScenarioRepository.Has("some name");
			var contract = new Contract("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), ruleSet, skill);
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, firstDay, 1));

			Target.DoScheduling(period);

			NightRestWhiteSpotSolverService.NumberOfNightRestWhiteSpotServiceCalls
				.Should().Be.EqualTo(0);
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDoubleForType(_schedulingProgressFake).For<ISchedulingProgress>();
			system.UseTestDouble<CountCallsToNightRestWhiteSpotSolverServiceFactory>().For<INightRestWhiteSpotSolverServiceFactory>();
		}
	}
}