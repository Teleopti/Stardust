using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using DotNetOpenAuth.Messaging;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.UserTexts;
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
		private readonly string GivenDescriptionIsInvalidErrorMessage = Resources.CampaignNameInvalid;

		private readonly IOutboundCampaignPersister _outboundCampaignPersister;
		private readonly IOutboundCampaignRepository _outboundCampaignRepository;
		private readonly IOutboundCampaignViewModelMapper _outboundCampaignViewModelMapper;
		private readonly IOutboundActivityProvider _outboundActivityProvider;
		private readonly ICampaignStatisticsProvider _campaignStatisticsProvider;
		private readonly IOutboundCampaignListViewModelMapper _campaignListViewModelMapper;

		public OutboundController(IOutboundCampaignPersister outboundCampaignPersister, IOutboundCampaignRepository outboundCampaignRepository, IOutboundCampaignViewModelMapper outboundCampaignViewModelMapper, IOutboundActivityProvider outboundActivityProvider, ICampaignStatisticsProvider campaignStatisticsProvider, IOutboundCampaignListViewModelMapper campaignListViewModelMapper)
		{
			_outboundCampaignPersister = outboundCampaignPersister;
			_outboundCampaignRepository = outboundCampaignRepository;
			_outboundCampaignViewModelMapper = outboundCampaignViewModelMapper;
			_outboundActivityProvider = outboundActivityProvider;
			_campaignStatisticsProvider = campaignStatisticsProvider;
			_campaignListViewModelMapper = campaignListViewModelMapper;
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

		[HttpPost, Route("api/Outbound/Campaigns"), UnitOfWork]
		public virtual CampaignList GetCamapigns(int flag)
		{
			if (flag == 0) flag = 15;
			var campaignList = new CampaignList()
			{
				WarningCampaigns = new List<CampaignListViewModel>(),
				Campaigns = new List<CampaignListViewModel>()
			};

			if ((flag & 4) == 4)
			{
				var warningCampaigns = _campaignStatisticsProvider.GetOnGoingWarningCamapigns();
				if (warningCampaigns.Count != 0)
				{
					campaignList.WarningCampaigns = _campaignListViewModelMapper.Map(warningCampaigns, Resources.OnGoing);
				}

				var onGoingCampaigns = _campaignStatisticsProvider.GetOnGoingCampaigns();
				if (onGoingCampaigns.Count != 0)
				{
					campaignList.Campaigns = _campaignListViewModelMapper.Map(onGoingCampaigns, Resources.OnGoing);
				}
			}

			if ((flag & 1) == 1)
			{
				var plannedCampaigns = _campaignStatisticsProvider.GetPlannedCampaigns();
				if (plannedCampaigns.Count != 0)
				{
					campaignList.Campaigns.AddRange(_campaignListViewModelMapper.Map(plannedCampaigns, Resources.Planned));
				}
			}

			if ((flag & 2) == 2)
			{
				var warningCampaigns = _campaignStatisticsProvider.GetScheduledWarningCampaigns();
				if (warningCampaigns.Count != 0)
				{
					campaignList.WarningCampaigns.AddRange(_campaignListViewModelMapper.Map(warningCampaigns, Resources.Scheduled));
				}

				var scheduledCampaigns = _campaignStatisticsProvider.GetScheduledCampaigns();
				if (scheduledCampaigns.Count != 0)
				{
					campaignList.Campaigns.AddRange(_campaignListViewModelMapper.Map(scheduledCampaigns, Resources.Scheduled));
				}
				
			}

			if ((flag & 8) == 8)
			{
				var doneCampaigns = _campaignStatisticsProvider.GetDoneCampaigns();
				if (doneCampaigns.Count != 0)
				{
					campaignList.Campaigns.AddRange(_campaignListViewModelMapper.Map(doneCampaigns, Resources.Done));
				}
			}

			return campaignList;
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

		[HttpGet, Route("api/Outbound/Campaign/Statistics"), UnitOfWork]
		public virtual CampaignStatistics GetStatistics()
		{
			return _campaignStatisticsProvider.GetWholeStatistics();
		}
	}
}