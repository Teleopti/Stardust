using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core.Campaign.Mapping
{
	[TestFixture]
	internal class OutboundCampaignMapperTests
	{
		private IOutboundCampaignRepository _outboundCampaignRepository;
		private CampaignViewModel _campaignViewModel;

		[SetUp]
		public void Setup()
		{
			_outboundCampaignRepository = MockRepository.GenerateMock<IOutboundCampaignRepository>();
			_campaignViewModel = new CampaignViewModel() { Id = new Guid() };
		}

		[Test]
		public void ShouldMapNothingWhenNoCampaignFind()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignViewModel.Id.Value)).IgnoreArguments().Return(null);

			var target = new OutboundCampaignMapper(_outboundCampaignRepository);
			var result = target.Map(_campaignViewModel);

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldMapName()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignViewModel.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignViewModel.Name = "myCampaign";

			var target = new OutboundCampaignMapper(_outboundCampaignRepository);
			var result = target.Map(_campaignViewModel);

			result.Name.Should().Be.EqualTo(_campaignViewModel.Name);
		}		
		
		[Test]
		public void ShouldMapCallListLen()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignViewModel.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignViewModel.CallListLen = 8;

			var target = new OutboundCampaignMapper(_outboundCampaignRepository);
			var result = target.Map(_campaignViewModel);

			result.CallListLen.Should().Be.EqualTo(_campaignViewModel.CallListLen);
		}		
		
		[Test]
		public void ShouldMapTargetRate()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignViewModel.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignViewModel.TargetRate = 18;

			var target = new OutboundCampaignMapper(_outboundCampaignRepository);
			var result = target.Map(_campaignViewModel);

			result.TargetRate.Should().Be.EqualTo(_campaignViewModel.TargetRate);
		}		
		
		[Test]
		public void ShouldMapConnectRate()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignViewModel.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignViewModel.ConnectRate = 28;

			var target = new OutboundCampaignMapper(_outboundCampaignRepository);
			var result = target.Map(_campaignViewModel);

			result.ConnectRate.Should().Be.EqualTo(_campaignViewModel.ConnectRate);
		}		
		
		[Test]
		public void ShouldMapRightPartyAverageHandlingTime()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignViewModel.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignViewModel.RightPartyAverageHandlingTime = 38;

			var target = new OutboundCampaignMapper(_outboundCampaignRepository);
			var result = target.Map(_campaignViewModel);

			result.RightPartyAverageHandlingTime.Should().Be.EqualTo(_campaignViewModel.RightPartyAverageHandlingTime);
		}		
		
		[Test]
		public void ShouldMapConnectAverageHandlingTime()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignViewModel.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignViewModel.ConnectAverageHandlingTime = 38;

			var target = new OutboundCampaignMapper(_outboundCampaignRepository);
			var result = target.Map(_campaignViewModel);

			result.ConnectAverageHandlingTime.Should().Be.EqualTo(_campaignViewModel.ConnectAverageHandlingTime);
		}		
		
		[Test]
		public void ShouldMapRightPartyConnectRate()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignViewModel.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignViewModel.RightPartyConnectRate = 48;

			var target = new OutboundCampaignMapper(_outboundCampaignRepository);
			var result = target.Map(_campaignViewModel);

			result.RightPartyConnectRate.Should().Be.EqualTo(_campaignViewModel.RightPartyConnectRate);
		}
		
		[Test]
		public void ShouldMapUnproductiveTime()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignViewModel.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignViewModel.UnproductiveTime = 58;

			var target = new OutboundCampaignMapper(_outboundCampaignRepository);
			var result = target.Map(_campaignViewModel);

			result.UnproductiveTime.Should().Be.EqualTo(_campaignViewModel.UnproductiveTime);
		}		
		
		[Test]
		public void ShouldMapStartDate()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignViewModel.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignViewModel.StartDate = new DateOnly(2015, 4, 13);
			_campaignViewModel.EndDate = new DateOnly(2015, 5, 12);

			var target = new OutboundCampaignMapper(_outboundCampaignRepository);
			var result = target.Map(_campaignViewModel);

			result.StartDate.Should().Be.EqualTo(_campaignViewModel.StartDate);
		}		
		
		[Test]
		public void ShouldMapEndDate()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignViewModel.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignViewModel.StartDate = new DateOnly(2015, 4, 13);
			_campaignViewModel.StartDate = new DateOnly(2015, 4, 13);
			_campaignViewModel.EndDate = new DateOnly(2015, 5, 12);

			var target = new OutboundCampaignMapper(_outboundCampaignRepository);
			var result = target.Map(_campaignViewModel);

			result.EndDate.Should().Be.EqualTo(_campaignViewModel.EndDate);
		}
	}
}
