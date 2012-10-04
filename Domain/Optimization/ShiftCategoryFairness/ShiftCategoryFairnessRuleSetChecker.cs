using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness
{
    public class ShiftCategoryFairnessRuleSetChecker
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
            "CA1062:Validate arguments of public methods", MessageId = "1"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
             "CA1062:Validate arguments of public methods", MessageId = "0"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public bool Check(IPersonPeriod period1, IPersonPeriod period2)
        {
            return period1.RuleSetBag == period2.RuleSetBag ||
                   period1.RuleSetBag.RuleSetCollection.Intersect(period2.RuleSetBag.RuleSetCollection).Any();
        }
    }
}
