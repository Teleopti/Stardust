using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SystemSetting.OutboundSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Ccc.Web.Core.Data;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.Controllers
{

	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.Outbound)]
	public class OutboundController : ApiController
	{		
		private readonly IOutboundCampaignPersister _outboundCampaignPersister;
		private readonly IOutboundCampaignRepository _outboundCampaignRepository;
		private readonly IOutboundCampaignViewModelMapper _outboundCampaignViewModelMapper;
		private readonly IActivityProvider _activityProvider;
		private readonly ICampaignVisualizationProvider _campaignVisualizationProvider;
		private readonly ICampaignListProvider _campaignListProvider;
		private readonly ISettingsPersisterAndProvider<OutboundThresholdSettings> _thresholdsSettingPersisterAndProvider;
	

		public OutboundController(IOutboundCampaignPersister outboundCampaignPersister, IOutboundCampaignRepository outboundCampaignRepository, 
			IOutboundCampaignViewModelMapper outboundCampaignViewModelMapper, IActivityProvider activityProvider, 
			ICampaignVisualizationProvider campaignVisualizationProvider, ICampaignListProvider campaignListProvider, 
			ISettingsPersisterAndProvider<OutboundThresholdSettings> thresholdsSettingPersisterAndProvider)
		{
			_outboundCampaignPersister = outboundCampaignPersister;
			_outboundCampaignRepository = outboundCampaignRepository;
			_outboundCampaignViewModelMapper = outboundCampaignViewModelMapper;
			_activityProvider = activityProvider;
			_campaignVisualizationProvider = campaignVisualizationProvider;
			_campaignListProvider = campaignListProvider;
			_thresholdsSettingPersisterAndProvider = thresholdsSettingPersisterAndProvider;
		}

		[HttpGet, Route("api/Outbound/permc"), UnitOfWork]
		public virtual IHttpActionResult EarlyCheckPermission()
		{
			return Ok();
		}

		[HttpPost, Route("api/Outbound/Campaign"), UnitOfWork]
		public virtual IHttpActionResult CreateCampaign([FromBody]CampaignForm campaignForm)
		{			
			var campaignVm = _outboundCampaignPersister.Persist(campaignForm);

            return Created(Request.RequestUri + "/" + campaignVm.Id, campaignVm);
		}		
		
		[HttpPost, Route("api/Outbound/Campaigns/Status"), UnitOfWork]
		public virtual IEnumerable<CampaignStatusViewModel> GetCamapignsStatus([FromBody]GanttPeriod peroid)
		{
			return _campaignListProvider.GetCampaignsStatus(peroid);
		}		
		
		[HttpPost, Route("api/Outbound/Campaigns"), UnitOfWork]
		public virtual IEnumerable<CampaignSummaryViewModel> GetCampaigns([FromBody]GanttPeriod peroid)
		{
			return _campaignListProvider.GetCampaigns(peroid);
		}

		[HttpGet, Route("api/Outbound/Campaign/Activities"), UnitOfWork]
		public virtual ICollection<ActivityViewModel> GetActivities()
		{
			return _activityProvider.GetAll().ToArray();
		}

		[HttpGet, Route("api/Outbound/Campaign/{Id}"), UnitOfWork]
		public virtual CampaignViewModel Get(Guid Id)
		{
			var campaign = _outboundCampaignRepository.Get(Id);
			return _outboundCampaignViewModelMapper.Map(campaign);			
		}

		[HttpGet, Route("api/Outbound/Campaign/Visualization/{Id}"), UnitOfWork]
		public virtual CampaignVisualizationViewModel GetVisualization(Guid id)
		{
			return _campaignVisualizationProvider.ProvideVisualization(id);
		}

		[HttpPut, Route("api/Outbound/Campaign/{Id}"), UnitOfWork]
		public virtual IHttpActionResult UpdateCampaign(Guid Id, [FromBody]CampaignViewModel campaignViewModel)
		{
			var campaign = _outboundCampaignPersister.Persist(campaignViewModel);
			if (campaign == null)
			{
				return BadRequest();
			}
			return  Ok(campaignViewModel);
		}

		[HttpDelete, Route("api/Outbound/Campaign/{Id}"), UnitOfWork]
		public virtual IHttpActionResult Remove(Guid Id)
		{
			var campaign = _outboundCampaignRepository.Get(Id);
			if (campaign == null)
				return NotFound();

			_outboundCampaignPersister.RemoveCampaign(campaign);
			return Ok();
		}
		
		[HttpPost, Route("api/Outbound/Campaign/Load"), UnitOfWork]
		public virtual bool LoadData([FromBody]GanttPeriod period)
		{
			_campaignListProvider.ResetCache();
			_campaignListProvider.LoadData(period);
			return true;
		}

		[HttpPost, Route("api/Outbound/Campaign/ActualBacklog"), UnitOfWork]
		public virtual CampaignVisualizationViewModel AddActualBacklog([FromBody] ActualBacklogForm actualBacklog)
		{
			_outboundCampaignPersister.PersistActualBacklog(actualBacklog);
			return _campaignVisualizationProvider.ProvideVisualization(actualBacklog.CampaignId);
		}

		[HttpPost, Route("api/Outbound/Campaign/ActualBacklog/Remove"), UnitOfWork]
		public virtual CampaignVisualizationViewModel RemoveActualBacklog([FromBody] RemoveActualBacklogForm actualBacklog)
		{
			_outboundCampaignPersister.RemoveActualBacklog(actualBacklog);
			return _campaignVisualizationProvider.ProvideVisualization(actualBacklog.CampaignId);
		}

		[HttpPost, Route("api/Outbound/Campaign/ManualPlan"), UnitOfWork]
		public virtual CampaignVisualizationViewModel ManualPlan([FromBody] ManualPlanForm manualPlan)
		{
			_outboundCampaignPersister.PersistManualProductionPlan(manualPlan);
			return _campaignVisualizationProvider.ProvideVisualization(manualPlan.CampaignId);
		}		
		
		[HttpPost, Route("api/Outbound/Campaign/ManualPlan/Remove"), UnitOfWork]
		public virtual CampaignVisualizationViewModel RemoveManualPlan([FromBody] RemoveManualPlanForm manualPlan)
		{
			_outboundCampaignPersister.RemoveManualProductionPlan(manualPlan);
			return _campaignVisualizationProvider.ProvideVisualization(manualPlan.CampaignId);
		}	
		
		[HttpPost, Route("api/Outbound/Campaign/Status"), UnitOfWork]
		public virtual CampaignStatusViewModel GetCampaignStatus([FromBody]SummaryForm form)
		{
			return _campaignListProvider.GetCampaignStatus(form.CampaignId);
		}

		[HttpGet, Route("api/Outbound/Campaign/Replan/{Id}"), UnitOfWork]
		public virtual CampaignVisualizationViewModel CampaignProductionReplan(Guid Id)
		{
			_outboundCampaignPersister.ManualReplanCampaign(Id);
			return _campaignVisualizationProvider.ProvideVisualization(Id);
		}

		[HttpPut, Route("api/Outbound/Campaign/ThresholdsSetting"), UnitOfWork]
		public virtual void UpdateThresholdsSetting([FromBody]ThresholdSettingForm input)
		{
			var thresholdSetting = new OutboundThresholdSettings
			{
				RelativeWarningThreshold = new Percent(input.Value),
				WarningThresholdType = input.Type
			};
			_thresholdsSettingPersisterAndProvider.Persist(thresholdSetting);
		}

		[HttpPost, Route("api/Outbound/Campaign/ThresholdsSetting"), UnitOfWork]
		public virtual ThresholdSettingForm GetThresholdSetting()
		{
			var threshold = _thresholdsSettingPersisterAndProvider.Get();
			return new ThresholdSettingForm
			{
				Value = threshold.RelativeWarningThreshold.Value,
				Type = threshold.WarningThresholdType
			};
		}

		[HttpPut, Route("api/Outbound/Campaign/Update/Schedule"), UnitOfWork]
		public virtual bool UpdateCache(GanttPeriod period)
		{
			_campaignListProvider.CheckAndUpdateCache(period);
			return true;
		}
	}
}