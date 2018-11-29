using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.SystemSetting.OutboundSetting;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Rules;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Ccc.WebTest.Core.Common.DataProvider;


namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core
{
	[TestFixture]
	class CampaignListProviderTest
	{
		private CampaignListProvider target;

		private FakeCampaignRepository _outboundCampaignRepository;
		private FakeScheduleResourceProvider _scheduledResourcesProvider;
		private FakeCampaignWarningProvider _campaignWarningProvider;
		private FakeOutboundScheduledResourcesCacher _outboundScheduledResourcesCacher;

		private IOutboundCampaign doneCampaign;
		private IOutboundCampaign plannedCampaign;
		private IOutboundCampaign scheduledCampaign;
		private IOutboundCampaign ongoingCampaign;

		private readonly TimeZoneInfo timeZone = TimeZoneInfoFactory.HelsinkiTimeZoneInfo();

		[SetUp]
		public void SetUp()
		{
			_outboundCampaignRepository = new FakeCampaignRepository();
			_scheduledResourcesProvider = new FakeScheduleResourceProvider();
			_campaignWarningProvider = new FakeCampaignWarningProvider();
			_outboundScheduledResourcesCacher = new FakeOutboundScheduledResourcesCacher();

			_scheduledResourcesProvider.SetScheduledTimeOnDate(DateOnly.Today.AddDays(14), CreateSkill("B"),
				new TimeSpan(4, 0, 0));

			doneCampaign = GetTestCampaign(3);
			plannedCampaign = GetTestCampaign(0);
			scheduledCampaign = GetTestCampaign(1);
			ongoingCampaign = GetTestCampaign(2);

			target = new CampaignListProvider(_outboundCampaignRepository, _scheduledResourcesProvider, _campaignWarningProvider, _outboundScheduledResourcesCacher);
		}

		[Test]
		public void ShouldGetCampaigns()
		{
			var campaign1 = createCampaign("campaign1", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1));
			_outboundCampaignRepository.Add(campaign1);
			var campaign2 = createCampaign("campaign2", new DateOnlyPeriod(2015, 11, 1, 2015, 12, 1));
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
			target = new CampaignListProvider(_outboundCampaignRepository, scheduledResourcesProvider, null, _outboundScheduledResourcesCacher);

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
			var campaign = createCampaign("campaign", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1));
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

			var campaign = createCampaign("campaign", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1));
			_outboundCampaignRepository.Add(campaign);
			_scheduledResourcesProvider.SetForecastedTimeOnDate(new DateOnly(2015, 10, 15), campaign.Skill, new TimeSpan(8, 0, 0));
			target.LoadData(null);
			var forecastedTime = _outboundScheduledResourcesCacher.GetForecastedTime(campaign);
			forecastedTime.Should().Not.Be.Null();
			forecastedTime.Keys.Should().Contain(new DateOnly(2015, 10, 15));
			forecastedTime[new DateOnly(2015, 10, 15)].Should().Be.EqualTo(new TimeSpan(8, 0, 0));
		}
		
		[Test]
		public void ShouldGetAllCampaignsStatus()
		{
			var campaign1 = createCampaign("campaign1", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1));
			_outboundCampaignRepository.Add(campaign1);
			var campaign2 = createCampaign("campaign2", new DateOnlyPeriod(2015, 11, 1, 2015, 12, 1));
			_outboundCampaignRepository.Add(campaign2);

			var period = new GanttPeriod
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 12, 31)
			};

			var result = target.GetCampaignsStatus(period);
			result.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldGetCampaignId()
		{
			var campaign = createCampaign("campaign", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1));
			_outboundCampaignRepository.Add(campaign);
		
			var period = new GanttPeriod
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 12, 31)
			};

			var result = target.GetCampaignsStatus(period);
			result.First().CampaignSummary.Id.Should().Be.EqualTo(campaign.Id);
		}		
		
		[Test]
		public void ShouldGetCampaignName()
		{
			var campaign = createCampaign("campaign", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1));
			_outboundCampaignRepository.Add(campaign);

			var period = new GanttPeriod
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 12, 31)
			};

			var result = target.GetCampaignsStatus(period);
			result.First().CampaignSummary.Name.Should().Be.EqualTo("campaign");
		}		
		
		[Test]
		public void ShouldGetCampaignStartDate()
		{
			var campaign = createCampaign("campaign", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1));
			_outboundCampaignRepository.Add(campaign);

			var period = new GanttPeriod
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 12, 31)
			};

			var result = target.GetCampaignsStatus(period);
			result.First().CampaignSummary.StartDate.Should().Be.EqualTo(new DateOnly(2015, 10, 1));		
		}		
		
		[Test]
		public void ShouldGetCampaignEndDate()
		{
			var campaign = createCampaign("campaign", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1));
			_outboundCampaignRepository.Add(campaign);

			var period = new GanttPeriod
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 12, 31)
			};

			var result = target.GetCampaignsStatus(period);
			result.First().CampaignSummary.EndDate.Should().Be.EqualTo(new DateOnly(2015, 11, 1));				
		}

		[Test]
		public void ShouldGetScheduledInfoTrue()
		{
			var campaign = createCampaign("campaign", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1));
			_outboundCampaignRepository.Add(campaign);

			_scheduledResourcesProvider.SetScheduledTimeOnDate(new DateOnly(2015, 10, 15), campaign.Skill, new TimeSpan(8, 0, 0) );

			var period = new GanttPeriod
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 12, 31)
			};

			var result = target.GetCampaignsStatus(period);				
			result.First().IsScheduled.Should().Be.True();
		}		
		
		[Test]
		public void ShouldGetScheduledInfoFalse()
		{
			var campaign = createCampaign("campaign", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1));
			_outboundCampaignRepository.Add(campaign);
			
			var period = new GanttPeriod
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 12, 31)
			};

			var result = target.GetCampaignsStatus(period);		
			result.First().IsScheduled.Should().Be.False();
		}		
		
		[Test]
		public void ShouldGetWarningInfo()
		{
			
			var campaign = createCampaign("campaign", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1));
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

			var result = target.GetCampaignsStatus(period).ToList();
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
			var campaign1 = createCampaign("campaign1", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1));
			var forcast = new Dictionary<DateOnly, TimeSpan> {{new DateOnly(2015,10,26), TimeSpan.Zero}};
			_outboundScheduledResourcesCacher.SetForecastedTime(campaign1, forcast);

			target.ResetCache();
			_outboundScheduledResourcesCacher.GetForecastedTime(campaign1).Should().Be.Null();
		}

		[Test]
		public void ShouldLoadDataWhenThereIsNewCampaign()
		{
			var campaign1 = createCampaign("campaign1", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1));
			_outboundCampaignRepository.Add(campaign1);
			var campaign2 = createCampaign("campaign2", new DateOnlyPeriod(2015, 11, 1, 2015, 12, 1));
			_outboundCampaignRepository.Add(campaign2);
			var forcast = new Dictionary<DateOnly, TimeSpan> { { new DateOnly(2015,10,26), TimeSpan.Zero } };
			_outboundScheduledResourcesCacher.SetForecastedTime(campaign1, forcast);
			var scheduledResourcesProvider = MockRepository.GenerateMock<IOutboundScheduledResourcesProvider>();
			target = new CampaignListProvider(_outboundCampaignRepository, scheduledResourcesProvider, _campaignWarningProvider, _outboundScheduledResourcesCacher);

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
			var campaign1 = createCampaign("campaign1", new DateOnlyPeriod(2015, 10, 1, 2015, 11, 1));
			_outboundCampaignRepository.Add(campaign1);
			var forcast = new Dictionary<DateOnly, TimeSpan> { { new DateOnly(2015,10,26), TimeSpan.Zero } };
			_outboundScheduledResourcesCacher.SetForecastedTime(campaign1, forcast);
			var scheduledResourcesProvider = MockRepository.GenerateMock<IOutboundScheduledResourcesProvider>();
			target = new CampaignListProvider(_outboundCampaignRepository, scheduledResourcesProvider, _campaignWarningProvider, _outboundScheduledResourcesCacher);

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
				createCampaign("A", new DateOnlyPeriod(today.AddDays(7), today.AddDays(14)) ),
				createCampaign("B", new DateOnlyPeriod(today.AddDays(7), today.AddDays(21)) ),
				createCampaign("C", new DateOnlyPeriod(today.AddDays(-7), today.AddDays(7)) ),
				createCampaign("D", new DateOnlyPeriod(today.AddDays(-14), today.AddDays(-7)) ),				
			};

			return campaigns[index];
		}

		private IOutboundCampaign createCampaign(string name, DateOnlyPeriod period)
		{
			var campaign = new Domain.Outbound.Campaign
			{
				Name = name,
				SpanningPeriod = period.ToDateTimePeriod(timeZone),
				BelongsToPeriod = period,
				Skill = SkillFactory.CreateSkill(name, timeZone)
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

		private DateOnlyPeriod getCampaignPeriod(IEnumerable<IOutboundCampaign> campaigns)
		{
			var periods = campaigns.Select(c => c.SpanningPeriod.ToDateOnlyPeriod(TimeZoneInfo.Utc)).ToArray();
			var earliestStart = periods.Min(c => c.StartDate);
			var latestEnd = periods.Max(c => c.EndDate);
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
