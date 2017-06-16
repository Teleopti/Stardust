using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Export.Web;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.DomainTest.Intraday;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Export.Web
{

	[DomainTest]
	[Ignore("PBI #44291 Not yet done")]
	public class ForecastExportModelCreatorTest
	{
		public ForecastExportModelCreator Target;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;

		private const int minutesPerInterval = 15;

		[Test]
		public void ShouldReturnModelHeader()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);

			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, 0);
			var period = new DateOnlyPeriod(new DateOnly(userNow.Date), new DateOnly(userNow.Date).AddDays(1));
			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skillDayToday = SkillSetupHelper.CreateSkillDay(skill, scenario, userNow, new TimePeriod(8, 0, 8, 30), false);
			SkillRepository.Has(skill);
			SkillDayRepository.Add(skillDayToday);
			var model = Target.Load(skill.Id.Value, period);

			model.Header.Period.Should().Be.EqualTo(period);
			model.Header.SkillName.Should().Be.EqualTo(skill.Name);
			model.Header.SkillTimeZoneName.Should().Be.EqualTo(skill.TimeZone.DisplayName);
			model.Header.ServiceLevelPercent.Should().Be.EqualTo(ServiceAgreement.DefaultValues().ServiceLevel.Percent);
			model.Header.ServiceLevelSeconds.Should().Be.EqualTo(ServiceAgreement.DefaultValues().ServiceLevel.Seconds);
			model.Header.ShrinkagePercent.Should().Be.EqualTo(skillDayToday.SkillDataPeriodCollection.First().Shrinkage);
		}

		[Test]
		public void ShouldReturnDailyModel()
		{
			var userNow = new DateTime(2016, 8, 26, 8, 15, 0, DateTimeKind.Utc);

			var openHour = new TimePeriod(8, 0, 8, 30);
			var skill = createSkill(minutesPerInterval, "skill", openHour, false, 0);
			var period = new DateOnlyPeriod(new DateOnly(userNow.Date), new DateOnly(userNow.Date).AddDays(1));
			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skillDayToday = SkillSetupHelper.CreateSkillDay(skill, scenario, userNow, openHour, false, false);
			skillDayToday.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(0.2));
			SkillRepository.Has(skill);
			SkillDayRepository.Add(skillDayToday);
			var model = Target.Load(skill.Id.Value, period);
			var att = skillDayToday.TotalAverageTaskTime.Seconds;
			var acw = skillDayToday.TotalAverageAfterTaskTime.Seconds;
			model.Header.DailyModelForecast.First().ForecastDate.Should().Be.EqualTo(skillDayToday.CurrentDate.Date);
			model.Header.DailyModelForecast.First().OpenHours.Should().Be.EqualTo(openHour);
			model.Header.DailyModelForecast.First().Calls.Should().Be.EqualTo(skillDayToday.WorkloadDayCollection.First().Tasks);
			model.Header.DailyModelForecast.First().AverageTalkTime.Should().Be.EqualTo(att);
			model.Header.DailyModelForecast.First().AfterCallWork.Should().Be.EqualTo(acw);
			model.Header.DailyModelForecast.First().AverageHandleTime.Should().Be.EqualTo(att + acw);
			model.Header.DailyModelForecast.First().ForecastedHours.Should().Be.EqualTo(skillDayToday.SkillStaffPeriodCollection.Sum(x => x.FStaffHours()));
			model.Header.DailyModelForecast.First().ForecastedHoursShrinkage.Should().Be.EqualTo(skillDayToday.SkillStaffPeriodCollection.Sum(x => x.FStaffHours()));
		}

		private ISkill createSkill(int intervalLength, string skillName, TimePeriod openHours, bool isClosedOnWeekends, int midnigthBreakOffset)
		{
			var skill =
				new Skill(skillName, skillName, Color.Empty, intervalLength, new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony))
				{
					TimeZone = TimeZoneInfo.Utc,
					Activity = new Activity("activity_" + skillName).WithId()
				}.WithId();
			if (midnigthBreakOffset != 0)
			{
				skill.MidnightBreakOffset = TimeSpan.FromHours(midnigthBreakOffset);
			}

			IWorkload workload;
			if (isClosedOnWeekends)
				workload = WorkloadFactory.CreateWorkloadClosedOnWeekendsWithOpenHours(skill, openHours);
			else
				workload = WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHours);
			workload.SetId(Guid.NewGuid());

			return skill;
		}
	}
}
