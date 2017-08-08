using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetStaffingUsingReadModelTest
	{
		private GetStaffingUsingReadModel target;
		private IPersonRepository personRepository;
		private ICurrentScenario currentScenario;
		private ISkillRepository skillRepository;
		private IDateTimePeriodAssembler dateTimePeriodAssembler;
		private IResourceCalculation resourceOptimizationHelper;
		private FakeSkillDayRepository skillDayRepository;
		private FakeMultisiteDayRepository multisiteDayRepository;
		private FakeSkillCombinationResourceRepository skillCombinationResourceRepository;
		private SkillStaffingIntervalProvider skillStaffingIntervalProvider;


		[SetUp]
		public void Setup()
		{
			var userNow = new DateTime(2014, 4, 1, 8, 0, 0, DateTimeKind.Utc);
			skillCombinationResourceRepository = new FakeSkillCombinationResourceRepository(new ThisIsNow(userNow));
			dateTimePeriodAssembler = new DateTimePeriodAssembler();
			personRepository = new FakePersonRepositoryLegacy();
			currentScenario = new FakeCurrentScenario();
			skillRepository = new FakeSkillRepository();
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
			var skillLoadHelper = new SkillDayLoadHelper(skillDayRepository, multisiteDayRepository);
			skillStaffingIntervalProvider = new SkillStaffingIntervalProvider(null, skillCombinationResourceRepository, 
				skillRepository, resourceOptimizationHelper, new ExtractSkillForecastIntervals(skillDayRepository, currentScenario), new FakeActivityRepository(), skillDayRepository, currentScenario );
			target = new GetStaffingUsingReadModel(new FakeCurrentUnitOfWorkFactory(), skillRepository, currentScenario, skillLoadHelper, new FakeActivityRepository(), 
				skillStaffingIntervalProvider);
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

			target.GetSkillDayDto(new GetSkillDaysByPeriodQueryDto
			{
				Period = new DateOnlyPeriodDto { StartDate = new DateOnlyDto(2014, 4, 1), EndDate = new DateOnlyDto(2014, 4, 2) },
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
			var userNow = new DateTime(2014, 4, 1, 8, 0, 0, DateTimeKind.Utc);
			personRepository.Add(new Person().WithId().WithPersonPeriod(TeamFactory.CreateTeam("Team 1", "Paris"), skill));
			int minutesPerInterval = 15;
			PopulateStaffingReadModels(skill, userNow, userNow.AddMinutes(minutesPerInterval), 5.7, skillCombinationResourceRepository, minutesPerInterval);

			var vm = target.GetSkillDayDto(new GetSkillDaysByPeriodQueryDto
				{
					Period = new DateOnlyPeriodDto { StartDate = new DateOnlyDto(2014, 4, 1), EndDate = new DateOnlyDto(2014, 4, 1) },
					TimeZoneId = TimeZoneInfo.Utc.Id,
				});

			vm.First().SkillDataCollection.First().ForecastedAgents.Should().Be.EqualTo(1);
			vm.First().SkillDataCollection.First().IntervalStandardDeviation.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnCorrectScheduleStaffing()
		{
			var skill = SkillFactory.CreateSkill("skill").WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);

			skillRepository.Add(skill);
			skillDayRepository.Add(skill.CreateSkillDayWithDemand(currentScenario.Current(), new DateOnly(2014, 4, 1), 1));
			var userNow = new DateTime(2014, 4, 1, 0, 0, 0, DateTimeKind.Utc);
			personRepository.Add(new Person().WithId().WithPersonPeriod(TeamFactory.CreateTeam("Team 1", "Paris"), skill));
			int minutesPerInterval = 15;
			PopulateStaffingReadModels(skill, userNow, userNow.AddMinutes(minutesPerInterval), 5.7, skillCombinationResourceRepository, minutesPerInterval);

			var vm = target.GetSkillDayDto(new GetSkillDaysByPeriodQueryDto
			{
				Period = new DateOnlyPeriodDto { StartDate = new DateOnlyDto(2014, 4, 1), EndDate = new DateOnlyDto(2014, 4, 1) },
				TimeZoneId = TimeZoneInfo.Utc.Id,
			});

			vm.First().SkillDataCollection.First().ForecastedAgents.Should().Be.EqualTo(1);
			vm.First().SkillDataCollection.First().ScheduledAgents.Should().Be.EqualTo(5.7);
			vm.First().SkillDataCollection.First().ScheduledHeads.Should().Be.EqualTo(5.7);
		}

		[Test]
		public void ShouldReturnCorrectEsl()
		{
			var skill = SkillFactory.CreateSkill("skill").WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);

			skillRepository.Add(skill);
			skillDayRepository.Add(skill.CreateSkillDayWithDemand(currentScenario.Current(), new DateOnly(2014, 4, 1), 1));
			var userNow = new DateTime(2014, 4, 1, 0, 0, 0, DateTimeKind.Utc);
			personRepository.Add(new Person().WithId().WithPersonPeriod(TeamFactory.CreateTeam("Team 1", "Paris"), skill));
			int minutesPerInterval = 15;
			PopulateStaffingReadModels(skill, userNow, userNow.AddMinutes(minutesPerInterval), 5.7, skillCombinationResourceRepository, minutesPerInterval);

			var vm = target.GetSkillDayDto(new GetSkillDaysByPeriodQueryDto
			{
				Period = new DateOnlyPeriodDto { StartDate = new DateOnlyDto(2014, 4, 1), EndDate = new DateOnlyDto(2014, 4, 1) },
				TimeZoneId = TimeZoneInfo.Utc.Id,
			});

			vm.First().SkillDataCollection.First().EstimatedServiceLevel.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnCorrectForecastedDemandForEmailSkill()
		{
			var skill = SkillFactory.CreateSkill("skill", SkillTypeFactory.CreateSkillTypeEmail(), 15).WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var userNow = new DateTime(2014, 4, 1, 8, 0, 0, DateTimeKind.Utc);
			skillRepository.Add(skill);
			var skillDayWithDemand = skill.CreateSkillDayWithDemand(currentScenario.Current(), new DateOnly(2014, 4, 1), 1);
			foreach (var skillDataPeriod in skillDayWithDemand.SkillDataPeriodCollection)
			{
				skillDataPeriod.ServiceAgreement = ServiceAgreement.DefaultValuesEmail();
			}
			skillDayRepository.Add(skillDayWithDemand);

			personRepository.Add(new Person().WithId().WithPersonPeriod(TeamFactory.CreateTeam("Team 1", "Paris"), skill));
			PopulateStaffingReadModels(skill, userNow, userNow.AddMinutes(60), 5.7, skillCombinationResourceRepository, 60);

			target.GetSkillDayDto(new GetSkillDaysByPeriodQueryDto
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

			var skillLoadHelper = new SkillDayLoadHelper(skillDayRepository, multisiteDayRepository);
			target = new GetStaffingUsingReadModel(new FakeCurrentUnitOfWorkFactory(), skillRepository, currentScenario, skillLoadHelper, new FakeActivityRepository(), skillStaffingIntervalProvider);

			target.GetSkillDayDto(new GetSkillDaysByPeriodQueryDto
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

			skillDayRepository.Add(multisiteSkill.CreateSkillDayWithDemand(currentScenario.Current(), new DateOnly(2014, 4, 1), 1));
			skillDayRepository.Add(new SkillDay(new DateOnly(2014, 4, 1), childSkill, currentScenario.Current(),
				new IWorkloadDay[0],
				new ISkillDataPeriod[]
				{
					new SkillDataPeriod(ServiceAgreement.DefaultValues(), new SkillPersonData(),
						new DateOnly(2014, 4, 1).ToDateTimePeriod(childSkill.TimeZone))
				}));
			var multisiteDay = new MultisiteDay(new DateOnly(2014, 4, 1), multisiteSkill, currentScenario.Current());
			multisiteDay.SetMultisitePeriodCollection(new List<IMultisitePeriod>
			{
				new MultisitePeriod(new DateOnly(2014, 4, 1).ToDateTimePeriod(multisiteSkill.TimeZone),
					new Dictionary<IChildSkill, Percent> {{childSkill, new Percent(1)}})
			});
			multisiteDayRepository.Has(multisiteDay);

			personRepository.Add(new Person().WithId().WithPersonPeriod(TeamFactory.CreateTeam("Team 1", "Paris"), childSkill));

			target.GetSkillDayDto(new GetSkillDaysByPeriodQueryDto
			{
				Period = new DateOnlyPeriodDto { StartDate = new DateOnlyDto(2014, 4, 1), EndDate = new DateOnlyDto(2014, 4, 1) },
				TimeZoneId = TimeZoneInfo.Utc.Id
			}).Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotAllowLoadingMoreThan31Days()
		{
			Assert.Throws<FaultException>(() => target.GetSkillDayDto(new GetSkillDaysByPeriodQueryDto
			{
				Period = new DateOnlyPeriodDto { StartDate = new DateOnlyDto(2014, 4, 1), EndDate = new DateOnlyDto(2014, 5, 2) },
				TimeZoneId = TimeZoneInfo.Utc.Id
			}));
		}

		public static void PopulateStaffingReadModels(ISkill skill, DateTime scheduledStartTime,
			DateTime scheduledEndTime, double staffing,
			FakeSkillCombinationResourceRepository skillCombinationResourceRepository, int minutesPerInterval )
		{
			var skillCombinationResources = new List<SkillCombinationResource>();

			for (var intervalTime = scheduledStartTime;
				intervalTime < scheduledEndTime;
				intervalTime = intervalTime.AddMinutes(minutesPerInterval))
			{
				skillCombinationResources.Add(new SkillCombinationResource
				{
					StartDateTime = intervalTime,
					EndDateTime = intervalTime.AddMinutes(minutesPerInterval),
					Resource = staffing,
					SkillCombination = new[] { skill.Id.GetValueOrDefault() }
				});
			}
			skillCombinationResourceRepository.AddSkillCombinationResource(DateTime.UtcNow, skillCombinationResources);
		}
	}
}