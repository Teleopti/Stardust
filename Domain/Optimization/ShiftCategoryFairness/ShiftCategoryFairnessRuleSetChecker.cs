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
        public bool Check(IPersonPeriod period1, IPersonPeriod period2)
        {
            return period1.RuleSetBag == period2.RuleSetBag || period1.RuleSetBag.RuleSetCollection.Intersect(period2.RuleSetBag.RuleSetCollection).Any();
        }
    }
}
