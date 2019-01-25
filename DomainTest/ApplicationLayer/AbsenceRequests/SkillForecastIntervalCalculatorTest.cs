using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[TestFixture]
	[DomainTest]
	public class SkillForecastIntervalCalculatorTest : IIsolateSystem
	{
		public SkillForecastIntervalCalculator Target;
		public FakeSkillRepository SkillRepository;
		public MutableNow Now;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public ISkillForecastReadModelRepository SkillForecastReadModelRepository;
		

		
		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<SkillForecastIntervalCalculator>().For<SkillForecastIntervalCalculator>();
		}

		[Test]
		public void ShouldSaveTheIntervalsForSkillDay()
		{
			var dtp = new DateOnlyPeriod(new DateOnly(2019, 2, 16), new DateOnly(2019, 2, 19));
			var now = new DateTime(2019, 2, 17, 16, 0, 0);
			Now.Is(now);
			//var skillDayDate = new DateOnly(2019,2,17);
			//var skill = SkillFactory.CreateSkill("phone").WithId();
			var skillOpenHours = new TimePeriod(8, 9);
			var skill = createSkill(15, "phone", skillOpenHours);
			var scenario = ScenarioFactory.CreateScenarioAggregate().WithId();
			//var skillDay = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, now, new TimePeriod(10, 0, 12, 0), 10);
			var skillDay = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2019, 2, 15), skillOpenHours, 15.7,10,200);
			SkillRepository.Add(skill);
			SkillDayRepository.Add(skillDay);
			ScenarioRepository.Add(scenario);

			var skillList = new List<Guid>(){skill.Id.GetValueOrDefault()};

			Target.Calculate(skillList, dtp);

			var skillStaffIntervals= SkillForecastReadModelRepository.LoadSkillForecast(new[] {skill.Id.GetValueOrDefault()}, dtp.StartDate.Date,
				dtp.EndDate.Date);
			skillStaffIntervals.Count.Should().Be.EqualTo(96);
			skillStaffIntervals.Count(x => x.Agents == 15.7).Should().Be.EqualTo(4);
			skillStaffIntervals.Count(x => x.AverageHandleTime == 10).Should().Be.EqualTo(4);
			skillStaffIntervals.Count(x => x.Calls == 200).Should().Be.EqualTo(4);
		}

		protected ISkill createSkill(int intervalLength, string skillName, TimePeriod openHours)
		{
			var skill =
				new Domain.Forecasting.Skill(skillName, skillName, Color.Empty, intervalLength, new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony))
				{
					TimeZone = TimeZoneInfo.Utc,
					Activity = new Activity("activity_" + skillName).WithId()
				}.WithId();
			var workload = WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHours);
			workload.SetId(Guid.NewGuid());

			return skill;
		}

	}
}