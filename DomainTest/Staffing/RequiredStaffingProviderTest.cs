using System;
using NUnit.Framework;
using System.Reflection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.DomainTest.Staffing
{
	[DomainTest]
	public class RequiredStaffingProviderTest : ISetup
	{
		public ScheduledStaffingViewModelCreator Target;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public FakeIntradayQueueStatisticsLoader IntradayQueueStatisticsLoader;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeActivityRepository ActivityRepository;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;
		private const int minutesPerInterval = 15;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
		}

		[Test]
		public void AssignActualWorkloadToClonedSkillDayCheckForNaN()
		{
			TimeZone.IsSweden();

			var provider = new RequiredStaffingProvider(new FakeUserTimeZone());
			var method = provider.GetType().GetMethod("assignActualWorkloadToClonedSkillDay", BindingFlags.Instance | BindingFlags.NonPublic);

			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			var act = ActivityRepository.Has("act");

			var timePeriod = new TimePeriod(8, 0, 8, 30);
			var skill = SkillSetupHelper.CreateSkill(minutesPerInterval, "skill1", timePeriod, false, act).WithId();
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
			SkillRepository.Has(skill);
			var skillDay = SkillSetupHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime(), timePeriod,
				false);

			var worloadBacklog =
				new Dictionary<Guid, int> { { skillDay.WorkloadDayCollection.First().Workload.Id ?? Guid.Empty, 4 } };

			method.Invoke(provider, new object[] { skillDay, new List<SkillIntervalStatistics>()
			{
				new SkillIntervalStatistics()
				{
					AnsweredCalls = 5,
					AverageHandleTime = Double.NaN,
					Calls = 5,
					HandleTime = Double.NaN,
					SkillId = skill.Id ?? Guid.Empty,
					StartTime = new DateTime(userNow.Year, userNow.Month, userNow.Day, timePeriod.StartTime.Hours, timePeriod.StartTime.Minutes, timePeriod.StartTime.Seconds),
					WorkloadId = skillDay.WorkloadDayCollection.First().Workload.Id ?? Guid.Empty

				}
			},
				worloadBacklog });
		}
	}
}

