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
		public FakeWorkloadRepository WorkloadRepository;
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
			var skillDay = SkillSetupHelper.CreateSkillDay(skill, scenario, theDate.Date, new TimePeriod(8, 0, 8, 30), false);
			SkillRepository.Has(skill);
			WorkloadRepository.Add(skill.WorkloadCollection.First());
			SkillDayRepository.Add(skillDay);

			var model = Target.Load(skill.WorkloadCollection.First().Id.Value, period);

			model.Header.Period.Should().Be.EqualTo(period);
			model.Header.SkillName.Should().Be.EqualTo(skill.Name);
			model.Header.SkillTimeZoneName.Should().Be.EqualTo(skill.TimeZone.DisplayName);
			model.Header.ServiceLevelPercent.Value.Should().Be.EqualTo(ServiceAgreement.DefaultValues().ServiceLevel.Percent);
			model.Header.ServiceLevelSeconds.Value.Should().Be.EqualTo(ServiceAgreement.DefaultValues().ServiceLevel.Seconds);
			model.Header.ShrinkagePercent.Value.Should().Be.EqualTo(skillDay.SkillDataPeriodCollection.First().Shrinkage);
		}

		[Test]
		public void ShouldReturnDailyModel()
		{
			var theDate = new DateOnly(2016, 8, 26);
			var openHour = new TimePeriod(8, 0, 8, 30);
			var skill = createSkill(minutesPerInterval, "skill", openHour, false, 0);
			var period = new DateOnlyPeriod(theDate, theDate.AddDays(1));
			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skillDay1 = SkillSetupHelper.CreateSkillDay(skill, scenario, theDate.Date, openHour, false);
			var skillDay2 = SkillSetupHelper.CreateSkillDay(skill, scenario, theDate.Date.AddDays(1), openHour, false);
			
			skillDay1.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(0.2));
			var day1ForecastedHours = skillDay1.ForecastedIncomingDemand.TotalHours;
			var day1ForecastedHoursWithShrinkage = skillDay1.ForecastedIncomingDemandWithShrinkage.TotalHours;

			SkillRepository.Has(skill);
			WorkloadRepository.Add(skill.WorkloadCollection.First());
			SkillDayRepository.Add(skillDay1);
			SkillDayRepository.Add(skillDay2);

			var model = Target.Load(skill.WorkloadCollection.First().Id.Value, period);

			model.DailyModelForecast.Count().Should().Be.EqualTo(2);
			var dailyModel1 = model.DailyModelForecast.First();
			var dailyModel2 = model.DailyModelForecast.Last();

			dailyModel1.ForecastDate.Should().Be.EqualTo(skillDay1.CurrentDate.Date);
			dailyModel1.OpenHours.Should().Be.EqualTo(openHour);
			dailyModel1.Calls.Should().Be.EqualTo(skillDay1.TotalTasks);
			dailyModel1.AverageTalkTime.Should().Be.EqualTo(skillDay1.TotalAverageTaskTime.TotalSeconds);
			dailyModel1.AverageAfterCallWork.Should().Be.EqualTo(skillDay1.TotalAverageAfterTaskTime.TotalSeconds);
			dailyModel1.AverageHandleTime.Should().Be.EqualTo(dailyModel1.AverageTalkTime + dailyModel1.AverageAfterCallWork);
			dailyModel1.ForecastedHours.Should().Be.EqualTo(day1ForecastedHours);
			dailyModel1.ForecastedHoursShrinkage.Should().Be.EqualTo(day1ForecastedHoursWithShrinkage);

			dailyModel2.ForecastDate.Should().Be.EqualTo(skillDay2.CurrentDate.Date);
			dailyModel2.OpenHours.Should().Be.EqualTo(openHour);
			dailyModel2.Calls.Should().Be.EqualTo(skillDay2.TotalTasks);
			dailyModel2.AverageTalkTime.Should().Be.EqualTo(skillDay2.TotalAverageTaskTime.TotalSeconds);
			dailyModel2.AverageAfterCallWork.Should().Be.EqualTo(skillDay2.TotalAverageAfterTaskTime.TotalSeconds);
			dailyModel2.AverageHandleTime.Should().Be.EqualTo(dailyModel2.AverageTalkTime + dailyModel2.AverageAfterCallWork);
			dailyModel2.ForecastedHours.Should().Be.EqualTo(skillDay2.ForecastedIncomingDemand.TotalHours);
			dailyModel2.ForecastedHoursShrinkage.Should().Be.EqualTo(skillDay2.ForecastedIncomingDemandWithShrinkage.TotalHours);
		}


		[Test]
		public void ShouldReturnDailyModelWithOverrideValues()
		{
			var theDate = new DateOnly(2016, 8, 26);
			var openHour = new TimePeriod(8, 0, 8, 30);
			var skill = createSkill(minutesPerInterval, "skill", openHour, false, 0);
			var period = new DateOnlyPeriod(theDate, theDate.AddDays(1));
			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skillDay = SkillSetupHelper.CreateSkillDay(skill, scenario, theDate.Date, openHour, false);
			skillDay.WorkloadDayCollection.First().SetOverrideTasks(200,null);
			skillDay.WorkloadDayCollection.First().OverrideAverageAfterTaskTime = TimeSpan.FromMinutes(1);
			skillDay.WorkloadDayCollection.First().OverrideAverageTaskTime = TimeSpan.FromMinutes(1);
			SkillRepository.Has(skill);
			WorkloadRepository.Add(skill.WorkloadCollection.First());
			SkillDayRepository.Add(skillDay);

			var model = Target.Load(skill.WorkloadCollection.First().Id.Value, period);

			model.DailyModelForecast.Count().Should().Be.EqualTo(1);
			var dailyModel = model.DailyModelForecast.First();

			dailyModel.ForecastDate.Should().Be.EqualTo(skillDay.CurrentDate.Date);
			dailyModel.Calls.Should().Be.EqualTo(200);
			dailyModel.AverageTalkTime.Should().Be.EqualTo(60d);
			dailyModel.AverageAfterCallWork.Should().Be.EqualTo(60d);
			dailyModel.AverageHandleTime.Should().Be.EqualTo(120d);
		}

		[Test]
		public void ShouldReturnModelWithOnlyHeaderWhenSkillDayIsClosed()
		{
			var theDate = new DateOnly(2016, 8, 26);
			var period = new DateOnlyPeriod(theDate, theDate.AddDays(1));
			var openHour = new TimePeriod(8, 0, 8, 30);
			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skill = createSkill(minutesPerInterval, "skill", openHour, false, 0);
			var skillDay = SkillSetupHelper.CreateSkillDay(skill, scenario, theDate.Date, openHour, false);
			skillDay.WorkloadDayCollection.First().Close();
			SkillDayRepository.Add(skillDay);
			SkillRepository.Add(skill);
			WorkloadRepository.Add(skill.WorkloadCollection.First());

			var model = Target.Load(skill.WorkloadCollection.First().Id.Value, period);

			model.DailyModelForecast.Should().Be.Empty();
			model.IntervalModelForecast.Should().Be.Empty();
			model.Header.SkillName.Should().Be.EqualTo(skill.Name);
			model.Header.SkillTimeZoneName.Should().Be.EqualTo(skill.TimeZone.DisplayName);
			model.Header.ServiceLevelPercent.HasValue.Should().Be.True();
			model.Header.ServiceLevelSeconds.HasValue.Should().Be.True();
			model.Header.ShrinkagePercent.HasValue.Should().Be.True();
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
			WorkloadRepository.Add(skill.WorkloadCollection.First());

			var model = Target.Load(skill.WorkloadCollection.First().Id.Value, period);

			model.DailyModelForecast.Should().Be.Empty();
			model.IntervalModelForecast.Should().Be.Empty();
			model.Header.SkillName.Should().Be.EqualTo(skill.Name);
			model.Header.SkillTimeZoneName.Should().Be.EqualTo(skill.TimeZone.DisplayName);
			model.Header.ServiceLevelPercent.HasValue.Should().Be.False();
			model.Header.ServiceLevelSeconds.HasValue.Should().Be.False();
			model.Header.ShrinkagePercent.HasValue.Should().Be.False();
		}

		[Test]
		public void ShouldReturnIntervalModel()
		{
			var theDate = new DateOnly(2016, 8, 26);
			var openHour = new TimePeriod(8, 0, 8, 30);
			var skill = createSkill(minutesPerInterval, "skill", openHour, false, 0);
			skill.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var period = new DateOnlyPeriod(theDate, theDate.AddDays(1));
			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			const double skillDay1Shrinkage = 0.2d;
			var skillDay1 = SkillSetupHelper.CreateSkillDay(skill, scenario, theDate.Date, openHour, false);
			skillDay1.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(skillDay1Shrinkage));
			var skillDay2 = SkillSetupHelper.CreateSkillDay(skill, scenario, theDate.Date.AddDays(1), openHour, false);
			
			SkillRepository.Has(skill);
			WorkloadRepository.Add(skill.WorkloadCollection.First());
			SkillDayRepository.Add(skillDay1);
			SkillDayRepository.Add(skillDay2);

			var model = Target.Load(skill.WorkloadCollection.First().Id.Value, period);

			var todayOpenStartTime = theDate.Date.AddHours(openHour.StartTime.TotalHours);
			var skilldDay1TaskPeriods = skillDay1.WorkloadDayCollection.First().OpenTaskPeriodList;
			var skilldDay2TaskPeriods = skillDay2.WorkloadDayCollection.First().OpenTaskPeriodList;

			model.IntervalModelForecast.Count.Should().Be.EqualTo(4);
			assertIntervalData(
				model.IntervalModelForecast.First(),
				todayOpenStartTime, 
				skillDay1.SkillStaffPeriodCollection.First(),
				skilldDay1TaskPeriods.First(),
				skillDay1Shrinkage
			);
			assertIntervalData(
				model.IntervalModelForecast[1],
				todayOpenStartTime.AddMinutes(minutesPerInterval),
				skillDay1.SkillStaffPeriodCollection.Last(),
				skilldDay1TaskPeriods.Last(),
				skillDay1Shrinkage
			);
			assertIntervalData(
				model.IntervalModelForecast[2],
				todayOpenStartTime.AddDays(1),
				skillDay2.SkillStaffPeriodCollection.First(),
				skilldDay2TaskPeriods.First(),
				0
			);
			assertIntervalData(
				model.IntervalModelForecast[3],
				todayOpenStartTime.AddDays(1).AddMinutes(minutesPerInterval),
				skillDay2.SkillStaffPeriodCollection.Last(),
				skilldDay2TaskPeriods.Last(),
				0
			);
		}
		
		[Test]
		public void ShouldOnlyExportForGivenPeriod()
		{
			var theDate = new DateOnly(2016, 8, 26);
			var period = new DateOnlyPeriod(theDate, theDate);
			var openHour = new TimePeriod(8, 0, 8, 30);
			var skill = createSkill(minutesPerInterval, "skill", openHour, false, 0);
			
			var scenario = StaffingViewModelCreatorTestHelper.FakeScenarioAndIntervalLength(IntervalLengthFetcher, ScenarioRepository, minutesPerInterval);
			var skillDay1 = SkillSetupHelper.CreateSkillDay(skill, scenario, theDate.Date.AddDays(-1), openHour, false);
			var skillDay2 = SkillSetupHelper.CreateSkillDay(skill, scenario, theDate.Date, openHour, false);
			var skillDay3 = SkillSetupHelper.CreateSkillDay(skill, scenario, theDate.Date.AddDays(1), openHour, false);

			SkillRepository.Has(skill);
			WorkloadRepository.Add(skill.WorkloadCollection.First());
			SkillDayRepository.Add(skillDay1);
			SkillDayRepository.Add(skillDay2);
			SkillDayRepository.Add(skillDay3);

			var model = Target.Load(skill.WorkloadCollection.First().Id.Value, period);

			model.DailyModelForecast.Count.Should().Be.EqualTo(1);
			model.DailyModelForecast.First().ForecastDate.Should().Be.EqualTo(theDate.Date);
			model.IntervalModelForecast.Any(x => x.IntervalStart.Date == theDate.Date).Should().Be.True();
			model.IntervalModelForecast.Any(x => x.IntervalStart.Date != theDate.Date).Should().Be.False();
		}

		private void assertIntervalData(
			ForecastExportIntervalModel intervalModel, 
			DateTime intervalStart, 
			ISkillStaffPeriod staffingInterval, 
			ITemplateTaskPeriod taskPeriod, 
			double shrinkage)
		{
			var agentsNoShrinkage = staffingInterval.FStaff * (1 - shrinkage);
			intervalModel.IntervalStart.Should().Be.EqualTo(intervalStart);
			intervalModel.Calls.Should().Be.EqualTo(taskPeriod.TotalTasks);
			intervalModel.AverageTalkTime.Should().Be.EqualTo(taskPeriod.TotalAverageTaskTime.TotalSeconds);
			intervalModel.AverageAfterCallWork.Should().Be.EqualTo(taskPeriod.TotalAverageAfterTaskTime.TotalSeconds);
			intervalModel.AverageHandleTime.Should().Be.EqualTo(intervalModel.AverageTalkTime + intervalModel.AverageAfterCallWork);
			intervalModel.Agents.Should().Be.EqualTo(agentsNoShrinkage);
			intervalModel.AgentsShrinkage.Should().Be.EqualTo(staffingInterval.FStaff);
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
