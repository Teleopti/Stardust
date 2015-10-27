using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.SystemSetting.OutboundSetting;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Rules;
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
		private FakeCampaignWarningProvider _campaignWarningProvider;
		private FakeOutboundCampaignListOrderProvider _campaignListOrderProvider;
		private FakeOutboundScheduledResourcesCacher _outboundScheduledResourcesCacher;
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
			_campaignWarningProvider = new FakeCampaignWarningProvider();
			_outboundScheduledResourcesCacher = new FakeOutboundScheduledResourcesCacher();

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

			target = new CampaignListProvider(_outboundCampaignRepository, _scheduledResourcesProvider, _campaignWarningProvider, _campaignListOrderProvider, _userTimeZone, _outboundScheduledResourcesCacher);
		}

		[Test]
		public void ShouldGetCampaigns()
		{
			var campaign1 = createCampaign("campaign1", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1), _userTimeZone);
			_outboundCampaignRepository.Add(campaign1);
			var campaign2 = createCampaign("campaign2", new DateOnlyPeriod(2015, 11, 1, 2015, 12, 1), _userTimeZone);
			_outboundCampaignRepository.Add(campaign2);

			var period = new GanttPeriod
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 12, 31)
			};

			var result = target.GetCampaigns(period).ToList();
			result.Count.Should().Be.EqualTo(2);		
		}

		[Test]
		public void ShouldLoadDataWithAllCampaigns()
		{
			setCampaigns();
			var scheduledResourcesProvider = MockRepository.GenerateMock<IOutboundScheduledResourcesProvider>();
			target = new CampaignListProvider(_outboundCampaignRepository, scheduledResourcesProvider, null, null, _userTimeZone, _outboundScheduledResourcesCacher);

			target.LoadData(null);

			var campaigns = _outboundCampaignRepository.LoadAll();
			var period = getCampaignPeriod(campaigns);

			scheduledResourcesProvider.AssertWasCalled(x => x.Load(
				Arg<IList<IOutboundCampaign>>.List.ContainsAll(campaigns),
				Arg<DateOnlyPeriod>.Matches( p => p.Contains(period))));								
		}

		[Test]
		public void ShouldUpdateCacheScheduleAfterLoadingData()
		{
			var campaign = createCampaign("campaign", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1), _userTimeZone);
			_outboundCampaignRepository.Add(campaign);
			_scheduledResourcesProvider.SetScheduledTimeOnDate(new DateOnly(2015, 10, 15), campaign.Skill, new TimeSpan(8, 0, 0) );
			target.LoadData(null);
			var scheduledTime = _outboundScheduledResourcesCacher.GetScheduledTime(campaign);
			scheduledTime.Should().Not.Be.Null();
			scheduledTime.Keys.Should().Contain(new DateOnly(2015, 10, 15));
			scheduledTime[new DateOnly(2015, 10, 15)].Should().Be.EqualTo(new TimeSpan(8, 0, 0));
		}

		[Test]
		public void ShouldUpdateCacheForecastAfterLoadingData()
		{

			var campaign = createCampaign("campaign", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1), _userTimeZone);
			_outboundCampaignRepository.Add(campaign);
			_scheduledResourcesProvider.SetForecastedTimeOnDate(new DateOnly(2015, 10, 15), campaign.Skill, new TimeSpan(8, 0, 0));
			target.LoadData(null);
			var forecastedTime = _outboundScheduledResourcesCacher.GetForecastedTime(campaign);
			forecastedTime.Should().Not.Be.Null();
			forecastedTime.Keys.Should().Contain(new DateOnly(2015, 10, 15));
			forecastedTime[new DateOnly(2015, 10, 15)].Should().Be.EqualTo(new TimeSpan(8, 0, 0));
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
			_campaignWarningProvider.SetCampaignRuleCheckResponse(ongoingCampaign, new List<CampaignWarning>
			{
				new CampaignWarning()
				{
					TypeOfRule = typeof (CampaignOverstaffRule)
				}
			});

			var result = target.ListCampaign(CampaignStatus.Ongoing, null).ToList();
			result.ForEach(c =>
			{
				var warnings = c.WarningInfo.ToList();
				warnings.Should().Not.Be.Empty();
				warnings.ForEach(w =>
				{
					w.TypeOfRule.Should().Be.EqualTo(typeof(CampaignOverstaffRule));
				});
			});
		}

		[Test]
		public void ShouldAttachCorrectRuleCheckResultToScheduledCampaigns()
		{
			setCampaigns();
			_campaignWarningProvider.SetCampaignRuleCheckResponse(scheduledCampaign, new List<CampaignWarning>
			{
				new CampaignWarning()
				{
					TypeOfRule = typeof (CampaignOverstaffRule)
				}
			});

			var result = target.ListCampaign(CampaignStatus.Scheduled, null).ToList();
			result.ForEach(c =>
			{
				var warnings = c.WarningInfo.ToList();
				warnings.Should().Not.Be.Empty();
				warnings.ForEach(w =>
				{
					w.TypeOfRule.Should().Be.EqualTo(typeof (CampaignOverstaffRule));
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

			_campaignWarningProvider.SetCampaignRuleCheckResponse(ongoingCampaign, new List<CampaignWarning>
			{
				new CampaignWarning()
				{
					TypeOfRule = typeof (CampaignOverstaffRule)
				}
			});

			_campaignWarningProvider.SetCampaignRuleCheckResponse(scheduledCampaign, new List<CampaignWarning>
			{
				new CampaignWarning()
				{
					TypeOfRule = typeof (CampaignOverstaffRule)
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
		
		[Test]
		public void ShouldGetAllCampaignsSummaryWithinPeriod()
		{
			var campaign1 = createCampaign("campaign1", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1), _userTimeZone);
			_outboundCampaignRepository.Add(campaign1);
			var campaign2 = createCampaign("campaign2", new DateOnlyPeriod(2015, 11, 1, 2015, 12, 1), _userTimeZone);
			_outboundCampaignRepository.Add(campaign2);

			var period = new GanttPeriod
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 12, 31)
			};

			var result = target.GetPeriodCampaignsSummary(period);
			result.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldGetCampaignId()
		{
			var campaign = createCampaign("campaign", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1), _userTimeZone);
			_outboundCampaignRepository.Add(campaign);
		
			var period = new GanttPeriod
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 12, 31)
			};

			var result = target.GetPeriodCampaignsSummary(period);
			result.First().Id.Should().Be.EqualTo(campaign.Id);
		}		
		
		[Test]
		public void ShouldGetCampaignName()
		{
			var campaign = createCampaign("campaign", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1), _userTimeZone);
			_outboundCampaignRepository.Add(campaign);

			var period = new GanttPeriod
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 12, 31)
			};

			var result = target.GetPeriodCampaignsSummary(period);
			result.First().Name.Should().Be.EqualTo("campaign");
		}		
		
		[Test]
		public void ShouldGetCampaignStartDate()
		{
			var campaign = createCampaign("campaign", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1), _userTimeZone);
			_outboundCampaignRepository.Add(campaign);

			var period = new GanttPeriod
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 12, 31)
			};

			var result = target.GetPeriodCampaignsSummary(period);
			result.First().StartDate.Should().Be.EqualTo(new DateOnly(2015, 10, 1));		
		}		
		
		[Test]
		public void ShouldGetCampaignEndDate()
		{
			var campaign = createCampaign("campaign", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1), _userTimeZone);
			_outboundCampaignRepository.Add(campaign);

			var period = new GanttPeriod
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 12, 31)
			};

			var result = target.GetPeriodCampaignsSummary(period);
			result.First().EndDate.Should().Be.EqualTo(new DateOnly(2015, 11, 1));				
		}

		[Test]
		public void ShouldGetScheduledInfoTrue()
		{
			var campaign = createCampaign("campaign", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1), _userTimeZone);
			_outboundCampaignRepository.Add(campaign);

			_scheduledResourcesProvider.SetScheduledTimeOnDate(new DateOnly(2015, 10, 15), campaign.Skill, new TimeSpan(8, 0, 0) );

			var period = new GanttPeriod
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 12, 31)
			};

			var result = target.GetPeriodCampaignsSummary(period);				
			result.First().IsScheduled.Should().Be.True();
		}		
		
		[Test]
		public void ShouldGetScheduledInfoFalse()
		{
			var campaign = createCampaign("campaign", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1), _userTimeZone);
			_outboundCampaignRepository.Add(campaign);
			
			var period = new GanttPeriod
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 12, 31)
			};

			var result = target.GetPeriodCampaignsSummary(period);		
			result.First().IsScheduled.Should().Be.False();
		}		
		
		[Test]
		public void ShouldGetWarningInfo()
		{
			
			var campaign = createCampaign("campaign", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1), _userTimeZone);
			_outboundCampaignRepository.Add(campaign);

			_campaignWarningProvider.SetCampaignRuleCheckResponse(campaign, new List<CampaignWarning>()
			{
				new CampaignWarning()
				{
					TypeOfRule = typeof(CampaignOverstaffRule),
					Threshold = 20,
					WarningThresholdType = WarningThresholdType.Absolute,
					TargetValue = 10,
					WarningName = "CampaignOverstaff"
				}
			});

			var period = new GanttPeriod
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 12, 31)
			};

			var result = target.GetPeriodCampaignsSummary(period).ToList();
			result.First().WarningInfo.ToList().Count.Should().Be.EqualTo(1);

			var warning = result.First().WarningInfo.First();

			warning.TypeOfRule.Should().Be.EqualTo("CampaignOverstaff");
			warning.Threshold.Should().Be.EqualTo(20);
			warning.TargetValue.Should().Be.EqualTo(10);
			warning.ThresholdType.Should().Be.EqualTo(WarningThresholdType.Absolute);
			
		}

		[Test]
		public void ShouldResetCache()
		{
			var campaign1 = createCampaign("campaign1", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1), _userTimeZone);
			var forcast = new Dictionary<DateOnly, TimeSpan> {{new DateOnly(2015,10,26), TimeSpan.Zero}};
			_outboundScheduledResourcesCacher.SetForecastedTime(campaign1, forcast);

			target.ResetCache();
			_outboundScheduledResourcesCacher.GetForecastedTime(campaign1).Should().Be.Null();
		}

		[Test]
		public void ShouldLoadDataWhenThereIsNewCampaign()
		{
			var campaign1 = createCampaign("campaign1", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1), _userTimeZone);
			_outboundCampaignRepository.Add(campaign1);
			var campaign2 = createCampaign("campaign2", new DateOnlyPeriod(2015, 11, 1, 2015, 12, 1), _userTimeZone);
			_outboundCampaignRepository.Add(campaign2);
			var forcast = new Dictionary<DateOnly, TimeSpan> { { new DateOnly(2015,10,26), TimeSpan.Zero } };
			_outboundScheduledResourcesCacher.SetForecastedTime(campaign1, forcast);
			var scheduledResourcesProvider = MockRepository.GenerateMock<IOutboundScheduledResourcesProvider>();
			target = new CampaignListProvider(_outboundCampaignRepository, scheduledResourcesProvider, _campaignWarningProvider, _campaignListOrderProvider, _userTimeZone, _outboundScheduledResourcesCacher);

			target.CheckAndUpdateCache(new GanttPeriod()
			{
				StartDate = new DateOnly(2015,10,1),
				EndDate = new DateOnly(2015,12,1)
			});

			scheduledResourcesProvider.AssertWasCalled(x => x.Load(null, new DateOnlyPeriod()), y => y.IgnoreArguments());
		}		
		
		[Test]
		public void ShouldNotLoadDataWhenThereIsNewCampaign()
		{
			var campaign1 = createCampaign("campaign1", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1), _userTimeZone);
			_outboundCampaignRepository.Add(campaign1);
			var forcast = new Dictionary<DateOnly, TimeSpan> { { new DateOnly(2015,10,26), TimeSpan.Zero } };
			_outboundScheduledResourcesCacher.SetForecastedTime(campaign1, forcast);
			var scheduledResourcesProvider = MockRepository.GenerateMock<IOutboundScheduledResourcesProvider>();
			target = new CampaignListProvider(_outboundCampaignRepository, scheduledResourcesProvider, _campaignWarningProvider, _campaignListOrderProvider, _userTimeZone, _outboundScheduledResourcesCacher);

			target.CheckAndUpdateCache(new GanttPeriod()
			{
				StartDate = new DateOnly(2015,10,1),
				EndDate = new DateOnly(2015,11,1)
			});

			scheduledResourcesProvider.AssertWasNotCalled(x => x.Load(null, new DateOnlyPeriod()), y => y.IgnoreArguments());
		}

		public IOutboundCampaign GetTestCampaign(int index)
		{	
			var today = DateOnly.Today;	
			var campaigns = new[]
			{
				createCampaign("A", new DateOnlyPeriod(today.AddDays(7), today.AddDays(14)), _userTimeZone ),
				createCampaign("B", new DateOnlyPeriod(today.AddDays(7), today.AddDays(21)), _userTimeZone ),
				createCampaign("C", new DateOnlyPeriod(today.AddDays(-7), today.AddDays(7)), _userTimeZone ),
				createCampaign("D", new DateOnlyPeriod(today.AddDays(-14), today.AddDays(-7)), _userTimeZone ),				
			};

			return campaigns[index];
		}

		private IOutboundCampaign createCampaign(string name, DateOnlyPeriod period, IUserTimeZone userTimeZone)
		{
			var campaign = new Domain.Outbound.Campaign()
			{
				Name = name,
				SpanningPeriod = period.ToDateTimePeriod(userTimeZone.TimeZone()),
				Skill = SkillFactory.CreateSkill(name, userTimeZone.TimeZone())
			};

			campaign.SetId(Guid.NewGuid());
			return campaign;
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
