using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
	[NoDefaultData]
	[DomainTest]
	public class GetStaffingUsingResourceCalculationTest : IExtendSystem
	{
		public GetStaffingUsingResourceCalculation Target;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeMultisiteDayRepository MultisiteDayRepository;
		
		[Test]
		public void ShouldReturnSkillDaysForGivenPeriod()
		{
			var scenario = ScenarioRepository.Has("Default");

			var skill = SkillFactory.CreateSkill("skill").WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);

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
			var scenario = ScenarioRepository.Has("Default");

			var skill = SkillFactory.CreateSkill("skill").WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);

			SkillRepository.Add(skill);
			SkillDayRepository.Add(skill.CreateSkillDayWithDemand(scenario, new DateOnly(2014, 4, 1), 1));

			PersonRepository.Add(new Person().WithId().WithPersonPeriod(TeamFactory.CreateTeam("Team 1", "Paris"), skill));

			Target.GetSkillDayDto(new GetSkillDaysByPeriodQueryDto
			{
				Period = new DateOnlyPeriodDto { StartDate = new DateOnlyDto(2014, 4, 1), EndDate = new DateOnlyDto(2014, 4, 1) },
				TimeZoneId = TimeZoneInfo.Utc.Id,
			}).First().SkillDataCollection.First().ForecastedAgents.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnCorrectForecastedDemandForEmailSkill()
		{
			var scenario = ScenarioRepository.Has("Default");

			var skill = SkillFactory.CreateSkill("skill", SkillTypeFactory.CreateSkillTypeEmail(), 15).WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);

			SkillRepository.Add(skill);
			var skillDayWithDemand = skill.CreateSkillDayWithDemand(scenario, new DateOnly(2014, 4, 1), 1);
			foreach (var skillDataPeriod in skillDayWithDemand.SkillDataPeriodCollection)
			{
				skillDataPeriod.ServiceAgreement = ServiceAgreement.DefaultValuesEmail();
			}
			SkillDayRepository.Add(skillDayWithDemand);

			PersonRepository.Add(new Person().WithId().WithPersonPeriod(TeamFactory.CreateTeam("Team 1", "Paris"), skill));

			Target.GetSkillDayDto(new GetSkillDaysByPeriodQueryDto
			{
				Period = new DateOnlyPeriodDto { StartDate = new DateOnlyDto(2014, 4, 1), EndDate = new DateOnlyDto(2014, 4, 1) },
				TimeZoneId = TimeZoneInfo.Utc.Id,
			}).First().SkillDataCollection.First().ForecastedAgents.Should().Be.EqualTo(0.125);
		}

		[Test]
		public void ShouldInitializeAllSkills()
		{
			var scenario = ScenarioRepository.Has("Default");

			var skill = SkillFactory.CreateSkill("skill").WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			
			SkillRepository.Has(skill);
			SkillDayRepository.Add(skill.CreateSkillDayWithDemand(scenario, new DateOnly(2014, 4, 1), 1));

			PersonRepository.Add(new Person().WithId().WithPersonPeriod(TeamFactory.CreateTeam("Team 1", "Paris"), skill));
			
			var result = Target.GetSkillDayDto(new GetSkillDaysByPeriodQueryDto
			{
				Period = new DateOnlyPeriodDto { StartDate = new DateOnlyDto(2014, 4, 1), EndDate = new DateOnlyDto(2014, 4, 2) },
				TimeZoneId = TimeZoneInfo.Utc.Id
			});

			result.Count.Should().Be.GreaterThanOrEqualTo(1);
		}

		[Test]
		public void ShouldOnlyIncludeSubSkillForMultisiteSkills()
		{
			var scenario = ScenarioRepository.Has("Default");
			var multisiteSkill = SkillFactory.CreateMultisiteSkill("multi").WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(multisiteSkill);
			var childSkill = SkillFactory.CreateChildSkill("S1", multisiteSkill).WithId();

			SkillRepository.Add(multisiteSkill);
			SkillRepository.Add(childSkill);

			SkillDayRepository.Add(multisiteSkill.CreateSkillDayWithDemand(scenario, new DateOnly(2014, 4, 1), 1));
			SkillDayRepository.Add(new SkillDay(new DateOnly(2014, 4, 1), childSkill, scenario, new IWorkloadDay[0],
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
			Assert.Throws<FaultException>(() => Target.GetSkillDayDto(new GetSkillDaysByPeriodQueryDto
			{
				Period = new DateOnlyPeriodDto { StartDate = new DateOnlyDto(2014, 4, 1), EndDate = new DateOnlyDto(2014, 5, 2) },
				TimeZoneId = TimeZoneInfo.Utc.Id
			}));
		}
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<GetStaffingUsingResourceCalculation>();
			extend.AddModule(new AssemblerModule());
		}

	}
}