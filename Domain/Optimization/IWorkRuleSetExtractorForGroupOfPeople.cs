using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IWorkRuleSetExtractorForGroupOfPeople
    {
        IEnumerable<IWorkShiftRuleSet> ExtractRuleSets(DateOnlyPeriod period);
    }
}