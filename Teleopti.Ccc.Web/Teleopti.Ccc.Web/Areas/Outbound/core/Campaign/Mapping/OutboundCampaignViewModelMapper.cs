using System.Collections.Generic;
using System.Linq;
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

		public CampaignViewModel Map(Campaign campaign)
		{
			var skills = _skillRepository.LoadAll();
			var skillVMs = new List<SkillViewModel>();
			foreach (var skill in skills)
			{
				var vm = new SkillViewModel() {Id = skill.Id, IsSelected = false, SkillName = skill.Name};
				skillVMs.Add(vm);
			}

			var selectedSkill = skillVMs.First(x => x.Id.Equals(campaign.Skill.Id));
			selectedSkill.IsSelected = true;

			var workingPeriods = new List<CampaignWorkingPeriodViewModel>();
			foreach (var workingPeriod in campaign.CampaignWorkingPeriods)
			{
				var periodAssignments = new List<CampaignWorkingPeriodAssignmentViewModel>();
				foreach (var assignment in workingPeriod.CampaignWorkingPeriodAssignments)
				{
					var periodAssignment = new CampaignWorkingPeriodAssignmentViewModel {Id=assignment.Id, WeekDay = assignment.WeekdayIndex};
					periodAssignments.Add(periodAssignment);
				}

				var period = new CampaignWorkingPeriodViewModel { Id = workingPeriod.Id, WorkingPeriod = workingPeriod.TimePeriod, WorkingPeroidAssignments = periodAssignments};
				workingPeriods.Add(period);
			}

			var campaignVm = new CampaignViewModel
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

			return campaignVm;
		}

		public IEnumerable<CampaignViewModel> Map(IEnumerable<Campaign> campaigns)
		{
			return campaigns.Select( Map);
		}
	}
}