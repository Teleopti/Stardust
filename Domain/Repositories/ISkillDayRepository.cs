using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
   
	public interface ISkillDayRepository : IRepository<ISkillDay>
    {
      
        ICollection<ISkillDay> FindRange(DateOnlyPeriod period, ISkill skill, IScenario scenario);

        ICollection<ISkillDay> GetAllSkillDays(DateOnlyPeriod period, ICollection<ISkillDay> skillDays, ISkill skill, IScenario scenario, Action<IEnumerable<ISkillDay>> optionalAction);

        void Delete(DateOnlyPeriod dateTimePeriod, ISkill skill, IScenario scenario);

        DateOnly FindLastSkillDayDate(IWorkload workload, IScenario scenario);

        ISkillDay FindLatestUpdated(ISkill skill, IScenario scenario, bool withLongterm);

        ICollection<ISkillDay> FindReadOnlyRange(DateOnlyPeriod period, IEnumerable<ISkill> skills, IScenario scenario);

		bool HasSkillDaysWithinPeriod(DateOnly startDate, DateOnly endDate, IBusinessUnit businessUnit, IScenario scenario);
	}

	//refactor this PBI 35176
	public class SkillTaskDetailsModel
	{
		public string Name { get; set; }
		public DateTime Minimum { get; set; }
		public DateTime Maximum { get; set; }
		public Double TotalTasks { get; set; }
		public Guid Scenario { get; set; }
		public Guid SkillId { get; set; }
	}
}