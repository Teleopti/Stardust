using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public class OutboundCampaignPersister  : IOutboundCampaignPersister
	{
		private readonly IOutboundCampaignRepository _outboundCampaignRepository;
		private readonly ISkillRepository _skillRepository;

		public OutboundCampaignPersister(IOutboundCampaignRepository outboundCampaignRepository, ISkillRepository skillRepository)
		{
			_outboundCampaignRepository = outboundCampaignRepository;
			_skillRepository = skillRepository;
		}

		public CampaignViewModel Persist(string name)
		{			
			var skills = _skillRepository.LoadAll();
			var campaign = new Domain.Outbound.Campaign(name, skills.FirstOrDefault());
			_outboundCampaignRepository.Add(campaign);

			var skillVMs = new List<SkillViewModel>();
			var isFirst = true;
			foreach (var skill in skills)
			{
				SkillViewModel vm;
				if (isFirst)
				{
					vm = new SkillViewModel() { Id = skill.Id, IsSelected = true, SkillName = skill.Description };
					isFirst = false;
				}
				else
				{
					vm = new SkillViewModel() { Id = skill.Id, IsSelected = false, SkillName = skill.Description };
				}
				skillVMs.Add(vm);
			}

			return new CampaignViewModel()
			{
				Id = campaign.Id.ToString(),
				Name = campaign.Name,
				Skills = skillVMs,
				CallListLen = campaign.CallListLen,
				TargetRate = campaign.TargetRate,
				ConnectRate = campaign.ConnectRate,
				RightPartyConnectRate = campaign.RightPartyConnectRate,
				ConnectAverageHandlingTime = campaign.ConnectAverageHandlingTime,
				RightPartyAverageHandlingTime = campaign.RightPartyAverageHandlingTime,
				UnproductiveTime = campaign.UnproductiveTime,
				StartDate = campaign.StartDate,
				EndDate = campaign.EndDate,
				CampaignStatus = campaign.CampaignStatus,
			};
			
		}
	}
}