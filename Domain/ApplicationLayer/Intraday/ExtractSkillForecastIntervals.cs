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


			var skillStaffPeriods = skillDays.SelectMany(y => y.SkillStaffPeriodCollection.Where(x => period.Intersect(x.Period))).ToList();

			if (useShrinkage)
				skillStaffPeriods.ForEach(x => x.Payload.UseShrinkage = true);


			/*
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
			}*/

			getSkillStaffingIntervals(skillStaffPeriods).ForEach(i => returnList.Add(i));

			return returnList.Where(x => period.Contains(x.StartDateTime) || x.DateTimePeriod.Contains(period.StartDateTime));
		}

		public IEnumerable<SkillStaffingInterval> GetBySkills(IList<ISkill> skills, DateTimePeriod period)
		{
			var returnList = new HashSet<SkillStaffingInterval>();
			var skillDays = _skillDayRepository.FindReadOnlyRange(GetLongestPeriod(skills, period), skills, _currentScenario.Current());
			//var skillStaffPeriod = new List<SkillStaffingInterval>();
			foreach (var skillDay in skillDays)
			{
				var skillStaffPeriods = skillDay.SkillStaffPeriodCollection;

				var temp =  skillStaffPeriods.Select(skillStaffPeriod => new SkillStaffingInterval
				{
					SkillId = skillDay.Skill.Id.GetValueOrDefault(),
					StartDateTime = skillStaffPeriod.Period.StartDateTime,
					EndDateTime = skillStaffPeriod.Period.EndDateTime,
					ForecastWithoutShrinkage = skillStaffPeriod.FStaff,
					Shrinkage = skillStaffPeriod.Payload.Shrinkage,
					Forecast = skillStaffPeriod.FStaff,
					StaffingLevel = 0,
				});
				foreach (var skillStaffingInterval in temp)
				{
					returnList.Add(skillStaffingInterval);
				}
			}
			return returnList.Where(x => period.Contains(x.StartDateTime) || x.DateTimePeriod.Contains(period.StartDateTime));
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

		private IEnumerable<SkillStaffingInterval> getSkillStaffingIntervals(IEnumerable<ISkillStaffPeriod> skillStaffPeriods)
		{			
			return skillStaffPeriods.Select(skillStaffPeriod => new SkillStaffingInterval
			{
				SkillId = skillStaffPeriod.SkillDay.Skill.Id.GetValueOrDefault(),
				StartDateTime = skillStaffPeriod.Period.StartDateTime,
				EndDateTime = skillStaffPeriod.Period.EndDateTime,
				Forecast = skillStaffPeriod.FStaff,
				StaffingLevel = 0,
			});
		}
	}
}