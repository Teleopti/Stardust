using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Backlog;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;


namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core
{
	[TestFixture]
	class CampaignTaskManagerTest
	{
		private OutboundProductionPlanFactory _outboundProductionPlanFactory;
		private IOutboundScheduledResourcesProvider _outboundScheduledResourcesProvider;
		private FakeOutboundScheduledResourcesCacher _outboundScheduledResourcesCacher;

		private IOutboundCampaign _campaign;

		[SetUp]
		public void Setup()
		{
			var skill = SkillFactory.CreateSkill("mySkill", SkillTypeFactory.CreateSkillTypePhone(), 15, TimeZoneInfo.Utc, TimeSpan.Zero);
			_outboundProductionPlanFactory = new OutboundProductionPlanFactory(new IncomingTaskFactory(new FlatDistributionSetter()));
			_outboundScheduledResourcesProvider = MockRepository.GenerateMock<IOutboundScheduledResourcesProvider>();
			_outboundScheduledResourcesCacher = new FakeOutboundScheduledResourcesCacher();
			
			_campaign = MockRepository.GenerateMock<IOutboundCampaign>();
			_campaign.Stub(x => x.Id).Return(new Guid());
			_campaign.Stub(x => x.SpanningPeriod).Return(new DateTimePeriod(2015, 8, 18, 2015, 8, 18));
			_campaign.Stub(x => x.CampaignTasks()).Return(1000);
			_campaign.Stub(x => x.AverageTaskHandlingTime()).Return(TimeSpan.FromMinutes(6));
			_campaign.Stub(x => x.Skill).Return(skill);
			var workingHours = new Dictionary<DayOfWeek, TimePeriod>();
			workingHours.Add(DayOfWeek.Tuesday, new TimePeriod(TimeSpan.FromHours(9), TimeSpan.FromHours(17)));
			_campaign.Stub(x => x.WorkingHours).Return(workingHours);
		}

		[Test]
		public void ShouldGetRealPlannedTimeWithoutForecastedData()
		{
			var date = new DateOnly(2015, 8, 18);
			_outboundScheduledResourcesProvider.Stub(x => x.GetScheduledTimeOnDate(date, _campaign.Skill))
				.IgnoreArguments()
				.Return(TimeSpan.Zero);

			var target = new CampaignTaskManager(_outboundProductionPlanFactory, _outboundScheduledResourcesProvider, _outboundScheduledResourcesCacher);
			var result = target.GetIncomingTaskFromCampaign(_campaign, new List<DateOnly>());

			result.GetRealPlannedTimeOnDate(date).Should().Be.EqualTo(new TimeSpan(4, 4, 0, 0));
		}

		[Test]
		public void ShouldGetRealPlannedTimeWithForecastedData()
		{
			var date = new DateOnly(2015, 8, 18);
			var expectedTime = new TimeSpan(1, 0, 0);
			_outboundScheduledResourcesProvider.Stub(x => x.GetScheduledTimeOnDate(date, _campaign.Skill))
				.IgnoreArguments()
				.Return(TimeSpan.Zero);
			_outboundScheduledResourcesProvider.Stub(x => x.GetForecastedTimeOnDate(date, _campaign.Skill))
				.IgnoreArguments()
				.Return(expectedTime);

			var target = new CampaignTaskManager(_outboundProductionPlanFactory, _outboundScheduledResourcesProvider, _outboundScheduledResourcesCacher);
			var result = target.GetIncomingTaskFromCampaign(_campaign, new List<DateOnly>());

			result.GetRealPlannedTimeOnDate(date).Should().Be.EqualTo(expectedTime);
		}

		[Test]
		public void ShouldCoverWithManualPlannedHours()
		{
			var date = new DateOnly(2015, 8, 18);
			var expectedTime = new TimeSpan(1, 0, 0);
			var forecastedTime = new TimeSpan(2, 0, 0);
			_campaign.Stub(x => x.GetManualProductionPlan(date)).Return(expectedTime);
			_outboundScheduledResourcesProvider.Stub(x => x.GetScheduledTimeOnDate(date, _campaign.Skill))
				.IgnoreArguments()
				.Return(TimeSpan.Zero);
			_outboundScheduledResourcesProvider.Stub(x => x.GetForecastedTimeOnDate(date, _campaign.Skill))
				.IgnoreArguments()
				.Return(forecastedTime);

			var target = new CampaignTaskManager(_outboundProductionPlanFactory, _outboundScheduledResourcesProvider, _outboundScheduledResourcesCacher);
			var result = target.GetIncomingTaskFromCampaign(_campaign, new List<DateOnly>());

			result.GetTimeOnDate(date).Should().Be.EqualTo(expectedTime);
			result.GetRealPlannedTimeOnDate(date).Should().Be.EqualTo(expectedTime);
		}

		[Test]
		public void ShouldCoverWithScheduleHours()
		{
			var date = new DateOnly(2015, 8, 18);
			var manualTime = new TimeSpan(1, 0, 0);
			var forecastedTime = new TimeSpan(2, 0, 0);
			var expectedTime = new TimeSpan(3, 0, 0);
			_campaign.Stub(x => x.GetManualProductionPlan(date)).Return(manualTime);
			_outboundScheduledResourcesProvider.Stub(x => x.GetScheduledTimeOnDate(date, _campaign.Skill))
				.IgnoreArguments()
				.Return(expectedTime);
			_outboundScheduledResourcesProvider.Stub(x => x.GetForecastedTimeOnDate(date, _campaign.Skill))
				.IgnoreArguments()
				.Return(forecastedTime);

			var target = new CampaignTaskManager(_outboundProductionPlanFactory, _outboundScheduledResourcesProvider, _outboundScheduledResourcesCacher);
			var result = target.GetIncomingTaskFromCampaign(_campaign, new List<DateOnly>());

			result.GetTimeOnDate(date).Should().Be.EqualTo(expectedTime);
			result.GetRealScheduledTimeOnDate(date).Should().Be.EqualTo(expectedTime);
		}

		[Test]
		public void ShouldCoverWithForecastedHours()
		{
			var date = new DateOnly(2015, 8, 18);
			var expectedTime = new TimeSpan(1, 0, 0);
			_outboundScheduledResourcesProvider.Stub(x => x.GetScheduledTimeOnDate(date, _campaign.Skill))
				.IgnoreArguments()
				.Return(TimeSpan.Zero);
			_outboundScheduledResourcesProvider.Stub(x => x.GetForecastedTimeOnDate(date, _campaign.Skill))
				.IgnoreArguments()
				.Return(expectedTime);

			var target = new CampaignTaskManager(_outboundProductionPlanFactory, _outboundScheduledResourcesProvider, _outboundScheduledResourcesCacher);
			var result = target.GetIncomingTaskFromCampaign(_campaign, new List<DateOnly>());

			result.GetTimeOnDate(date).Should().Be.EqualTo(expectedTime);
		}

		[Test]
		public void ShouldGetTrueForManualInfoWhenThereIsManualPlan()
		{
			var date = new DateOnly(2015, 8, 18);
			_campaign.Stub(x => x.GetManualProductionPlan(date)).Return(new TimeSpan(1, 0, 0));
			_outboundScheduledResourcesProvider.Stub(x => x.GetScheduledTimeOnDate(date, _campaign.Skill))
				.IgnoreArguments()
				.Return(TimeSpan.Zero);

			var target = new CampaignTaskManager(_outboundProductionPlanFactory, _outboundScheduledResourcesProvider, _outboundScheduledResourcesCacher);
			var result = target.GetIncomingTaskFromCampaign(_campaign, new List<DateOnly>());

			result.GetManualPlannedInfoOnDate(date).Should().Be.True();
		}

		[Test]
		public void ShouldGetFalseForManualInfoWhenThereIsNoManualPlan()
		{
			var date = new DateOnly(2015, 8, 18);
			_outboundScheduledResourcesProvider.Stub(x => x.GetScheduledTimeOnDate(date, _campaign.Skill))
				.IgnoreArguments()
				.Return(TimeSpan.Zero);

			var target = new CampaignTaskManager(_outboundProductionPlanFactory, _outboundScheduledResourcesProvider, _outboundScheduledResourcesCacher);
			var result = target.GetIncomingTaskFromCampaign(_campaign, new List<DateOnly>());

			result.GetManualPlannedInfoOnDate(date).Should().Be.False();
		}

		[Test]
		public void ShouldUseCachedScheduleIfCacheIsAvailable()
		{
			var date = new DateOnly(2015, 8, 18);
			_outboundScheduledResourcesProvider.Stub(x => x.GetScheduledTimeOnDate(date, _campaign.Skill))
				.IgnoreArguments()
				.Return(TimeSpan.Zero);

			_outboundScheduledResourcesCacher.AddCampaignSchedule(_campaign, new Dictionary<DateOnly, TimeSpan>
			{
				{date, new TimeSpan(10, 0, 0)}
			});

			var target = new CampaignTaskManager(_outboundProductionPlanFactory, _outboundScheduledResourcesProvider, _outboundScheduledResourcesCacher);
			var result = target.GetIncomingTaskFromCampaign(_campaign, new List<DateOnly>());

			result.GetRealScheduledTimeOnDate(date).Should().Be.EqualTo(new TimeSpan(10, 0, 0));
		}

		[Test]
		public void ShouldUseCachedForecastIfCacheIsAvailable()
		{
			var date = new DateOnly(2015, 8, 18);
			_outboundScheduledResourcesProvider.Stub(x => x.GetScheduledTimeOnDate(date, _campaign.Skill))
				.IgnoreArguments()
				.Return(TimeSpan.Zero);

			_outboundScheduledResourcesCacher.AddCampaignForecasts(_campaign, new Dictionary<DateOnly, TimeSpan>
			{
				{date, new TimeSpan(10, 0, 0)}
			});

			var target = new CampaignTaskManager(_outboundProductionPlanFactory, _outboundScheduledResourcesProvider, _outboundScheduledResourcesCacher);
			var result = target.GetIncomingTaskFromCampaign(_campaign, new List<DateOnly>());

			result.GetRealPlannedTimeOnDate(date).Should().Be.EqualTo(new TimeSpan(10, 0, 0));
		}

		[Test]
		public void ShouldHideScheduledForSkippedDates()
		{
			var date = new DateOnly(2015, 8, 18);
			var manualTime = new TimeSpan(1, 0, 0);
			var forecastedTime = new TimeSpan(2, 0, 0);
			var scheduledTime = new TimeSpan(3, 0, 0);
			_campaign.Stub(x => x.GetManualProductionPlan(date)).Return(manualTime);
			_outboundScheduledResourcesProvider.Stub(x => x.GetScheduledTimeOnDate(date, _campaign.Skill))
				.IgnoreArguments()
				.Return(scheduledTime);
			_outboundScheduledResourcesProvider.Stub(x => x.GetForecastedTimeOnDate(date, _campaign.Skill))
				.IgnoreArguments()
				.Return(forecastedTime);

			var target = new CampaignTaskManager(_outboundProductionPlanFactory, _outboundScheduledResourcesProvider, _outboundScheduledResourcesCacher);
			var result = target.GetIncomingTaskFromCampaign(_campaign, new List<DateOnly> {date});

			result.GetTimeOnDate(date).Should().Be.EqualTo(manualTime);
			result.GetRealScheduledTimeOnDate(date).Should().Be.EqualTo(TimeSpan.Zero);
		}
	}
}
