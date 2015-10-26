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
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.ViewModelFactory;
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
		private readonly ICampaignSummaryViewModelFactory _campaignSummaryViewModelFactory;
		private readonly ICampaignVisualizationProvider _campaignVisualizationProvider;
		private readonly ICampaignListProvider _campaignListProvider;
		private readonly ISettingsPersisterAndProvider<OutboundThresholdSettings> _thresholdsSettingPersisterAndProvider;
	

		public OutboundController(IOutboundCampaignPersister outboundCampaignPersister, IOutboundCampaignRepository outboundCampaignRepository, 
			IOutboundCampaignViewModelMapper outboundCampaignViewModelMapper, IActivityProvider activityProvider, 
			ICampaignSummaryViewModelFactory campaignSummaryViewModelFactory, ICampaignVisualizationProvider campaignVisualizationProvider, 
			ICampaignListProvider campaignListProvider, ISettingsPersisterAndProvider<OutboundThresholdSettings> thresholdsSettingPersisterAndProvider)
		{
			_outboundCampaignPersister = outboundCampaignPersister;
			_outboundCampaignRepository = outboundCampaignRepository;
			_outboundCampaignViewModelMapper = outboundCampaignViewModelMapper;
			_activityProvider = activityProvider;
		    _campaignSummaryViewModelFactory = campaignSummaryViewModelFactory;
			_campaignVisualizationProvider = campaignVisualizationProvider;
			_campaignListProvider = campaignListProvider;
			_thresholdsSettingPersisterAndProvider = thresholdsSettingPersisterAndProvider;
		}

		[HttpPost, Route("api/Outbound/Campaign"), UnitOfWork]
		public virtual IHttpActionResult CreateCampaign([FromBody]CampaignForm campaignForm)
		{			
			var campaignVm = _outboundCampaignPersister.Persist(campaignForm);

            return Created(Request.RequestUri + "/" + campaignVm.Id, campaignVm);
		}

		[HttpPost, Route("api/Outbound/Campaigns"), UnitOfWork]
		public virtual List<CampaignSummaryViewModel> GetCamapigns([FromBody]CampaignStatus flag)
		{
		    return _campaignSummaryViewModelFactory.GetCampaignSummaryList(flag, null);
		}			
		
		[HttpPost, Route("api/Outbound/Period/Campaigns"), UnitOfWork]
		public virtual IEnumerable<PeriodCampaignSummaryViewModel> GetCamapigns([FromBody]GanttPeriod peroid)
		{
			return _campaignListProvider.GetPeriodCampaignsSummary(peroid);
		}		
		
		[HttpPost, Route("api/Outbound/Gantt/Campaigns"), UnitOfWork]
		public virtual IEnumerable<GanttCampaignViewModel> GanttGetCampaigns([FromBody]GanttPeriod peroid)
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
		public virtual IHttpActionResult UpdateCampaign(Guid Id, CampaignViewModel campaignViewModel)
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

		[HttpPost, Route("api/Outbound/Campaign/Statistics"), UnitOfWork]
		public virtual CampaignStatistics GetStatistics()
		{
			return _campaignListProvider.GetCampaignStatistics(null);
		}

		[HttpPost, Route("api/Outbound/Campaign/Period/Statistics"), UnitOfWork]
		public virtual CampaignStatistics GetStatistics([FromBody] GanttPeriod period)
		{
			return _campaignListProvider.GetCampaignStatistics(period);
		}		
		
		[HttpPost, Route("api/Outbound/Campaign/Load"), UnitOfWork]
		public virtual bool LoadData()
		{
			_campaignListProvider.LoadData(null);
			return true;
		}		
		
		[HttpPost, Route("api/Outbound/Campaign/Period/Load"), UnitOfWork]
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

		[HttpGet, Route("api/Outbound/Campaigns/{Id}"), UnitOfWork]
		public virtual CampaignSummaryViewModel GetCampaignById(Guid Id)
		{
			return _campaignSummaryViewModelFactory.GetCampaignSummary(Id);
		}		
		
		[HttpPost, Route("api/Outbound/Campaign/Detail"), UnitOfWork]
		public virtual PeriodCampaignSummaryViewModel GetCampaignSummary([FromBody]SummaryForm form)
		{
			return _campaignListProvider.GetCampaignSummary(form.CampaignId);
		}

		[HttpGet, Route("api/Outbound/Campaign/Replan/{Id}"), UnitOfWork]
		public virtual CampaignVisualizationViewModel CampaignProductionReplan(Guid Id)
		{
			_outboundCampaignPersister.ManualReplanCampaign(Id);
			return _campaignVisualizationProvider.ProvideVisualization(Id);
		}

		[HttpPut, Route("api/Outbound/Campaign/ThresholdsSetting"), UnitOfWork]
		public virtual void UpdateThresholdsSetting(ThresholdSettingForm input)
		{
			var thresholdSetting = new OutboundThresholdSettings()
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
			return new ThresholdSettingForm()
			{
				Value = threshold.RelativeWarningThreshold.Value,
				Type = threshold.WarningThresholdType
			};
		}
	}
}