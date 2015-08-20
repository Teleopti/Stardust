﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Outbound.Controllers
{

	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.Outbound)]
	public class OutboundController : ApiController
	{		
		private readonly IOutboundCampaignPersister _outboundCampaignPersister;
		private readonly IOutboundCampaignRepository _outboundCampaignRepository;
		private readonly IOutboundCampaignViewModelMapper _outboundCampaignViewModelMapper;
		private readonly IOutboundActivityProvider _outboundActivityProvider;
		private readonly ICampaignSummaryViewModelFactory _campaignSummaryViewModelFactory;
		private readonly ICampaignVisualizationProvider _campaignVisualizationProvider;
	

		public OutboundController(IOutboundCampaignPersister outboundCampaignPersister, IOutboundCampaignRepository outboundCampaignRepository, 
			IOutboundCampaignViewModelMapper outboundCampaignViewModelMapper, IOutboundActivityProvider outboundActivityProvider, 
			ICampaignSummaryViewModelFactory campaignSummaryViewModelFactory, ICampaignVisualizationProvider campaignVisualizationProvider)
		{
			_outboundCampaignPersister = outboundCampaignPersister;
			_outboundCampaignRepository = outboundCampaignRepository;
			_outboundCampaignViewModelMapper = outboundCampaignViewModelMapper;
			_outboundActivityProvider = outboundActivityProvider;
		    _campaignSummaryViewModelFactory = campaignSummaryViewModelFactory;
			_campaignVisualizationProvider = campaignVisualizationProvider;
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
		    return _campaignSummaryViewModelFactory.GetCampaignSummaryList(flag);
		}

		[HttpGet, Route("api/Outbound/Campaign/Activities"), UnitOfWork]
		public virtual ICollection<ActivityViewModel> GetActivities()
		{
			return _outboundActivityProvider.GetAll().ToArray();
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

			_outboundCampaignRepository.Remove(campaign);
			return Ok();
		}

		[HttpGet, Route("api/Outbound/Campaign/Statistics"), UnitOfWork]
		public virtual CampaignStatistics GetStatistics()
		{
			return _campaignSummaryViewModelFactory.GetCampaignStatistics();
		}		
		
		[HttpGet, Route("api/Outbound/Campaign/Load"), UnitOfWork]
		public virtual bool LoadData()
		{
			_campaignSummaryViewModelFactory.Load();
			return true;
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

		[HttpGet, Route("api/Outbound/Campaign/{Id}/Replan"), UnitOfWork]
		public virtual bool CampaignProductionReplan(Guid Id)
		{
			_outboundCampaignPersister.ManualReplanCampaign(Id);
			return true;
		}
	}
}