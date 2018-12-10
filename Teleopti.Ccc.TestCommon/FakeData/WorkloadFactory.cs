using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Workload factory
    /// </summary>
    public static class WorkloadFactory
    {
        /// <summary>
        /// Creates a SkillType
        /// </summary>
        /// <returns></returns>
        public static IWorkload CreateWorkload(ISkill skill)
        {
            IWorkload workload = new Workload(skill);
            workload.Description = "desc from factory";
			   workload.Name = "name from factory";
            return workload;
        }

        public static IWorkload CreateWorkload(string name, ISkill skill)
        {
            IWorkload workload = new Workload(skill);
            workload.Description = "desc from factory";
            workload.Name = name;
            return workload;
        }

        public static IWorkload CreateWorkloadWithFullOpenHours(ISkill skill)
        {
            IWorkload workload = new Workload(skill);
            workload.Description = "desc from factory";
            workload.Name = "name from factory";
            workload.TemplateWeekCollection.ForEach(x=>x.Value.MakeOpen24Hours());
            return workload;
        }

		public static IWorkload CreateWorkloadWithFullOpenHoursDuringWeekdays(ISkill skill)
		{
			var workload = CreateWorkload(skill);
			for (var i = 0; i < workload.TemplateWeekCollection.Count; i++)
			{
				if (i == 0 || i == 6)
				{
					workload.TemplateWeekCollection[i].Close();
				}
				else
				{
					workload.TemplateWeekCollection[i].MakeOpen24Hours();
				}
			}
			return workload;
		}

		public static IWorkload CreateWorkloadWithFullOpenHoursDuringWeekdaysAndClosedOnWeekends(ISkill skill)
		{
			var workload = CreateWorkload(skill);
			for (var i = 0; i < workload.TemplateWeekCollection.Count; i++)
			{
				if (i == 0 || i == 6)
				{
					workload.TemplateWeekCollection[i].Close();
				}
				else
				{
					workload.TemplateWeekCollection[i].MakeOpen24Hours();
				}
			}
			return workload;
		}

		public static IWorkload CreateWorkloadWithOpenHours(ISkill skill, params TimePeriod[] openHours)
		{
			IWorkload workload = new Workload(skill);
			workload.Description = "desc from factory";
			workload.Name = "name from factory";
			workload.TemplateWeekCollection.ForEach(x => x.Value.ChangeOpenHours(openHours));
			return workload;
		}

		public static IWorkload CreateWorkloadWithOpenHoursOnDays(ISkill skill, IDictionary<DayOfWeek, TimePeriod> days)
		{
			IWorkload workload = new Workload(skill);
			workload.Description = "desc from factory";
			workload.Name = "name from factory";
			for (var i = 0; i < workload.TemplateWeekCollection.Count; i++)
			{
				var open = false;
				foreach (var day in days)
				{
					var weekDay = (int)day.Key;
					if (!weekDay.Equals(i)) continue;
					workload.TemplateWeekCollection[i].ChangeOpenHours(new[] { day.Value });
					open = true;
					break;
				}
				if (!open)
				{
					workload.TemplateWeekCollection[i].Close();
				}
			}

			return workload;
		}

		public static IWorkload CreateWorkloadThatIsClosed(ISkill skill)
		{
			IWorkload workload = new Workload(skill);
			workload.Description = "desc from factory";
			workload.Name = "name from factory";
			workload.TemplateWeekCollection.ForEach(x => x.Value.Close());
			return workload;
		}

	    public static IWorkload CreateWorkloadClosedOnWeekendsWithOpenHours(ISkill skill, params TimePeriod[] openHours)
	    {
			var workload = CreateWorkload(skill);
			for (var i = 0; i < workload.TemplateWeekCollection.Count; i++)
			{
				if (i == 0 || i == 6)
				{
					workload.TemplateWeekCollection[i].Close();
				}
				else
				{
					workload.TemplateWeekCollection[i].ChangeOpenHours(openHours);
				}
			}
			return workload;
		}

	 }
}