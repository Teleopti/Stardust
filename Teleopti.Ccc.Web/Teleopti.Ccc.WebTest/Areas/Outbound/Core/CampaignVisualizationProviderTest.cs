﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core
{
	[TestFixture]
	class CampaignVisualizationProviderTest
	{
		private IOutboundCampaignRepository _campaignRepository;
		private IOutboundCampaignTaskManager _taskManager;

		[SetUp]
		public void Setup()
		{
			_campaignRepository = MockRepository.GenerateMock<IOutboundCampaignRepository>();
			_taskManager = MockRepository.GenerateMock<IOutboundCampaignTaskManager>();
		}

		[Test]
		public void ShouldReturnEmptyWhenCampaignIsNull()
		{
			var id = new Guid();
			_campaignRepository.Stub(x => x.Get(id)).Return(null);

			var target = new CampaignVisualizationProvider(_campaignRepository, null);
			var result = target.ProvideVisualization(id);

			result.Dates.Count.Should().Be.EqualTo(0);
			result.PlannedPersonHours.Count.Should().Be.EqualTo(0);
			result.BacklogPersonHours.Count.Should().Be.EqualTo(0);
			result.ScheduledPersonHours.Count.Should().Be.EqualTo(0);
			result.IsManualPlanned.Count.Should().Be.EqualTo(0);
			result.IsCloseDays.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetDates()
		{
			var id = new Guid();
			var campaign = new Domain.Outbound.Campaign();
			var date = new DateOnly(2015, 7, 24);
			campaign.SpanningPeriod = new DateOnlyPeriod(date, date);
			_campaignRepository.Stub(x => x.Get(id)).Return(campaign);
			var incomingTask = MockRepository.GenerateMock<IBacklogTask>();
			incomingTask.Stub(x => x.GetRealPlannedTimeOnDate(date)).Return(TimeSpan.FromHours(1));
			incomingTask.Stub(x => x.GetRealScheduledTimeOnDate(date)).Return(TimeSpan.FromHours(1));
			incomingTask.Stub(x => x.GetBacklogOnDate(date)).Return(TimeSpan.FromHours(1));
			_taskManager.Stub(x => x.GetIncomingTaskFromCampaign(campaign)).Return(incomingTask);

			var target = new CampaignVisualizationProvider(_campaignRepository, _taskManager);
			var result = target.ProvideVisualization(id);

			result.Dates.Count.Should().Be.EqualTo(1);
			result.Dates[0].Should().Be.EqualTo(date);
		}

		[Test]
		public void ShouldGetPlannedHours()
		{
			var id = new Guid();
			var campaign = new Domain.Outbound.Campaign();
			var date = new DateOnly(2015, 7, 24);
			var hour = TimeSpan.FromHours(1);
			campaign.SpanningPeriod = new DateOnlyPeriod(date, date);
			_campaignRepository.Stub(x => x.Get(id)).Return(campaign);
			var incomingTask = MockRepository.GenerateMock<IBacklogTask>();
			incomingTask.Stub(x => x.GetRealPlannedTimeOnDate(date)).Return(hour);
			incomingTask.Stub(x => x.GetRealScheduledTimeOnDate(date)).Return(TimeSpan.FromHours(1));
			incomingTask.Stub(x => x.GetBacklogOnDate(date)).Return(TimeSpan.FromHours(1));
			_taskManager.Stub(x => x.GetIncomingTaskFromCampaign(campaign)).Return(incomingTask);

			var target = new CampaignVisualizationProvider(_campaignRepository, _taskManager);
			var result = target.ProvideVisualization(id);

			result.PlannedPersonHours.Count.Should().Be.EqualTo(1);
			result.PlannedPersonHours[0].Should().Be.EqualTo(hour.Hours);
		}		
		
		[Test]
		public void ShouldGetBacklogHours()
		{
			var id = new Guid();
			var campaign = new Domain.Outbound.Campaign();
			var date = new DateOnly(2015, 7, 24);
			var hour = TimeSpan.FromHours(1);
			campaign.SpanningPeriod = new DateOnlyPeriod(date, date);
			_campaignRepository.Stub(x => x.Get(id)).Return(campaign);
			var incomingTask = MockRepository.GenerateMock<IBacklogTask>();
			incomingTask.Stub(x => x.GetRealPlannedTimeOnDate(date)).Return(hour);
			incomingTask.Stub(x => x.GetRealScheduledTimeOnDate(date)).Return(TimeSpan.FromHours(1));
			incomingTask.Stub(x => x.GetBacklogOnDate(date)).Return(hour);
			_taskManager.Stub(x => x.GetIncomingTaskFromCampaign(campaign)).Return(incomingTask);

			var target = new CampaignVisualizationProvider(_campaignRepository, _taskManager);
			var result = target.ProvideVisualization(id);

			result.BacklogPersonHours.Count.Should().Be.EqualTo(1);
			result.BacklogPersonHours[0].Should().Be.EqualTo(hour.Hours);
		}

		[Test]
		public void ShouldGetOverstaffHours()
		{
			var id = new Guid();
			var campaign = new Domain.Outbound.Campaign();
			var date = new DateOnly(2015, 7, 24);
			var endDate = new DateOnly(2015, 7, 25);
			
			campaign.SpanningPeriod = new DateOnlyPeriod(date, endDate);

			_campaignRepository.Stub(x => x.Get(id)).Return(campaign);
			var incomingTask = MockRepository.GenerateMock<IBacklogTask>();
			incomingTask.Stub(x => x.GetRealPlannedTimeOnDate(date)).Return(TimeSpan.FromHours(10));
			incomingTask.Stub(x => x.GetRealScheduledTimeOnDate(date)).Return(TimeSpan.FromHours(0));
			incomingTask.Stub(x => x.GetBacklogOnDate(date)).Return(TimeSpan.FromHours(5));
			incomingTask.Stub(x => x.GetRealPlannedTimeOnDate(endDate)).Return(TimeSpan.FromHours(10));
			incomingTask.Stub(x => x.GetRealScheduledTimeOnDate(endDate)).Return(TimeSpan.FromHours(0));
			incomingTask.Stub(x => x.GetBacklogOnDate(endDate)).Return(TimeSpan.FromHours(0));

			_taskManager.Stub(x => x.GetIncomingTaskFromCampaign(campaign)).Return(incomingTask);

			var target = new CampaignVisualizationProvider(_campaignRepository, _taskManager);
			var result = target.ProvideVisualization(id);

			result.OverstaffPersonHours.Count.Should().Be.EqualTo(2);
			result.OverstaffPersonHours[0].Should().Be.EqualTo(0);
			result.OverstaffPersonHours[1].Should().Be.EqualTo(5);
			result.PlannedPersonHours[1].Should().Be.EqualTo(5);

		}

		[Test]
		public void ShouldGetScheduledHours()
		{
			var id = new Guid();
			var campaign = new Domain.Outbound.Campaign();
			var date = new DateOnly(2015, 7, 24);
			var hour = TimeSpan.FromHours(1);
			campaign.SpanningPeriod = new DateOnlyPeriod(date, date);
			_campaignRepository.Stub(x => x.Get(id)).Return(campaign);
			var incomingTask = MockRepository.GenerateMock<IBacklogTask>();
			incomingTask.Stub(x => x.GetRealPlannedTimeOnDate(date)).Return(hour);
			incomingTask.Stub(x => x.GetRealScheduledTimeOnDate(date)).Return(hour);
			incomingTask.Stub(x => x.GetBacklogOnDate(date)).Return(hour);
			_taskManager.Stub(x => x.GetIncomingTaskFromCampaign(campaign)).Return(incomingTask);

			var target = new CampaignVisualizationProvider(_campaignRepository, _taskManager);
			var result = target.ProvideVisualization(id);

			result.ScheduledPersonHours.Count.Should().Be.EqualTo(1);
			result.ScheduledPersonHours[0].Should().Be.EqualTo(hour.Hours);
		}			
		
		[Test]
		public void ShouldGetManualPlanInfo()
		{
			var id = new Guid();
			var campaign = new Domain.Outbound.Campaign();
			var date = new DateOnly(2015, 7, 24);
			var hour = TimeSpan.FromHours(1);
			campaign.SpanningPeriod = new DateOnlyPeriod(date, date);
			_campaignRepository.Stub(x => x.Get(id)).Return(campaign);
			var incomingTask = MockRepository.GenerateMock<IBacklogTask>();
			incomingTask.Stub(x => x.GetRealPlannedTimeOnDate(date)).Return(hour);
			incomingTask.Stub(x => x.GetRealScheduledTimeOnDate(date)).Return(hour);
			incomingTask.Stub(x => x.GetBacklogOnDate(date)).Return(hour);
			incomingTask.Stub(x => x.GetManualPlannedInfoOnDate(date)).Return(true);
			_taskManager.Stub(x => x.GetIncomingTaskFromCampaign(campaign)).Return(incomingTask);

			var target = new CampaignVisualizationProvider(_campaignRepository, _taskManager);
			var result = target.ProvideVisualization(id);

			result.IsManualPlanned.Count.Should().Be.EqualTo(1);
			result.IsManualPlanned[0].Should().Be.True();
		}			
		
		[Test]
		public void ShouldGetTrueWhenCloseDay()
		{
			var id = new Guid();
			var campaign = new Domain.Outbound.Campaign();
			var date = new DateOnly(2015, 7, 24);
			var hour = TimeSpan.FromHours(1);
			campaign.SpanningPeriod = new DateOnlyPeriod(date, date);
			_campaignRepository.Stub(x => x.Get(id)).Return(campaign);
			var incomingTask = MockRepository.GenerateMock<IBacklogTask>();
			incomingTask.Stub(x => x.GetRealPlannedTimeOnDate(date)).Return(hour);
			incomingTask.Stub(x => x.GetRealScheduledTimeOnDate(date)).Return(hour);
			incomingTask.Stub(x => x.GetBacklogOnDate(date)).Return(hour);
			incomingTask.Stub(x => x.PlannedTimeTypeOnDate(date)).Return(PlannedTimeTypeEnum.Closed);
			_taskManager.Stub(x => x.GetIncomingTaskFromCampaign(campaign)).Return(incomingTask);

			var target = new CampaignVisualizationProvider(_campaignRepository, _taskManager);
			var result = target.ProvideVisualization(id);

			result.IsCloseDays.Count.Should().Be.EqualTo(1);
			result.IsCloseDays[0].Should().Be.True();
		}		
		
		[Test]
		public void ShouldGetFalseWhenNotCloseDay()
		{
			var id = new Guid();
			var campaign = new Domain.Outbound.Campaign();
			var date = new DateOnly(2015, 7, 24);
			var hour = TimeSpan.FromHours(1);
			campaign.SpanningPeriod = new DateOnlyPeriod(date, date);
			_campaignRepository.Stub(x => x.Get(id)).Return(campaign);
			var incomingTask = MockRepository.GenerateMock<IBacklogTask>();
			incomingTask.Stub(x => x.GetRealPlannedTimeOnDate(date)).Return(hour);
			incomingTask.Stub(x => x.GetRealScheduledTimeOnDate(date)).Return(hour);
			incomingTask.Stub(x => x.GetBacklogOnDate(date)).Return(hour);
			incomingTask.Stub(x => x.PlannedTimeTypeOnDate(date)).Return(PlannedTimeTypeEnum.Scheduled);
			_taskManager.Stub(x => x.GetIncomingTaskFromCampaign(campaign)).Return(incomingTask);

			var target = new CampaignVisualizationProvider(_campaignRepository, _taskManager);
			var result = target.ProvideVisualization(id);

			result.IsCloseDays.Count.Should().Be.EqualTo(1);
			result.IsCloseDays[0].Should().Be.False();
		}		
		
		[Test]
		public void AllListShouldBeSameLength()
		{
			var id = new Guid();
			var campaign = new Domain.Outbound.Campaign();
			var date = new DateOnly(2015, 7, 24);
			var hour = TimeSpan.FromHours(1);
			campaign.SpanningPeriod = new DateOnlyPeriod(date, date.AddDays(10));
			_campaignRepository.Stub(x => x.Get(id)).Return(campaign);
			var incomingTask = MockRepository.GenerateMock<IBacklogTask>();
			incomingTask.Stub(x => x.GetRealPlannedTimeOnDate(date)).IgnoreArguments().Return(hour);
			incomingTask.Stub(x => x.GetRealScheduledTimeOnDate(date)).IgnoreArguments().Return(hour);
			incomingTask.Stub(x => x.GetBacklogOnDate(date)).IgnoreArguments().Return(hour);
			incomingTask.Stub(x => x.GetManualPlannedInfoOnDate(date)).IgnoreArguments().Return(false);
			_taskManager.Stub(x => x.GetIncomingTaskFromCampaign(campaign)).Return(incomingTask);

			var target = new CampaignVisualizationProvider(_campaignRepository, _taskManager);
			var result = target.ProvideVisualization(id);

			result.Dates.Count.Should().Be.EqualTo(result.PlannedPersonHours.Count);
			result.PlannedPersonHours.Count.Should().Be.EqualTo(result.BacklogPersonHours.Count);
			result.BacklogPersonHours.Count.Should().Be.EqualTo(result.ScheduledPersonHours.Count);
			result.ScheduledPersonHours.Count.Should().Be.EqualTo(result.IsManualPlanned.Count);
			result.IsManualPlanned.Count.Should().Be.EqualTo(result.IsCloseDays.Count);
		}
	}
}
