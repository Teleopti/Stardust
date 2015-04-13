using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping;
using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core.Campaign.Mapping
{
	[TestFixture]
	internal class OutboundCampaignMapperTests
	{
		private IOutboundCampaignRepository _outboundCampaignRepository;
		private CampaignForm _campaignForm;

		[SetUp]
		public void Setup()
		{
			_outboundCampaignRepository = MockRepository.GenerateMock<IOutboundCampaignRepository>();
			_campaignForm = new CampaignForm(){Id = new Guid()};
		}

		[Test]
		public void ShouldMapNothingWhenNoCampaignFind()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignForm.Id.Value)).IgnoreArguments().Return(null);

			var target = new OutboundCampaignMapper(_outboundCampaignRepository);
			var result = target.Map(_campaignForm);

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldMapName()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignForm.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignForm.Name = "myCampaign";

			var target = new OutboundCampaignMapper(_outboundCampaignRepository);
			var result = target.Map(_campaignForm);

			result.Name.Should().Be.EqualTo(_campaignForm.Name);
		}		
		
		[Test]
		public void ShouldMapCallListLen()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignForm.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignForm.CallListLen = 8;

			var target = new OutboundCampaignMapper(_outboundCampaignRepository);
			var result = target.Map(_campaignForm);

			result.CallListLen.Should().Be.EqualTo(_campaignForm.CallListLen);
		}		
		
		[Test]
		public void ShouldMapTargetRate()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignForm.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignForm.TargetRate = 18;

			var target = new OutboundCampaignMapper(_outboundCampaignRepository);
			var result = target.Map(_campaignForm);

			result.TargetRate.Should().Be.EqualTo(_campaignForm.TargetRate);
		}		
		
		[Test]
		public void ShouldMapConnectRate()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignForm.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignForm.ConnectRate = 28;

			var target = new OutboundCampaignMapper(_outboundCampaignRepository);
			var result = target.Map(_campaignForm);

			result.ConnectRate.Should().Be.EqualTo(_campaignForm.ConnectRate);
		}		
		
		[Test]
		public void ShouldMapRightPartyAverageHandlingTime()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignForm.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignForm.RightPartyAverageHandlingTime = 38;

			var target = new OutboundCampaignMapper(_outboundCampaignRepository);
			var result = target.Map(_campaignForm);

			result.RightPartyAverageHandlingTime.Should().Be.EqualTo(_campaignForm.RightPartyAverageHandlingTime);
		}		
		
		[Test]
		public void ShouldMapConnectAverageHandlingTime()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignForm.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignForm.ConnectAverageHandlingTime = 38;

			var target = new OutboundCampaignMapper(_outboundCampaignRepository);
			var result = target.Map(_campaignForm);

			result.ConnectAverageHandlingTime.Should().Be.EqualTo(_campaignForm.ConnectAverageHandlingTime);
		}		
		
		[Test]
		public void ShouldMapRightPartyConnectRate()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignForm.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignForm.RightPartyConnectRate = 48;

			var target = new OutboundCampaignMapper(_outboundCampaignRepository);
			var result = target.Map(_campaignForm);

			result.RightPartyConnectRate.Should().Be.EqualTo(_campaignForm.RightPartyConnectRate);
		}
		
		[Test]
		public void ShouldMapUnproductiveTime()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignForm.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignForm.UnproductiveTime = 58;

			var target = new OutboundCampaignMapper(_outboundCampaignRepository);
			var result = target.Map(_campaignForm);

			result.UnproductiveTime.Should().Be.EqualTo(_campaignForm.UnproductiveTime);
		}
	}
}
