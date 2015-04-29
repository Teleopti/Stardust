using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Outbound
{
	public class OutboundSkillCreator
	{
		private readonly IUserTimeZone _userTimeZone;
		private readonly IOutboundSkillTypeProvider _outboundSkillTypeProvider;

		public OutboundSkillCreator(IUserTimeZone userTimeZone, IOutboundSkillTypeProvider outboundSkillTypeProvider)
		{
			_userTimeZone = userTimeZone;
			_outboundSkillTypeProvider = outboundSkillTypeProvider;
		}

		public ISkill CreateSkill(IActivity activity, Campaign campaign)
		{
			var skill = new Skill(campaign.Name, "", Color.Blue, 60, _outboundSkillTypeProvider.OutboundSkillType())
			{
				TimeZone = _userTimeZone.TimeZone(),
				Activity = activity
			};
		
			var workLoad = new Workload(skill) {Name = campaign.Name};
			setOpenHours(campaign, workLoad);

			skill.AddWorkload(workLoad);

			setHandledWithinOnSkillTemplates(skill);
			setIntradayProfile(skill);

			return skill;
		}

		private static void setIntradayProfile(ISkill skill)
		{
			foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof (DayOfWeek)))
			{
				var template =
					(IWorkloadDayTemplate)skill.WorkloadCollection.First().GetTemplate(TemplateTarget.Workload, dayOfWeek);
				if(!template.OpenForWork.IsOpen)
					continue;

				for (int i = 0; i < template.OpenTaskPeriodList.Count; i++)
				{
					var templateTaskPeriod = template.OpenTaskPeriodList[i];
					templateTaskPeriod.SetTasks(i == 0 ? 1 : 0);
				}
			}
		}

		private static void setOpenHours(Campaign campaign, Workload workLoad)
		{
			foreach (var campaignWorkingPeriod in campaign.CampaignWorkingPeriods)
			{
				foreach (var campaignWorkingPeriodAssignment in campaignWorkingPeriod.CampaignWorkingPeriodAssignments)
				{
					IWorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();
					workloadDayTemplate.Create(campaignWorkingPeriodAssignment.WeekdayIndex.ToString(), DateTime.UtcNow, workLoad,
						new List<TimePeriod> {campaignWorkingPeriod.TimePeriod});

					workLoad.SetTemplate(campaignWorkingPeriodAssignment.WeekdayIndex, workloadDayTemplate);
				}
			}
		}

		private static void setHandledWithinOnSkillTemplates(ISkill skill)
		{
			DateTime startDateUtc = TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date, skill.TimeZone);
			DateTimePeriod timePeriod = new DateTimePeriod(
				startDateUtc, startDateUtc.AddDays(1)).MovePeriod(skill.MidnightBreakOffset);

			foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
			{
				var template =
					(IWorkloadDayTemplate) skill.WorkloadCollection.First().GetTemplate(TemplateTarget.Workload, dayOfWeek);
				var opehourList = template.OpenHourList;
				var workLoadOpenHours = new TimePeriod();
				if(opehourList.Any())
					workLoadOpenHours = template.OpenHourList.First();
				var serviceAgreement =
					new ServiceAgreement(new ServiceLevel(new Percent(1), (int) workLoadOpenHours.SpanningTime().TotalSeconds),
						new Percent(0), new Percent(1));
				var templateSkillDataPeriod = new TemplateSkillDataPeriod(serviceAgreement, new SkillPersonData(), timePeriod);
				var skillDayTemplate = new SkillDayTemplate(dayOfWeek.ToString(),
					new List<ITemplateSkillDataPeriod> {templateSkillDataPeriod});

				skill.SetTemplateAt((int)dayOfWeek, skillDayTemplate);
			}
		}
	}
}