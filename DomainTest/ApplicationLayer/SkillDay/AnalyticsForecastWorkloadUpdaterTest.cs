using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.SkillDay;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.SkillDay
{
	[TestFixture]
	[DomainTestWithStaticDependenciesAvoidUse]
	public class AnalyticsForecastWorkloadUpdaterTest : ISetup
	{
		public AnalyticsForecastWorkloadUpdater Target;
		public ISkillDayRepository SkillDayRepository;
		public IAnalyticsIntervalRepository AnalyticsIntervalRepository;
		public IAnalyticsWorkloadRepository AnalyticsWorkloadRepository;
		public FakeAnalyticsDateRepository AnalyticsDateRepository;
		public IAnalyticsScenarioRepository AnalyticsScenarioRepository;
		public FakeAnalyticsForecastWorkloadRepository AnalyticsForecastWorkloadRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<AnalyticsForecastWorkloadUpdater>();
		}

		[Test]
		public void ShouldDoNothingWhenSkillDayMissing()
		{
			var businessUnitId = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));
			AnalyticsDateRepository.HasDatesBetween(DateTime.Today - TimeSpan.FromDays(365), DateTime.Today + TimeSpan.FromDays(365));

			Target.Handle(new SkillDayChangedEvent
			{
				SkillDayId = Guid.NewGuid(),
				LogOnBusinessUnitId = businessUnitId
			});

			AnalyticsForecastWorkloadRepository.AnalyticsForcastWorkloads.Should().Be.Empty();
		}

		[Test]
		public void ShouldDoNothingForNotReportableScenario()
		{
			var businessUnitId = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));
			AnalyticsDateRepository.HasDatesBetween(DateTime.Today - TimeSpan.FromDays(365), DateTime.Today + TimeSpan.FromDays(365));

			var skill = SkillFactory.CreateSkill("TestSkill");
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, false);
			var skillDay = SkillDayFactory.CreateSkillDay(skill, DateOnly.Today, scenario).WithId();
			SkillDayRepository.Add(skillDay);

			Target.Handle(new SkillDayChangedEvent
			{
				SkillDayId = skillDay.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = businessUnitId
			});

			AnalyticsForecastWorkloadRepository.AnalyticsForcastWorkloads.Should().Be.Empty();
		}

		[Test]
		public void ShouldThrowWhenScenarioMissingFromAnalytics()
		{
			var businessUnitId = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));
			AnalyticsDateRepository.HasDatesBetween(DateTime.Today - TimeSpan.FromDays(365), DateTime.Today + TimeSpan.FromDays(365));

			var skill = SkillFactory.CreateSkill("TestSkill");
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, true);
			var skillDay = SkillDayFactory.CreateSkillDay(skill, DateOnly.Today, scenario).WithId();
			SkillDayRepository.Add(skillDay);

			Assert.Throws<ScenarioMissingInAnalyticsException>(() => Target.Handle(new SkillDayChangedEvent
			{
				SkillDayId = skillDay.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = businessUnitId
			}));
		}

		[Test]
		public void ShouldThrowWhenWorkloadMissingFromAnalytics()
		{
			var businessUnitId = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));
			AnalyticsDateRepository.HasDatesBetween(DateTime.Today - TimeSpan.FromDays(365), DateTime.Today + TimeSpan.FromDays(365));

			var skill = SkillFactory.CreateSkill("TestSkill");
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, true);
			var skillDay = SkillDayFactory.CreateSkillDay(skill, DateOnly.Today, scenario).WithId();
			foreach (var workloadDay in skillDay.WorkloadDayCollection)
			{
				workloadDay.Workload.SetId(Guid.NewGuid());
			}
			SkillDayRepository.Add(skillDay);
			AnalyticsScenarioRepository.AddScenario(AnalyticsScenarioFactory.CreateAnalyticsScenario(scenario));

			Assert.Throws<WorkloadMissingInAnalyticsException>(() => Target.Handle(new SkillDayChangedEvent
			{
				SkillDayId = skillDay.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = businessUnitId
			}));
		}

		[Test]
		public void ShouldThrowWhenDateMissingFromAnalytics()
		{
			var businessUnitId = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));
			AnalyticsDateRepository.Clear();
			var skill = SkillFactory.CreateSkill("TestSkill");
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, true);
			var skillDay = SkillDayFactory.CreateSkillDay(skill, DateOnly.Today, scenario).WithId(); 

			foreach (var workloadDay in skillDay.WorkloadDayCollection)
			{
				workloadDay.Workload.SetId(Guid.NewGuid());
				AnalyticsWorkloadRepository.AddOrUpdate(AnalyticsWorkloadFactory.CreateAnalyticsWorkload(workloadDay.Workload));
			}
			SkillDayRepository.Add(skillDay);
			AnalyticsScenarioRepository.AddScenario(AnalyticsScenarioFactory.CreateAnalyticsScenario(scenario));

			Assert.Throws<DateMissingInAnalyticsException>(() => Target.Handle(new SkillDayChangedEvent
			{
				SkillDayId = skillDay.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = businessUnitId
			}));
		}

		[Test]
		public void ShouldAddForecastWorkloadsForEachWorkloadAndInterval()
		{
			var businessUnitId = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));
			AnalyticsDateRepository.HasDatesBetween(DateTime.Today - TimeSpan.FromDays(365), DateTime.Today + TimeSpan.FromDays(365));
			var skill = SkillFactory.CreateSkill("TestSkill");
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, true);
			var skillDay = SkillDayFactory.CreateSkillDay(skill, DateOnly.Today, scenario).WithId();

			var counter = 1;
			foreach (var workloadDay in skillDay.WorkloadDayCollection)
			{
				workloadDay.Workload.SetId(Guid.NewGuid());
				AnalyticsWorkloadRepository.AddOrUpdate(AnalyticsWorkloadFactory.CreateAnalyticsWorkload(workloadDay.Workload, counter, counter));
				counter++;
			}
			SkillDayRepository.Add(skillDay);
			AnalyticsScenarioRepository.AddScenario(AnalyticsScenarioFactory.CreateAnalyticsScenario(scenario));
			

			Target.Handle(new SkillDayChangedEvent
			{
				SkillDayId = skillDay.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = businessUnitId
			});

			AnalyticsForecastWorkloadRepository.AnalyticsForcastWorkloads.Should().Not.Be.Empty();
			AnalyticsForecastWorkloadRepository.AnalyticsForcastWorkloads
				.Count(x => x.WorkloadId == 1)
				.Should()
				.Be.EqualTo(AnalyticsIntervalRepository.IntervalsPerDay());
			AnalyticsForecastWorkloadRepository.AnalyticsForcastWorkloads
				.Count(x => x.WorkloadId == 2)
				.Should()
				.Be.EqualTo(AnalyticsIntervalRepository.IntervalsPerDay());
		}
	}
}