using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Outbound
{
	public interface IOutboundSkillCreator
	{
		ISkill CreateSkill(IActivity activity, IOutboundCampaign campaign);
		void SetOpenHours(IOutboundCampaign campaign, IWorkload workLoad);
		void SetHandledWithinOnSkillTemplates(ISkill skill);
	}

	public class OutboundSkillCreator : IOutboundSkillCreator
	{
		private readonly IUserTimeZone _userTimeZone;
		private readonly ISkillTypeProvider _skillTypeProvider;

		public OutboundSkillCreator(IUserTimeZone userTimeZone, ISkillTypeProvider skillTypeProvider)
		{
			_userTimeZone = userTimeZone;
			_skillTypeProvider = skillTypeProvider;
		}

        public ISkill CreateSkill(IActivity activity, IOutboundCampaign campaign)
        {
			var skill = new Skill(campaign.Name, "", Color.Blue, 60, _skillTypeProvider.Outbound())
			{
				TimeZone = _userTimeZone.TimeZone(),
				Activity = activity
			};
	        skill.MidnightBreakOffset = getMidnightOffset(campaign.WorkingHours.Values);

			var workLoad = new Workload(skill) {Name = campaign.Name};
			SetOpenHours(campaign, workLoad);

			skill.AddWorkload(workLoad);

			SetHandledWithinOnSkillTemplates(skill);
			setIntradayProfile(skill);

			return skill;
		}

		private TimeSpan getMidnightOffset(IEnumerable<TimePeriod> openHours)
		{
			var offset = new TimeSpan(0,0,0);

			foreach (var openHour in openHours)
			{
				var days = openHour.EndTime.Days;
				var pureTime = openHour.EndTime.Add(TimeSpan.FromDays(-days));
				if (days > 0 && offset < pureTime) offset = pureTime;
			}

			return offset;
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

		public void SetOpenHours(IOutboundCampaign campaign, IWorkload workLoad)
		{
			foreach (DayOfWeek weekDay in Enum.GetValues(typeof (DayOfWeek)))
			{				
				TimePeriod workingHour;
				if (campaign.WorkingHours.TryGetValue(weekDay, out workingHour))
				{
					IWorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();
					workloadDayTemplate.Create(weekDay.ToString(), DateTime.UtcNow, workLoad, new List<TimePeriod> { workingHour });
					workloadDayTemplate.SetUseSkewedDistribution(true);
					workloadDayTemplate.Tasks = 1;
					workLoad.SetTemplate(weekDay, workloadDayTemplate);
				}
				else
				{
					IWorkloadDayTemplate existingTemplate;
					if (workLoad.TemplateWeekCollection.TryGetValue((int) weekDay, out existingTemplate))
					{
						var template =
							workLoad.TryFindTemplateByName(TemplateTarget.Workload, existingTemplate.Name) as IWorkloadDayTemplate;
						if (template == null) continue;
						template.Tasks = 0;
						template.Close();
					}				
				}
				
			}		
		}

		public void SetHandledWithinOnSkillTemplates(ISkill skill)
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