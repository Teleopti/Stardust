using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping
{
	using Campaign = Domain.Outbound.Campaign;

	public class OutboundCampaignViewModelMapper : IOutboundCampaignViewModelMapper
	{

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
			var startTime = TimeHelper.TimeOfDayFromTimeSpan(workingPeriod.TimePeriod.StartTime);
			var endTime = TimeHelper.TimeOfDayFromTimeSpan(workingPeriod.TimePeriod.EndTime);
			var period = new CampaignWorkingPeriodViewModel
			{
				Id = workingPeriod.Id, 
				StartTime = startTime,
				EndTime = endTime,
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

			var skillVMs = new List<SkillViewModel>();

			var vm = new SkillViewModel() { Id = campaign.Skill.Id, IsSelected = false, SkillName = campaign.Skill.Name };
				skillVMs.Add(vm);

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
				StartDate = campaign.SpanningPeriod.StartDate,
				EndDate = campaign.SpanningPeriod.EndDate,
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