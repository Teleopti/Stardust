using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    

    public class FairnessValueResult : IFairnessValueResult
    {
        public double FairnessPoints { get; set; }
        public double TotalNumberOfShifts { get; set; }

        public double FairnessPointsPerShift
        {
            get
            {
                if (TotalNumberOfShifts == 0)
                    return 0;
                return FairnessPoints/TotalNumberOfShifts;
            }
        }

    }
}
