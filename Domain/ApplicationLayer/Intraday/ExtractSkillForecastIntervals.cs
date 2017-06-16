using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Intraday
{
	public class ExtractSkillForecastIntervals
	{
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly ICurrentScenario _currentScenario;

		public ExtractSkillForecastIntervals(ISkillDayRepository skillDayRepository, ICurrentScenario currentScenario)
		{
			_skillDayRepository = skillDayRepository;
			_currentScenario = currentScenario;
		}

		public IEnumerable<SkillStaffingInterval> GetBySkills(IList<ISkill> skills, DateTimePeriod period, bool useShrinkage)
		{
			var returnList = new HashSet<SkillStaffingInterval>();
			var skillDays =  _skillDayRepository.FindReadOnlyRange(GetLongestPeriod(skills, period), skills, _currentScenario.Current());
			foreach (var skillDay in skillDays)
			{
				if (useShrinkage)
				{
					foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
					{
						skillStaffPeriod.Payload.UseShrinkage = true;
					}
				}
				getSkillStaffingIntervals(skillDay).ForEach(i => returnList.Add(i));
			}
			return returnList.Where(x => period.Contains((DateTime) x.StartDateTime));
		}

		public static DateOnlyPeriod GetLongestPeriod(IList<ISkill> skills, DateTimePeriod period)
		{
			var returnPeriod = new DateOnlyPeriod(new DateOnly(period.StartDateTime.Date), new DateOnly(period.EndDateTime.Date));
			foreach (var skill in skills)
			{
				var temp = period.ToDateOnlyPeriod(skill.TimeZone);
				if(temp.StartDate < returnPeriod.StartDate)
					returnPeriod = new DateOnlyPeriod(temp.StartDate, returnPeriod.EndDate);
				if (temp.EndDate > returnPeriod.EndDate)
					returnPeriod = new DateOnlyPeriod(returnPeriod.StartDate, temp.EndDate);
			}
			return returnPeriod;
		}
		private IEnumerable<SkillStaffingInterval> getSkillStaffingIntervals(ISkillDay skillDay)
		{
			var skillStaffPeriods = skillDay.SkillStaffPeriodCollection;
			
			return skillStaffPeriods.Select(skillStaffPeriod => new SkillStaffingInterval
			{
				SkillId = skillDay.Skill.Id.GetValueOrDefault(),
				StartDateTime = skillStaffPeriod.Period.StartDateTime,
				EndDateTime = skillStaffPeriod.Period.EndDateTime,
				Forecast = skillStaffPeriod.FStaff,
				StaffingLevel = 0,
			});
		}
	}
}