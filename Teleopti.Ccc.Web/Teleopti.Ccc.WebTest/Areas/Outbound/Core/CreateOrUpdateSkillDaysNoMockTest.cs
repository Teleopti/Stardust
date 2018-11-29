using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Persisters.Outbound;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Ccc.WebTest.Areas.Outbound.IoC;


namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core
{
	[TestFixture]
	[OutboundTest]
	public class CreateOrUpdateSkillDaysNoMockTest
	{
		public ICreateOrUpdateSkillDays Target;
		public FakeSkillDayRepository SkillDayRepository;
		public IOutboundSkillCreator OutboundSkillCreator;
		public IOutboundSkillPersister OutboundSkillPersister;
		public FakeSkillTypeRepository SkillTypeRepository;
		public OutboundProductionPlanFactory OutboundProductionPlanFactory;

		[Test]
		public void ShouldPreserveIntradayDistributionWhenZeroForecastTake25()
		{
			SkillTypeRepository.Add(new SkillTypeEmail(new Description("SkillTypeOutbound"), ForecastSource.OutboundTelephony));
			var activity = new Activity().WithId();
			var date = new DateOnly(2018, 4, 5);
			var period = new DateOnlyPeriod(date, date);
			var campaign = new Domain.Outbound.Campaign
			{
				Name = "hello",
				CallListLen = 100,
				TargetRate = 100,
				ConnectRate = 100,
				RightPartyConnectRate = 100,
				ConnectAverageHandlingTime = 120,
				RightPartyAverageHandlingTime = 120,
				UnproductiveTime = 0,
				BelongsToPeriod = period
			}.WithId();

			var openHours = new CampaignWorkingHour
			{
				StartTime = TimeSpan.FromHours(8),
				EndTime = TimeSpan.FromHours(17),
				WeekDay = DayOfWeek.Thursday
			};
			campaign.WorkingHours.Add(openHours.WeekDay, new TimePeriod(openHours.StartTime, openHours.EndTime));
			var outboundSkill = OutboundSkillCreator.CreateSkill(activity, campaign);
			OutboundSkillPersister.PersistSkill(outboundSkill);
			campaign.Skill = outboundSkill;
			var spanningPeriod = period.ToDateTimePeriod(campaign.Skill.TimeZone).ChangeEndTime(TimeSpan.FromSeconds(-1));
			campaign.SpanningPeriod = spanningPeriod;

			Target.Create(campaign.Skill, period, campaign.CampaignTasks(),
				campaign.AverageTaskHandlingTime(), campaign.WorkingHours);

			var skillDay = SkillDayRepository.LoadAll().First();
			skillDay.WorkloadDayCollection.First().OpenTaskPeriodList[0].Task.Tasks.Should().Be.EqualTo(100);
			skillDay.WorkloadDayCollection.First().OpenTaskPeriodList[0].Task.AverageTaskTime.Should().Be.EqualTo(TimeSpan.FromMinutes(2));
			skillDay.WorkloadDayCollection.First().OpenTaskPeriodList[1].Task.Tasks.Should().Be.EqualTo(0);

			var incomingTask = OutboundProductionPlanFactory.CreateAndMakeInitialPlan(
				campaign.SpanningPeriod.ToDateOnlyPeriod(campaign.Skill.TimeZone), campaign.CampaignTasks(),
				campaign.AverageTaskHandlingTime(), campaign.WorkingHours);
			var timeOnDate = incomingTask.GetTimeOnDate(date);
			incomingTask.SetTimeOnDate(date, TimeSpan.Zero, PlannedTimeTypeEnum.Calculated);

			Target.UpdateSkillDays(outboundSkill, incomingTask);
			skillDay.WorkloadDayCollection.First().OpenTaskPeriodList[0].Task.Tasks.Should().Be.EqualTo(0);
			skillDay.WorkloadDayCollection.First().OpenTaskPeriodList[0].Task.AverageTaskTime.Should().Be.EqualTo(TimeSpan.FromMinutes(2));

			incomingTask.SetTimeOnDate(date, timeOnDate, PlannedTimeTypeEnum.Scheduled);

			Target.UpdateSkillDays(outboundSkill, incomingTask);
			var tasks = Math.Round(skillDay.WorkloadDayCollection.First().OpenTaskPeriodList[0].Task.Tasks, 2);
			tasks.Should().Be.EqualTo(100.00);
			var handlingTime =
				Math.Round(skillDay.WorkloadDayCollection.First().OpenTaskPeriodList[0].Task.AverageTaskTime.TotalSeconds, 0);
			handlingTime.Should().Be.EqualTo(TimeSpan.FromMinutes(2).TotalSeconds);
			skillDay.WorkloadDayCollection.First().OpenTaskPeriodList[1].Task.Tasks.Should().Be.EqualTo(0);
		}
	}
}