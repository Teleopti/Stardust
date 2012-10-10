

using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness
{
    public interface IShiftCategoryFairnessContractTimeChecker
    {
        bool Check(IScheduleDay day1, IScheduleDay day2);
    }

    public class ShiftCategoryFairnessContractTimeChecker : IShiftCategoryFairnessContractTimeChecker
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public bool Check(IScheduleDay day1, IScheduleDay day2)
        {
            return day1.ProjectionService().CreateProjection().ContractTime()
                .Equals(day2.ProjectionService().CreateProjection().ContractTime());
        }
    }
}
