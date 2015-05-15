using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness
{
    

    public class FairnessValueResult : IFairnessValueResult
    {
        public double FairnessPoints { get; set; }
        public double TotalNumberOfShifts { get; set; }

        public double FairnessPointsPerShift
        {
            get { return 0; }
        }

    }
}
