using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Intraday
{
	public class ExtractSkillForecastIntervals
	{
		private readonly ICurrentScenario _currentScenario;
		private readonly ISkillDayRepository _skillDayRepository;

		public ExtractSkillForecastIntervals(ISkillDayRepository skillDayRepository, ICurrentScenario currentScenario)
		{
			_currentScenario = currentScenario;
			_skillDayRepository = skillDayRepository;
		}

		public IEnumerable<SkillStaffingInterval> GetBySkills(IList<ISkill> skills, DateTimePeriod period, bool useShrinkage)
		{
			var returnList = new HashSet<SkillStaffingInterval>();
			var skillDays =  _skillDayRepository.FindReadOnlyRange(GetLongestPeriod(skills, period), skills, _currentScenario.Current());

			foreach (var skillDay in skillDays)
			{
				skillDay.RecalculateDailyTasks();
				
				if (useShrinkage)
				{
					foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
					{
						skillStaffPeriod.Payload.UseShrinkage = true;
					}
				}
				
				getSkillStaffingIntervals(skillDay).ForEach(i => returnList.Add(i));
			}
			return returnList.Where(x => period.Contains(x.StartDateTime) || x.DateTimePeriod.Contains(period.StartDateTime));
		}

		public IEnumerable<SkillStaffingInterval> GetBySkills(IList<ISkill> skills, DateTimePeriod period)
		{
			return GetBySkills(skills, period, false);
		}

		public static DateOnlyPeriod GetLongestPeriod(IList<ISkill> skills, DateTimePeriod period)
		{
			var returnPeriod = new DateOnlyPeriod(new DateOnly(period.StartDateTime.Date), new DateOnly(period.EndDateTime.Date));
			foreach (var timeZone in skills.Select(s => s.TimeZone).Distinct())
			{
				var temp = period.ToDateOnlyPeriod(timeZone);
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
				ForecastWithoutShrinkage = skillStaffPeriod.FStaff,
				Shrinkage = skillStaffPeriod.Payload.Shrinkage,
				Forecast = skillStaffPeriod.FStaff,
				StaffingLevel = 0,
			});
		}
	}
}