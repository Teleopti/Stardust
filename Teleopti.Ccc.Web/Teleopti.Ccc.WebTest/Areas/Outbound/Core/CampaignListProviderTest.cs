using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Outbound.Rules;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Ccc.WebTest.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core
{
	[TestFixture]
	class CampaignListProviderTest
	{
		private CampaignListProvider target;

		private FakeCampaignRepository _outboundCampaignRepository;
		private FakeScheduleResourceProvider _scheduledResourcesProvider;
		private FakeOutboundRuleChecker _outboundRuleChecker;
		private FakeOutboundCampaignListOrderProvider _campaignListOrderProvider;
		private IUserTimeZone _userTimeZone;

		private IOutboundCampaign doneCampaign;
		private IOutboundCampaign plannedCampaign;
		private IOutboundCampaign scheduledCampaign;
		private IOutboundCampaign ongoingCampaign;

		[SetUp]
		public void SetUp()
		{
			_userTimeZone = new FakeUserTimeZone(TimeZoneInfo.CreateCustomTimeZone("tzid", TimeSpan.FromHours(2), "", ""));
			_outboundCampaignRepository = new FakeCampaignRepository();
			_campaignListOrderProvider = new FakeOutboundCampaignListOrderProvider();
			_scheduledResourcesProvider = new FakeScheduleResourceProvider();
			_outboundRuleChecker = new FakeOutboundRuleChecker();

			_scheduledResourcesProvider.SetScheduledTimeOnDate(DateOnly.Today.AddDays(14), CreateSkill("B"),
				new TimeSpan(4, 0, 0));

			doneCampaign = GetTestCampaign(3);
			plannedCampaign = GetTestCampaign(0);
			scheduledCampaign = GetTestCampaign(1);
			ongoingCampaign = GetTestCampaign(2);

			_campaignListOrderProvider.SetCampaignListOrder(new List<CampaignStatus>
			{
				CampaignStatus.Ongoing,
				CampaignStatus.Done,
				CampaignStatus.Planned,
				CampaignStatus.Scheduled
			});

			target = new CampaignListProvider(_outboundCampaignRepository, _scheduledResourcesProvider, _outboundRuleChecker, _campaignListOrderProvider, _userTimeZone);
		}

		[Test]
		public void ShouldGetCampaigns()
		{
			var campaign1 = new Domain.Outbound.Campaign()
			{
				Name = "campaign1",
				SpanningPeriod = new DateTimePeriod(new DateTime(2015, 9, 9, 22, 0, 0, DateTimeKind.Utc),
															   new DateTime(2015, 9, 19, 21, 59, 59, DateTimeKind.Utc))
			};
			campaign1.SetId(Guid.NewGuid());

			_outboundCampaignRepository.Add(campaign1);

			var result = target.GetCampaigns(new GanttPeriod() {StartDate = new DateOnly(2015, 9, 10), EndDate = new DateOnly(2015, 9, 19)});

			result.ToList()[0].Name.Should().Be.EqualTo(campaign1.Name);
			result.ToList()[0].Id.Should().Be.EqualTo(campaign1.Id);
			result.ToList()[0].StartDate.Should().Be.EqualTo(new DateOnly(2015, 9, 10));
			result.ToList()[0].EndDate.Should().Be.EqualTo(new DateOnly(2015, 9, 19));
		}

		[Test]
		public void ShouldLoadDataWithAllCampaigns()
		{
			setCampaigns();
			var scheduledResourcesProvider = MockRepository.GenerateMock<IOutboundScheduledResourcesProvider>();
			target = new CampaignListProvider(_outboundCampaignRepository, scheduledResourcesProvider, null, null, _userTimeZone);

			target.LoadData(null);

			var campaigns = _outboundCampaignRepository.LoadAll();
			scheduledResourcesProvider.AssertWasCalled(x => x.Load(campaigns, getCampaignPeriod(campaigns)));
		}

		[Test]
		public void ShouldListDoneCampaigns()
		{
			setCampaigns();
			var result = target.ListDoneCampaign(null).ToList();
			result.Count.Should().Be.EqualTo(1);
			result.ForEach(c =>
			{
				c.Name.Should().Be.EqualTo("D");
				c.Status.Should().Be.EqualTo(CampaignStatus.Done);
			});
		}

		[Test]
		public void ShouldListOngoingCampaigns()
		{
			setCampaigns();
			var result = target.ListOngoingCampaign(null).ToList();
			result.Count.Should().Be.EqualTo(1);
			result.ForEach(c =>
			{
				c.Name.Should().Be.EqualTo("C");
				c.Status.Should().Be.EqualTo(CampaignStatus.Ongoing);
			});
		}

		[Test]
		public void ShouldListScheduledCampaigns()
		{
			setCampaigns();
			var result = target.ListScheduledCampaign(null).ToList();
			result.Count.Should().Be.EqualTo(1);
			result.ForEach(c =>
			{
				c.Name.Should().Be.EqualTo("B");
				c.Status.Should().Be.EqualTo(CampaignStatus.Scheduled);
			});
		}

		[Test]
		public void ShouldListPlannedCampaigns()
		{
			setCampaigns();
			var result = target.ListPlannedCampaign(null).ToList();
			result.Count.Should().Be.EqualTo(1);
			result.ForEach(c =>
			{
				c.Name.Should().Be.EqualTo("A");
				c.Status.Should().Be.EqualTo(CampaignStatus.Planned);
			});
		}

		[Test]
		public void ScheduledCampaignShouldNotBeListedInPlannedCampaigns()
		{
			setCampaigns();
			var result = target.ListPlannedCampaign(null).ToList();
			result.ForEach(c => c.Name.Should().Not.Be.EqualTo("B"));
		}

		[Test]
		public void ShouldListCampaignsByStatusOngoing()
		{
			setCampaigns();
			var result = target.ListCampaign(CampaignStatus.Ongoing, null).ToList();
			result.Count.Should().Be.EqualTo(1);
			result.ForEach(c => c.Name.Should().Be.EqualTo("C"));
		}

		[Test]
		public void ShouldListCampaignsByStatusDone()
		{
			setCampaigns();
			var result = target.ListCampaign(CampaignStatus.Done, null).ToList();
			result.Count.Should().Be.EqualTo(1);
			result.ForEach(c => c.Name.Should().Be.EqualTo("D"));
		}

		[Test]
		public void ShouldListCampaignsByStatusScheduled()
		{
			setCampaigns();
			var result = target.ListCampaign(CampaignStatus.Scheduled, null).ToList();
			result.Count.Should().Be.EqualTo(1);
			result.ForEach(c => c.Name.Should().Be.EqualTo("B"));
		}

		[Test]
		public void ShouldListCampaignsByStatusPlanned()
		{
			setCampaigns();
			var result = target.ListCampaign(CampaignStatus.Planned, null).ToList();
			result.Count.Should().Be.EqualTo(1);
			result.ForEach(c => c.Name.Should().Be.EqualTo("A"));
		}

		[Test]
		public void ShouldListAllCampaignsByStatusNone()
		{
			setCampaigns();
			var result = target.ListCampaign(CampaignStatus.None, null).ToList();
			result.Count.Should().Be.EqualTo(4);
		}

		[Test]
		public void ShouldListCampaignsByCompositeStatus()
		{
			setCampaigns();
			var result = target.ListCampaign(CampaignStatus.Done | CampaignStatus.Scheduled, null).ToList();

			result.Count.Should().Be.EqualTo(2);
			var expectedNames = new List<string> {"B", "D"};
			result.ForEach(c => expectedNames.Should().Contain(c.Name));
		}

		[Test]
		public void ShouldListAllCampaignsByAllStatus()
		{
			setCampaigns();
			var result = target.ListCampaign(
				CampaignStatus.Done | CampaignStatus.Scheduled | CampaignStatus.Planned | CampaignStatus.Ongoing, null).ToList();

			result.Count.Should().Be.EqualTo(4);
		}

		[Test]
		public void ShouldListCampaignsWithGivenOrder()
		{
			setCampaigns();
			_campaignListOrderProvider.SetCampaignListOrder(new List<CampaignStatus>
			{
				CampaignStatus.Ongoing,
				CampaignStatus.Planned,
				CampaignStatus.Scheduled,
				CampaignStatus.Done,
			});

			var result = target.ListCampaign(CampaignStatus.None, null);
			result.Select(c => c.Name).Should().Have.SameSequenceAs(new List<string> {"C", "A", "B", "D"});
		}


		[Test]
		public void ShouldAttachCorrectRuleCheckResultToOngoingCampaigns()
		{
			setCampaigns();
			_outboundRuleChecker.SetCampaignRuleCheckResponse(ongoingCampaign, new List<OutboundRuleResponse>
			{
				new OutboundRuleResponse()
				{
					TypeOfRule = typeof (OutboundOverstaffRule)
				}
			});

			var result = target.ListCampaign(CampaignStatus.Ongoing, null).ToList();
			result.ForEach(c =>
			{
				var warnings = c.WarningInfo.ToList();
				warnings.Should().Not.Be.Empty();
				warnings.ForEach(w =>
				{
					w.TypeOfRule.Should().Be.EqualTo(typeof (OutboundOverstaffRule));
				});
			});
		}

		[Test]
		public void ShouldAttachCorrectRuleCheckResultToScheduledCampaigns()
		{
			setCampaigns();
			_outboundRuleChecker.SetCampaignRuleCheckResponse(scheduledCampaign, new List<OutboundRuleResponse>
			{
				new OutboundRuleResponse()
				{
					TypeOfRule = typeof (OutboundOverstaffRule)
				}
			});

			var result = target.ListCampaign(CampaignStatus.Scheduled, null).ToList();
			result.ForEach(c =>
			{
				var warnings = c.WarningInfo.ToList();
				warnings.Should().Not.Be.Empty();
				warnings.ForEach(w =>
				{
					w.TypeOfRule.Should().Be.EqualTo(typeof (OutboundOverstaffRule));
				});
			});
		}

		[Test]
		public void ShouldReturnCorrectStatisticsWhenThereAreNoWarnings()
		{
			setCampaigns();
			var result = target.GetCampaignStatistics(null);

			result.Planned.Should().Be.EqualTo(1);
			result.Scheduled.Should().Be.EqualTo(1);
			result.OnGoing.Should().Be.EqualTo(1);
			result.Done.Should().Be.EqualTo(1);
			result.ScheduledWarning.Should().Be.EqualTo(0);
			result.OnGoingWarning.Should().Be.EqualTo(0);
		}		

		[Test]
		public void ShouldReturnCorrectStatisticsWhenThereAreWarnings()
		{
			setCampaigns();

			_outboundRuleChecker.SetCampaignRuleCheckResponse(ongoingCampaign, new List<OutboundRuleResponse>
			{
				new OutboundRuleResponse()
				{
					TypeOfRule = typeof (OutboundOverstaffRule)
				}
			});

			_outboundRuleChecker.SetCampaignRuleCheckResponse(scheduledCampaign, new List<OutboundRuleResponse>
			{
				new OutboundRuleResponse()
				{
					TypeOfRule = typeof (OutboundOverstaffRule)
				}
			});

			var result = target.GetCampaignStatistics(null);

			result.Planned.Should().Be.EqualTo(1);
			result.Scheduled.Should().Be.EqualTo(1);
			result.OnGoing.Should().Be.EqualTo(1);
			result.Done.Should().Be.EqualTo(1);
			result.ScheduledWarning.Should().Be.EqualTo(1);
			result.OnGoingWarning.Should().Be.EqualTo(1);
		}

		private IOutboundCampaign createCampaign(string name, DateTimePeriod period)
		{
			var campaign = new Domain.Outbound.Campaign()
			{
				Name = name,
				SpanningPeriod = period,
				Skill = SkillFactory.CreateSkill("mySkill")
			};

			campaign.SetId(Guid.NewGuid());
			return campaign;
		}

		[Test]
		public void ShouldGetAllCampaignsSummaryWithinPeriod()
		{
			var period = new GanttPeriod();
			var campaigns = new List<IOutboundCampaign>() {createCampaign("campaign1", new DateTimePeriod()), createCampaign("campaign2", new DateTimePeriod())};
			var repository = MockRepository.GenerateMock<IOutboundCampaignRepository>();
			repository.Stub(x => x.GetCampaigns(getUtcPeroid(period))).IgnoreArguments().Return(campaigns);
			var ruleChecker = MockRepository.GenerateMock<IOutboundRuleChecker>();
			ruleChecker.Stub(x => x.CheckCampaign(campaigns[0])).IgnoreArguments();
			_userTimeZone = new FakeUserTimeZone(TimeZoneInfo.CreateCustomTimeZone("tzid", TimeSpan.FromHours(0), "", ""));

			var _target = new CampaignListProvider(repository, null, ruleChecker, null, _userTimeZone);
			var result = _target.GetPeriodCampaignsSummary(period);

			result.Count().Should().Be.EqualTo(2);
		}		
		
		[Test]
		public void ShouldGetCampaignId()
		{
			var period = new GanttPeriod();
			var campaigns = new List<IOutboundCampaign>() { createCampaign("myCampaign", new DateTimePeriod(new DateTime(DateTime.Today.Ticks, DateTimeKind.Utc), new DateTime(DateTime.Today.AddDays(10).Ticks, DateTimeKind.Utc)))};
			var repository = MockRepository.GenerateMock<IOutboundCampaignRepository>();
			repository.Stub(x => x.GetCampaigns(getUtcPeroid(period))).IgnoreArguments().Return(campaigns);
			repository.Stub(x => x.Get((Guid) campaigns[0].Id)).Return(campaigns[0]);
			var _scheduledResourceProvider = MockRepository.GenerateMock<IOutboundScheduledResourcesProvider>();
			_scheduledResourceProvider.Stub(x=>x.GetScheduledTimeOnDate(DateOnly.Today, campaigns[0].Skill)).Return(TimeSpan.FromHours(8));
			var ruleChecker = MockRepository.GenerateMock<IOutboundRuleChecker>();
			ruleChecker.Stub(x => x.CheckCampaign(campaigns[0]));
			_userTimeZone = new FakeUserTimeZone(TimeZoneInfo.CreateCustomTimeZone("tzid", TimeSpan.FromHours(0), "", ""));

			var _target = new CampaignListProvider(repository, _scheduledResourceProvider, ruleChecker, null, _userTimeZone);
			var result = _target.GetPeriodCampaignsSummary(period);

			result.ToList()[0].Id.Should().Be.EqualTo(campaigns[0].Id);
		}		
		
		[Test]
		public void ShouldGetCampaignName()
		{
			var name = "campaignName";
			var period = new GanttPeriod();
			var campaigns = new List<IOutboundCampaign>() { createCampaign(name, new DateTimePeriod(new DateTime(DateTime.Today.Ticks, DateTimeKind.Utc), new DateTime(DateTime.Today.AddDays(10).Ticks, DateTimeKind.Utc))) };
			var repository = MockRepository.GenerateMock<IOutboundCampaignRepository>();
			repository.Stub(x => x.GetCampaigns(getUtcPeroid(period))).IgnoreArguments().Return(campaigns);
			repository.Stub(x => x.Get((Guid)campaigns[0].Id)).Return(campaigns[0]);
			var _scheduledResourceProvider = MockRepository.GenerateMock<IOutboundScheduledResourcesProvider>();
			_scheduledResourceProvider.Stub(x => x.GetScheduledTimeOnDate(DateOnly.Today, campaigns[0].Skill)).Return(TimeSpan.FromHours(8));
			var ruleChecker = MockRepository.GenerateMock<IOutboundRuleChecker>();
			ruleChecker.Stub(x => x.CheckCampaign(campaigns[0]));
			_userTimeZone = new FakeUserTimeZone(TimeZoneInfo.CreateCustomTimeZone("tzid", TimeSpan.FromHours(0), "", ""));

			var _target = new CampaignListProvider(repository, _scheduledResourceProvider, ruleChecker, null, _userTimeZone);
			var result = _target.GetPeriodCampaignsSummary(period);

			result.ToList()[0].Name.Should().Be.EqualTo(name);
		}		
		
		[Test]
		public void ShouldGetCampaignStartDate()
		{
			var period = new GanttPeriod();
			var campaigns = new List<IOutboundCampaign>() { createCampaign("myCampaign", new DateTimePeriod(new DateTime(DateTime.Today.Ticks, DateTimeKind.Utc), new DateTime(DateTime.Today.AddDays(10).Ticks, DateTimeKind.Utc))) };
			var repository = MockRepository.GenerateMock<IOutboundCampaignRepository>();
			repository.Stub(x => x.GetCampaigns(getUtcPeroid(period))).IgnoreArguments().Return(campaigns);
			repository.Stub(x => x.Get((Guid)campaigns[0].Id)).Return(campaigns[0]);
			var _scheduledResourceProvider = MockRepository.GenerateMock<IOutboundScheduledResourcesProvider>();
			_scheduledResourceProvider.Stub(x => x.GetScheduledTimeOnDate(DateOnly.Today, campaigns[0].Skill)).Return(TimeSpan.FromHours(8));
			var ruleChecker = MockRepository.GenerateMock<IOutboundRuleChecker>();
			ruleChecker.Stub(x => x.CheckCampaign(campaigns[0]));
			_userTimeZone = new FakeUserTimeZone(TimeZoneInfo.CreateCustomTimeZone("tzid", TimeSpan.FromHours(0), "", ""));

			var _target = new CampaignListProvider(repository, _scheduledResourceProvider, ruleChecker, null, _userTimeZone);
			var result = _target.GetPeriodCampaignsSummary(period);

			result.ToList()[0].StartDate.Should().Be.EqualTo(campaigns[0].SpanningPeriod.ToDateOnlyPeriod(TimeZoneInfo.Utc).StartDate);
		}		
		
		[Test]
		public void ShouldGetCampaignEndDate()
		{
			var period = new GanttPeriod();
			var campaigns = new List<IOutboundCampaign>() { createCampaign("myCampaign", new DateTimePeriod(new DateTime(DateTime.Today.Ticks, DateTimeKind.Utc), new DateTime(DateTime.Today.AddDays(10).Ticks, DateTimeKind.Utc))) };
			var repository = MockRepository.GenerateMock<IOutboundCampaignRepository>();
			repository.Stub(x => x.GetCampaigns(getUtcPeroid(period))).IgnoreArguments().Return(campaigns);
			repository.Stub(x => x.Get((Guid)campaigns[0].Id)).Return(campaigns[0]);
			var _scheduledResourceProvider = MockRepository.GenerateMock<IOutboundScheduledResourcesProvider>();
			_scheduledResourceProvider.Stub(x => x.GetScheduledTimeOnDate(DateOnly.Today, campaigns[0].Skill)).Return(TimeSpan.FromHours(8));
			var ruleChecker = MockRepository.GenerateMock<IOutboundRuleChecker>();
			ruleChecker.Stub(x => x.CheckCampaign(campaigns[0]));
			_userTimeZone = new FakeUserTimeZone(TimeZoneInfo.CreateCustomTimeZone("tzid", TimeSpan.FromHours(0), "", ""));

			var _target = new CampaignListProvider(repository, _scheduledResourceProvider, ruleChecker, null, _userTimeZone);
			var result = _target.GetPeriodCampaignsSummary(period);

			result.ToList()[0].EndDate.Should().Be.EqualTo(campaigns[0].SpanningPeriod.ToDateOnlyPeriod(campaigns[0].Skill.TimeZone).EndDate);
		}

		[Test]
		public void ShouldGetScheduledInfoTrue()
		{
			var period = new GanttPeriod();
			var campaigns = new List<IOutboundCampaign>() { createCampaign("myCampaign", new DateTimePeriod(new DateTime(DateTime.Today.Ticks, DateTimeKind.Utc), new DateTime(DateTime.Today.AddDays(10).Ticks, DateTimeKind.Utc))) };
			var repository = MockRepository.GenerateMock<IOutboundCampaignRepository>();
			repository.Stub(x => x.GetCampaigns(getUtcPeroid(period))).IgnoreArguments().Return(campaigns);
			repository.Stub(x => x.Get((Guid)campaigns[0].Id)).Return(campaigns[0]);
			var _scheduledResourceProvider = MockRepository.GenerateMock<IOutboundScheduledResourcesProvider>();
			_scheduledResourceProvider.Stub(x => x.GetScheduledTimeOnDate(DateOnly.Today, campaigns[0].Skill)).Return(TimeSpan.FromHours(8));
			var ruleChecker = MockRepository.GenerateMock<IOutboundRuleChecker>();
			ruleChecker.Stub(x => x.CheckCampaign(campaigns[0]));
			_userTimeZone = new FakeUserTimeZone(TimeZoneInfo.CreateCustomTimeZone("tzid", TimeSpan.FromHours(0), "", ""));

			var _target = new CampaignListProvider(repository, _scheduledResourceProvider, ruleChecker, null, _userTimeZone);
			var result = _target.GetPeriodCampaignsSummary(period);

			result.ToList()[0].IsScheduled.Should().Be.True();
		}		
		
		[Test]
		public void ShouldGetScheduledInfoFalse()
		{
			var period = new GanttPeriod();
			var campaigns = new List<IOutboundCampaign>() { createCampaign("myCampaign", new DateTimePeriod(new DateTime(DateTime.Today.Ticks, DateTimeKind.Utc), new DateTime(DateTime.Today.AddDays(10).Ticks, DateTimeKind.Utc))) };
			var repository = MockRepository.GenerateMock<IOutboundCampaignRepository>();
			repository.Stub(x => x.GetCampaigns(getUtcPeroid(period))).IgnoreArguments().Return(campaigns);
			repository.Stub(x => x.Get((Guid)campaigns[0].Id)).Return(campaigns[0]);
			var _scheduledResourceProvider = MockRepository.GenerateMock<IOutboundScheduledResourcesProvider>();
			_scheduledResourceProvider.Stub(x => x.GetScheduledTimeOnDate(DateOnly.Today, campaigns[0].Skill)).Return(TimeSpan.Zero);
			var ruleChecker = MockRepository.GenerateMock<IOutboundRuleChecker>();
			ruleChecker.Stub(x => x.CheckCampaign(campaigns[0]));
			_userTimeZone = new FakeUserTimeZone(TimeZoneInfo.CreateCustomTimeZone("tzid", TimeSpan.FromHours(0), "", ""));

			var _target = new CampaignListProvider(repository, _scheduledResourceProvider, ruleChecker, null, _userTimeZone);
			var result = _target.GetPeriodCampaignsSummary(period);

			result.ToList()[0].IsScheduled.Should().Be.False();
		}		
		
		[Test]
		public void ShouldGetWarningInfo()
		{
			var period = new GanttPeriod();
			var campaigns = new List<IOutboundCampaign>() { createCampaign("myCampaign", new DateTimePeriod(new DateTime(DateTime.Today.Ticks, DateTimeKind.Utc), new DateTime(DateTime.Today.AddDays(10).Ticks, DateTimeKind.Utc))) };
			var repository = MockRepository.GenerateMock<IOutboundCampaignRepository>();
			repository.Stub(x => x.GetCampaigns(getUtcPeroid(period))).IgnoreArguments().Return(campaigns);
			repository.Stub(x => x.Get((Guid)campaigns[0].Id)).Return(campaigns[0]);
			var _scheduledResourceProvider = MockRepository.GenerateMock<IOutboundScheduledResourcesProvider>();
			_scheduledResourceProvider.Stub(x => x.GetScheduledTimeOnDate(DateOnly.Today, campaigns[0].Skill)).Return(TimeSpan.Zero);
			var ruleChecker = MockRepository.GenerateMock<IOutboundRuleChecker>();
			ruleChecker.Stub(x => x.CheckCampaign(campaigns[0])).Return(new List<OutboundRuleResponse>()
			{
				new OutboundRuleResponse()
				{
					TypeOfRule = typeof(OutboundOverstaffRule),
					Threshold = 20,
					ThresholdType = ThresholdType.Absolute,
					TargetValue = 10,
					Date = DateTime.Today
				}
			});
			_userTimeZone = new FakeUserTimeZone(TimeZoneInfo.CreateCustomTimeZone("tzid", TimeSpan.FromHours(0), "", ""));

			var _target = new CampaignListProvider(repository, _scheduledResourceProvider, ruleChecker, null, _userTimeZone);
			var result = _target.GetPeriodCampaignsSummary(period);

			result.ToList()[0].WarningInfo.ToList()[0].TypeOfRule.Should().Be.EqualTo("OutboundOverstaffRule");
			result.ToList()[0].WarningInfo.ToList()[0].Threshold.Should().Be.EqualTo(20);
			result.ToList()[0].WarningInfo.ToList()[0].TargetValue.Should().Be.EqualTo(10);
			result.ToList()[0].WarningInfo.ToList()[0].ThresholdType.Should().Be.EqualTo(ThresholdType.Absolute);
			result.ToList()[0].WarningInfo.ToList()[0].Date.Should().Be.EqualTo(DateOnly.Today);
		}

		public IOutboundCampaign GetTestCampaign(int index)
		{
			var today = new DateTime(DateOnly.Today.Year, DateOnly.Today.Month, DateOnly.Today.Day, 0, 0, 0, DateTimeKind.Utc);
			var dayAfterOneWeek = today.AddDays(7);
			var dayAfterTwoWeeks = today.AddDays(14);
			var dayAfterThreeWeeks = today.AddDays(21);
			var dayBeforeOneWeek = today.AddDays(-7);
			var dayBeforeTwoWeeks = today.AddDays(-14);

			var campaigns = new IOutboundCampaign[4]
			{
				new Domain.Outbound.Campaign()
				{
					Name = "A",
					SpanningPeriod = new DateTimePeriod(dayAfterOneWeek.Date, dayAfterTwoWeeks.Date),
					Skill = SkillFactory.CreateSkill("A", SkillTypeFactory.CreateSkillType(), 15, TimeZoneInfo.Utc, TimeSpan.Zero),
				},
				new Domain.Outbound.Campaign()
				{
					Name = "B",
					SpanningPeriod = new DateTimePeriod(dayAfterOneWeek.Date, dayAfterThreeWeeks.Date),
					Skill = SkillFactory.CreateSkill("B", SkillTypeFactory.CreateSkillType(), 15, TimeZoneInfo.Utc, TimeSpan.Zero)
				},
				new Domain.Outbound.Campaign()
				{
					Name = "C",
					SpanningPeriod = new DateTimePeriod(dayBeforeOneWeek.Date, dayAfterOneWeek.Date),
					Skill = SkillFactory.CreateSkill("C", SkillTypeFactory.CreateSkillType(), 15, TimeZoneInfo.Utc, TimeSpan.Zero)
				},
				new Domain.Outbound.Campaign()
				{
					Name = "D",
					SpanningPeriod = new DateTimePeriod(dayBeforeTwoWeeks.Date, dayBeforeOneWeek.Date),
					Skill = SkillFactory.CreateSkill("D", SkillTypeFactory.CreateSkillType(), 15, TimeZoneInfo.Utc, TimeSpan.Zero)
				}
			};

			return campaigns[index];
		}

		public ISkill CreateSkill(string name)
		{
			return new Skill(name, "", Color.Blue, 60, new FakeSkillTypeProvider().Outbound())
			{
				TimeZone = new HawaiiTimeZone().TimeZone()
			};
		}

		private DateOnlyPeriod getCampaignPeriod(IList<IOutboundCampaign> campaigns)
		{
			var earliestStart = campaigns.Min(c => c.SpanningPeriod.ToDateOnlyPeriod(TimeZoneInfo.Utc).StartDate);
			var latestEnd = campaigns.Max(c => c.SpanningPeriod.ToDateOnlyPeriod(TimeZoneInfo.Utc).EndDate);
			var campaignPeriod = new DateOnlyPeriod(earliestStart, latestEnd);

			return campaignPeriod;
		}

		private void setCampaigns()
		{
			_outboundCampaignRepository.Add(doneCampaign);
			_outboundCampaignRepository.Add(plannedCampaign);
			_outboundCampaignRepository.Add(scheduledCampaign);
			_outboundCampaignRepository.Add(ongoingCampaign);
		}

		private DateTimePeriod getUtcPeroid(GanttPeriod period)
		{
			var start = new DateTime(period.StartDate.Date.Ticks);
			start = TimeZoneHelper.ConvertToUtc(start, _userTimeZone.TimeZone());
			var end = new DateTime(period.EndDate.Year, period.EndDate.Month, period.EndDate.Day, 23, 59, 59);
			end = TimeZoneHelper.ConvertToUtc(end, _userTimeZone.TimeZone());

			return new DateTimePeriod(start, end);
		}
	}

	public class FakeSkillTypeProvider : ISkillTypeProvider
	{
		public ISkillType Outbound()
		{
			var desc = new Description("SkillTypeOutbound");
			return new SkillTypeEmail(desc, ForecastSource.OutboundTelephony);
		}

		public ISkillType InboundTelephony()
		{
			throw new NotImplementedException();
		}
	}
}
