using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IMultisiteDayRepository
    {
        ICollection<IMultisiteDay> FindRange(DateOnlyPeriod period, ISkill skill, IScenario scenario);
		
        ICollection<IMultisiteDay> GetAllMultisiteDays(DateOnlyPeriod period, ICollection<IMultisiteDay> multisiteDays, IMultisiteSkill skill, IScenario scenario, bool addToRepository = true);
		
        void Delete(DateOnlyPeriod dateTimePeriod, IMultisiteSkill multisiteSkill, IScenario scenario);
    }
}
