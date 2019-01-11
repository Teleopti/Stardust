using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
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
		void CreateLongtermWorkloadDays(ISkill skill, IEnumerable<ISkillDay> skillDays);
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
			return skillDays.SelectMany(sd => sd.WorkloadDayCollection)
				.Where(wd => wd.Workload.Equals(workload))
				.OrderBy(wd => wd.CurrentDate)
				.OfType<IWorkloadDayBase>()
				.ToArray();
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