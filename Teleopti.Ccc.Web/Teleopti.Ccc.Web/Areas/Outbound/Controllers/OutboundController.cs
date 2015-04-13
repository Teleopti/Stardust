﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Outbound.Controllers
{

	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OpenForecasterPage)]
	public class OutboundController : ApiController
	{
		private const string GivenDescriptionIsInvalidErrorMessage =
			"The given campaign name is invalid. It can contain at most 255 characters.";

		private readonly IOutboundCampaignPersister _outboundCampaignPersister;
		private readonly IOutboundCampaignRepository _outboundCampaignRepository;
		private readonly IOutboundCampaignViewModelMapper _outboundCampaignViewModelMapper;

		public OutboundController(IOutboundCampaignPersister outboundCampaignPersister, IOutboundCampaignRepository outboundCampaignRepository, IOutboundCampaignViewModelMapper outboundCampaignViewModelMapper)
		{
			_outboundCampaignPersister = outboundCampaignPersister;
			_outboundCampaignRepository = outboundCampaignRepository;
			_outboundCampaignViewModelMapper = outboundCampaignViewModelMapper;
		}

		[HttpPost, Route("api/Outbound/Campaign"), UnitOfWork]
		public virtual IHttpActionResult CreateCampaign([FromBody]CampaignForm campaignForm)
		{
			if (NameValidator.DescriptionIsInvalid(campaignForm.Name)) return BadRequest(GivenDescriptionIsInvalidErrorMessage);

			campaignForm.Id = null;
			var campaignVm = _outboundCampaignPersister.Persist(campaignForm);

			return Created(Request.RequestUri + "/" + campaignVm.Id, campaignVm);
		}		
		
		[HttpGet, Route("api/Outbound/Campaign"), UnitOfWork]
		public virtual ICollection<CampaignViewModel> Get()
		{
			var campaigns = _outboundCampaignRepository.LoadAll();
			var campaignViewModels = _outboundCampaignViewModelMapper.Map(campaigns);

			return campaignViewModels.ToArray();
		}

		[HttpGet, Route("api/Outbound/Campaign/{Id}"), UnitOfWork]
		public virtual CampaignViewModel Get(Guid id)
		{
			var campaign = _outboundCampaignRepository.Get(id);
			return _outboundCampaignViewModelMapper.Map(campaign);			
		}

		[HttpPut, Route("api/Outbound/Campaign/{Id}"), UnitOfWork]
		public virtual IHttpActionResult UpdateCampaign(Guid id, CampaignForm campaignForm)
		{			
			campaignForm.Id = id;
			var campaignVM = _outboundCampaignPersister.Persist(campaignForm);
			if (campaignVM == null)
			{
				return BadRequest();
			}
			return Ok(campaignVM);
		}

		[HttpDelete, Route("api/Outbound/Campaign/{Id}"), UnitOfWork]
		public virtual IHttpActionResult Remove(Guid Id)
		{
			var campaign = _outboundCampaignRepository.Get(Id);
			if (campaign == null)
				return BadRequest();

			_outboundCampaignRepository.Remove(campaign);
			return Ok();
		}
	}
}