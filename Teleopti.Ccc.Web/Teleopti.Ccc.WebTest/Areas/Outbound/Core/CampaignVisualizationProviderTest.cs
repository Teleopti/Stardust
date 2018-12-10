using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;


namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core
{
	[TestFixture]
	class CampaignVisualizationProviderTest
	{
		private IOutboundCampaignRepository _campaignRepository;
		private IOutboundCampaignTaskManager _taskManager;
		private IOutboundCampaign _campaign;
		private IList<DateOnly> _skippedDates;

		[SetUp]
		public void Setup()
		{
			_campaign = new Domain.Outbound.Campaign();
			_campaign.Skill = SkillFactory.CreateSkill("mySkill", SkillTypeFactory.CreateSkillType(), 15, TimeZoneInfo.Utc, TimeSpan.Zero);
			_campaignRepository = MockRepository.GenerateMock<IOutboundCampaignRepository>();
			_taskManager = MockRepository.GenerateMock<IOutboundCampaignTaskManager>();
			_skippedDates = new List<DateOnly>();
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
			var date = new DateTime(2015, 7, 24, 0, 0, 0, DateTimeKind.Utc);
			var dateOnly = new DateOnly(date.Date);
			_campaign.SpanningPeriod = new DateTimePeriod(date, date);
			_campaignRepository.Stub(x => x.Get(id)).Return(_campaign);
			var incomingTask = MockRepository.GenerateMock<IBacklogTask>();
			incomingTask.Stub(x => x.GetRealPlannedTimeOnDate(dateOnly)).Return(TimeSpan.FromHours(1));
			incomingTask.Stub(x => x.GetRealScheduledTimeOnDate(dateOnly)).Return(TimeSpan.FromHours(1));
			incomingTask.Stub(x => x.GetBacklogOnDate(dateOnly)).Return(TimeSpan.FromHours(1));
			_taskManager.Stub(x => x.GetIncomingTaskFromCampaign(_campaign, _skippedDates)).Return(incomingTask);

			var target = new CampaignVisualizationProvider(_campaignRepository, _taskManager);
			var result = target.ProvideVisualization(id, _skippedDates.ToArray());

			result.Dates.Count.Should().Be.EqualTo(1);
			result.Dates[0].Should().Be.EqualTo(dateOnly);
		}

		[Test]
		public void ShouldGetPlannedHours()
		{
			var id = new Guid();
			var date = new DateTime(2015, 7, 24, 0, 0, 0, DateTimeKind.Utc);
			var dateOnly = new DateOnly(date.Date);
			var hour = TimeSpan.FromHours(1);
			_campaign.SpanningPeriod = new DateTimePeriod(date, date);
			_campaignRepository.Stub(x => x.Get(id)).Return(_campaign);
			var incomingTask = MockRepository.GenerateMock<IBacklogTask>();
			incomingTask.Stub(x => x.GetRealPlannedTimeOnDate(dateOnly)).Return(hour);
			incomingTask.Stub(x => x.GetRealScheduledTimeOnDate(dateOnly)).Return(TimeSpan.FromHours(1));
			incomingTask.Stub(x => x.GetBacklogOnDate(dateOnly)).Return(TimeSpan.FromHours(1));
			_taskManager.Stub(x => x.GetIncomingTaskFromCampaign(_campaign, _skippedDates)).Return(incomingTask);

			var target = new CampaignVisualizationProvider(_campaignRepository, _taskManager);
			var result = target.ProvideVisualization(id, _skippedDates.ToArray());

			result.PlannedPersonHours.Count.Should().Be.EqualTo(1);
			result.PlannedPersonHours[0].Should().Be.EqualTo(hour.Hours);
		}		
		
		[Test]
		public void ShouldGetBacklogHours()
		{
			var id = new Guid();
			var date = new DateTime(2015, 7, 24, 0, 0, 0, DateTimeKind.Utc);
			var dateOnly = new DateOnly(date.Date);
			var hour = TimeSpan.FromHours(1);
			_campaign.SpanningPeriod = new DateTimePeriod(date, date);
			_campaignRepository.Stub(x => x.Get(id)).Return(_campaign);
			var incomingTask = MockRepository.GenerateMock<IBacklogTask>();
			incomingTask.Stub(x => x.GetRealPlannedTimeOnDate(dateOnly)).Return(hour);
			incomingTask.Stub(x => x.GetRealScheduledTimeOnDate(dateOnly)).Return(TimeSpan.FromHours(1));
			incomingTask.Stub(x => x.GetBacklogOnDate(dateOnly)).Return(hour);
			_taskManager.Stub(x => x.GetIncomingTaskFromCampaign(_campaign, _skippedDates)).Return(incomingTask);

			var target = new CampaignVisualizationProvider(_campaignRepository, _taskManager);
			var result = target.ProvideVisualization(id, _skippedDates.ToArray());

			result.BacklogPersonHours.Count.Should().Be.EqualTo(1);
			result.BacklogPersonHours[0].Should().Be.EqualTo(hour.Hours);
		}

		[Test]
		public void ShouldGetOverstaffHours()
		{
			var id = new Guid();
			var date = new DateOnly(2015, 7, 24);
			var endDate = new DateOnly(2015, 7, 25);
			
			_campaign.SpanningPeriod = new DateTimePeriod(new DateTime(date.Date.Ticks, DateTimeKind.Utc), new DateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 59, DateTimeKind.Utc));

			_campaignRepository.Stub(x => x.Get(id)).Return(_campaign);
			var incomingTask = MockRepository.GenerateMock<IBacklogTask>();
			incomingTask.Stub(x => x.GetRealPlannedTimeOnDate(date)).Return(TimeSpan.FromHours(10));
			incomingTask.Stub(x => x.GetRealScheduledTimeOnDate(date)).Return(TimeSpan.FromHours(0));
			incomingTask.Stub(x => x.GetBacklogOnDate(date)).Return(TimeSpan.FromHours(5));
			incomingTask.Stub(x => x.GetRealPlannedTimeOnDate(endDate)).Return(TimeSpan.FromHours(10));
			incomingTask.Stub(x => x.GetRealScheduledTimeOnDate(endDate)).Return(TimeSpan.FromHours(0));
			incomingTask.Stub(x => x.GetBacklogOnDate(endDate)).Return(TimeSpan.FromHours(0));

			_taskManager.Stub(x => x.GetIncomingTaskFromCampaign(_campaign, _skippedDates)).Return(incomingTask);

			var target = new CampaignVisualizationProvider(_campaignRepository, _taskManager);
			var result = target.ProvideVisualization(id, _skippedDates.ToArray());

			result.OverstaffPersonHours.Count.Should().Be.EqualTo(2);
			result.OverstaffPersonHours[0].Should().Be.EqualTo(0);
			result.OverstaffPersonHours[1].Should().Be.EqualTo(5);
			
		}

		[Test]
		public void ShouldGetScheduledHours()
		{
			var id = new Guid();
			var date = new DateTime(2015, 7, 24, 0, 0, 0, DateTimeKind.Utc);
			var dateOnly = new DateOnly(date.Date);
			var hour = TimeSpan.FromHours(1);
			_campaign.SpanningPeriod = new DateTimePeriod(date, date);
			_campaignRepository.Stub(x => x.Get(id)).Return(_campaign);
			var incomingTask = MockRepository.GenerateMock<IBacklogTask>();
			incomingTask.Stub(x => x.GetRealPlannedTimeOnDate(dateOnly)).Return(hour);
			incomingTask.Stub(x => x.GetRealScheduledTimeOnDate(dateOnly)).Return(hour);
			incomingTask.Stub(x => x.GetBacklogOnDate(dateOnly)).Return(hour);
			_taskManager.Stub(x => x.GetIncomingTaskFromCampaign(_campaign, _skippedDates)).Return(incomingTask);

			var target = new CampaignVisualizationProvider(_campaignRepository, _taskManager);
			var result = target.ProvideVisualization(id, _skippedDates.ToArray());

			result.ScheduledPersonHours.Count.Should().Be.EqualTo(1);
			result.ScheduledPersonHours[0].Should().Be.EqualTo(hour.Hours);
		}			
		
		[Test]
		public void ShouldGetManualPlanInfo()
		{
			var id = new Guid();
			var date = new DateTime(2015, 7, 24, 0, 0, 0, DateTimeKind.Utc);
			var dateOnly = new DateOnly(date.Date);
			var hour = TimeSpan.FromHours(1);
			_campaign.SpanningPeriod = new DateTimePeriod(date, date);
			_campaignRepository.Stub(x => x.Get(id)).Return(_campaign);
			var incomingTask = MockRepository.GenerateMock<IBacklogTask>();
			incomingTask.Stub(x => x.GetRealPlannedTimeOnDate(dateOnly)).Return(hour);
			incomingTask.Stub(x => x.GetRealScheduledTimeOnDate(dateOnly)).Return(hour);
			incomingTask.Stub(x => x.GetBacklogOnDate(dateOnly)).Return(hour);
			incomingTask.Stub(x => x.GetManualPlannedInfoOnDate(dateOnly)).Return(true);
			_taskManager.Stub(x => x.GetIncomingTaskFromCampaign(_campaign, _skippedDates)).Return(incomingTask);

			var target = new CampaignVisualizationProvider(_campaignRepository, _taskManager);
			var result = target.ProvideVisualization(id, _skippedDates.ToArray());

			result.IsManualPlanned.Count.Should().Be.EqualTo(1);
			result.IsManualPlanned[0].Should().Be.True();
		}			
		
		[Test]
		public void ShouldGetTrueWhenCloseDay()
		{
			var id = new Guid();
			var date = new DateTime(2015, 7, 24, 0, 0, 0, DateTimeKind.Utc);
			var dateOnly = new DateOnly(date.Date);
			var hour = TimeSpan.FromHours(1);
			_campaign.SpanningPeriod = new DateTimePeriod(date, date);
			_campaignRepository.Stub(x => x.Get(id)).Return(_campaign);
			var incomingTask = MockRepository.GenerateMock<IBacklogTask>();
			incomingTask.Stub(x => x.GetRealPlannedTimeOnDate(dateOnly)).Return(hour);
			incomingTask.Stub(x => x.GetRealScheduledTimeOnDate(dateOnly)).Return(hour);
			incomingTask.Stub(x => x.GetBacklogOnDate(dateOnly)).Return(hour);
			incomingTask.Stub(x => x.PlannedTimeTypeOnDate(dateOnly)).Return(PlannedTimeTypeEnum.Closed);
			_taskManager.Stub(x => x.GetIncomingTaskFromCampaign(_campaign, _skippedDates)).Return(incomingTask);

			var target = new CampaignVisualizationProvider(_campaignRepository, _taskManager);
			var result = target.ProvideVisualization(id, _skippedDates.ToArray());

			result.IsCloseDays.Count.Should().Be.EqualTo(1);
			result.IsCloseDays[0].Should().Be.True();
		}		
		
		[Test]
		public void ShouldGetFalseWhenNotCloseDay()
		{
			var id = new Guid();
			var date = new DateTime(2015, 7, 24, 0, 0, 0, DateTimeKind.Utc);
			var dateOnly = new DateOnly(date.Date);
			var hour = TimeSpan.FromHours(1);
			_campaign.SpanningPeriod = new DateTimePeriod(date, date);
			_campaignRepository.Stub(x => x.Get(id)).Return(_campaign);
			var incomingTask = MockRepository.GenerateMock<IBacklogTask>();
			incomingTask.Stub(x => x.GetRealPlannedTimeOnDate(dateOnly)).Return(hour);
			incomingTask.Stub(x => x.GetRealScheduledTimeOnDate(dateOnly)).Return(hour);
			incomingTask.Stub(x => x.GetBacklogOnDate(dateOnly)).Return(hour);
			incomingTask.Stub(x => x.PlannedTimeTypeOnDate(dateOnly)).Return(PlannedTimeTypeEnum.Scheduled);
			_taskManager.Stub(x => x.GetIncomingTaskFromCampaign(_campaign, _skippedDates)).Return(incomingTask);

			var target = new CampaignVisualizationProvider(_campaignRepository, _taskManager);
			var result = target.ProvideVisualization(id, _skippedDates.ToArray());

			result.IsCloseDays.Count.Should().Be.EqualTo(1);
			result.IsCloseDays[0].Should().Be.False();
		}

		[Test]
		public void ShouldGetTrueWhenHasActualBacklog()
		{
			var id = new Guid();
			var date = new DateTime(2015, 7, 24, 0, 0, 0, DateTimeKind.Utc);
			var dateOnly = new DateOnly(date.Date);
			var hour = TimeSpan.FromHours(1);
			_campaign.SpanningPeriod = new DateTimePeriod(date, date);
			_campaignRepository.Stub(x => x.Get(id)).Return(_campaign);
			var incomingTask = MockRepository.GenerateMock<IBacklogTask>();
			incomingTask.Stub(x => x.GetRealPlannedTimeOnDate(dateOnly)).Return(hour);
			incomingTask.Stub(x => x.GetRealScheduledTimeOnDate(dateOnly)).Return(hour);
			incomingTask.Stub(x => x.GetBacklogOnDate(dateOnly)).Return(hour);
			incomingTask.Stub(x => x.PlannedTimeTypeOnDate(dateOnly)).Return(PlannedTimeTypeEnum.Scheduled);
			incomingTask.Stub(x => x.GetActualBacklogOnDate(dateOnly)).Return(hour);
			_taskManager.Stub(x => x.GetIncomingTaskFromCampaign(_campaign, _skippedDates)).Return(incomingTask);

			var target = new CampaignVisualizationProvider(_campaignRepository, _taskManager);
			var result = target.ProvideVisualization(id, _skippedDates.ToArray());

			result.IsActualBacklog.Count.Should().Be.EqualTo(1);
			result.IsActualBacklog[0].Should().Be.True();
		}		
		
		[Test]
		public void AllListShouldBeSameLength()
		{
			var id = new Guid();
			var date = new DateTime(2015, 7, 24, 0, 0, 0, DateTimeKind.Utc);
			var dateOnly = new DateOnly(date.Date);
			var hour = TimeSpan.FromHours(1);
			_campaign.SpanningPeriod = new DateTimePeriod(date.Date, date.AddDays(10).Date);
			_campaignRepository.Stub(x => x.Get(id)).Return(_campaign);
			var incomingTask = MockRepository.GenerateMock<IBacklogTask>();
			incomingTask.Stub(x => x.GetRealPlannedTimeOnDate(dateOnly)).IgnoreArguments().Return(hour);
			incomingTask.Stub(x => x.GetRealScheduledTimeOnDate(dateOnly)).IgnoreArguments().Return(hour);
			incomingTask.Stub(x => x.GetBacklogOnDate(dateOnly)).IgnoreArguments().Return(hour);
			incomingTask.Stub(x => x.GetManualPlannedInfoOnDate(dateOnly)).IgnoreArguments().Return(false);
			_taskManager.Stub(x => x.GetIncomingTaskFromCampaign(_campaign, _skippedDates)).Return(incomingTask);

			var target = new CampaignVisualizationProvider(_campaignRepository, _taskManager);
			var result = target.ProvideVisualization(id, _skippedDates.ToArray());

			result.Dates.Count.Should().Be.EqualTo(result.PlannedPersonHours.Count);
			result.PlannedPersonHours.Count.Should().Be.EqualTo(result.BacklogPersonHours.Count);
			result.BacklogPersonHours.Count.Should().Be.EqualTo(result.ScheduledPersonHours.Count);
			result.ScheduledPersonHours.Count.Should().Be.EqualTo(result.IsManualPlanned.Count);
			result.IsManualPlanned.Count.Should().Be.EqualTo(result.IsCloseDays.Count);
		}

		[Test]
		public void ShouldGetOverstaffForFirstDay()
		{
			var id = new Guid();
			var date = new DateOnly(2015, 7, 24);

			_campaign.SpanningPeriod = new DateTimePeriod(new DateTime(date.Date.Ticks, DateTimeKind.Utc), new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, DateTimeKind.Utc));

			_campaignRepository.Stub(x => x.Get(id)).Return(_campaign);
			var incomingTask = MockRepository.GenerateMock<IBacklogTask>();
			incomingTask.Stub(x => x.GetRealPlannedTimeOnDate(date)).Return(TimeSpan.FromHours(20));
			incomingTask.Stub(x => x.GetRealScheduledTimeOnDate(date)).Return(TimeSpan.FromHours(0));
			incomingTask.Stub(x => x.GetBacklogOnDate(date)).Return(TimeSpan.FromHours(0));
			incomingTask.Stub(x => x.GetBacklogOnDate(date.AddDays(-1))).Return(TimeSpan.FromHours(10));

			_taskManager.Stub(x => x.GetIncomingTaskFromCampaign(_campaign, _skippedDates)).Return(incomingTask);

			var target = new CampaignVisualizationProvider(_campaignRepository, _taskManager);
			var result = target.ProvideVisualization(id, _skippedDates.ToArray());

			result.OverstaffPersonHours[0].Should().Be.EqualTo(10);
		}
	}
}
