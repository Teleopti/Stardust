using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Results;
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
using Teleopti.Interfaces.Domain;

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
			var campaignForm = new CampaignForm() {Name = "myCampaign"};
			var expectedCampaignViewModel = new CampaignViewModel();
			_outboundCampaignPersister.Stub(x => x.Persist(campaignForm)).Return(expectedCampaignViewModel);

			var target = new OutboundController(_outboundCampaignPersister, null, null, null, null, null)
			{
				Request = new HttpRequestMessage()
			};
			var result = target.CreateCampaign(campaignForm);

			var contentResult = (CreatedNegotiatedContentResult<CampaignViewModel>) result;
			contentResult.Content.Should().Be.SameInstanceAs(expectedCampaignViewModel);
		}

		[Test]
		public void ShouldNotCreateCampaignWhenGivenNameIsTooLong()
		{
			var campaignForm = new CampaignForm() {Name = "myCampaign".PadRight(256, 'a')};

			var target = new OutboundController(_outboundCampaignPersister, null, null, null, null, null)
			{
				Request = new HttpRequestMessage()
			};
			var result = target.CreateCampaign(campaignForm);

			result.Should().Be.OfType<BadRequestErrorMessageResult>();
		}

		[Test]
		public void ShouldGetAllCampaigns()
		{
			var skill = SkillFactory.CreateSkill("mySkill");
			var campaigns = new List<IOutboundCampaign>() {new Campaign() {Name = "myCampaign", Skill = skill}};
			var campaignVMs = new List<CampaignViewModel>() {new CampaignViewModel()};
			_outboundCampaignRepository.Stub(x => x.LoadAll()).Return(campaigns);
			_outboundCampaignViewModelMapper.Stub(x => x.Map(campaigns)).Return(campaignVMs);

			var target = new OutboundController(null, _outboundCampaignRepository, _outboundCampaignViewModelMapper, null, null, null)
			{
				Request = new HttpRequestMessage()
			};
			var result = target.Get();

			result.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetAllOutboundActivities()
		{
			var outboundActivityProvider = MockRepository.GenerateMock<IOutboundActivityProvider>();
			outboundActivityProvider.Stub(x => x.GetAll()).Return(new List<ActivityViewModel>() {new ActivityViewModel()});

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
			var provider = MockRepository.GenerateMock<ICampaignStatisticsProvider>();
			provider.Stub(x => x.GetWholeStatistics()).Return(campaignStatistic);
			var target = new OutboundController(_outboundCampaignPersister, null, null, null, provider, null);
			var result = target.GetStatistics();

			result.Should().Be.SameInstanceAs(campaignStatistic);
		}

		[Test]
		public void ShouldGetPlannedCampains()
		{
			var mapper = MockRepository.GenerateMock<IOutboundCampaignListViewModelMapper>();
			var provider = MockRepository.GenerateMock<ICampaignStatisticsProvider>();
			var campaignList = new List<IOutboundCampaign>() { new Campaign()};
			provider.Stub(x => x.GetPlannedCampaigns()).Return(campaignList);
			mapper.Stub(x => x.Map(campaignList, "Planned")).Return(new List<CampaignListViewModel>() { new CampaignListViewModel() });
			var target = new OutboundController(null, null, null, null, provider, mapper);

			var result = target.GetCamapigns(1);

			result.Campaigns.Count.Should().Be.EqualTo(1);
			result.WarningCampaigns.Count.Should().Be.EqualTo(0);
		}		
		
		[Test]
		public void ShouldGetScheduledCampains()
		{
			var mapper = MockRepository.GenerateMock<IOutboundCampaignListViewModelMapper>();
			var provider = MockRepository.GenerateMock<ICampaignStatisticsProvider>();
			var campaignList = new List<IOutboundCampaign>() { new Campaign()};
			provider.Stub(x => x.GetScheduledCampaigns()).Return(campaignList);
			provider.Stub(x => x.GetScheduledWarningCampaigns()).Return(new List<IOutboundCampaign>());
			mapper.Stub(x => x.Map(campaignList, "Scheduled")).Return(new List<CampaignListViewModel>() { new CampaignListViewModel() });
			var target = new OutboundController(null, null, null, null, provider, mapper);

			var result = target.GetCamapigns(2);

			result.Campaigns.Count.Should().Be.EqualTo(1);
			result.WarningCampaigns.Count.Should().Be.EqualTo(0);
		}		
		
		[Test]
		public void ShouldGetScheduledWarningCampains()
		{
			var mapper = MockRepository.GenerateMock<IOutboundCampaignListViewModelMapper>();
			var provider = MockRepository.GenerateMock<ICampaignStatisticsProvider>();
			var campaignList = new List<IOutboundCampaign>() { new Campaign()};
			provider.Stub(x => x.GetScheduledCampaigns()).Return(new List<IOutboundCampaign>());
			provider.Stub(x => x.GetScheduledWarningCampaigns()).Return(campaignList);
			mapper.Stub(x => x.Map(campaignList, "Scheduled")).Return(new List<CampaignListViewModel>() { new CampaignListViewModel() });
			var target = new OutboundController(null, null, null, null, provider, mapper);

			var result = target.GetCamapigns(2);

			result.Campaigns.Count.Should().Be.EqualTo(0);
			result.WarningCampaigns.Count.Should().Be.EqualTo(1);
		}		
		
		[Test]
		public void ShouldGetOnGoingCampains()
		{
			var mapper = MockRepository.GenerateMock<IOutboundCampaignListViewModelMapper>();
			var provider = MockRepository.GenerateMock<ICampaignStatisticsProvider>();
			var campaignList = new List<IOutboundCampaign>() { new Campaign()};
			provider.Stub(x => x.GetOnGoingCampaigns()).Return(campaignList);
			provider.Stub(x => x.GetOnGoingWarningCamapigns()).Return(new List<IOutboundCampaign>());
			mapper.Stub(x => x.Map(campaignList, "OnGoing")).Return(new List<CampaignListViewModel>() { new CampaignListViewModel() });
			var target = new OutboundController(null, null, null, null, provider, mapper);

			var result = target.GetCamapigns(4);

			result.Campaigns.Count.Should().Be.EqualTo(1);
			result.WarningCampaigns.Count.Should().Be.EqualTo(0);
		}			
		
		[Test]
		public void ShouldGetOnGoingWarningCampains()
		{
			var mapper = MockRepository.GenerateMock<IOutboundCampaignListViewModelMapper>();
			var provider = MockRepository.GenerateMock<ICampaignStatisticsProvider>();
			var campaignList = new List<IOutboundCampaign>() { new Campaign()};
			provider.Stub(x => x.GetOnGoingCampaigns()).Return(new List<IOutboundCampaign>());
			provider.Stub(x => x.GetOnGoingWarningCamapigns()).Return(campaignList);
			mapper.Stub(x => x.Map(campaignList, "OnGoing")).Return(new List<CampaignListViewModel>() { new CampaignListViewModel() });
			var target = new OutboundController(null, null, null, null, provider, mapper);

			var result = target.GetCamapigns(4);

			result.Campaigns.Count.Should().Be.EqualTo(0);
			result.WarningCampaigns.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetDoneCampains()
		{
			var mapper = MockRepository.GenerateMock<IOutboundCampaignListViewModelMapper>();
			var provider = MockRepository.GenerateMock<ICampaignStatisticsProvider>();
			var campaignList = new List<IOutboundCampaign>() { new Campaign() };
			provider.Stub(x => x.GetDoneCampaigns()).Return(campaignList);
			mapper.Stub(x => x.Map(campaignList, "Done")).Return(new List<CampaignListViewModel>() { new CampaignListViewModel() });
			var target = new OutboundController(null, null, null, null, provider, mapper);

			var result = target.GetCamapigns(8);

			result.Campaigns.Count.Should().Be.EqualTo(1);
			result.WarningCampaigns.Count.Should().Be.EqualTo(0);
		}			
		
		[Test]
		public void ShouldGetPlannedAndDoneCampainsTogether()
		{
			var mapper = MockRepository.GenerateMock<IOutboundCampaignListViewModelMapper>();
			var provider = MockRepository.GenerateMock<ICampaignStatisticsProvider>();
			var campaignList = new List<IOutboundCampaign>() { new Campaign() };
			provider.Stub(x => x.GetPlannedCampaigns()).Return(campaignList);
			provider.Stub(x => x.GetDoneCampaigns()).Return(campaignList);
			mapper.Stub(x => x.Map(campaignList, "Planned")).Return(new List<CampaignListViewModel>() { new CampaignListViewModel() });
			mapper.Stub(x => x.Map(campaignList, "Done")).Return(new List<CampaignListViewModel>() { new CampaignListViewModel() });
			var target = new OutboundController(null, null, null, null, provider, mapper);

			var result = target.GetCamapigns(9);

			result.Campaigns.Count.Should().Be.EqualTo(2);
			result.WarningCampaigns.Count.Should().Be.EqualTo(0);
		}		
		
		[Test]
		public void ShouldGetAllCampainsWithSequence()
		{
			var mapper = new OutboundCampaignListViewModelMapper();
			var provider = MockRepository.GenerateMock<ICampaignStatisticsProvider>();
			var plannedCampaignList = new List<IOutboundCampaign>() { new Campaign() {Name = "planned"}};
			var scheduledCampaignList = new List<IOutboundCampaign>() { new Campaign() { Name = "scheduled" } };
			var scheduledWarningCampaignList = new List<IOutboundCampaign>() { new Campaign() { Name = "scheduledWarning" } };
			var onGoingCampaignList = new List<IOutboundCampaign>() { new Campaign() { Name = "onGoing" } };
			var onGoingWarningCampaignList = new List<IOutboundCampaign>() { new Campaign() { Name = "onGoingWarning" } };
			var doneCampaignList = new List<IOutboundCampaign>() { new Campaign() { Name = "done" } };
			provider.Stub(x => x.GetPlannedCampaigns()).Return(plannedCampaignList);
			provider.Stub(x => x.GetScheduledCampaigns()).Return(scheduledCampaignList);
			provider.Stub(x => x.GetScheduledWarningCampaigns()).Return(scheduledWarningCampaignList);
			provider.Stub(x => x.GetOnGoingCampaigns()).Return(onGoingCampaignList);
			provider.Stub(x => x.GetOnGoingWarningCamapigns()).Return(onGoingWarningCampaignList);
			provider.Stub(x => x.GetDoneCampaigns()).Return(doneCampaignList);
			var target = new OutboundController(null, null, null, null, provider, mapper);

			var result = target.GetCamapigns(15);

			result.WarningCampaigns[0].Name.Should().Be.EqualTo("onGoingWarning");
			result.WarningCampaigns[1].Name.Should().Be.EqualTo("scheduledWarning");
			result.Campaigns[0].Name.Should().Be.EqualTo("onGoing");
			result.Campaigns[1].Name.Should().Be.EqualTo("planned");
			result.Campaigns[2].Name.Should().Be.EqualTo("scheduled");
			result.Campaigns[3].Name.Should().Be.EqualTo("done");
		}

		[Test]
		public void ShouldGetAllCampainsWhenFlagIsZero()
		{
			var mapper = new OutboundCampaignListViewModelMapper();
			var provider = MockRepository.GenerateMock<ICampaignStatisticsProvider>();
			var plannedCampaignList = new List<IOutboundCampaign>() { new Campaign() { Name = "planned" } };
			var scheduledCampaignList = new List<IOutboundCampaign>() { new Campaign() { Name = "scheduled" } };
			var scheduledWarningCampaignList = new List<IOutboundCampaign>() { new Campaign() { Name = "scheduledWarning" } };
			var onGoingCampaignList = new List<IOutboundCampaign>() { new Campaign() { Name = "onGoing" } };
			var onGoingWarningCampaignList = new List<IOutboundCampaign>() { new Campaign() { Name = "onGoingWarning" } };
			var doneCampaignList = new List<IOutboundCampaign>() { new Campaign() { Name = "done" } };
			provider.Stub(x => x.GetPlannedCampaigns()).Return(plannedCampaignList);
			provider.Stub(x => x.GetScheduledCampaigns()).Return(scheduledCampaignList);
			provider.Stub(x => x.GetScheduledWarningCampaigns()).Return(scheduledWarningCampaignList);
			provider.Stub(x => x.GetOnGoingCampaigns()).Return(onGoingCampaignList);
			provider.Stub(x => x.GetOnGoingWarningCamapigns()).Return(onGoingWarningCampaignList);
			provider.Stub(x => x.GetDoneCampaigns()).Return(doneCampaignList);
			var target = new OutboundController(null, null, null, null, provider, mapper);

			var result = target.GetCamapigns(0);

			result.WarningCampaigns[0].Name.Should().Be.EqualTo("onGoingWarning");
			result.WarningCampaigns[1].Name.Should().Be.EqualTo("scheduledWarning");
			result.Campaigns[0].Name.Should().Be.EqualTo("onGoing");
			result.Campaigns[1].Name.Should().Be.EqualTo("planned");
			result.Campaigns[2].Name.Should().Be.EqualTo("scheduled");
			result.Campaigns[3].Name.Should().Be.EqualTo("done");
		}
	}
}
