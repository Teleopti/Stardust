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


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.SkillDay
{
	[DomainTest]
	public class AnalyticsForecastWorkloadUpdaterTest : IExtendSystem
	{
		public AnalyticsForecastWorkloadUpdater Target;
		public ISkillDayRepository SkillDayRepository;
		public IAnalyticsIntervalRepository AnalyticsIntervalRepository;
		public IAnalyticsWorkloadRepository AnalyticsWorkloadRepository;
		public FakeAnalyticsDateRepository AnalyticsDateRepository;
		public IAnalyticsScenarioRepository AnalyticsScenarioRepository;
		public FakeAnalyticsForecastWorkloadRepository AnalyticsForecastWorkloadRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<AnalyticsForecastWorkloadUpdater>();
		}

		[Test]
		public void ShouldUpdateAnalyticsWithMultipleForecastedDaysInSameEvent()
		{
			AnalyticsDateRepository.Clear();
			AnalyticsDateRepository.HasDatesBetween(new DateTime(2018, 9, 27), new DateTime(2018, 9, 30));
			var businessUnitId = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));
			var forecastedDay1 = new DateOnly(2018, 9, 28);
			var forecastedDay2 = new DateOnly(2018, 9, 29);
			var skill = SkillFactory.CreateSkill("TestSkill");
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, true);
			var skillDay1 = SkillDayFactory.CreateSkillDay(skill, forecastedDay1, scenario).WithId();
			var skillDay2 = SkillDayFactory.CreateSkillDay(skill, forecastedDay2, scenario).WithId();

			foreach (var workloadDay in skillDay1.WorkloadDayCollection)
			{
				workloadDay.Workload.SetId(Guid.NewGuid());
				AnalyticsWorkloadRepository.AddOrUpdate(AnalyticsWorkloadFactory.CreateAnalyticsWorkload(workloadDay.Workload, 1, 1));
			}

			foreach (var workloadDay in skillDay2.WorkloadDayCollection)
			{
				workloadDay.Workload.SetId(Guid.NewGuid());
				AnalyticsWorkloadRepository.AddOrUpdate(AnalyticsWorkloadFactory.CreateAnalyticsWorkload(workloadDay.Workload, 1, 1));
			}

			SkillDayRepository.Add(skillDay1);
			SkillDayRepository.Add(skillDay2);
			AnalyticsScenarioRepository.AddScenario(AnalyticsScenarioFactory.CreateAnalyticsScenario(scenario));

			Target.Handle(new ForecastChangedEvent()
			{
				SkillDayIds = new []{ skillDay1.Id.Value, skillDay2.Id.Value },
				LogOnBusinessUnitId = businessUnitId
			});

			var analyticsforecastedDay1 = AnalyticsDateRepository.Date(forecastedDay1.Date);
			var analyticsforecastedDay2 = AnalyticsDateRepository.Date(forecastedDay2.Date);
			AnalyticsForecastWorkloadRepository.AnalyticsForcastWorkloads.Any(x => x.DateId == analyticsforecastedDay1.DateId)
				.Should().Be.True();
			AnalyticsForecastWorkloadRepository.AnalyticsForcastWorkloads.Any(x => x.DateId == analyticsforecastedDay2.DateId)
				.Should().Be.True();
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
		public void ShouldAddForecastForMountainTimezone()
		{
			var theDate = new DateOnly(2018, 2, 1);
			var businessUnitId = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));
			AnalyticsDateRepository.HasDatesBetween(theDate.Date - TimeSpan.FromDays(2), theDate.Date + TimeSpan.FromDays(2));
			var utcDateId1 = AnalyticsDateRepository.Date(theDate.Date); 
			var utcDateId2 = AnalyticsDateRepository.Date(theDate.AddDays(1).Date);
			var utcDateId3 = AnalyticsDateRepository.Date(theDate.AddDays(2).Date);
			var skill = SkillFactory.CreateSkill("TestSkill");
			skill.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, true);
			var skillDay1 = SkillDayFactory.CreateSkillDay(skill, theDate, scenario).WithId();
			var skillDay2 = SkillDayFactory.CreateSkillDay(skill, theDate.AddDays(1), scenario).WithId();


			var counter = 1;
			foreach (var workloadDay in skillDay1.WorkloadDayCollection)
			{
				workloadDay.Workload.SetId(Guid.NewGuid());
				AnalyticsWorkloadRepository.AddOrUpdate(AnalyticsWorkloadFactory.CreateAnalyticsWorkload(workloadDay.Workload, counter, counter));
				counter++;
			}
			counter = 1;
			foreach (var workloadDay in skillDay2.WorkloadDayCollection)
			{
				workloadDay.Workload.SetId(skillDay1.WorkloadDayCollection[counter-1].Workload.Id);
				counter++;
			}
			SkillDayRepository.Add(skillDay1);
			SkillDayRepository.Add(skillDay2);

			AnalyticsScenarioRepository.AddScenario(AnalyticsScenarioFactory.CreateAnalyticsScenario(scenario));

			Target.Handle(new SkillDayChangedEvent
			{
				SkillDayId = skillDay1.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = businessUnitId
			});

			var forecastDay1 = AnalyticsForecastWorkloadRepository.AnalyticsForcastWorkloads
				.Where(x => x.WorkloadId == 1)
				.OrderBy(x => x.DateId)
				.ThenBy(x => x.IntervalId)
				.ToList();

			forecastDay1.Should().Not.Be.Empty();
			forecastDay1.Count().Should().Be.EqualTo(96);
			forecastDay1.First().DateId.Should().Be.EqualTo(utcDateId1.DateId);
			forecastDay1.First().IntervalId.Should().Be.EqualTo(28);
			forecastDay1.Last().DateId.Should().Be.EqualTo(utcDateId2.DateId);
			forecastDay1.Last().IntervalId.Should().Be.EqualTo(27);

			Target.Handle(new SkillDayChangedEvent
			{
				SkillDayId = skillDay2.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = businessUnitId
			});

			var forecastDays = AnalyticsForecastWorkloadRepository.AnalyticsForcastWorkloads
				.Where(x => x.WorkloadId == 1)
				.OrderBy(x => x.DateId)
				.ThenBy(x => x.IntervalId)
				.ToList();

			forecastDays.Should().Not.Be.Empty();
			forecastDays.Count().Should().Be.EqualTo(192);
			forecastDays.First().DateId.Should().Be.EqualTo(utcDateId1.DateId);
			forecastDays.First().IntervalId.Should().Be.EqualTo(28);
			forecastDays.Last().DateId.Should().Be.EqualTo(utcDateId3.DateId);
			forecastDays.Last().IntervalId.Should().Be.EqualTo(27);
		}

		[Test]
		public void ShouldAddForecastForBeijingTimezone()
		{
			var theDate = new DateOnly(2018, 2, 1);
			var businessUnitId = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));
			AnalyticsDateRepository.HasDatesBetween(theDate.Date - TimeSpan.FromDays(2), theDate.Date + TimeSpan.FromDays(2));
			var utcDateId1 = AnalyticsDateRepository.Date(theDate.AddDays(-1).Date);
			var utcDateId2 = AnalyticsDateRepository.Date(theDate.Date);
			var utcDateId3 = AnalyticsDateRepository.Date(theDate.AddDays(1).Date);
			var skill = SkillFactory.CreateSkill("TestSkill");
			skill.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
			var scenario = ScenarioFactory.CreateScenario("TestScenario", true, true);
			var skillDay1 = SkillDayFactory.CreateSkillDay(skill, theDate, scenario).WithId();
			var skillDay2 = SkillDayFactory.CreateSkillDay(skill, theDate.AddDays(1), scenario).WithId();


			var counter = 1;
			foreach (var workloadDay in skillDay1.WorkloadDayCollection)
			{
				workloadDay.Workload.SetId(Guid.NewGuid());
				AnalyticsWorkloadRepository.AddOrUpdate(AnalyticsWorkloadFactory.CreateAnalyticsWorkload(workloadDay.Workload, counter, counter));
				counter++;
			}
			counter = 1;
			foreach (var workloadDay in skillDay2.WorkloadDayCollection)
			{
				workloadDay.Workload.SetId(skillDay1.WorkloadDayCollection[counter - 1].Workload.Id);
				counter++;
			}
			SkillDayRepository.Add(skillDay1);
			SkillDayRepository.Add(skillDay2);

			AnalyticsScenarioRepository.AddScenario(AnalyticsScenarioFactory.CreateAnalyticsScenario(scenario));

			Target.Handle(new SkillDayChangedEvent
			{
				SkillDayId = skillDay1.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = businessUnitId
			});

			var forecastDay1 = AnalyticsForecastWorkloadRepository.AnalyticsForcastWorkloads
				.Where(x => x.WorkloadId == 1)
				.OrderBy(x => x.DateId)
				.ThenBy(x => x.IntervalId)
				.ToList();

			forecastDay1.Should().Not.Be.Empty();
			forecastDay1.Count().Should().Be.EqualTo(96);
			forecastDay1.First().DateId.Should().Be.EqualTo(utcDateId1.DateId);
			forecastDay1.First().IntervalId.Should().Be.EqualTo(64);
			forecastDay1.Last().DateId.Should().Be.EqualTo(utcDateId2.DateId);
			forecastDay1.Last().IntervalId.Should().Be.EqualTo(63);

			Target.Handle(new SkillDayChangedEvent
			{
				SkillDayId = skillDay2.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = businessUnitId
			});

			var forecastDays = AnalyticsForecastWorkloadRepository.AnalyticsForcastWorkloads
				.Where(x => x.WorkloadId == 1)
				.OrderBy(x => x.DateId)
				.ThenBy(x => x.IntervalId)
				.ToList();

			forecastDays.Should().Not.Be.Empty();
			forecastDays.Count().Should().Be.EqualTo(192);
			forecastDays.First().DateId.Should().Be.EqualTo(utcDateId1.DateId);
			forecastDays.First().IntervalId.Should().Be.EqualTo(64);
			forecastDays.Last().DateId.Should().Be.EqualTo(utcDateId3.DateId);
			forecastDays.Last().IntervalId.Should().Be.EqualTo(63);
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
			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new[] { skillDay }, new DateOnlyPeriod());

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