using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core
{
	[TestFixture]
	class CampaignStatisticsProviderTest
	{
		private IOutboundCampaignRepository _campaignRepository;
		private IOutboundScheduledResourcesProvider _scheduledResourcesProvider;

		[SetUp]
		public void Setup()
		{
			_campaignRepository = MockRepository.GenerateMock<IOutboundCampaignRepository>();
			_scheduledResourcesProvider = MockRepository.GenerateMock<IOutboundScheduledResourcesProvider>();
		}

		[Test]
		public void ShouldProvidePlannedCampaign()
		{
            _campaignRepository.Stub(x => x.GetPlannedCampaigns()).Return(new List<IOutboundCampaign>() { new Domain.Outbound.Campaign() });
            _campaignRepository.Stub(x => x.GetOnGoingCampaigns()).Return(new List<IOutboundCampaign>());
            _campaignRepository.Stub(x => x.GetDoneCampaigns()).Return(new List<IOutboundCampaign>());
				_campaignRepository.Stub(x => x.LoadAll()).Return(new List<IOutboundCampaign>());
			_scheduledResourcesProvider.Stub(x=>x.GetScheduledTimeOnDate(new DateOnly(), null)).IgnoreArguments().Return(TimeSpan.Zero);

			var provider = new CampaignStatisticsProvider(_campaignRepository, _scheduledResourcesProvider);
			var result = provider.GetWholeStatistics();

			result.Planned.Should().Be.EqualTo(1);
		}			
		
		[Test]
		public void ShouldGetNoPlannedCampaignWhenItScheduled()
		{
			var campaign = new Domain.Outbound.Campaign();
			campaign.SpanningPeriod = new DateOnlyPeriod(DateOnly.MaxValue, DateOnly.MaxValue);
			_campaignRepository.Stub(x => x.GetPlannedCampaigns()).Return(new List<IOutboundCampaign>() { campaign });
			_campaignRepository.Stub(x => x.LoadAll()).Return(new List<IOutboundCampaign>() { campaign });
			_scheduledResourcesProvider.Stub(x=>x.GetScheduledTimeOnDate(new DateOnly(), null)).IgnoreArguments().Return(TimeSpan.FromHours(1));

			var provider = new CampaignStatisticsProvider(_campaignRepository, _scheduledResourcesProvider);
			var result = provider.GetPlannedCampaigns();

			result.Count.Should().Be.EqualTo(0);
		}		
		
		[Test]
		public void ShouldProvideOnGoingCampaign()
		{
            _campaignRepository.Stub(x => x.GetPlannedCampaigns()).Return(new List<IOutboundCampaign>());
            _campaignRepository.Stub(x => x.GetDoneCampaigns()).Return(new List<IOutboundCampaign>());
            _campaignRepository.Stub(x => x.GetOnGoingCampaigns()).Return(new List<IOutboundCampaign>() { new Domain.Outbound.Campaign() });
				_campaignRepository.Stub(x => x.LoadAll()).Return(new List<IOutboundCampaign>());
			_scheduledResourcesProvider.Stub(x => x.GetScheduledTimeOnDate(new DateOnly(), null)).IgnoreArguments().Return(TimeSpan.Zero);

			var provider = new CampaignStatisticsProvider(_campaignRepository, _scheduledResourcesProvider);
			var result = provider.GetWholeStatistics();

			result.OnGoing.Should().Be.EqualTo(1);
		}		
		
		[Test]
		public void ShouldProvideDoneCampaign()
		{
            _campaignRepository.Stub(x => x.GetPlannedCampaigns()).Return(new List<IOutboundCampaign>());
            _campaignRepository.Stub(x => x.GetDoneCampaigns()).Return(new List<IOutboundCampaign>() { new Domain.Outbound.Campaign() });
            _campaignRepository.Stub(x => x.GetOnGoingCampaigns()).Return(new List<IOutboundCampaign>());
				_campaignRepository.Stub(x => x.LoadAll()).Return(new List<IOutboundCampaign>());
			_scheduledResourcesProvider.Stub(x => x.GetScheduledTimeOnDate(new DateOnly(), null)).IgnoreArguments().Return(TimeSpan.Zero);

			var provider = new CampaignStatisticsProvider(_campaignRepository, _scheduledResourcesProvider);
			var result = provider.GetWholeStatistics();

			result.Done.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldScheduledCampaign()
		{
			var campaign = new Domain.Outbound.Campaign();
			campaign.SpanningPeriod = new DateOnlyPeriod(DateOnly.MaxValue, DateOnly.MaxValue);
			_campaignRepository.Stub(x => x.LoadAll()).Return(new List<IOutboundCampaign>() { campaign });
			_scheduledResourcesProvider.Stub(x => x.GetScheduledTimeOnDate(new DateOnly(), null)).IgnoreArguments().Return(TimeSpan.FromHours(1));

			var provider = new CampaignStatisticsProvider(_campaignRepository, _scheduledResourcesProvider);
			var result = provider.GetScheduledCampaigns();

			result.Count.Should().Be.EqualTo(1);
		}		
		
		[Test]
		public void ShouldNotGetScheduledCampaignWhenItStarted()
		{
			var campaign = new Domain.Outbound.Campaign();
			campaign.SpanningPeriod = new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MaxValue);
			_campaignRepository.Stub(x => x.LoadAll()).Return(new List<IOutboundCampaign>() { campaign });
			_scheduledResourcesProvider.Stub(x => x.GetScheduledTimeOnDate(new DateOnly(), null)).IgnoreArguments().Return(TimeSpan.FromHours(1));

			var provider = new CampaignStatisticsProvider(_campaignRepository, _scheduledResourcesProvider);
			var result = provider.GetScheduledCampaigns();

			result.Count.Should().Be.EqualTo(0);
		}		
		
		[Test]
		public void ShouldNotGetScheduledCampaignWhenItNotScheduled()
		{
			var campaign = new Domain.Outbound.Campaign();
			campaign.SpanningPeriod = new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MaxValue);
			_campaignRepository.Stub(x => x.LoadAll()).Return(new List<IOutboundCampaign>() { campaign });
			_scheduledResourcesProvider.Stub(x => x.GetScheduledTimeOnDate(new DateOnly(), null)).IgnoreArguments().Return(TimeSpan.Zero);

			var provider = new CampaignStatisticsProvider(_campaignRepository, _scheduledResourcesProvider);
			var result = provider.GetScheduledCampaigns();

			result.Count.Should().Be.EqualTo(0);
		}
	}
}
