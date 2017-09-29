using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public interface ISkillDayLoadHelper
	{
		/// <summary>
		/// Loads the scheduler skill days.
		/// </summary>
		/// <param name="period">The period.</param>
		/// <param name="skills">The skills.</param>
		/// <param name="scenario">The scenario.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-05-08
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		IDictionary<ISkill, IEnumerable<ISkillDay>> LoadSchedulerSkillDays(DateOnlyPeriod period, IEnumerable<ISkill> skills, IScenario scenario);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        IDictionary<ISkill, IEnumerable<ISkillDay>> LoadBudgetSkillDays(DateOnlyPeriod period, IEnumerable<ISkill> skills, IScenario scenario);
	}

	/// <summary>
    /// Helper for loading of skill days
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-05-08
    /// </remarks>
    public class SkillDayLoadHelper : ISkillDayLoadHelper
	{
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IMultisiteDayRepository _multisiteDayRepository;

		public SkillDayLoadHelper(ISkillDayRepository skillDayRepository, IMultisiteDayRepository multisiteDayRepository)
		{
			InParameter.NotNull(nameof(multisiteDayRepository), multisiteDayRepository);
			InParameter.NotNull(nameof(skillDayRepository), skillDayRepository);

			_skillDayRepository = skillDayRepository;
			_multisiteDayRepository = multisiteDayRepository;
		}

		/// <summary>
        /// Loads the scheduler skill days.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="skills">The skills.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-08
        /// </remarks>
        public IDictionary<ISkill, IEnumerable<ISkillDay>> LoadSchedulerSkillDays(DateOnlyPeriod period, IEnumerable<ISkill> skills, IScenario scenario)
        {
            if (skills == null || scenario==null) return new Dictionary<ISkill, IEnumerable<ISkillDay>>();
            
		    var skillsToLoad = skills.Where(skill => !(skill is IChildSkill)).ToList();
		    return calculateSkillSkillDayDictionary(period, scenario, skillsToLoad);
        }

        public IDictionary<ISkill, IEnumerable<ISkillDay>> LoadBudgetSkillDays(DateOnlyPeriod period, IEnumerable<ISkill> skills, IScenario scenario)
        {
            if (skills == null || scenario == null) return new Dictionary<ISkill, IEnumerable<ISkillDay>>();
            var skillsToLoad = new List<ISkill>();
            foreach (var skill in skills)
            {
                if (skill is IMultisiteSkill) throw new ArgumentException("Should have no multisite skill.");
                var childSkill = skill as IChildSkill;
                if (childSkill != null)
                {
                    var multisiteSkill = childSkill.ParentSkill;
                    if (!skillsToLoad.Contains(multisiteSkill))
                        skillsToLoad.Add(multisiteSkill);
                    continue;
                }
                skillsToLoad.Add(skill);
            }
            return calculateSkillSkillDayDictionary(period, scenario, skillsToLoad);
        }

	    private IDictionary<ISkill, IEnumerable<ISkillDay>> calculateSkillSkillDayDictionary(DateOnlyPeriod period, IScenario scenario, IEnumerable<ISkill> skillsToLoad)
	    {
	        IList<SkillDayCalculator> calculators = new List<SkillDayCalculator>();
	        // now we don't know the timezone stretch the period one day more in start and end
	        var periodToLoad = new DateOnlyPeriod(period.StartDate.AddDays(-8), period.EndDate.AddDays(2));
	        var testSkillDays = _skillDayRepository.FindReadOnlyRange(periodToLoad, skillsToLoad, scenario).ToLookup(k => k.Skill);

			periodToLoad = SkillDayCalculator.GetPeriodToLoad(period);
	        foreach (var skill in skillsToLoad)
	        {
	            var skillDays = testSkillDays[skill].OrderBy(s => s.CurrentDate).ToArray();

	            var multisiteSkill = skill as IMultisiteSkill;
	            if (multisiteSkill != null)
	            {
	                IList<IMultisiteDay> multisiteDays =
	                    _multisiteDayRepository.FindRange(periodToLoad, multisiteSkill, scenario).ToList();
	                var multisiteCalculator = new MultisiteSkillDayCalculator(multisiteSkill, skillDays,
	                                                                                                  multisiteDays, period);

					var childSkillDays =
						_skillDayRepository.FindReadOnlyRange(periodToLoad, multisiteSkill.ChildSkills, scenario).ToLookup(g => (IChildSkill)g.Skill);

					foreach (var childSkill in multisiteSkill.ChildSkills)
	                {
	                    multisiteCalculator.SetChildSkillDays(childSkill, childSkillDays[childSkill].OrderBy(s => s.CurrentDate).ToList());
	                }

	                multisiteCalculator.InitializeChildSkills();

	                calculators.Add(multisiteCalculator);
	            }
	            else
	            {
	                calculators.Add(new SkillDayCalculator(skill, skillDays, period));
	            }
	            foreach (ISkillDay skillDay in skillDays)
	            {
	                skillDay.RecalculateDailyTasks();
	            }
	        }

	        var skillSkillDayDictionary = new Dictionary<ISkill, IEnumerable<ISkillDay>>();
	        foreach (ISkillDayCalculator calculator in calculators)
	        {
	            var multisiteSkillDayCalculator = calculator as MultisiteSkillDayCalculator;
	            if (multisiteSkillDayCalculator != null)
	            {
	                foreach (var childSkill in multisiteSkillDayCalculator.MultisiteSkill.ChildSkills)
	                {
	                    if (!skillSkillDayDictionary.ContainsKey(childSkill))
	                        skillSkillDayDictionary.Add(childSkill,
	                                                    multisiteSkillDayCalculator.GetVisibleChildSkillDays(childSkill));
	                }
	            }
	            if (!skillSkillDayDictionary.ContainsKey(calculator.Skill))
	                skillSkillDayDictionary.Add(calculator.Skill, calculator.SkillDays.ToList());
	        }
	        return skillSkillDayDictionary;
	    }
	}

	public interface IWorkloadDayHelper
	{
		/// <summary>
		/// Gets the workload days from skill days.
		/// </summary>
		/// <param name="skillDays">The skill days.</param>
		/// <param name="workload">The workload.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-04-02
		/// </remarks>
		IList<IWorkloadDayBase> GetWorkloadDaysFromSkillDays(IEnumerable<ISkillDay> skillDays, IWorkload workload);

		/// <summary>
		/// Create all workload days that belongs to a set of skill days with the longterm template (one merged interval for the entire open hours).
		/// </summary>
		/// <param name="skill">The skill.</param>
		/// <param name="skillDays">The skill days.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-01-24
		/// </remarks>
		void CreateLongtermWorkloadDays(ISkill skill,IEnumerable<ISkillDay> skillDays);
	}

	public class WorkloadDayHelper : IWorkloadDayHelper
	{
        /// <summary>
        /// Gets the workload days from skill days.
        /// </summary>
        /// <param name="skillDays">The skill days.</param>
        /// <param name="workload">The workload.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public IList<IWorkloadDayBase> GetWorkloadDaysFromSkillDays(IEnumerable<ISkillDay> skillDays, IWorkload workload)
        {
            return (from sd in skillDays
                    from wd in sd.WorkloadDayCollection
                    where wd.Workload.Equals(workload)
                    orderby wd.CurrentDate
                    select (IWorkloadDayBase)wd).ToList();
        }

        /// <summary>
        /// Create all workload days that belongs to a set of skill days with the longterm template (one merged interval for the entire open hours).
        /// </summary>
        /// <param name="skill">The skill.</param>
        /// <param name="skillDays">The skill days.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-24
        /// </remarks>
        public void CreateLongtermWorkloadDays(ISkill skill,IEnumerable<ISkillDay> skillDays)
        {
        	foreach (var workload in skill.WorkloadCollection)
        	{
        		var skillDaysToProcess = from sd in skillDays
        		                         group sd by sd.WorkloadDayCollection.Count(wd => wd.Workload.Equals(workload))
        		                         into g
        		                         where g.Key == 0
        		                         select g;

				if (skillDaysToProcess.Any())
				{
					IDictionary<DayOfWeek, IWorkloadDayTemplate> basicTemplates = GetDefaultWorkloadDayTemplatesAsLongterm(workload);

        			var skillDaysForProcessing = skillDaysToProcess.First();
        			TaskOwnerHelper taskOwnerHelper = new TaskOwnerHelper(skillDaysForProcessing);
        			taskOwnerHelper.BeginUpdate();
        			foreach (var skillDay in skillDaysForProcessing)
        			{
        				IWorkloadDayTemplate workloadDayTemplate = basicTemplates[skillDay.CurrentDate.DayOfWeek];

        				IWorkloadDay workloadDay = new WorkloadDay();
        				workloadDay.CreateFromTemplate(skillDay.CurrentDate, workload, workloadDayTemplate);
        				skillDay.AddWorkloadDay(workloadDay);
        			}
        			taskOwnerHelper.EndUpdate();
        		}
        	}
        }

        /// <summary>
        /// Gets the default workload day templates.
        /// </summary>
        /// <param name="workload">The workload.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-25
        /// </remarks>
		private static IDictionary<DayOfWeek, IWorkloadDayTemplate> GetDefaultWorkloadDayTemplatesAsLongterm(IWorkload workload)
        {
        	IDictionary<DayOfWeek, IWorkloadDayTemplate> basicTemplates = new Dictionary<DayOfWeek, IWorkloadDayTemplate>();
        	foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof (DayOfWeek)))
        	{
        		IWorkloadDayTemplate dayTemplate =
        			(IWorkloadDayTemplate) workload.GetTemplate(TemplateTarget.Workload, dayOfWeek);
        		IWorkloadDayTemplate newTemplate = new WorkloadDayTemplate();
        		newTemplate.Create(TemplateReference.LongtermTemplateKey, DateTime.UtcNow, workload,
        		                   new List<TimePeriod>(dayTemplate.OpenHourList));
        		((WorkloadDayTemplate) newTemplate).MergeTemplateTaskPeriods(
        			new List<ITemplateTaskPeriod>(newTemplate.TaskPeriodList));
        		basicTemplates.Add(dayOfWeek, newTemplate);
        	}

        	return basicTemplates;
        }
	}
}