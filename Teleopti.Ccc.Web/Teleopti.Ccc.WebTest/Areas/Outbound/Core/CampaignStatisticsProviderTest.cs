using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;

namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core
{
	[TestFixture]
	class CampaignStatisticsProviderTest
	{
		private IOutboundCampaignRepository _campaignRepository;

		[SetUp]
		public void Setup()
		{
			_campaignRepository = MockRepository.GenerateMock<IOutboundCampaignRepository>();
		}

		[Test]
		public void ShouldProvidePlannedCampaign()
		{
			_campaignRepository.Stub(x => x.GetPlannedCampaigns()).Return(new List<Domain.Outbound.Campaign>(){new Domain.Outbound.Campaign()});
			_campaignRepository.Stub(x => x.GetOnGoingCampaigns()).Return(new List<Domain.Outbound.Campaign>());
			_campaignRepository.Stub(x => x.GetDoneCampaigns()).Return(new List<Domain.Outbound.Campaign>());
			var provider = new CampaignStatisticsProvider(_campaignRepository);

			var result = provider.GetWholeStatistics();

			result.Planned.Should().Be.EqualTo(1);
		}		
		
		[Test]
		public void ShouldProvideOnGoingCampaign()
		{
			_campaignRepository.Stub(x => x.GetPlannedCampaigns()).Return(new List<Domain.Outbound.Campaign>());
			_campaignRepository.Stub(x => x.GetDoneCampaigns()).Return(new List<Domain.Outbound.Campaign>());
			_campaignRepository.Stub(x => x.GetOnGoingCampaigns()).Return(new List<Domain.Outbound.Campaign>(){new Domain.Outbound.Campaign()});
			var provider = new CampaignStatisticsProvider(_campaignRepository);

			var result = provider.GetWholeStatistics();

			result.OnGoing.Should().Be.EqualTo(1);
		}		
		
		[Test]
		public void ShouldProvideDoneCampaign()
		{
			_campaignRepository.Stub(x => x.GetPlannedCampaigns()).Return(new List<Domain.Outbound.Campaign>());
			_campaignRepository.Stub(x => x.GetDoneCampaigns()).Return(new List<Domain.Outbound.Campaign>() { new Domain.Outbound.Campaign() });
			_campaignRepository.Stub(x => x.GetOnGoingCampaigns()).Return(new List<Domain.Outbound.Campaign>());
			var provider = new CampaignStatisticsProvider(_campaignRepository);

			var result = provider.GetWholeStatistics();

			result.Done.Should().Be.EqualTo(1);
		}
	}
}
