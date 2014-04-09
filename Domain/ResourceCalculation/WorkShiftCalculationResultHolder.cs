using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public  class WorkShiftCalculationResult : IWorkShiftCalculationResultHolder
    {
        public double Value { get; set; }
		public IShiftProjectionCache ShiftProjection { get; set; }
	    public IWorkShiftCalculatableProjection WorkShiftCalculatableProjection { get { return ShiftProjection; } }
    }
}
