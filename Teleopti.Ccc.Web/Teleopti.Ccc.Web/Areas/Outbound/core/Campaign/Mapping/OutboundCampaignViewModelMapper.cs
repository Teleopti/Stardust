using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping
{
	public class OutboundCampaignViewModelMapper : IOutboundCampaignViewModelMapper
	{
		private readonly ISkillRepository _skillRepository;

		public OutboundCampaignViewModelMapper(ISkillRepository skillRepository)
		{
			_skillRepository = skillRepository;
		}

		public IEnumerable<CampaignViewModel> Map(IEnumerable<Domain.Outbound.Campaign> campaigns)
		{
			var skills = _skillRepository.LoadAll();
			var skillVMs = new List<SkillViewModel>();
			foreach (var skill in skills)
			{
				var vm = new SkillViewModel() { Id = skill.Id, IsSelected = false, SkillName = skill.Name };
				skillVMs.Add(vm);
			}

			var campaignViewModels = new List<CampaignViewModel>();
			foreach (var campaign in campaigns)
			{
				var selectedSkill = skillVMs.First(x => x.Id.Equals(campaign.Skill.Id));
				selectedSkill.IsSelected = true;
				var workingPeriods = new List<CampaignWorkingPeriodViewModel>();
				foreach (var workingPeriod in campaign.CampaignWorkingPeriods)
				{
					var period = new CampaignWorkingPeriodViewModel() {Id = workingPeriod.Id, WorkingPeriod = workingPeriod.TimePeriod};
					workingPeriods.Add(period);
				}

				var campaignVm = new CampaignViewModel()
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
					CampaignWorkingPeriods = workingPeriods,
				};
				campaignViewModels.Add(campaignVm);
			}

			return campaignViewModels;
		}
	}
}