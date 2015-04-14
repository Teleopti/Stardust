using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping
{
	using Campaign = Domain.Outbound.Campaign;

	public class OutboundCampaignViewModelMapper : IOutboundCampaignViewModelMapper
	{
		private readonly ISkillRepository _skillRepository;

		public OutboundCampaignViewModelMapper(ISkillRepository skillRepository)
		{
			_skillRepository = skillRepository;
		}

		public CampaignWorkingPeriodAssignmentViewModel Map(CampaignWorkingPeriodAssignment assignment)
		{
			return new CampaignWorkingPeriodAssignmentViewModel { Id = assignment.Id, WeekDay = assignment.WeekdayIndex };
		}

		public IEnumerable<CampaignWorkingPeriodAssignmentViewModel> Map(
			IEnumerable<CampaignWorkingPeriodAssignment> assignments)
		{
			return assignments.Select(Map);
		}

		public CampaignWorkingPeriodViewModel Map(CampaignWorkingPeriod workingPeriod)
		{
			var period = new CampaignWorkingPeriodViewModel
			{
				Id = workingPeriod.Id, WorkingPeriod = workingPeriod.TimePeriod, 
				WorkingPeroidAssignments =Map(workingPeriod.CampaignWorkingPeriodAssignments).ToList()
			};
			return period;
		}

		public IEnumerable<CampaignWorkingPeriodViewModel> Map(IEnumerable<CampaignWorkingPeriod> workingPeriods)
		{
			return workingPeriods.Select(Map);
		}

		public CampaignViewModel Map(Campaign campaign)
		{
			if (campaign == null) return null;

			var skills = _skillRepository.LoadAll();
			var skillVMs = new List<SkillViewModel>();
			foreach (var skill in skills)
			{
				var vm = new SkillViewModel() {Id = skill.Id, IsSelected = false, SkillName = skill.Name};
				skillVMs.Add(vm);
			}

			var selectedSkill = skillVMs.First(x => x.Id.Equals(campaign.Skill.Id));
			selectedSkill.IsSelected = true;

			var campaignVm = new CampaignViewModel
			{
				Id = campaign.Id,
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
				CampaignWorkingPeriods = Map(campaign.CampaignWorkingPeriods).ToList(),
			};

			return campaignVm;
		}

		public IEnumerable<CampaignViewModel> Map(IEnumerable<Campaign> campaigns)
		{
			return campaigns.Select(Map);
		}
	}
}