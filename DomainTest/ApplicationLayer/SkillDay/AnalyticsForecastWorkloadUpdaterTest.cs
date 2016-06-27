using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.SkillDay;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.SkillDay
{
	[TestFixture]
	public class AnalyticsForecastWorkloadUpdaterTest
	{
		private AnalyticsForecastWorkloadUpdater _target;
		private ISkillDayRepository _skillDayRepository;
		private IAnalyticsIntervalRepository _analyticsIntervalRepository;
		private IAnalyticsWorkloadRepository _analyticsWorkloadRepository;
		private IAnalyticsDateRepository _analyticsDateRepository;
		private IAnalyticsScenarioRepository _analyticsScenarioRepository;
		private FakeAnalyticsForecastWorkloadRepository _analyticsForecastWorkloadRepository;

		[SetUp]
		public void Setup()
		{
			_skillDayRepository = new FakeSkillDayRepository();
			_analyticsIntervalRepository = new FakeAnalyticsIntervalRepository();
			_analyticsWorkloadRepository = new FakeAnalyticsWorkloadRepository();
			_analyticsDateRepository = new FakeAnalyticsDateRepository(DateTime.Today - TimeSpan.FromDays(365), DateTime.Today + TimeSpan.FromDays(365));
			_analyticsScenarioRepository = new FakeAnalyticsScenarioRepository();
			_analyticsForecastWorkloadRepository = new FakeAnalyticsForecastWorkloadRepository();

			_target = new AnalyticsForecastWorkloadUpdater(_skillDayRepository, _analyticsWorkloadRepository, _analyticsDateRepository, _analyticsScenarioRepository, _analyticsForecastWorkloadRepository, _analyticsIntervalRepository);
		}

		[Test]
		public void ShouldDoNothingWhenSkillDayMissing()
		{
			_target.Handle(new SkillDayChangedEvent
			{
				SkillDayId = Guid.NewGuid(),
				LogOnBusinessUnitId = Guid.NewGuid()
			});

			_analyticsForecastWorkloadRepository.AnalyticsForcastWorkloads.Should().Be.Empty();
		}

		[Test]
		public void ShouldDoNothingForNotReportableScenario()
		{
			var skill = SkillFactory.CreateSkill("TestSkill");
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, false);
			var skillDay = SkillDayFactory.CreateSkillDay(skill, DateOnly.Today, scenario);
			skillDay.SetId(Guid.NewGuid());
			_skillDayRepository.Add(skillDay);

			_target.Handle(new SkillDayChangedEvent
			{
				SkillDayId = skillDay.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = Guid.NewGuid()
			});

			_analyticsForecastWorkloadRepository.AnalyticsForcastWorkloads.Should().Be.Empty();
		}

		[Test, ExpectedException(typeof(ScenarioMissingInAnalyticsException))]
		public void ShouldThrowWhenScenarioMissingFromAnalytics()
		{
			var skill = SkillFactory.CreateSkill("TestSkill");
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, true);
			var skillDay = SkillDayFactory.CreateSkillDay(skill, DateOnly.Today, scenario);
			skillDay.SetId(Guid.NewGuid());
			_skillDayRepository.Add(skillDay);

			_target.Handle(new SkillDayChangedEvent
			{
				SkillDayId = skillDay.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = Guid.NewGuid()
			});
		}

		[Test, ExpectedException(typeof(WorkloadMissingInAnalyticsException))]
		public void ShouldThrowWhenWorkloadMissingFromAnalytics()
		{
			var skill = SkillFactory.CreateSkill("TestSkill");
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, true);
			var skillDay = SkillDayFactory.CreateSkillDay(skill, DateOnly.Today, scenario);
			skillDay.SetId(Guid.NewGuid());
			foreach (var workloadDay in skillDay.WorkloadDayCollection)
			{
				workloadDay.Workload.SetId(Guid.NewGuid());
			}
			_skillDayRepository.Add(skillDay);
			_analyticsScenarioRepository.AddScenario(AnalyticsScenarioFactory.CreateAnalyticsScenario(scenario));

			_target.Handle(new SkillDayChangedEvent
			{
				SkillDayId = skillDay.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = Guid.NewGuid()
			});
		}

		[Test, ExpectedException(typeof(DateMissingInAnalyticsException))]
		public void ShouldThrowWhenDateMissingFromAnalytics()
		{
			_analyticsDateRepository.Dates().Clear();
			var skill = SkillFactory.CreateSkill("TestSkill");
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, true);
			var skillDay = SkillDayFactory.CreateSkillDay(skill, DateOnly.Today, scenario);
			skillDay.SetId(Guid.NewGuid());

			foreach (var workloadDay in skillDay.WorkloadDayCollection)
			{
				workloadDay.Workload.SetId(Guid.NewGuid());
				_analyticsWorkloadRepository.AddOrUpdate(AnalyticsWorkloadFactory.CreateAnalyticsWorkload(workloadDay.Workload));
			}
			_skillDayRepository.Add(skillDay);
			_analyticsScenarioRepository.AddScenario(AnalyticsScenarioFactory.CreateAnalyticsScenario(scenario));

			_target.Handle(new SkillDayChangedEvent
			{
				SkillDayId = skillDay.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = Guid.NewGuid()
			});
		}

		[Test]
		public void ShouldAddForecastWorkloadsForEachWorkloadAndInterval()
		{

			var skill = SkillFactory.CreateSkill("TestSkill");
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, true);
			var skillDay = SkillDayFactory.CreateSkillDay(skill, DateOnly.Today, scenario);
			skillDay.SetId(Guid.NewGuid());

			var counter = 1;
			foreach (var workloadDay in skillDay.WorkloadDayCollection)
			{
				workloadDay.Workload.SetId(Guid.NewGuid());
				_analyticsWorkloadRepository.AddOrUpdate(AnalyticsWorkloadFactory.CreateAnalyticsWorkload(workloadDay.Workload, counter, counter));
				counter++;
			}
			_skillDayRepository.Add(skillDay);
			_analyticsScenarioRepository.AddScenario(AnalyticsScenarioFactory.CreateAnalyticsScenario(scenario));
			

			_target.Handle(new SkillDayChangedEvent
			{
				SkillDayId = skillDay.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = Guid.NewGuid()
			});

			_analyticsForecastWorkloadRepository.AnalyticsForcastWorkloads.Should().Not.Be.Empty();
			_analyticsForecastWorkloadRepository.AnalyticsForcastWorkloads
				.Count(x => x.WorkloadId == 1)
				.Should()
				.Be.EqualTo(_analyticsIntervalRepository.IntervalsPerDay());
			_analyticsForecastWorkloadRepository.AnalyticsForcastWorkloads
				.Count(x => x.WorkloadId == 2)
				.Should()
				.Be.EqualTo(_analyticsIntervalRepository.IntervalsPerDay());
		}
	}
}