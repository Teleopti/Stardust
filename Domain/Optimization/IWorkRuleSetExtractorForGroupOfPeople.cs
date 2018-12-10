using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IWorkRuleSetExtractorForGroupOfPeople
    {
        IEnumerable<IWorkShiftRuleSet> ExtractRuleSets(DateOnlyPeriod period);
    }
}