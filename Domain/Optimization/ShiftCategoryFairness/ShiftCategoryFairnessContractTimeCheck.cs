

using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness
{
    public interface IShiftCategoryFairnessContractTimeChecker
    {
        bool Check(IPersonPeriod period1, IPersonPeriod period2);
    }

    public class ShiftCategoryFairnessContractTimeChecker : IShiftCategoryFairnessContractTimeChecker
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public bool Check(IPersonPeriod period1, IPersonPeriod period2)
        {
            return period1.PersonContract.Contract == period2.PersonContract.Contract &&
                   period1.PersonContract.PartTimePercentage == period2.PersonContract.PartTimePercentage &&
                   period1.PersonContract.ContractSchedule == period2.PersonContract.ContractSchedule;
        }
    }
}
