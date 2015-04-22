using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping
{
	using Campaign = Domain.Outbound.Campaign;

	public class OutboundCampaignViewModelMapper : IOutboundCampaignViewModelMapper
	{
		private readonly ISkillRepository _skillRepository;
		private readonly ICreateHourText _createHourText;

		public OutboundCampaignViewModelMapper(ISkillRepository skillRepository, ICreateHourText createHourText)
		{
			_skillRepository = skillRepository;
			_createHourText = createHourText;
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
			var startTime = _createHourText.CreateText(new DateTime(1900, 1, 1, workingPeriod.TimePeriod.StartTime.Hours,
				workingPeriod.TimePeriod.EndTime.Minutes, workingPeriod.TimePeriod.EndTime.Seconds));		
			var endTime =  _createHourText.CreateText(new DateTime(1900, 1, 1, workingPeriod.TimePeriod.EndTime.Hours,
				workingPeriod.TimePeriod.EndTime.Minutes, workingPeriod.TimePeriod.EndTime.Seconds));
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