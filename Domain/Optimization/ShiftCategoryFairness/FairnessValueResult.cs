using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness
{
    public class FairnessValueResult : IFairnessValueResult
    {
        public double TotalNumberOfShifts { get; set; }
    }
}
