using System;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

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
    }
}