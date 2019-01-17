using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.Sdk.WcfHost.Ioc;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	[DomainTest]
	public class GetStaffingUsingReadModelTest : IExtendSystem
	{
		public GetStaffingUsingReadModel Target;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeMultisiteDayRepository MultisiteDayRepository;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public MutableNow Now;
		
		[Test]
		public void ShouldReturnSkillDaysForGivenPeriod()
		{
			Now.Is(new DateTime(2014, 4, 1, 8, 0, 0, DateTimeKind.Utc));
			var skill = SkillFactory.CreateSkill("skill").WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);

			var scenario = ScenarioRepository.Has("Default");

			SkillRepository.Add(skill);
			SkillDayRepository.Add(skill.CreateSkillDayWithDemand(scenario, new DateOnly(2014, 4, 1), 1));
			SkillDayRepository.Add(skill.CreateSkillDayWithDemand(scenario, new DateOnly(2014, 4, 2), 1));

			PersonRepository.Add(new Person().WithId().WithPersonPeriod(TeamFactory.CreateTeam("Team 1", "Paris"), skill));

			Target.GetSkillDayDto(new GetSkillDaysByPeriodQueryDto
			{
				Period = new DateOnlyPeriodDto { StartDate = new DateOnlyDto(2014, 4, 1), EndDate = new DateOnlyDto(2014, 4, 2) },
				TimeZoneId = TimeZoneInfo.Utc.Id
			}).Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldReturnCorrectForecastedDemand()
		{
			Now.Is(new DateTime(2014, 4, 1, 8, 0, 0, DateTimeKind.Utc));
			var skill = SkillFactory.CreateSkill("skill").WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);

			var scenario = ScenarioRepository.Has("Default");

			SkillRepository.Add(skill);
			SkillDayRepository.Add(skill.CreateSkillDayWithDemand(scenario, new DateOnly(2014, 4, 1), 1));
			var userNow = new DateTime(2014, 4, 1, 8, 0, 0, DateTimeKind.Utc);
			PersonRepository.Add(new Person().WithId().WithPersonPeriod(TeamFactory.CreateTeam("Team 1", "Paris"), skill));
			int minutesPerInterval = 15;
			PopulateStaffingReadModels(skill, userNow, userNow.AddMinutes(minutesPerInterval), 5.7, SkillCombinationResourceRepository, minutesPerInterval);

			var vm = Target.GetSkillDayDto(new GetSkillDaysByPeriodQueryDto
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
			Now.Is(new DateTime(2014, 4, 1, 8, 0, 0, DateTimeKind.Utc));
			var skill = SkillFactory.CreateSkill("skill").WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);

			var scenario = ScenarioRepository.Has("Default");

			SkillRepository.Add(skill);
			SkillDayRepository.Add(skill.CreateSkillDayWithDemand(scenario, new DateOnly(2014, 4, 1), 1));
			var userNow = new DateTime(2014, 4, 1, 0, 0, 0, DateTimeKind.Utc);
			PersonRepository.Add(new Person().WithId().WithPersonPeriod(TeamFactory.CreateTeam("Team 1", "Paris"), skill));
			int minutesPerInterval = 15;
			PopulateStaffingReadModels(skill, userNow, userNow.AddMinutes(minutesPerInterval), 5.7, SkillCombinationResourceRepository, minutesPerInterval);

			var vm = Target.GetSkillDayDto(new GetSkillDaysByPeriodQueryDto
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
			Now.Is(new DateTime(2014, 4, 1, 8, 0, 0, DateTimeKind.Utc));
			var skill = SkillFactory.CreateSkill("skill").WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);

			var scenario = ScenarioRepository.Has("Default");

			SkillRepository.Add(skill);
			SkillDayRepository.Add(skill.CreateSkillDayWithDemand(scenario, new DateOnly(2014, 4, 1), 1));
			var userNow = new DateTime(2014, 4, 1, 0, 0, 0, DateTimeKind.Utc);
			PersonRepository.Add(new Person().WithId().WithPersonPeriod(TeamFactory.CreateTeam("Team 1", "Paris"), skill));
			int minutesPerInterval = 15;
			PopulateStaffingReadModels(skill, userNow, userNow.AddMinutes(minutesPerInterval), 5.7, SkillCombinationResourceRepository, minutesPerInterval);

			var vm = Target.GetSkillDayDto(new GetSkillDaysByPeriodQueryDto
			{
				Period = new DateOnlyPeriodDto { StartDate = new DateOnlyDto(2014, 4, 1), EndDate = new DateOnlyDto(2014, 4, 1) },
				TimeZoneId = TimeZoneInfo.Utc.Id,
			});

			vm.First().SkillDataCollection.First().EstimatedServiceLevel.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnCorrectForecastedDemandForEmailSkill()
		{
			Now.Is(new DateTime(2014, 4, 1, 8, 0, 0, DateTimeKind.Utc));
			var skill = SkillFactory.CreateSkill("skill", SkillTypeFactory.CreateSkillTypeEmail(), 15).WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);

			var scenario = ScenarioRepository.Has("Default");
			
			SkillRepository.Add(skill);
			var skillDayWithDemand = skill.CreateSkillDayWithDemand(scenario, new DateOnly(2014, 4, 1), 1);
			foreach (var skillDataPeriod in skillDayWithDemand.SkillDataPeriodCollection)
			{
				skillDataPeriod.ServiceAgreement = ServiceAgreement.DefaultValuesEmail();
			}
			SkillDayRepository.Add(skillDayWithDemand);

			PersonRepository.Add(new Person().WithId().WithPersonPeriod(TeamFactory.CreateTeam("Team 1", "Paris"), skill));
			PopulateStaffingReadModels(skill, Now.UtcDateTime(), Now.UtcDateTime().AddMinutes(60), 5.7, SkillCombinationResourceRepository, 60);

			Target.GetSkillDayDto(new GetSkillDaysByPeriodQueryDto
			{
				Period = new DateOnlyPeriodDto { StartDate = new DateOnlyDto(2014, 4, 1), EndDate = new DateOnlyDto(2014, 4, 1) },
				TimeZoneId = TimeZoneInfo.Utc.Id,
			}).First().SkillDataCollection.First().ForecastedAgents.Should().Be.EqualTo(0.125);
		}
		
		[Test]
		public void ShouldOnlyIncludeSubSkillForMultisiteSkills()
		{
			Now.Is(new DateTime(2014, 4, 1, 8, 0, 0, DateTimeKind.Utc));

			var scenario = ScenarioRepository.Has("Default");

			var multisiteSkill = SkillFactory.CreateMultisiteSkill("multi").WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(multisiteSkill);
			var childSkill = SkillFactory.CreateChildSkill("S1", multisiteSkill).WithId();

			SkillRepository.Add(multisiteSkill);
			SkillRepository.Add(childSkill);

			SkillDayRepository.Add(multisiteSkill.CreateSkillDayWithDemand(scenario, new DateOnly(2014, 4, 1), 1));
			SkillDayRepository.Add(new SkillDay(new DateOnly(2014, 4, 1), childSkill, scenario,
				new IWorkloadDay[0],
				new ISkillDataPeriod[]
				{
					new SkillDataPeriod(ServiceAgreement.DefaultValues(), new SkillPersonData(),
						new DateOnly(2014, 4, 1).ToDateTimePeriod(childSkill.TimeZone))
				}));
			var multisiteDay = new MultisiteDay(new DateOnly(2014, 4, 1), multisiteSkill, scenario);
			multisiteDay.SetMultisitePeriodCollection(new List<IMultisitePeriod>
			{
				new MultisitePeriod(new DateOnly(2014, 4, 1).ToDateTimePeriod(multisiteSkill.TimeZone),
					new Dictionary<IChildSkill, Percent> {{childSkill, new Percent(1)}})
			});
			MultisiteDayRepository.Has(multisiteDay);

			PersonRepository.Add(new Person().WithId().WithPersonPeriod(TeamFactory.CreateTeam("Team 1", "Paris"), childSkill));

			Target.GetSkillDayDto(new GetSkillDaysByPeriodQueryDto
			{
				Period = new DateOnlyPeriodDto { StartDate = new DateOnlyDto(2014, 4, 1), EndDate = new DateOnlyDto(2014, 4, 1) },
				TimeZoneId = TimeZoneInfo.Utc.Id
			}).Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotAllowLoadingMoreThan31Days()
		{
			Now.Is(new DateTime(2014, 4, 1, 8, 0, 0, DateTimeKind.Utc));
			Assert.Throws<FaultException>(() => Target.GetSkillDayDto(new GetSkillDaysByPeriodQueryDto
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
					SkillCombination = new HashSet<Guid> { skill.Id.GetValueOrDefault() }
				});
			}
			skillCombinationResourceRepository.AddSkillCombinationResource(DateTime.UtcNow, skillCombinationResources);
		}
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<GetStaffingUsingReadModel>();
			extend.AddModule(new AssemblerModule());
		}

	}
}