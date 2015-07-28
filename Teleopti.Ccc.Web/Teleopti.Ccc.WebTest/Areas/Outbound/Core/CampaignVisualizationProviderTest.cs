using System;
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
			incomingTask.Stub(x => x.GetPlannedTimeOnDate(date)).Return(TimeSpan.FromHours(1));
			incomingTask.Stub(x => x.GetScheduledTimeOnDate(date)).Return(TimeSpan.FromHours(1));
			incomingTask.Stub(x => x.GetEstimatedIncomingBacklogOnDate(date)).Return(TimeSpan.FromHours(1));
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
			incomingTask.Stub(x => x.GetPlannedTimeOnDate(date)).Return(hour);
			incomingTask.Stub(x => x.GetScheduledTimeOnDate(date)).Return(TimeSpan.FromHours(1));
			incomingTask.Stub(x => x.GetEstimatedIncomingBacklogOnDate(date)).Return(TimeSpan.FromHours(1));
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
			incomingTask.Stub(x => x.GetPlannedTimeOnDate(date)).Return(hour);
			incomingTask.Stub(x => x.GetScheduledTimeOnDate(date)).Return(TimeSpan.FromHours(1));
			incomingTask.Stub(x => x.GetEstimatedIncomingBacklogOnDate(date)).Return(hour);
			_taskManager.Stub(x => x.GetIncomingTaskFromCampaign(campaign)).Return(incomingTask);

			var target = new CampaignVisualizationProvider(_campaignRepository, _taskManager);
			var result = target.ProvideVisualization(id);

			result.BacklogPersonHours.Count.Should().Be.EqualTo(1);
			result.BacklogPersonHours[0].Should().Be.EqualTo(hour.Hours);
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
			incomingTask.Stub(x => x.GetPlannedTimeOnDate(date)).Return(hour);
			incomingTask.Stub(x => x.GetScheduledTimeOnDate(date)).Return(hour);
			incomingTask.Stub(x => x.GetEstimatedIncomingBacklogOnDate(date)).Return(hour);
			_taskManager.Stub(x => x.GetIncomingTaskFromCampaign(campaign)).Return(incomingTask);

			var target = new CampaignVisualizationProvider(_campaignRepository, _taskManager);
			var result = target.ProvideVisualization(id);

			result.ScheduledPersonHours.Count.Should().Be.EqualTo(1);
			result.ScheduledPersonHours[0].Should().Be.EqualTo(hour.Hours);
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
			incomingTask.Stub(x => x.GetPlannedTimeOnDate(date)).IgnoreArguments().Return(hour);
			incomingTask.Stub(x => x.GetScheduledTimeOnDate(date)).IgnoreArguments().Return(hour);
			incomingTask.Stub(x => x.GetEstimatedIncomingBacklogOnDate(date)).IgnoreArguments().Return(hour);
			_taskManager.Stub(x => x.GetIncomingTaskFromCampaign(campaign)).Return(incomingTask);

			var target = new CampaignVisualizationProvider(_campaignRepository, _taskManager);
			var result = target.ProvideVisualization(id);

			result.Dates.Count.Should().Be.EqualTo(result.PlannedPersonHours.Count);
			result.PlannedPersonHours.Count.Should().Be.EqualTo(result.BacklogPersonHours.Count);
			result.BacklogPersonHours.Count.Should().Be.EqualTo(result.ScheduledPersonHours.Count);
		}
	}
}
