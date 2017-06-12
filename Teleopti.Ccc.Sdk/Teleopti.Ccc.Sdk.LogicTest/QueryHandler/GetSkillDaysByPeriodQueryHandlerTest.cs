using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetSkillDaysByPeriodQueryHandlerTest
	{
		private GetSkillDaysByPeriodQueryHandler target;
		private IAssembler<ISkillStaffPeriod, SkillDataDto> assembler;
		private IPersonRepository personRepository;
		private ICurrentScenario currentScenario;
		private ISkillRepository skillRepository;
		private IResourceCalculationPrerequisitesLoader resourceCalculationPrerequisitesLoader;
		private IDateTimePeriodAssembler dateTimePeriodAssembler;
		private IResourceCalculation resourceOptimizationHelper;
		private ILoadSchedulingStateHolderForResourceCalculation loadSchedulingStateHolderForResourceCalculation;
		private ISchedulingResultStateHolder schedulingResultStateHolder;
		private FakeSkillDayRepository skillDayRepository;
		private FakeMultisiteDayRepository multisiteDayRepository;

		[SetUp]
		public void Setup()
		{
			dateTimePeriodAssembler = new DateTimePeriodAssembler();
			assembler = new SkillDataAssembler(dateTimePeriodAssembler);
			personRepository = new FakePersonRepositoryLegacy();
			currentScenario = new FakeCurrentScenario();
			skillRepository = new FakeSkillRepository();
			resourceCalculationPrerequisitesLoader =
				new ResourceCalculationPrerequisitesLoader(new FakeCurrentUnitOfWorkFactory(), new FakeContractScheduleRepository(),
					new FakeActivityRepository(), new FakeAbsenceRepository());
			resourceOptimizationHelper =
				new CascadingResourceCalculation(
					new ResourceOptimizationHelper(new OccupiedSeatCalculator(), new NonBlendSkillCalculator(),
						new PersonSkillProvider(), new PeriodDistributionService(),
						new IntraIntervalFinderService(new SkillDayIntraIntervalFinder(new IntraIntervalFinder(),
							new SkillActivityCountCollector(new SkillActivityCounter()), new FullIntervalFinder())), new FakeTimeZoneGuard(),
						new CascadingResourceCalculationContextFactory(new CascadingPersonSkillProvider(), new FakeTimeZoneGuard())),
					new ShovelResources(new ReducePrimarySkillResources(), new AddResourcesToSubSkills(), 
						new SkillGroupPerActivityProvider(), new PrimarySkillOverstaff(), new FakeTimeZoneGuard()));
			skillDayRepository = new FakeSkillDayRepository();
			multisiteDayRepository = new FakeMultisiteDayRepository();
			loadSchedulingStateHolderForResourceCalculation =
				new LoadSchedulingStateHolderForResourceCalculation(personRepository, new FakePersonAbsenceAccountRepository(),
					skillRepository, new FakeWorkloadRepository(), new FakeScheduleStorage(),
					new PeopleAndSkillLoaderDecider(personRepository, new PairMatrixService<Guid>(new PairDictionaryFactory<Guid>())),
					new SkillDayLoadHelper(skillDayRepository, multisiteDayRepository));
			schedulingResultStateHolder = new SchedulingResultStateHolder();
			target = new GetSkillDaysByPeriodQueryHandler(dateTimePeriodAssembler, assembler, currentScenario, personRepository,
			                                              skillRepository, resourceCalculationPrerequisitesLoader,
			                                              new FakeCurrentUnitOfWorkFactory(), resourceOptimizationHelper,
			                                              loadSchedulingStateHolderForResourceCalculation,
			                                              schedulingResultStateHolder, new FakeActivityRepository());
		}

		[Test]
		public void ShouldReturnSkillDaysForGivenPeriod()
		{
			var skill = SkillFactory.CreateSkill("skill").WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);

			skillRepository.Add(skill);
			skillDayRepository.Add(skill.CreateSkillDayWithDemand(currentScenario.Current(), new DateOnly(2014, 4, 1), 1));
			skillDayRepository.Add(skill.CreateSkillDayWithDemand(currentScenario.Current(), new DateOnly(2014, 4, 2), 1));

			personRepository.Add(new Person().WithId().WithPersonPeriod(TeamFactory.CreateTeam("Team 1", "Paris"), skill));
			
			target.Handle(new GetSkillDaysByPeriodQueryDto
			{
				Period = new DateOnlyPeriodDto {StartDate = new DateOnlyDto(2014, 4, 1), EndDate = new DateOnlyDto(2014, 4, 2)},
				TimeZoneId = TimeZoneInfo.Utc.Id
			}).Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldReturnCorrectForecastedDemand()
		{
			var skill = SkillFactory.CreateSkill("skill").WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);

			skillRepository.Add(skill);
			skillDayRepository.Add(skill.CreateSkillDayWithDemand(currentScenario.Current(), new DateOnly(2014, 4, 1), 1));
			
			personRepository.Add(new Person().WithId().WithPersonPeriod(TeamFactory.CreateTeam("Team 1", "Paris"), skill));

			target.Handle(new GetSkillDaysByPeriodQueryDto
			{
				Period = new DateOnlyPeriodDto { StartDate = new DateOnlyDto(2014, 4, 1), EndDate = new DateOnlyDto(2014, 4, 1) },
				TimeZoneId = TimeZoneInfo.Utc.Id,
			}).First().SkillDataCollection.First().ForecastedAgents.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnCorrectForecastedDemandForEmailSkill()
		{
			var skill = SkillFactory.CreateSkill("skill",SkillTypeFactory.CreateSkillTypeEmail(),15).WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);

			skillRepository.Add(skill);
			var skillDayWithDemand = skill.CreateSkillDayWithDemand(currentScenario.Current(), new DateOnly(2014, 4, 1), 1);
			foreach (var skillDataPeriod in skillDayWithDemand.SkillDataPeriodCollection)
			{
				skillDataPeriod.ServiceAgreement = ServiceAgreement.DefaultValuesEmail();
			}
			skillDayRepository.Add(skillDayWithDemand);
			
			personRepository.Add(new Person().WithId().WithPersonPeriod(TeamFactory.CreateTeam("Team 1", "Paris"), skill));

			target.Handle(new GetSkillDaysByPeriodQueryDto
			{
				Period = new DateOnlyPeriodDto { StartDate = new DateOnlyDto(2014, 4, 1), EndDate = new DateOnlyDto(2014, 4, 1) },
				TimeZoneId = TimeZoneInfo.Utc.Id,
			}).First().SkillDataCollection.First().ForecastedAgents.Should().Be.EqualTo(0.125);
		}

		[Test]
		public void ShouldInitializeAllSkills()
		{
			var skill = SkillFactory.CreateSkill("skill").WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);

			var skills = new List<ISkill> { skill };
			var period = new DateOnlyPeriod(2014, 3, 31, 2014, 4, 3);

			skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			skillDayRepository.Add(skill.CreateSkillDayWithDemand(currentScenario.Current(), new DateOnly(2014, 4, 1), 1));

			personRepository.Add(new Person().WithId().WithPersonPeriod(TeamFactory.CreateTeam("Team 1", "Paris"), skill));
			skillRepository.Stub(x => x.LoadAll()).Return(skills);
			skillRepository.Stub(x => x.FindAllWithSkillDays(period)).Return(skills);
			
			loadSchedulingStateHolderForResourceCalculation =
				new LoadSchedulingStateHolderForResourceCalculation(personRepository, new FakePersonAbsenceAccountRepository(),
					skillRepository, new FakeWorkloadRepository(), new FakeScheduleStorage(),
					new PeopleAndSkillLoaderDecider(personRepository, new PairMatrixService<Guid>(new PairDictionaryFactory<Guid>())),
					new SkillDayLoadHelper(skillDayRepository, multisiteDayRepository));

			target = new GetSkillDaysByPeriodQueryHandler(dateTimePeriodAssembler, assembler, currentScenario, personRepository,
														  skillRepository, resourceCalculationPrerequisitesLoader,
														  new FakeCurrentUnitOfWorkFactory(), resourceOptimizationHelper,
														  loadSchedulingStateHolderForResourceCalculation,
														  schedulingResultStateHolder, new FakeActivityRepository());

			target.Handle(new GetSkillDaysByPeriodQueryDto
			{
				Period = new DateOnlyPeriodDto { StartDate = new DateOnlyDto(2014, 4, 1), EndDate = new DateOnlyDto(2014, 4, 2) },
				TimeZoneId = TimeZoneInfo.Utc.Id
			});

			skillRepository.AssertWasCalled(x => x.LoadAll());
		}
		
		[Test]
		public void ShouldOnlyIncludeSubSkillForMultisiteSkills()
		{
			var multisiteSkill = SkillFactory.CreateMultisiteSkill("multi").WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(multisiteSkill);
			var childSkill = SkillFactory.CreateChildSkill("S1", multisiteSkill).WithId();

			skillRepository.Add(multisiteSkill);
			skillRepository.Add(childSkill);
			
			skillDayRepository.Add(multisiteSkill.CreateSkillDayWithDemand(currentScenario.Current(),new DateOnly(2014,4,1),1));
			skillDayRepository.Add(new SkillDay(new DateOnly(2014, 4, 1), childSkill, currentScenario.Current(),
				new IWorkloadDay[0],
				new ISkillDataPeriod[]
				{
					new SkillDataPeriod(ServiceAgreement.DefaultValues(), new SkillPersonData(),
						new DateOnly(2014, 4, 1).ToDateTimePeriod(childSkill.TimeZone))
				}));
			var multisiteDay = new MultisiteDay(new DateOnly(2014,4,1), multisiteSkill, currentScenario.Current());
			multisiteDay.SetMultisitePeriodCollection(new List<IMultisitePeriod>
			{
				new MultisitePeriod(new DateOnly(2014, 4, 1).ToDateTimePeriod(multisiteSkill.TimeZone),
					new Dictionary<IChildSkill, Percent> {{childSkill, new Percent(1)}})
			});
			multisiteDayRepository.Has(multisiteDay);
			
			personRepository.Add(new Person().WithId().WithPersonPeriod(TeamFactory.CreateTeam("Team 1","Paris"),childSkill));
			
			target.Handle(new GetSkillDaysByPeriodQueryDto
			{
				Period = new DateOnlyPeriodDto { StartDate = new DateOnlyDto(2014, 4, 1), EndDate = new DateOnlyDto(2014, 4, 1) },
				TimeZoneId = TimeZoneInfo.Utc.Id
			}).Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotAllowLoadingMoreThan31Days()
		{
			Assert.Throws<FaultException>(()=>target.Handle(new GetSkillDaysByPeriodQueryDto
			{
				Period = new DateOnlyPeriodDto {StartDate = new DateOnlyDto(2014, 4, 1), EndDate = new DateOnlyDto(2014, 5, 2)},
				TimeZoneId = TimeZoneInfo.Utc.Id
			}));
		}
	}
}