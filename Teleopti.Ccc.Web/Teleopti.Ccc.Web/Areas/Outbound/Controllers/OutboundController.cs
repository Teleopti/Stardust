using System;
using System.Collections.Generic;
using System.Linq;
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

	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.Outbound)]
	public class OutboundController : ApiController
	{
		private const string GivenDescriptionIsInvalidErrorMessage =
			"The given campaign name is invalid. It can contain at most 255 characters.";

		private readonly IOutboundCampaignPersister _outboundCampaignPersister;
		private readonly IOutboundCampaignRepository _outboundCampaignRepository;
		private readonly IOutboundCampaignViewModelMapper _outboundCampaignViewModelMapper;
		private readonly IOutboundActivityProvider _outboundActivityProvider;

		public OutboundController(IOutboundCampaignPersister outboundCampaignPersister, IOutboundCampaignRepository outboundCampaignRepository, IOutboundCampaignViewModelMapper outboundCampaignViewModelMapper, IOutboundActivityProvider outboundActivityProvider)
		{
			_outboundCampaignPersister = outboundCampaignPersister;
			_outboundCampaignRepository = outboundCampaignRepository;
			_outboundCampaignViewModelMapper = outboundCampaignViewModelMapper;
			_outboundActivityProvider = outboundActivityProvider;
		}

		[HttpPost, Route("api/Outbound/Campaign"), UnitOfWork]
		public virtual IHttpActionResult CreateCampaign([FromBody]CampaignForm campaignForm)
		{
			if (NameValidator.DescriptionIsInvalid(campaignForm.Name)) return BadRequest(GivenDescriptionIsInvalidErrorMessage);

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

		[HttpPut, Route("api/Outbound/Campaign/{Id}/WorkingPeriod"), UnitOfWork]
		public virtual IHttpActionResult UpdateCampaignWorkingPeriodAssignment(Guid Id, CampaignWorkingPeriodAssignmentForm form)
		{
			form.CampaignId = Id;
			var campaign = _outboundCampaignPersister.Persist(form);
			if (campaign == null)
			{
				return BadRequest();
			}
			return Ok();
		}

		[HttpDelete, Route("api/Outbound/Campaign/{Id}/WorkingPeriod/{CampaignWorkingPeriodId}"), UnitOfWork]
		public virtual void Remove(Guid Id, Guid CampaignWorkingPeriodId)
		{
			var campaign = _outboundCampaignRepository.Get(Id);
			var targets = campaign.CampaignWorkingPeriods.Where(x => CampaignWorkingPeriodId == x.Id).ToList();
			foreach (var target in targets)
			{
				campaign.RemoveWorkingPeriod(target);
			}			
		}

		[HttpPost, Route("api/Outbound/Campaign/{Id}/WorkingPeriod")]
		public virtual IHttpActionResult AddCampaignWorkingPeriod(Guid Id, CampaignWorkingPeriodForm form)
		{
			CampaignWorkingPeriodViewModel viewModel;
			CampaignWorkingPeriod entity;
			PersistAndMapWorkingPeriod(Id, form, out viewModel, out entity);
			if (viewModel == null)
				return BadRequest();
			viewModel.Id = entity.Id;
			return Ok(viewModel);
		}

		[UnitOfWork]
		public virtual void PersistAndMapWorkingPeriod(Guid Id, CampaignWorkingPeriodForm form, out CampaignWorkingPeriodViewModel viewModel, out CampaignWorkingPeriod entity)
		{
			form.CampaignId = Id;
			entity = _outboundCampaignPersister.Persist(form);			
			viewModel = (entity != null)? _outboundCampaignViewModelMapper.Map(entity): null;			
		}
	}
}