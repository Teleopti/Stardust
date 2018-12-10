using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.InfrastructureTest.Scheduling.ScheduleRangePersisterWithConflictSolve
{
	[Category("BucketC")]
	[InfrastructureTest]
	public abstract class ScheduleRangePersisterWithConflictSolveBase : IIsolateSystem
	{
		public WithUnitOfWork WithUnitOfWork { get; set; }

		public IScheduleDictionaryPersister Target;

		public IScenarioRepository ScenarioRepository;
		public IDayOffTemplateRepository DayOffTemplateRepository;
		public IPersonRepository PersonRepository;
		public ITeamRepository TeamRepository;
		public ISiteRepository SiteRepository;
		public IPartTimePercentageRepository PartTimePercentageRepository;
		public IContractRepository ContractRepository;
		public IContractScheduleRepository ContractScheduleRepository;
		public IRuleSetBagRepository RuleSetBagRepository;
		public IWorkShiftRuleSetRepository WorkShiftRuleSetRepository;
		public IActivityRepository ActivityRepository;
		public IShiftCategoryRepository ShiftCategoryRepository;
		public IScheduleStorage ScheduleStorage;
		public IPersonAssignmentRepository PersonAssignmentRepository;

		protected readonly DateOnly StartDate = new DateOnly(2017, 07, 03);

		protected IPerson Agent;
		protected IScenario Scenario;
		protected IActivity Activity;
		protected IDayOffTemplate DayOffTemplate;

		[Test]
		public void RunTest()
		{
			WithUnitOfWork.Do(createBasicEntitiesForScheduling);
			WithUnitOfWork.Do(CreateBaseSchedules);

			var firstScheduleDictionary = WithUnitOfWork.Get(() => ScheduleStorage.FindSchedulesForPersons(Scenario,
				new[] {Agent}, new ScheduleDictionaryLoadOptions(false, false), DateOnlyPeriod.CreateWithNumberOfWeeks(StartDate, 1).ToDateTimePeriod(TimeZoneInfo.Local), new[] {Agent}, false));
			var secondScheduleDictionary = WithUnitOfWork.Get(() => ScheduleStorage.FindSchedulesForPersons(Scenario,
				new[] {Agent}, new ScheduleDictionaryLoadOptions(false, false), DateOnlyPeriod.CreateWithNumberOfWeeks(StartDate, 1).ToDateTimePeriod(TimeZoneInfo.Local), new[] {Agent}, false));

			ModifyFirst(firstScheduleDictionary);
			Target.Persist(firstScheduleDictionary);
			
			ModifySecond(secondScheduleDictionary);
			Target.Persist(secondScheduleDictionary);
			
			WithUnitOfWork.Do(() =>
			{
				var result = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(Agent,
					new ScheduleDictionaryLoadOptions(false, false), DateOnlyPeriod.CreateWithNumberOfWeeks(StartDate, 1), Scenario);
				VerifyResult(result);
			});
		}

		protected void DoModify(IScheduleDay scheduleDay)
		{
			scheduleDay.Owner.Modify(ScheduleModifier.Scheduler, scheduleDay, NewBusinessRuleCollection.Minimum(),
				new DoNothingScheduleDayChangeCallBack(),
				new NoScheduleTagSetter());
		}

		private void createBasicEntitiesForScheduling()
		{
			Scenario = new Scenario("_") { DefaultScenario = true };
			Activity = new Activity("_");
			DayOffTemplate = new DayOffTemplate(new Description("_"));
			var shiftCategory = new ShiftCategory("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(Activity, new TimePeriodWithSegment(1, 1, 1, 1, 1), new TimePeriodWithSegment(1, 1, 1, 1, 1), shiftCategory)) { Description = new Description("_") };
			var ruleSetBag = new RuleSetBag(ruleSet) { Description = new Description("_") };
			var team = new Team();
			team.SetDescription(new Description("_"));
			team.Site = new Site("_");
			Agent = new Person()
				.WithPersonPeriod(ruleSetBag, null, team).InTimeZone(TimeZoneInfo.Utc)
				.WithSchedulePeriodOneWeek(StartDate);

			ScenarioRepository.Add(Scenario);
			
			DayOffTemplateRepository.Add(DayOffTemplate);
			SiteRepository.Add(Agent.Period(StartDate).Team.Site);
			TeamRepository.Add(Agent.Period(StartDate).Team);
			PartTimePercentageRepository.Add(Agent.Period(StartDate).PersonContract.PartTimePercentage);
			ContractRepository.Add(Agent.Period(StartDate).PersonContract.Contract);
			ContractScheduleRepository.Add(Agent.Period(StartDate).PersonContract.ContractSchedule);
			ActivityRepository.Add(Activity);
			ShiftCategoryRepository.Add(shiftCategory);
			WorkShiftRuleSetRepository.Add(Agent.Period(StartDate).RuleSetBag.RuleSetCollection.Single());
			RuleSetBagRepository.Add(Agent.Period(StartDate).RuleSetBag);
			PersonRepository.Add(Agent);
		}

		protected abstract void CreateBaseSchedules();

		protected abstract void ModifyFirst(IScheduleDictionary scheduleDictionary);

		protected abstract void ModifySecond(IScheduleDictionary scheduleDictionary);

		protected abstract void VerifyResult(IScheduleDictionary result);

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FullPermission>().For<IAuthorization>();
		}
	}
}