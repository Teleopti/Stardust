using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Staffing
{
	[DomainTest]
	public class ExportBpoFileTest : ISetup
	{
		public ExportBpoFile Target;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
		}

		[Test, Ignore("WIP")]
		public void ShouldReturnSkillIntervalData()
		{
			var skill = SkillRepository.Has("Direct sales", new Activity());
			var scenario = SkillSetupHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository);
			Tuple<int, TimeSpan> specificHourDemand = new Tuple<int, TimeSpan>(5, new TimeSpan(1,0,0));
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, new DateOnly(2017, 8, 15), new TimeSpan(2,0,0), specificHourDemand, new TimePeriod(8, 12));
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);
			Now.Is(TimeZoneHelper.ConvertToUtc(userNow, TimeZone.TimeZone()));
		}
	}
}
