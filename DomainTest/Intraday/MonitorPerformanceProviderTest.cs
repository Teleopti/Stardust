using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.DomainTest.ResourceCalculation;
using Teleopti.Ccc.Infrastructure.Intraday;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Intraday
{
	[DomainTest]
	[TestWithStaticDependenciesAvoidUse]
	public class MonitorPerformanceProviderTest : ISetup
	{
		public MonitorPerformanceProvider Target;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScheduleForecastSkillReadModelRepository ScheduleForecastSkillReadModelRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		const int minutesPerInterval = 15;
		public FakeIntradayMonitorDataLoader IntradayMonitorDataLoader;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
		}

		[Test]
		public void ShouldReturnEslForOneSkill()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			var latestStatsTime = new DateTime(2016, 8, 26, 8, 0, 0, DateTimeKind.Utc);

			var scenario = fakeScenarioAndIntervalLength();
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 15));
			var skillDay = createSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 8, 15));

			var scheduledStaffingList = new List<SkillStaffingInterval>()
			{
				new SkillStaffingInterval() {
					SkillId = skill.Id.Value,
					StartDateTime = userNow,
					EndDateTime = userNow.AddMinutes(minutesPerInterval),
					StaffingLevel = 6d
				}
			};

			IntradayMonitorDataLoader.AddInterval(new IncomingIntervalModel()
			{
				IntervalDate = latestStatsTime.Date,
				IntervalId = new IntervalBase(latestStatsTime, (60/minutesPerInterval)*24).Id,
				OfferedCalls = 22
			});

			SkillRepository.Has(skill);
			SkillDayRepository.Add(skillDay);
			ScheduleForecastSkillReadModelRepository.Persist(scheduledStaffingList, DateTime.MinValue);

			var result = Target.Load(new Guid[] {skill.Id.Value});

			result.EstimatedServiceLevels.Length.Should().Be.EqualTo(1);
			result.EstimatedServiceLevels.First().Should().Be.GreaterThan(0d);
		}


		private IList<SkillStaffingInterval> createScheduledStaffing(ISkillDay skillDay, DateTime latestStatsTime)
		{
			var scheduledStats = new List<SkillStaffingInterval>();
			var shiftStartTime = skillDay.SkillStaffPeriodCollection.First().Period.StartDateTime;
			var shiftEndTime = skillDay.SkillStaffPeriodCollection.Last().Period.EndDateTime;
			if (shiftStartTime > latestStatsTime || shiftEndTime < latestStatsTime)
			{
				return scheduledStats;
			}
			var random = new Random();

			for (DateTime intervalTime = shiftStartTime;
						 intervalTime <= latestStatsTime;
						 intervalTime = intervalTime.AddMinutes(minutesPerInterval))
			{
				scheduledStats.Add(new SkillStaffingInterval
				{
					SkillId = skillDay.Skill.Id.Value,
					StartDateTime = TimeZoneHelper.ConvertFromUtc(intervalTime, TimeZone.TimeZone()),
					EndDateTime = TimeZoneHelper.ConvertFromUtc(intervalTime, TimeZone.TimeZone()).AddMinutes(minutesPerInterval),
					StaffingLevel = random.Next(5,50)
				});
			}
			return scheduledStats;
		}

		private ISkill createSkill(int intervalLength, string skillName, TimePeriod openHours)
		{
			var activity = new Activity("activity_" + skillName).WithId();
			var skill =
				new Skill(skillName, skillName, Color.Empty, intervalLength, new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony))
				{
					Activity = activity,
					TimeZone = TimeZoneInfo.Utc
				}.WithId();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHours);

			return skill;
		}

		private ISkillDay createSkillDay(ISkill skill, IScenario scenario, DateTime userNow, TimePeriod openHours)
		{
			var random = new Random();
			var skillDay = skill.CreateSkillDayWithDemandOnInterval(scenario, new DateOnly(userNow), 3, new Tuple<TimePeriod, double>(openHours, 3)).WithId();
			var index = 0;

			for (TimeSpan intervalStart = openHours.StartTime; intervalStart < openHours.EndTime; intervalStart = intervalStart.Add(TimeSpan.FromMinutes(skill.DefaultResolution)))
			{
				skillDay.WorkloadDayCollection.First().TaskPeriodList[index].Tasks = random.Next(5, 50);
				skillDay.WorkloadDayCollection.First().TaskPeriodList[index].AverageTaskTime = TimeSpan.FromSeconds(120);
				skillDay.WorkloadDayCollection.First().TaskPeriodList[index].AverageAfterTaskTime = TimeSpan.FromSeconds(200);
				index++;
			}

			return skillDay;
		}

		private Scenario fakeScenarioAndIntervalLength()
		{
			IntervalLengthFetcher.Has(minutesPerInterval);
			var scenario = ScenarioFactory.CreateScenario("scenariorita", true, true).WithId();
			ScenarioRepository.Has(scenario);
			return scenario;
		}
	}
}