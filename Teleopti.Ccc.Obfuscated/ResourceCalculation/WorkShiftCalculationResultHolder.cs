using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Obfuscated.ResourceCalculation
{
    public  class WorkShiftCalculationResult : IWorkShiftCalculationResultHolder
    {
        public double Value { get; set; }
		public IShiftProjectionCache ShiftProjection { get; set; }
	    public IWorkShiftCalculatableProjection ShiftProjection2 { get { return ShiftProjection; } }
    }
}
