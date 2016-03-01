using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public interface ISchedulerSkillDayHelper
    {
		IDictionary<ISkill, IList<ISkillDay>> AddMaxSeatSkillDaysToStateHolder(DateOnlyPeriod datePeriod, IEnumerable<ISkill> maxSeatSkills, IScenario scenario);
    }

    public class SchedulerSkillDayHelper : ISchedulerSkillDayHelper
    {
	    private readonly IWorkloadDayHelper _workLoadDayHelper;

	    public SchedulerSkillDayHelper(IWorkloadDayHelper workLoadDayHelper)
	    {
		    _workLoadDayHelper = workLoadDayHelper;
	    }

	    public IDictionary<ISkill, IList<ISkillDay>> AddMaxSeatSkillDaysToStateHolder(DateOnlyPeriod datePeriod, IEnumerable<ISkill> maxSeatSkills, IScenario scenario)
		{
			var theSkillDays = new Dictionary<ISkill, IList<ISkillDay>>();
			foreach (var skill in maxSeatSkills)
			{
					ICollection<ISkillDay> skillDays =
					getAllSkillDays(datePeriod, skill, scenario);
					foreach (ISkillDay skillDay in skillDays)
					{
						var sDay = skillDay as IMaxSeatSkillDay;
						if (sDay != null)
							sDay.OpenAllSkillStaffPeriods();
					}
					theSkillDays.Add(skill, new List<ISkillDay>(skillDays));			
			}
			
			return theSkillDays;
		}

		private ICollection<ISkillDay> getAllSkillDays(DateOnlyPeriod period, ISkill skill, IScenario scenario)
		{
			ICollection<ISkillDay> skillDays = new Collection<ISkillDay>();
			var uniqueDays = period.DayCollection();
			var datesToProcess = uniqueDays.Except(skillDays.Select(s => s.CurrentDate)).ToArray();

			if (datesToProcess.Any())
			{
				IList<ISkillDay> skillDaysToRepository = new List<ISkillDay>();
				foreach (var uniqueDate in datesToProcess)
				{
					ISkillDay skillDay = new SkillDay();
					skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new List<ISkillDay> { skillDay }, new DateOnlyPeriod());
					ISkillDayTemplate skillDayTemplate = skill.GetTemplateAt((int)uniqueDate.DayOfWeek);

					skillDay.CreateFromTemplate(uniqueDate, skill, scenario, skillDayTemplate);

					skillDays.Add(skillDay);
					skillDaysToRepository.Add(skillDay);
				}
			}

			_workLoadDayHelper.CreateLongtermWorkloadDays(skill, skillDays);

			var ret = skillDays.OrderBy(wd => wd.CurrentDate).ToList();
			return ret;
		}
    }
}