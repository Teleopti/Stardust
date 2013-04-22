using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness
{
    public interface IShiftCategoryFairnessRuleSetChecker
    {
        bool Check(IPersonPeriod period1, IPersonPeriod period2);
    }

    public class ShiftCategoryFairnessRuleSetChecker : IShiftCategoryFairnessRuleSetChecker
    {
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public bool Check(IPersonPeriod period1, IPersonPeriod period2)
        {
            return period1.RuleSetBag == period2.RuleSetBag;
        }
    }
}
