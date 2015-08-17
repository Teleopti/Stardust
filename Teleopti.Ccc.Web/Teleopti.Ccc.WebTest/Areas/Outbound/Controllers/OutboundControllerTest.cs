﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Results;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Outbound.Controllers;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.WebTest.Areas.Outbound.Controllers
{
    [TestFixture]
    internal class OutboundControllerTest
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
            var campaignForm = new CampaignForm() { Name = "myCampaign" };
            var expectedCampaignViewModel = new CampaignViewModel();
            _outboundCampaignPersister.Stub(x => x.Persist(campaignForm)).Return(expectedCampaignViewModel);

            var target = new OutboundController(_outboundCampaignPersister, null, null, null, null, null)
            {
                Request = new HttpRequestMessage()
            };
            var result = target.CreateCampaign(campaignForm);

            var contentResult = (CreatedNegotiatedContentResult<CampaignViewModel>)result;
            contentResult.Content.Should().Be.SameInstanceAs(expectedCampaignViewModel);
        }

        [Test]
        [Ignore("To be consistent with verification of other fileds to rely on Front-end verification. Server-side verification is TO-DO ")]
        public void ShouldNotCreateCampaignWhenGivenNameIsTooLong()
        {
            var campaignForm = new CampaignForm() { Name = "myCampaign".PadRight(256, 'a') };

            var target = new OutboundController(_outboundCampaignPersister, null, null, null, null, null)
            {
                Request = new HttpRequestMessage()
            };
            var result = target.CreateCampaign(campaignForm);

            result.Should().Be.OfType<BadRequestErrorMessageResult>();
        }

        [Test]
        public void ShouldGetAllOutboundActivities()
        {
            var outboundActivityProvider = MockRepository.GenerateMock<IOutboundActivityProvider>();
            outboundActivityProvider.Stub(x => x.GetAll()).Return(new List<ActivityViewModel>() { new ActivityViewModel() });

            var target = new OutboundController(null, null, null, outboundActivityProvider, null, null)
            {
                Request = new HttpRequestMessage()
            };
            var result = target.GetActivities();

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
            var target = new OutboundController(null, _outboundCampaignRepository, _outboundCampaignViewModelMapper, null, null, null)
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

            var target = new OutboundController(null, _outboundCampaignRepository, null, null, null, null);
            target.Remove(campaignId);
            _outboundCampaignRepository.AssertWasCalled(x => x.Remove(campaign), o => o.IgnoreArguments());
        }

        [Test]
        public void ShouldUpdateCampaignVM()
        {
            var campaignVM = new CampaignViewModel();
            var target = new OutboundController(_outboundCampaignPersister, null, null, null, null, null);
            target.UpdateCampaign(new Guid(), campaignVM);
            _outboundCampaignPersister.AssertWasCalled((x => x.Persist(campaignVM)));
        }

        [Test]
        public void ShouldGetCampaignStatistics()
        {
            var campaignStatistic = new CampaignStatistics();
            var factory = MockRepository.GenerateMock<ICampaignSummaryViewModelFactory>();
            factory.Stub(x => x.GetCampaignStatistics()).Return(campaignStatistic);
            var target = new OutboundController(_outboundCampaignPersister, null, null, null, factory, null);
            var result = target.GetStatistics();

            result.Should().Be.SameInstanceAs(campaignStatistic);
        }

        [Test]
        public void ShouldGetCampainSummaryList()
        {
            var factory = MockRepository.GenerateMock<ICampaignSummaryViewModelFactory>();
           
            var target = new OutboundController(null, null, null, null, factory, null);
            target.GetCamapigns(CampaignStatus.Done);

            factory.AssertWasCalled(x => x.GetCampaignSummaryList(CampaignStatus.Done));
        }

	    [Test]
	    public void ShouldGetCampaignVisualization()
	    {
		    var expectedVisualizationVM = new CampaignVisualizationViewModel();
		    var id = new Guid();
		    var visualizationProvider = MockRepository.GenerateMock<ICampaignVisualizationProvider>();
			 visualizationProvider.Stub(x => x.ProvideVisualization(id)).Return(expectedVisualizationVM);
			 
			 var target = new OutboundController(null, null, null, null, null, visualizationProvider);
			 var result = target.GetVisualization(id);

		    result.Should().Be.SameInstanceAs(expectedVisualizationVM);
	    }

		 [Test]
		 public void ShouldGetCampaignVisualizationWhenManuPlan()
		 {
			 var expectedVisualizationVM = new CampaignVisualizationViewModel();
			 var id = new Guid();
			 var visualizationProvider = MockRepository.GenerateMock<ICampaignVisualizationProvider>();
			 visualizationProvider.Stub(x => x.ProvideVisualization(id)).Return(expectedVisualizationVM);
			 var manualPlanVM = new ManualPlanForm() { CampaignId = id };
			 var campaignPersister = MockRepository.GenerateMock<IOutboundCampaignPersister>();

			 var target = new OutboundController(campaignPersister, null, null, null, null, visualizationProvider);
			 var result = target.ManualPlan(manualPlanVM);

			 campaignPersister.AssertWasCalled(x => x.PersistManualProductionPlan(manualPlanVM));
			 result.Should().Be.SameInstanceAs(expectedVisualizationVM);
		 }
    }
}
