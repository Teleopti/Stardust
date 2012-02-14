using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Obfuscated.ResourceCalculation
{
    public  class WorkShiftCalculationResultHolder : IWorkShiftCalculationResultHolder
    {
        public double Value { get; set; }
        public IShiftProjectionCache ShiftProjection { get; set; }
    }
}
