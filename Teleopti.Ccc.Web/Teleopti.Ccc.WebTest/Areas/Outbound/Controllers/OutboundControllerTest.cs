using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Outbound.Controllers;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping;
using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.WebTest.Areas.Outbound.Controllers
{
	[TestFixture]
	class OutboundControllerTest
	{
		private IOutboundCampaignPersister _outboundCampaignPersister;
		private IOutboundCampaignRepository _outboundCampaignRepository;
		private IOutboundCampaignViewModelMapper _outboundCampaignViewModelMapper;

		[SetUp]
		public void Setup()
		{
			_outboundCampaignPersister = MockRepository.GenerateMock<IOutboundCampaignPersister>();
			_outboundCampaignRepository = MockRepository.GenerateMock<IOutboundCampaignRepository>();
			_outboundCampaignViewModelMapper = MockRepository.GenerateMock<IOutboundCampaignViewModelMapper>();
		}

		[Test]
		public void ShouldGetCampaignViewModel()
		{
			var campaignForm = new CampaignForm() {Name = "myCampaign"};
			var expectedCampaignViewModel = new CampaignViewModel();
			_outboundCampaignPersister.Stub(x => x.Persist(campaignForm.Name)).Return(expectedCampaignViewModel);

			var target = new OutboundController(_outboundCampaignPersister, null, null) {Request = new HttpRequestMessage()};
			var result = target.CreateCampaign(campaignForm);

			var contentResult = (CreatedNegotiatedContentResult<CampaignViewModel>)result;
			contentResult.Content.Should().Be.SameInstanceAs(expectedCampaignViewModel);
		}

		[Test]
		public void ShouldNotCreateCampaignWhenGivenNameIsTooLong()
		{
			var campaignForm = new CampaignForm() { Name = "myCampaign".PadRight(256, 'a') };

			var target = new OutboundController(_outboundCampaignPersister, null, null) { Request = new HttpRequestMessage() };
			var result = target.CreateCampaign(campaignForm);

			result.Should().Be.OfType<BadRequestErrorMessageResult>();
		}

		[Test]
		public void ShouldGetAllCampaigns()
		{
			var skill = SkillFactory.CreateSkill("mySkill");
			var campaigns = new List<Campaign>(){new Campaign("myCampaign", skill)};
			var campaignVMs = new List<CampaignViewModel>(){new CampaignViewModel()};
			_outboundCampaignRepository.Stub(x => x.LoadAll()).Return(campaigns);
			_outboundCampaignViewModelMapper.Stub(x => x.Map(campaigns)).Return(campaignVMs);

			var target = new OutboundController(null, _outboundCampaignRepository, _outboundCampaignViewModelMapper) { Request = new HttpRequestMessage() };
			var result = target.Get();

			result.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetCampaignById()
		{
			var campaign = new Campaign();
			var campaignVM = new CampaignViewModel();
			var campaignId = new Guid();
			
			_outboundCampaignRepository.Stub(x => x.Get(campaignId)).Return(campaign);
			_outboundCampaignViewModelMapper.Stub((x => x.Map(campaign))).Return(campaignVM);
			var target = new OutboundController(null, _outboundCampaignRepository, _outboundCampaignViewModelMapper)
			{
				Request = new HttpRequestMessage()
			};

			var result = target.Get(campaignId);
			result.Should().Be.SameInstanceAs(campaignVM);
		}

		[Test]
		public void ShouldRemoveCampaignById()
		{
			var campaign = new Campaign();
			var campaignId = new Guid();
			_outboundCampaignRepository.Stub(x => x.Get(campaignId)).IgnoreArguments().Return(campaign);
	
			var target = new OutboundController(null, _outboundCampaignRepository, null);
			target.Remove(campaignId);
			_outboundCampaignRepository.AssertWasCalled(x=>x.Remove(campaign), o=>o.IgnoreArguments());			
		}

		[Test]
		public void ShouldUpdateCampaignVM()
		{		
			var campaignVM = new CampaignViewModel();			
			var target = new OutboundController(_outboundCampaignPersister, null, null);
			target.UpdateCampaign(new Guid(), campaignVM);
			_outboundCampaignPersister.AssertWasCalled((x=>x.Persist(campaignVM)));
		}

		[Test]
		public void ShouldUpdateCampaignWorkingPeriodAssignment()
		{
			var form = new CampaignWorkingPeriodAssignmentForm();
			var formId = new Guid();
			
			var target = new OutboundController(_outboundCampaignPersister, null, null);
			target.UpdateCampaignWorkingPeriodAssignment(formId, form);
			_outboundCampaignPersister.AssertWasCalled(x=>x.Persist(form));
		}
	}
}
