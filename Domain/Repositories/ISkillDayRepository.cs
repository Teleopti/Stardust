using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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

		ICollection<ISkillDay> LoadSkillDays(IEnumerable<Guid> skillDaysIdList);
	}
}