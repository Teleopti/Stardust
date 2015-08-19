using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Backlog;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Outbound
{
	[TestFixture]
	class CampaignTaskManagerTest
	{
		private OutboundProductionPlanFactory _outboundProductionPlanFactory;
		private IOutboundScheduledResourcesProvider _outboundScheduledResourcesProvider;
		private IOutboundCampaign _campaign;

		[SetUp]
		public void Setup()
		{
			_outboundProductionPlanFactory = new OutboundProductionPlanFactory(new IncomingTaskFactory(new FlatDistributionSetter()));
			_outboundScheduledResourcesProvider = MockRepository.GenerateMock<IOutboundScheduledResourcesProvider>();
			_campaign = MockRepository.GenerateMock<IOutboundCampaign>();
			_campaign.Stub(x => x.SpanningPeriod).Return(new DateOnlyPeriod(2015, 8, 18, 2015, 8, 18));
			_campaign.Stub(x => x.CampaignTasks()).Return(1000);
			_campaign.Stub(x => x.AverageTaskHandlingTime()).Return(TimeSpan.FromMinutes(6));
			var workingHours = new Dictionary<DayOfWeek, TimePeriod>();
			workingHours.Add(DayOfWeek.Tuesday, new TimePeriod(TimeSpan.FromHours(9), TimeSpan.FromHours(17)));
			_campaign.Stub(x => x.WorkingHours).Return(workingHours);
		}

		[Test]
		public void ShouldGetRealPlannedTime()
		{
			var date = new DateOnly(2015, 8, 18);
			_outboundScheduledResourcesProvider.Stub(x => x.GetScheduledTimeOnDate(date, _campaign.Skill))
				.IgnoreArguments()
				.Return(TimeSpan.Zero);

			var target = new CampaignTaskManager(_outboundProductionPlanFactory, _outboundScheduledResourcesProvider);
			var result = target.GetIncomingTaskFromCampaign(_campaign);

			result.GetRealPlannedTimeOnDate(date).Should().Be.EqualTo(new TimeSpan(4, 4, 0, 0));
		}

		[Test]
		public void ShouldCoverWithManualPlannedHours()
		{
			var date = new DateOnly(2015, 8, 18);
			var expectedTime = new TimeSpan(1, 0, 0);
			_campaign.Stub(x => x.GetManualProductionPlan(date)).Return(expectedTime);
			_outboundScheduledResourcesProvider.Stub(x => x.GetScheduledTimeOnDate(date, _campaign.Skill))
				.IgnoreArguments()
				.Return(TimeSpan.Zero);

			var target = new CampaignTaskManager(_outboundProductionPlanFactory, _outboundScheduledResourcesProvider);
			var result = target.GetIncomingTaskFromCampaign(_campaign);

			result.GetTimeOnDate(date).Should().Be.EqualTo(expectedTime);
			result.GetRealPlannedTimeOnDate(date).Should().Be.EqualTo(expectedTime);
		}

		[Test]
		public void ShouldCoverWithScheduleHours()
		{
			var date = new DateOnly(2015, 8, 18);
			var manualTime = new TimeSpan(1, 0, 0);
			var expectedTime = new TimeSpan(2, 0, 0);
			_campaign.Stub(x => x.GetManualProductionPlan(date)).Return(manualTime);
			_outboundScheduledResourcesProvider.Stub(x => x.GetScheduledTimeOnDate(date, _campaign.Skill))
				.IgnoreArguments()
				.Return(expectedTime);

			var target = new CampaignTaskManager(_outboundProductionPlanFactory, _outboundScheduledResourcesProvider);
			var result = target.GetIncomingTaskFromCampaign(_campaign);

			result.GetTimeOnDate(date).Should().Be.EqualTo(expectedTime);
			result.GetRealScheduledTimeOnDate(date).Should().Be.EqualTo(expectedTime);
		}

		[Test]
		public void ShouldGetTrueForManualInfoWhenThereIsManualPlan()
		{
			var date = new DateOnly(2015, 8, 18);
			_campaign.Stub(x => x.GetManualProductionPlan(date)).Return(new TimeSpan(1, 0, 0));
			_outboundScheduledResourcesProvider.Stub(x => x.GetScheduledTimeOnDate(date, _campaign.Skill))
				.IgnoreArguments()
				.Return(TimeSpan.Zero);

			var target = new CampaignTaskManager(_outboundProductionPlanFactory, _outboundScheduledResourcesProvider);
			var result = target.GetIncomingTaskFromCampaign(_campaign);

			result.GetManualPlannedInfoOnDate(date).Should().Be.True();
		}		
		
		[Test]
		public void ShouldGetFalseForManualInfoWhenThereIsNoManualPlan()
		{
			var date = new DateOnly(2015, 8, 18);
			_outboundScheduledResourcesProvider.Stub(x => x.GetScheduledTimeOnDate(date, _campaign.Skill))
				.IgnoreArguments()
				.Return(TimeSpan.Zero);

			var target = new CampaignTaskManager(_outboundProductionPlanFactory, _outboundScheduledResourcesProvider);
			var result = target.GetIncomingTaskFromCampaign(_campaign);

			result.GetManualPlannedInfoOnDate(date).Should().Be.False();
		}
	}
}
