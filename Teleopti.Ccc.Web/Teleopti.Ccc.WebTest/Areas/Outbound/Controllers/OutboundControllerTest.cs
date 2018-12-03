using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Results;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.SystemSetting.OutboundSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.Controllers;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping;
using Teleopti.Ccc.Web.Areas.Outbound.Models;

using Teleopti.Ccc.Web.Core.Data;

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

			var target = new OutboundController(_outboundCampaignPersister, null, null, null, null, null, null)
			{
				Request = new HttpRequestMessage()
			};
			var result = target.CreateCampaign(campaignForm);

			var contentResult = (CreatedNegotiatedContentResult<CampaignViewModel>) result;
			contentResult.Content.Should().Be.SameInstanceAs(expectedCampaignViewModel);
		}

		[Test]
		public void ShouldGetAllOutboundActivities()
		{
			var outboundActivityProvider = MockRepository.GenerateMock<IActivityProvider>();
			outboundActivityProvider.Stub(x => x.GetAllRequireSkill()).Return(new List<ActivityViewModel>() {new ActivityViewModel()});

			var target = new OutboundController(null, null, null, outboundActivityProvider, null, null, null)
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
			var target = new OutboundController(null, _outboundCampaignRepository, _outboundCampaignViewModelMapper, null, null,
				null, null)
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
			campaign.SetId(new Guid());
			_outboundCampaignRepository.Stub(x => x.Get(campaign.Id.Value)).Return(campaign);

			var target = new OutboundController(_outboundCampaignPersister, _outboundCampaignRepository, null, null, null, null, null);
			target.Remove(campaign.Id.Value);

			_outboundCampaignPersister.AssertWasCalled(x => x.RemoveCampaign(campaign));
		}

		[Test]
		public void ShouldUpdateCampaignVM()
		{
			var campaignVM = new CampaignViewModel();
			var target = new OutboundController(_outboundCampaignPersister, null, null, null, null, null, null);
			target.UpdateCampaign(new Guid(), campaignVM);
			_outboundCampaignPersister.AssertWasCalled((x => x.Persist(campaignVM)));
		}
		
		[Test]
		public void ShouldGetCampainStatusList()
		{
			var period = new GanttPeriod();
			var listProvider = MockRepository.GenerateMock<ICampaignListProvider>();

			var target = new OutboundController(null, null, null, null, null, listProvider, null);
			target.GetCamapignsStatus(period);

			listProvider.AssertWasCalled(x => x.GetCampaignsStatus(period));
		}

		[Test]
		public void ShouldGetCampaignVisualization()
		{
			var expectedVisualizationVM = new CampaignVisualizationViewModel();
			var visualizationForm = new VisualizationForm(){CampaignId = new Guid(), SkipDates = new List<DateOnly>()};
			var visualizationProvider = MockRepository.GenerateMock<ICampaignVisualizationProvider>();
			visualizationProvider.Stub(x => x.ProvideVisualization(visualizationForm.CampaignId, visualizationForm.SkipDates.ToArray())).Return(expectedVisualizationVM);

			var target = new OutboundController(null, null, null, null, visualizationProvider, null, null);
			var result = target.GetVisualization(visualizationForm);

			result.Should().Be.SameInstanceAs(expectedVisualizationVM);
		}

		[Test]
		public void ShouldGetCampaignVisualizationWhenManuPlan()
		{
			var expectedVisualizationVM = new CampaignVisualizationViewModel();
			var skipDates = new List<DateOnly>();
			var id = new Guid();
			var visualizationProvider = MockRepository.GenerateMock<ICampaignVisualizationProvider>();
			visualizationProvider.Stub(x => x.ProvideVisualization(id, skipDates.ToArray())).Return(expectedVisualizationVM);
			var manualPlanVM = new ManualPlanForm() {CampaignId = id, SkipDates = skipDates};
			var campaignPersister = MockRepository.GenerateMock<IOutboundCampaignPersister>();

			var target = new OutboundController(campaignPersister, null, null, null, visualizationProvider, null, null);
			var result = target.ManualPlan(manualPlanVM);

			campaignPersister.AssertWasCalled(x => x.PersistManualProductionPlan(manualPlanVM));
			result.Should().Be.SameInstanceAs(expectedVisualizationVM);
		}

		[Test]
		public void ShouldGetCampaignVisualizationWhenRemoveManuPlan()
		{
			var expectedVisualizationVM = new CampaignVisualizationViewModel();
			var skipDates = new List<DateOnly>();
			var id = new Guid();
			var visualizationProvider = MockRepository.GenerateMock<ICampaignVisualizationProvider>();
			visualizationProvider.Stub(x => x.ProvideVisualization(id, skipDates.ToArray())).Return(expectedVisualizationVM);
			var removeManualPlanVM = new RemoveManualPlanForm() {CampaignId = id, SkipDates = skipDates};
			var campaignPersister = MockRepository.GenerateMock<IOutboundCampaignPersister>();

			var target = new OutboundController(campaignPersister, null, null, null, visualizationProvider, null, null);
			var result = target.RemoveManualPlan(removeManualPlanVM);

			campaignPersister.AssertWasCalled(x => x.RemoveManualProductionPlan(removeManualPlanVM));
			result.Should().Be.SameInstanceAs(expectedVisualizationVM);
		}

		[Test]
		public void ShouldInvokeCampaignProductionReplan()
		{
			var campaignId = new Guid();
			var skipDates = new List<DateOnly>();
			var form = new PlanWithScheduleForm(){CampaignId = campaignId, SkipDates = skipDates};
			var expectedVisualizationVM = new CampaignVisualizationViewModel();
			_outboundCampaignPersister.Stub(x => x.ManualReplanCampaign(form));
			var visualizationProvider = MockRepository.GenerateMock<ICampaignVisualizationProvider>();
			visualizationProvider.Stub(x => x.ProvideVisualization(campaignId, skipDates.ToArray())).Return(expectedVisualizationVM);
			var target = new OutboundController(_outboundCampaignPersister, null, null, null, visualizationProvider, null, null);
			var result = target.CampaignProductionReplan(form);
			_outboundCampaignPersister.AssertWasCalled(x => x.ManualReplanCampaign(form));

			result.Should().Be.SameInstanceAs(expectedVisualizationVM);
		}

		[Test]
		public void ShouldGetCampaignsSummary()
		{
			var expectedResult = new List<CampaignSummaryViewModel>();
			var peroid = new GanttPeriod();
			var campaignListProvider = MockRepository.GenerateMock<ICampaignListProvider>();
			campaignListProvider.Stub(x => x.GetCampaigns(peroid)).Return(expectedResult);

			var target = new OutboundController(null, null, null, null, null, campaignListProvider, null);
			var result = target.GetCampaigns(peroid);

			result.Should().Be.SameInstanceAs(expectedResult);
		}

		[Test]
		public void ShouldPersistThresholdSetting()
		{
			var thresholdSetting = MockRepository.GenerateMock<ISettingsPersisterAndProvider<OutboundThresholdSettings>>();

			var target = new OutboundController(null, null, null, null, null, null, thresholdSetting);
			target.UpdateThresholdsSetting(new ThresholdSettingForm(){Value = 1, Type = 0});

			thresholdSetting.AssertWasCalled(x => x.Persist(new OutboundThresholdSettings() { RelativeWarningThreshold = new Percent(1) }), y=>y.IgnoreArguments());
		}

		[Test]
		public void ShouldGetThresholdSettingValue()
		{
			var expectedValue = 0.01;
			var thresholdSetting = MockRepository.GenerateMock<ISettingsPersisterAndProvider<OutboundThresholdSettings>>();
			thresholdSetting.Stub(x => x.Get())
				.Return(new OutboundThresholdSettings() {RelativeWarningThreshold = new Percent(expectedValue)});

			var target = new OutboundController(null, null, null, null, null, null, thresholdSetting);
			var result = target.GetThresholdSetting();

			result.Value.Should().Be.EqualTo(expectedValue);
		}		
		
		[Test]
		public void ShouldGetThresholdSettingType()
		{
			var expectedType = WarningThresholdType.Relative;
			var thresholdSetting = MockRepository.GenerateMock<ISettingsPersisterAndProvider<OutboundThresholdSettings>>();
			thresholdSetting.Stub(x => x.Get())
				.Return(new OutboundThresholdSettings() { WarningThresholdType = expectedType });

			var target = new OutboundController(null, null, null, null, null, null, thresholdSetting);
			var result = target.GetThresholdSetting();

			result.Type.Should().Be.EqualTo(expectedType);
		}

		[Test]
		public void ShouldUpdateCache()
		{
			var period = new GanttPeriod();
			var listProvider = MockRepository.GenerateMock<ICampaignListProvider>();

			var target = new OutboundController(null, null, null, null, null, listProvider, null);
			target.UpdateCache(period);

			listProvider.AssertWasCalled(x=>x.CheckAndUpdateCache(period));
		}
	}
}
