using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping;
using Teleopti.Ccc.Web.Areas.Outbound.Models;


namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	using Campaign = Domain.Outbound.Campaign;

	public class OutboundCampaignPersister  : IOutboundCampaignPersister
	{		
		private readonly IOutboundCampaignRepository _outboundCampaignRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly IOutboundCampaignMapper _outboundCampaignMapper;
		private readonly IOutboundCampaignViewModelMapper _outboundCampaignViewModelMapper;

		public OutboundCampaignPersister(IOutboundCampaignRepository outboundCampaignRepository, ISkillRepository skillRepository, 
			IOutboundCampaignMapper outboundCampaignMapper, IOutboundCampaignViewModelMapper outboundCampaignViewModelMapper)
		{
			_outboundCampaignRepository = outboundCampaignRepository;
			_skillRepository = skillRepository;
			_outboundCampaignMapper = outboundCampaignMapper;
			_outboundCampaignViewModelMapper = outboundCampaignViewModelMapper;
		}

		public CampaignViewModel Persist(string name)
		{
			var skills = _skillRepository.LoadAll();
			var campaign = new Campaign(name, skills.FirstOrDefault());
			_outboundCampaignRepository.Add(campaign);

			return _outboundCampaignViewModelMapper.Map(campaign);
		}

		public Campaign Persist(CampaignViewModel campaignViewModel)
		{
			Campaign campaign = null;

			if (campaignViewModel.Id.HasValue)
			{
				campaign = _outboundCampaignMapper.Map(campaignViewModel);
			}

			return campaign;
		}
	}
}