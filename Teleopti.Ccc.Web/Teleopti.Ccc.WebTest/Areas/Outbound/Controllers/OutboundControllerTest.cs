using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Outbound.Controllers;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.WebTest.Areas.Outbound.Controllers
{
	[TestFixture]
	class OutboundControllerTest
	{
		private IOutboundCampaignPersister _outboundCampaignPersister;

		[SetUp]
		public void Setup()
		{
			_outboundCampaignPersister = MockRepository.GenerateMock<IOutboundCampaignPersister>();
		}

		[Test]
		public void ShouldGetCampaignViewModel()
		{
			const string campaignName = "myCampaign";
			var expectedCampaignViewModel = new CampaignViewModel();
			_outboundCampaignPersister.Stub(x => x.Persist(campaignName)).Return(expectedCampaignViewModel);

			var target = new OutboundController(_outboundCampaignPersister);
			var viewModel = target.CreateCampaign(campaignName);

			viewModel.Result.Should().Be.SameInstanceAs(expectedCampaignViewModel);	
		}
	}
}
