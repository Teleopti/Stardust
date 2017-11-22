using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.SkillDay;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.SkillDay
{
	[DomainTest]
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

		[Test]
		public void ShouldDeleteWorkloadsForPeriodsNotInTaskPeriodCollection()
		{
			var businessUnitId = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));
			AnalyticsDateRepository.HasDatesBetween(DateTime.Today - TimeSpan.FromDays(365), DateTime.Today + TimeSpan.FromDays(365));
			var skill = SkillFactory.CreateSkill("TestSkill");
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, true);
			var skillDay = SkillDayFactory.CreateSkillDay(skill, DateOnly.Today, scenario).WithId();
			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new [] {skillDay}, new DateOnlyPeriod());
			
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

			var workloadDayNewOpenHours = skillDay.WorkloadDayCollection[0];
			workloadDayNewOpenHours.ChangeOpenHours(new List<TimePeriod>() { new TimePeriod(8, 0, 17, 0) });

			Target.Handle(new SkillDayChangedEvent
			{
				SkillDayId = skillDay.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = businessUnitId
			});

			AnalyticsForecastWorkloadRepository.AnalyticsForcastWorkloads.Should().Not.Be.Empty();
			AnalyticsForecastWorkloadRepository.AnalyticsForcastWorkloads
				.Count(x => x.WorkloadId == 1)
				.Should()
				.Be.EqualTo(36);
			AnalyticsForecastWorkloadRepository.AnalyticsForcastWorkloads
				.Count(x => x.WorkloadId == 2)
				.Should()
				.Be.EqualTo(AnalyticsIntervalRepository.IntervalsPerDay());
		}

		[Test]
		public void ShouldOnlyDeleteWorkloadsForSpecifiedSkillDay()
		{
			var businessUnitId = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));
			AnalyticsDateRepository.HasDatesBetween(DateTime.Today - TimeSpan.FromDays(365), DateTime.Today + TimeSpan.FromDays(365));
			var skill = SkillFactory.CreateSkill("TestSkill");
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, true);
			var skillDayToday = SkillDayFactory.CreateSkillDay(skill, DateOnly.Today, scenario).WithId();
			skillDayToday.SkillDayCalculator = new SkillDayCalculator(skill, new[] { skillDayToday }, new DateOnlyPeriod());
			var skillDayYesterday = SkillDayFactory.CreateSkillDay(skill, DateOnly.Today.AddDays(-1), scenario).WithId();

			var workload1Id = Guid.NewGuid();
			var workload2Id = Guid.NewGuid();
			skillDayToday.WorkloadDayCollection[0].Workload.SetId(workload1Id);
			skillDayToday.WorkloadDayCollection[1].Workload.SetId(workload2Id);
			skillDayYesterday.WorkloadDayCollection[0].Workload.SetId(workload1Id);
			skillDayYesterday.WorkloadDayCollection[1].Workload.SetId(workload2Id);

			createAnalyticsWorkload(skillDayToday);

			SkillDayRepository.Add(skillDayToday);
			SkillDayRepository.Add(skillDayYesterday);
			AnalyticsScenarioRepository.AddScenario(AnalyticsScenarioFactory.CreateAnalyticsScenario(scenario));

			Target.Handle(new SkillDayChangedEvent
			{
				SkillDayId = skillDayToday.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = businessUnitId
			});
			Target.Handle(new SkillDayChangedEvent
			{
				SkillDayId = skillDayYesterday.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = businessUnitId
			});

			skillDayToday.WorkloadDayCollection.ForEach(x => x.ChangeOpenHours(new List<TimePeriod>() { new TimePeriod(8, 0, 17, 0) }));

			Target.Handle(new SkillDayChangedEvent
			{
				SkillDayId = skillDayToday.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = businessUnitId
			});

			var dateIdToday = AnalyticsDateRepository.Date(DateTime.Today).DateId;
			var dateIdYesterday = AnalyticsDateRepository.Date(DateTime.Today.AddDays(-1)).DateId;

			AnalyticsForecastWorkloadRepository.AnalyticsForcastWorkloads.Should().Not.Be.Empty();
			AnalyticsForecastWorkloadRepository.AnalyticsForcastWorkloads
				.Count(x => x.WorkloadId == 1 && x.DateId == dateIdToday)
				.Should()
				.Be.EqualTo(36);
			AnalyticsForecastWorkloadRepository.AnalyticsForcastWorkloads
				.Count(x => x.WorkloadId == 2 && x.DateId == dateIdToday)
				.Should()
				.Be.EqualTo(36);
			AnalyticsForecastWorkloadRepository.AnalyticsForcastWorkloads
				.Count(x => x.WorkloadId == 1 && x.DateId == dateIdYesterday)
				.Should()
				.Be.EqualTo(AnalyticsIntervalRepository.IntervalsPerDay());
			AnalyticsForecastWorkloadRepository.AnalyticsForcastWorkloads
				.Count(x => x.WorkloadId == 2 && x.DateId == dateIdYesterday)
				.Should()
				.Be.EqualTo(AnalyticsIntervalRepository.IntervalsPerDay());
		}

		private void createAnalyticsWorkload(ISkillDay skillday)
		{
			var counter = 1;
			foreach (var workloadDay in skillday.WorkloadDayCollection)
			{
				AnalyticsWorkloadRepository.AddOrUpdate(AnalyticsWorkloadFactory.CreateAnalyticsWorkload(workloadDay.Workload, counter, counter));
				counter++;
			}
		}
	}
}