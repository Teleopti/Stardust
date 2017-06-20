using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Export.Web;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.DomainTest.Intraday;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Export.Web
{
	[DomainTest]
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
			var theDate = new DateOnly(2016, 8, 26);
			var skill = createSkill(minutesPerInterval, "skill", new TimePeriod(8, 0, 8, 30), false, 0);
			var period = new DateOnlyPeriod(theDate, theDate.AddDays(1));
			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skillDayToday = SkillSetupHelper.CreateSkillDay(skill, scenario, theDate.Date, new TimePeriod(8, 0, 8, 30), false);
			SkillRepository.Has(skill);
			SkillDayRepository.Add(skillDayToday);
			var model = Target.Load(skill.Id.Value, period);

			model.Header.Period.Should().Be.EqualTo(period);
			model.Header.SkillName.Should().Be.EqualTo(skill.Name);
			model.Header.SkillTimeZoneName.Should().Be.EqualTo(skill.TimeZone.DisplayName);
			model.Header.ServiceLevelPercent.Value.Should().Be.EqualTo(ServiceAgreement.DefaultValues().ServiceLevel.Percent);
			model.Header.ServiceLevelSeconds.Value.Should().Be.EqualTo(ServiceAgreement.DefaultValues().ServiceLevel.Seconds);
			model.Header.ShrinkagePercent.Value.Should().Be.EqualTo(skillDayToday.SkillDataPeriodCollection.First().Shrinkage);
		}

		[Test]
		public void ShouldReturnDailyModel()
		{
			var theDate = new DateOnly(2016, 8, 26);
			var openHour = new TimePeriod(8, 0, 8, 30);
			var skill = createSkill(minutesPerInterval, "skill", openHour, false, 0);
			var period = new DateOnlyPeriod(theDate, theDate.AddDays(1));
			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skillDayToday = SkillSetupHelper.CreateSkillDay(skill, scenario, theDate.Date, openHour, false, false);
			var skillDayTomorow = SkillSetupHelper.CreateSkillDay(skill, scenario, theDate.Date.AddDays(1), openHour, false, false);
			skillDayToday.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(0.2));
			SkillRepository.Has(skill);
			SkillDayRepository.Add(skillDayToday);
			SkillDayRepository.Add(skillDayTomorow);
			var model = Target.Load(skill.Id.Value, period);
			var attToday = skillDayToday.TotalAverageTaskTime.Seconds;
			var acwToday = skillDayToday.TotalAverageAfterTaskTime.Seconds;
			var attTomorrow = skillDayToday.TotalAverageTaskTime.Seconds;
			var acwTomorrow = skillDayToday.TotalAverageAfterTaskTime.Seconds;

			model.DailyModelForecast.Count().Should().Be.EqualTo(2);
			model.DailyModelForecast.First().ForecastDate.Should().Be.EqualTo(skillDayToday.CurrentDate.Date);
			model.DailyModelForecast.First().OpenHours.Should().Be.EqualTo(openHour);
			model.DailyModelForecast.First().Calls.Should().Be.EqualTo(skillDayToday.WorkloadDayCollection.First().Tasks);
			model.DailyModelForecast.First().AverageTalkTime.Should().Be.EqualTo(attToday);
			model.DailyModelForecast.First().AfterCallWork.Should().Be.EqualTo(acwToday);
			model.DailyModelForecast.First().AverageHandleTime.Should().Be.EqualTo(attToday + acwToday);
			model.DailyModelForecast.First().ForecastedHours.Should().Be.EqualTo(skillDayToday.ForecastedDistributedDemand.TotalHours);
			model.DailyModelForecast.First().ForecastedHoursShrinkage.Should().Be.EqualTo(skillDayToday.ForecastedDistributedDemandWithShrinkage.TotalHours);

			model.DailyModelForecast.Last().ForecastDate.Should().Be.EqualTo(skillDayTomorow.CurrentDate.Date);
			model.DailyModelForecast.Last().OpenHours.Should().Be.EqualTo(openHour);
			model.DailyModelForecast.Last().Calls.Should().Be.EqualTo(skillDayTomorow.WorkloadDayCollection.First().Tasks);
			model.DailyModelForecast.Last().AverageTalkTime.Should().Be.EqualTo(attTomorrow);
			model.DailyModelForecast.Last().AfterCallWork.Should().Be.EqualTo(acwTomorrow);
			model.DailyModelForecast.Last().AverageHandleTime.Should().Be.EqualTo(attTomorrow + acwTomorrow);
			model.DailyModelForecast.Last().ForecastedHours.Should().Be.EqualTo(skillDayTomorow.ForecastedDistributedDemand.TotalHours);
			model.DailyModelForecast.Last().ForecastedHoursShrinkage.Should().Be.EqualTo(skillDayTomorow.ForecastedDistributedDemandWithShrinkage.TotalHours);
		}

		[Test]
		public void ShouldReturnEmptyDailyModelWhenSkillDayIsClosed()
		{
			var theDate = new DateOnly(2016, 8, 26);
			var period = new DateOnlyPeriod(theDate, theDate.AddDays(1));
			var openHour = new TimePeriod(8, 0, 8, 30);
			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skill = createSkill(minutesPerInterval, "skill", openHour, false, 0);
			var skillDay = SkillSetupHelper.CreateSkillDay(skill, scenario, theDate.Date, openHour, false, false);
			skillDay.WorkloadDayCollection.First().Close();
			SkillDayRepository.Add(skillDay);
			SkillRepository.Add(skill);
			var model = Target.Load(skill.Id.Value, period);

			model.DailyModelForecast.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnModelWithOnlyHeaderWhenNoForecast()
		{
			var theDate = new DateOnly(2016, 8, 26);
			var period = new DateOnlyPeriod(theDate, theDate.AddDays(1));
			var openHour = new TimePeriod(8, 0, 8, 30);
			StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skill = createSkill(minutesPerInterval, "skill", openHour, false, 0);
			SkillRepository.Add(skill);
			var model = Target.Load(skill.Id.Value, period);

			model.DailyModelForecast.Should().Be.Empty();
			model.Header.SkillName.Should().Be.EqualTo(skill.Name);
			model.Header.SkillTimeZoneName.Should().Be.EqualTo(skill.TimeZone.DisplayName);
			model.Header.ServiceLevelPercent.HasValue.Should().Be.False();
			model.Header.ServiceLevelSeconds.HasValue.Should().Be.False();
			model.Header.ShrinkagePercent.HasValue.Should().Be.False();
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

			var workload = isClosedOnWeekends 
				? WorkloadFactory.CreateWorkloadClosedOnWeekendsWithOpenHours(skill, openHours) 
				: WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHours);
			workload.SetId(Guid.NewGuid());

			return skill;
		}
	}
}
